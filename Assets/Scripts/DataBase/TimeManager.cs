using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;     // "HH:MM AM/PM"
    [SerializeField] private TMP_Text dayCountText; // "Day X"

    [Header("References")]
    [SerializeField] private PlayerController player; // Ссылка на PlayerController

    private void Start()
    {
        // Если не указали в инспекторе, пытаемся найти
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        // Сразу обновим UI в Start
        UpdateTimeUI();
        CheckDayOrNight();
    }

    private void Update()
    {
        if (player == null) return;

        // 1. Обновляем игровое время (по аналогии, но все расчеты ведем в PlayerController)
        UpdateGameTime();

        // 2. Обновляем UI
        UpdateTimeUI();

        // 3. Определяем день/ночь
        CheckDayOrNight();
    }

    /// <summary>
    /// Логика прогрессии игрового времени (вместо timeScale local – используем player.timeScale)
    /// </summary>
    private void UpdateGameTime()
    {
        float hoursPerSecond = player.timeScale / 60f;
        player.currentHour += hoursPerSecond * Time.deltaTime;

        if (player.currentHour >= 24f)
        {
            player.currentHour -= 24f;
            player.dayCount++;
        }
    }

    private void CheckDayOrNight()
    {
        // 8..20 -> день
        if (player.currentHour >= 8f && player.currentHour < 20f)
            player.isDay = true;
        else
            player.isDay = false;
    }

    private void UpdateTimeUI()
    {
        if (player == null) return;

        float h = player.currentHour;
        if (h < 0) h += 24;
        if (h >= 24) h -= 24;

        // AM/PM
        string ampm = (h < 12) ? "AM" : "PM";
        int hour12 = Mathf.FloorToInt(h) % 12;
        if (hour12 == 0) hour12 = 12;

        float fraction = h - Mathf.Floor(h);
        int minutes = Mathf.FloorToInt(fraction * 60);

        // Формируем строку
        string timeString = string.Format("{0:D2}:{1:D2} {2}", hour12, minutes, ampm);
        if (timeText != null)
            timeText.text = timeString;

        // "Day X"
        if (dayCountText != null)
            dayCountText.text = $"Day {player.dayCount}";
    }

    /// <summary>
    /// Кнопка "Спать": переносим на 8 AM, +1 день, и вызываем у PlayerController сохранение
    /// </summary>
 
}
