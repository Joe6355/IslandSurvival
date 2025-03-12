using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("������ ������������")]
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    [SerializeField] private Joystick joystick;
    private Vector2 movement;

    [Header("������� ������")]
    public float maxHealth = 100f;
    public float health = 100f;
    public float maxStamina = 100f;
    public float stamina = 100f;
    public float maxMood = 100f;
    public float mood = 100f;

    [Header("�������� ��������� ��������")]
    // ��� �������� ������� ����������� �� ��������� 1f/�
    public float staminaDecreaseRate = 1f;
    // � ����� ������� ����������������� �� ��������� 0.8f/�
    public float staminaRecoveryRate = 0.8f;
    // ���������� ������ ������ �� ��������� 0.1f/�
    public float moodDecreaseRate = 0.1f;

    [Header("UI ��������")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image moodBar;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void MoveInput()
    {
        // �������� ��� � ���������
        float horizontal = joystick.Horizontal * moveSpeed;
        float vertical = joystick.Vertical * moveSpeed;
        movement = new Vector2(horizontal, vertical);
    }

    private void FixedUpdate()
    {
        MoveInput();
        rb.velocity = movement;

        // ��������� ��������� ������
        UpdateStats();
        // ��������� ���������� ����������� �����
        UpdateUI();
    }

    private void UpdateStats()
    {
        // ���� ����� ��������� (��������� ��������� �����)
        if (movement.magnitude > 0.1f)
        {
            stamina -= staminaDecreaseRate * Time.fixedDeltaTime;
        }
        else // � ����� ��������������� �������
        {
            stamina += staminaRecoveryRate * Time.fixedDeltaTime;
        }

        // ���������� �������� ����������
        mood -= moodDecreaseRate * Time.fixedDeltaTime;

        // ������������ �������� �� 0 �� ���������
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        mood = Mathf.Clamp(mood, 0, maxMood);
    }

    private void UpdateUI()
    {
        // ��������� �������� fillAmount ��� ������� Image,
        // �������� ��� ��� ��������� �������� �������� � �������������
        if (healthBar != null)
            healthBar.fillAmount = health / maxHealth;
        if (staminaBar != null)
            staminaBar.fillAmount = stamina / maxStamina;
        if (moodBar != null)
            moodBar.fillAmount = mood / maxMood;
    }
}
