using MySql.Data.MySqlClient;
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

        // �������������� ���������� ������ 10 ������
        InvokeRepeating("SavePlayerData", 10f, 10f);
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

    public void LoadPlayerData(float hp, float stamina, float mood, float posX, float posY)
    {
        Debug.Log($"[PlayerController] ��������� ������: HP={hp}, Stamina={stamina}, Mood={mood}, PosX={posX}, PosY={posY}");

        this.health = hp;
        this.stamina = stamina;
        this.mood = mood;
        transform.position = new Vector3(posX, posY, 0);

        this.stamina = Mathf.Clamp(this.stamina, 0, maxStamina);
        this.mood = Mathf.Clamp(this.mood, 0, maxMood);

        UpdateUI();
    }

    public void SavePlayerData()
    {
        Debug.Log("[PlayerController] SavePlayerData() ������!");
        StartCoroutine(SendPlayerDataToServer());
        if (string.IsNullOrEmpty(MySQLLogin.ConnectionString))
        {
            Debug.LogError("������: ������ ����������� �����. ������ �� ���������.");
            return;
        }
    }

    private IEnumerator SendPlayerDataToServer()
    {
        using (MySqlConnection conn = new MySqlConnection(MySQLLogin.ConnectionString))
        {
            try
            {
                conn.Open();

                string updateQuery = @"
                UPDATE player_data
                SET hp = @hp, stamina = @stamina, mood = @mood, pos_x = @posX, pos_y = @posY
                WHERE user_id = @userId;
            ";

                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@userId", MySQLLogin.LoggedUserId);
                    updateCmd.Parameters.AddWithValue("@hp", health);
                    updateCmd.Parameters.AddWithValue("@stamina", stamina);
                    updateCmd.Parameters.AddWithValue("@mood", mood);
                    updateCmd.Parameters.AddWithValue("@posX", transform.position.x);
                    updateCmd.Parameters.AddWithValue("@posY", transform.position.y);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    Debug.Log($"[SQL] {rowsAffected} �����(�) ���������.");

                    if (rowsAffected == 0)
                    {
                        Debug.LogError("[SQL] ������: `UPDATE` �� ������� ������! ��������, ������ ��� � ����.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError("[SQL] ������ ���������� ������ ������: " + ex.Message);
            }
        }
        yield return null;
    }


}
