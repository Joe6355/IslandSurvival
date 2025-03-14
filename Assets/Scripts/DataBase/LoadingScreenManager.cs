using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;     // Панель (GameObject) с черным фоном
    [SerializeField] private TextMeshProUGUI loadingText; // Текст "Загрузка..."
    [Header("Можно вызывть в любом месте кода //LoadingScreenManager.Show();    //// ... Выполняем загрузку ...    //LoadingScreenManager.Hide();")]
    private CanvasGroup canvasGroup;                      // Для плавной анимации исчезновения


    
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

        // Проверяем, что loadingPanel назначен
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
        canvasGroup.alpha = 1; // Стартовое значение (полная непрозрачность)
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // По умолчанию можно спрятать панель, если хотим её выключенной в начале
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
            Debug.LogError("[LoadingScreenManager] Ошибка: Instance не найден! Возможно, скрипт не в сцене?");
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
    // Методы экземпляра, которые вызывают корутины
    // ----------------------------------------------

    /// <summary>
    /// Включаем панель и запускаем анимацию точек
    /// </summary>
    public void ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);
        // Ставим альфу в 1 (если вдруг она была 0)
        if (canvasGroup != null) canvasGroup.alpha = 1;

        // Запускаем корутину с анимацией текста
        StartCoroutine(AnimateLoadingText());
    }

    /// <summary>
    /// Плавно скрываем панель (через CanvasGroup)
    /// </summary>
    public void HideLoadingScreen()
    {
        // Останавливаем корутину с точками, чтобы не конфликтовать
        StopAllCoroutines();

        // Запускаем корутину плавного исчезновения
        StartCoroutine(FadeOutLoadingScreen());
    }

    // ----------------------------------------------
    // Корутин с плавной анимацией исчезновения
    // ----------------------------------------------

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
            canvasGroup.alpha -= Time.deltaTime * 1; // скорость плавного исчезновения
            yield return null;
        }
        canvasGroup.alpha = 0;
        loadingPanel.SetActive(false);
    }

    // ----------------------------------------------
    // Корутин с анимацией "...", можно крутить точки
    // ----------------------------------------------

    private IEnumerator AnimateLoadingText()
    {
        // Если не задан loadingText, просто выходим
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
    }
}
