using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;     // Панель (GameObject) с черным фоном
    [SerializeField] private TextMeshProUGUI loadingText; // Текст "Загрузка..."

    [Header("Подсказка вызова")]
    [TextArea(3,5)]
    //[SerializeField] private string usageInfo = 
    //    "Вызывай LoadingScreenManager.Show() / LoadingScreenManager.Hide() в любом месте кода.";

    private CanvasGroup canvasGroup; // Для плавной анимации исчезновения
    private Coroutine animateTextCoroutine; // Ссылка на корутину AnimateLoadingText
    private Coroutine fadeOutCoroutine;     // Ссылка на корутину FadeOutLoadingScreen

    private void Awake()
    {
        // Реализуем Singleton: если объект уже есть, новый уничтожаем
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохраняем при смене сцен
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingPanel == null)
        {
            Debug.LogError("[LoadingScreenManager] Ошибка: LoadingPanel не назначен!");
            return;
        }

        // Пытаемся взять (или добавить) CanvasGroup для плавного исчезновения
        canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = loadingPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;         // Стартовое значение
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // Если хотите, чтобы при старте была скрыта панель:
        // loadingPanel.SetActive(false);
        // canvasGroup.alpha = 0;
    }

    // ----------------------------------------------
    // Статические методы для упрощенного вызова
    // ----------------------------------------------

    /// <summary>
    /// Показываем загрузочный экран
    /// </summary>
    public static void Show()
    {
        if (Instance == null)
        {
            Debug.LogError("[LoadingScreenManager] Ошибка: Instance не найден! Возможно, скрипт отсутствует в сцене?");
            return;
        }
        Instance.ShowLoadingScreen();
    }

    /// <summary>
    /// Скрываем загрузочный экран
    /// </summary>
    public static void Hide()
    {
        if (Instance == null)
        {
            Debug.LogError("[LoadingScreenManager] Ошибка: Instance не найден!");
            return;
        }
        Instance.HideLoadingScreen();
    }

    // ----------------------------------------------
    // Методы экземпляра
    // ----------------------------------------------

    /// <summary>
    /// Активируем панель, запускаем корутину анимации текста
    /// </summary>
    public void ShowLoadingScreen()
    {
        // Если идёт FadeOut - остановим его
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        loadingPanel.SetActive(true);
        // Ставим альфу в 1 (если вдруг была 0)
        canvasGroup.alpha = 1f;

        // Запускаем корутину анимации "...", если не запущена
        if (animateTextCoroutine == null)
        {
            animateTextCoroutine = StartCoroutine(AnimateLoadingText());
        }
    }

    /// <summary>
    /// Плавно скрываем панель
    /// </summary>
    public void HideLoadingScreen()
    {
        // Останавливаем корутину анимации точек,
        // чтобы не конфликтовать во время исчезновения.
        if (animateTextCoroutine != null)
        {
            StopCoroutine(animateTextCoroutine);
            animateTextCoroutine = null;
        }

        // Запускаем корутину плавного исчезновения
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        fadeOutCoroutine = StartCoroutine(FadeOutLoadingScreen());
    }

    /// <summary>
    /// Плавно уменьшаем alpha и отключаем панель
    /// </summary>
    private IEnumerator FadeOutLoadingScreen()
    {
        if (canvasGroup == null)
        {
            // Если нет CanvasGroup, просто скрываем объект
            loadingPanel.SetActive(false);
            yield break;
        }

        // Плавно уменьшаем alpha
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 1f; // скорость исчезновения
            yield return null;
        }

        // Когда alpha=0 - скрываем
        canvasGroup.alpha = 0f;
        loadingPanel.SetActive(false);
        fadeOutCoroutine = null;
    }

    /// <summary>
    /// Корутин с анимацией "Загрузка."
    /// </summary>
    private IEnumerator AnimateLoadingText()
    {
        if (loadingText == null)
            yield break;

        string baseText = "Загрузка";
        while (loadingPanel.activeSelf)
        {
            loadingText.text = baseText + ".";
            yield return new WaitForSeconds(0.3f);

            loadingText.text = baseText + "..";
            yield return new WaitForSeconds(0.3f);

            loadingText.text = baseText + "...";
            yield return new WaitForSeconds(0.3f);
        }

        animateTextCoroutine = null;
    }
}
