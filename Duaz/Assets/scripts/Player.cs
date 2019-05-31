﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float Speed; // текущая скорость персонажа
    public Vector2 RangeSpeed = new Vector2(2, 5); // Минимальная и максимальная скорость игрока
    public float SpeedUp = 2.0f; // Ускорение при разовом нажатии
    public float SpeedUpDecrease = 0.15f; // Значение в процентах, на которое уменьшается SpeedUp
    float DefaultSpeedUp; // Изначальное значение SpeedUp
    float SpeedUpMax; // Ускорение при разовом нажатии
    public float SpeedLoss = 1.0f; // Потеря скорости при бездействии
    public float MaxEnergy = 100.0f;  // Максимальный запас энергии
    public float Energy = 100.0f;  // запас энергии
    public float DelyRecoveryEnergy; // временная здаержка восстновления шкалы енергииж
    float RecoveryEnergy; // Значение восстановления энергии
    public float EnergyTimer;
    public bool run;
    public bool RunFast;
    bool SmoothRunFastLayer2Anim; // Параметер второго анимациинного слоя на персонаже, параметер отвчает за прозрачность анимационного слоя
    float TimerRunFastLayer2Anim; // Время поистечению которого анимацинной слой ускорения становится прозрачным
    public bool isJump = false, isJumpOver = false; // используется в анимации при прыжке

    public Interface Interface; // ссылка на скрипт
    public Animator InterfaceAnimator; //Ссылка на аниматор графического интерфейса
    public GameObject EnergyBorder; // для анимации, когда заканчивается энергия

    public Text debug;

    Animator animator;
    Rigidbody rigidbody;
    Vector3 xMovePlayer, xLateMovePlayer; // х Персонажа до и после кадра
    bool isOneTouch, isTouchHold;

    // Use this for initialization
    void Start() {

        Energy = MaxEnergy;
        animator = this.GetComponent<Animator>();
        Speed = RangeSpeed.x;
        SpeedUpMax = SpeedUp;
        rigidbody = GetComponent<Rigidbody>();
        DefaultSpeedUp = SpeedUp;
        RecoveryEnergy = Energy * 0.15f;

        Input.simulateMouseWithTouches = true;
    }

    void FixedUpdate()
    {
        if (run)
        {
            rigidbody.MovePosition(transform.position + transform.right * Speed * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (! isOneTouch)
        {
            if (EnergyTimer > 0)
            {
                EnergyTimer -= Time.deltaTime;
            }
            else
            {
                if (Energy < MaxEnergy)
                {
                    Energy += RecoveryEnergy * Time.deltaTime;
                    SpeedUp = Mathf.Lerp(SpeedUp, SpeedUpMax, Time.deltaTime);
                    Interface.CalculationSizeEnergyBar();
                }
            }

            if (!isTouchHold)
            {
                CalculationSpeed();
            }

        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
        {
            SwipeUp();
        }
        if (Input.GetMouseButtonDown(0))
        {
            isOneTouch = true;
            OneTouch();
        }
        if (Input.GetMouseButton(0))
        {
            isTouchHold = true;
            HoldTouch();
        }
        if (Input.GetMouseButtonUp(0))
        {
            isTouchHold = false;
        }
#endif

        // Рассчёт скорости анимации бега
        animator.SetFloat("speed", 0.5f + ((Speed * 100) / (RangeSpeed.y - RangeSpeed.x) * 0.5f) / 100.0f);
        if (SmoothRunFastLayer2Anim)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0.75f, Time.deltaTime * 5));
            if (TimerRunFastLayer2Anim > 0)
            {
                TimerRunFastLayer2Anim -= Time.deltaTime;
            }
            else
            {
                SmoothRunFastLayer2Anim = false;
            }
        }
        else
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0.0f, Time.deltaTime * 2));
        }

        if (isJump)
        {
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 1.0f, Time.deltaTime * 2.0f * Speed));
        }
        else if (isJumpOver)
        {
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 0.0f, Time.deltaTime * 1.5f * Speed));
            if (animator.GetLayerWeight(2) <= 0.08f)
            {
                animator.SetLayerWeight(2, 0.0f);
                isJumpOver = false;
            }
        }

        // Проверка прикосновения к экрану
        TouchScreen();

    }
    // рассчет скорости бега персонажа
    void CalculationSpeed()
    {
        if (Speed > RangeSpeed.x)
        {
            Speed -= Time.deltaTime * SpeedLoss;
        }
        else
        {
            Speed += Time.deltaTime * 0.2f;
        }
        if (SpeedUp < DefaultSpeedUp)
        {
            //SpeedUpRecovery();
        }
    }

    Vector2 startPos;
    /*Определение касаний экрана*/
    void TouchScreen()
    {
        debug.text = "Debug:\nКоличество касаний: " + Input.touchCount + "\n";                                          /// DEBUG   DEBUG   DEBUG   DEBUG
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                // Отслеживание действия касания
                switch (touch.phase)
                {
                    // Если только что докаснулся до экрана
                    case TouchPhase.Began:
                        startPos = touch.position;
                        break;

                    // Если было совершено перемещение пальцем
                    case TouchPhase.Moved:
                        // Если свайп вверх и длина свайпа больше 15% высоты экрана
                        if (touch.position.y > startPos.y && touch.position.y - startPos.y > Screen.height * 0.15f)
                        {
                            debug.text += touch.deltaPosition.magnitude / touch.deltaTime;                              /// DEBUG   DEBUG   DEBUG   DEBUG
                            // Если это был быстрый свайп
                            if (touch.deltaPosition.magnitude / touch.deltaTime >= 600)
                            {
                                SwipeUp();
                            }
                        }
                        break;

                    case TouchPhase.Stationary:
                        debug.text += "Touch hold";                                                                    /// DEBUG   DEBUG   DEBUG   DEBUG
                        isTouchHold = true;
                        HoldTouch();
                        break;

                    // Если палец был убран с экрана
                    case TouchPhase.Ended:
                        // Если отпускание пальца лежит в доступном радиусе (1% от высоты экрана) от прикосновения пальца до экрана
                        if ((touch.position-startPos).magnitude <= Screen.height * 0.01f)
                        {
                            isOneTouch = true;
                            OneTouch();
                        }
                        isTouchHold = false;
                        break;
                }
            }
        }
    }

    /*рассчет действия на разовое нажатие на экран (ускорение)*/
    void OneTouch()
    {
        if (Energy > 0)
        {
            // Если скорость меньше максимальной
            if (Speed < RangeSpeed.y)
            {
                // уменьшение запаса выносливости и увеличение текущей скорости
                Energy -= 1.0f;
                Speed += SpeedUp;
                // уменьшение индекса ускорения и запуск таймер восстновления выносливости
                SpeedUp -= SpeedUp * SpeedUpDecrease;
                EnergyTimer = DelyRecoveryEnergy;
            }
            else
            {
                Energy -= 0.5f;
                //EnergyTimer = DelyRecoveryEnergy;
            }
            Interface.CalculationSizeEnergyBar();
        }
        else
        {
            Energy = 0;
            Interface.CalculationSizeEnergyBar();
            if (!EnergyBorder.activeSelf)
            {
                InterfaceAnimator.Play("energy border");
            }
        }
        SmoothRunFastLayer2Anim = true;
        TimerRunFastLayer2Anim = 0.5f;

        isOneTouch = false;
    }

    /*Если удержание пальца на экарне*/
    void HoldTouch()
    {
        Energy -= 0.5f * Time.deltaTime;
        TimerRunFastLayer2Anim = 0.5f;
        if (Energy <= 0)
        {
            isTouchHold = false;
            TimerRunFastLayer2Anim = 0.0f;
            CalculationSpeed();
        }
        else
        {
            SmoothRunFastLayer2Anim = true;
        }
        Interface.CalculationSizeEnergyBar();
    }

    void SwipeUp()
    {
        if (animator.GetLayerWeight(2) == 0.0f)
        {
            animator.SetBool("jump", true);
            isJumpOver = false;
            isJump = true;
        }
    }

    void JumpAddForce()
    {
        rigidbody.AddForce(transform.up * 350, ForceMode.Impulse);
    }

    void JumpOver()
    {
        animator.SetBool("jump", false);
        isJump = false;
        isJumpOver = true;
    }

    void AnimationStartAnimSpeedZero()
    {
        animator.SetFloat("speed", 0.0f);
    }

    void AnimationStartAnimSpeedRun()
    {
        animator.SetFloat("speed", 1.0f);
        animator.SetBool("run", true);
        run = true;
        this.GetComponent<Parallax>().enabled = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Barrier")
        {
            Destroy(collision.gameObject);
            Speed = Speed * 0.75f;
        }
    }
    
    
}
