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
    public float staminaDecreaseRate = 1f;
    public float staminaRecoveryRate = 0.8f;
    public float moodDecreaseRate = 0.1f;
    public float hpRegenRate = 2f; // HP-�����, ���� mood>50 � stamina>50

    [Header("UI ��������")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image moodBar;

    [Header("����� (����/����)")]
    public float timeScale = 1f;   // 1f => 1 ������ �� 1 ��� IRL
    public float currentHour = 7f;   // 0..24
    public int dayCount = 1;
    public bool isDay;

    [Header("���. ��������� (�� ��)")]
    public int kills;
    public int resourcesGathered;
    public int crafts;
    public int prayers;
    public float playtimeMinutes; // ������� �������� ����� ����� �����

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        MoveInput();
        rb.velocity = movement;

        UpdateStats();
        UpdateUI();
    }

    private void Update()
    {
        // ������� ������� ����� (����/����)
        UpdateGameTime(Time.deltaTime);

        // ���������� �������� ����� � playtimeMinutes
        // Time.deltaTime (���) => ����� �� 60, �������� ������
        playtimeMinutes += Time.deltaTime / 60f;
    }

    private void MoveInput()
    {
        float horizontal = joystick.Horizontal * moveSpeed;
        float vertical = joystick.Vertical * moveSpeed;
        movement = new Vector2(horizontal, vertical);
    }

    private void UpdateStats()
    {
        // �������
        if (movement.magnitude > 0.1f)
            stamina -= staminaDecreaseRate * Time.fixedDeltaTime;
        else
            stamina += staminaRecoveryRate * Time.fixedDeltaTime;

        // ����������
        mood -= moodDecreaseRate * Time.fixedDeltaTime;

        // HP-����� (���� mood>50 � stamina>50)
        if (mood > 50f && stamina > 50f)
        {
            health += hpRegenRate * Time.fixedDeltaTime;
            health = Mathf.Clamp(health, 0f, maxHealth);
        }

        // ������������ �������, ����������
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        mood = Mathf.Clamp(mood, 0, maxMood);
    }

    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = health / maxHealth;
        if (staminaBar != null)
            staminaBar.fillAmount = stamina / maxStamina;
        if (moodBar != null)
            moodBar.fillAmount = mood / maxMood;
    }

    private void UpdateGameTime(float deltaSec)
    {
        float hoursPerSec = timeScale / 60f;
        currentHour += hoursPerSec * deltaSec;

        if (currentHour >= 24f)
        {
            currentHour -= 24f;
            dayCount++;
        }

        // ����: 8..20
        if (currentHour >= 8f && currentHour < 20f)
            isDay = true;
        else
            isDay = false;
    }

    /// <summary>
    /// "�����": ��������� �� 8 AM, +1 ����
    /// </summary>
    public void GoToSleep()
    {
        currentHour = 8f;
        dayCount++;
    }

    /// <summary>
    /// ������������� ������, ��������� �� ��
    /// </summary>
    public void SetPlayerData(
        float hp, float stamina, float mood,
        float posX, float posY,
        int dayCount, float hour,
        int kills, int resourcesGathered,
        int crafts, int prayers,
        float playtime
    )
    {
        // ��������, �������, ����������
        this.health = Mathf.Clamp(hp, 0, maxHealth);
        this.stamina = Mathf.Clamp(stamina, 0, maxStamina);
        this.mood = Mathf.Clamp(mood, 0, maxMood);

        // �������
        transform.position = new Vector3(posX, posY, 0);

        // ����/�����
        this.dayCount = dayCount;
        this.currentHour = hour;

        // ���. ����
        this.kills = kills;
        this.resourcesGathered = resourcesGathered;
        this.crafts = crafts;
        this.prayers = prayers;
        this.playtimeMinutes = playtime;
    }
}
