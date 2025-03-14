using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Логика передвижения")]
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    [SerializeField] private Joystick joystick;
    private Vector2 movement;

    [Header("Статусы игрока")]
    public float maxHealth = 100f;
    public float health = 100f;
    public float maxStamina = 100f;
    public float stamina = 100f;
    public float maxMood = 100f;
    public float mood = 100f;

    [Header("Скорости изменения статусов")]
    public float staminaDecreaseRate = 1f;
    public float staminaRecoveryRate = 0.8f;
    public float moodDecreaseRate = 0.1f;
    public float hpRegenRate = 2f; // HP-реген, если mood>50 и stamina>50

    [Header("UI Элементы")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image moodBar;

    [Header("Время (День/Ночь)")]
    public float timeScale = 1f;   // 1f => 1 минута за 1 сек IRL
    public float currentHour = 7f;   // 0..24
    public int dayCount = 1;
    public bool isDay;

    [Header("Доп. Параметры (из БД)")]
    public int kills;
    public int resourcesGathered;
    public int crafts;
    public int prayers;
    public float playtimeMinutes; // Сколько реальных минут игрок провёл

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
        // Считаем игровое время (день/ночь)
        UpdateGameTime(Time.deltaTime);

        // Прибавляем реальное время в playtimeMinutes
        // Time.deltaTime (сек) => делим на 60, получаем минуты
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
        // Стамина
        if (movement.magnitude > 0.1f)
            stamina -= staminaDecreaseRate * Time.fixedDeltaTime;
        else
            stamina += staminaRecoveryRate * Time.fixedDeltaTime;

        // Настроение
        mood -= moodDecreaseRate * Time.fixedDeltaTime;

        // HP-реген (если mood>50 и stamina>50)
        if (mood > 50f && stamina > 50f)
        {
            health += hpRegenRate * Time.fixedDeltaTime;
            health = Mathf.Clamp(health, 0f, maxHealth);
        }

        // Ограничиваем стамину, настроение
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

        // День: 8..20
        if (currentHour >= 8f && currentHour < 20f)
            isDay = true;
        else
            isDay = false;
    }

    /// <summary>
    /// "Спать": переносим на 8 AM, +1 день
    /// </summary>
    public void GoToSleep()
    {
        currentHour = 8f;
        dayCount++;
    }

    /// <summary>
    /// Устанавливаем данные, пришедшие из БД
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
        // Здоровье, стамина, настроение
        this.health = Mathf.Clamp(hp, 0, maxHealth);
        this.stamina = Mathf.Clamp(stamina, 0, maxStamina);
        this.mood = Mathf.Clamp(mood, 0, maxMood);

        // Позиция
        transform.position = new Vector3(posX, posY, 0);

        // День/время
        this.dayCount = dayCount;
        this.currentHour = hour;

        // Доп. поля
        this.kills = kills;
        this.resourcesGathered = resourcesGathered;
        this.crafts = crafts;
        this.prayers = prayers;
        this.playtimeMinutes = playtime;
    }
}
