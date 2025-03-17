using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;     // ������ (GameObject) � ������ �����
    [SerializeField] private TextMeshProUGUI loadingText; // ����� "��������..."

    [Header("��������� ������")]
    [TextArea(3,5)]
    //[SerializeField] private string usageInfo = 
    //    "������� LoadingScreenManager.Show() / LoadingScreenManager.Hide() � ����� ����� ����.";

    private CanvasGroup canvasGroup; // ��� ������� �������� ������������
    private Coroutine animateTextCoroutine; // ������ �� �������� AnimateLoadingText
    private Coroutine fadeOutCoroutine;     // ������ �� �������� FadeOutLoadingScreen

    private void Awake()
    {
        // ��������� Singleton: ���� ������ ��� ����, ����� ����������
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ��������� ��� ����� ����
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingPanel == null)
        {
            Debug.LogError("[LoadingScreenManager] ������: LoadingPanel �� ��������!");
            return;
        }

        // �������� ����� (��� ��������) CanvasGroup ��� �������� ������������
        canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = loadingPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;         // ��������� ��������
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // ���� ������, ����� ��� ������ ���� ������ ������:
        // loadingPanel.SetActive(false);
        // canvasGroup.alpha = 0;
    }

    // ----------------------------------------------
    // ����������� ������ ��� ����������� ������
    // ----------------------------------------------

    /// <summary>
    /// ���������� ����������� �����
    /// </summary>
    public static void Show()
    {
        if (Instance == null)
        {
            Debug.LogError("[LoadingScreenManager] ������: Instance �� ������! ��������, ������ ����������� � �����?");
            return;
        }
        Instance.ShowLoadingScreen();
    }

    /// <summary>
    /// �������� ����������� �����
    /// </summary>
    public static void Hide()
    {
        if (Instance == null)
        {
            Debug.LogError("[LoadingScreenManager] ������: Instance �� ������!");
            return;
        }
        Instance.HideLoadingScreen();
    }

    // ----------------------------------------------
    // ������ ����������
    // ----------------------------------------------

    /// <summary>
    /// ���������� ������, ��������� �������� �������� ������
    /// </summary>
    public void ShowLoadingScreen()
    {
        // ���� ��� FadeOut - ��������� ���
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        loadingPanel.SetActive(true);
        // ������ ����� � 1 (���� ����� ���� 0)
        canvasGroup.alpha = 1f;

        // ��������� �������� �������� "...", ���� �� ��������
        if (animateTextCoroutine == null)
        {
            animateTextCoroutine = StartCoroutine(AnimateLoadingText());
        }
    }

    /// <summary>
    /// ������ �������� ������
    /// </summary>
    public void HideLoadingScreen()
    {
        // ������������� �������� �������� �����,
        // ����� �� ������������� �� ����� ������������.
        if (animateTextCoroutine != null)
        {
            StopCoroutine(animateTextCoroutine);
            animateTextCoroutine = null;
        }

        // ��������� �������� �������� ������������
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        fadeOutCoroutine = StartCoroutine(FadeOutLoadingScreen());
    }

    /// <summary>
    /// ������ ��������� alpha � ��������� ������
    /// </summary>
    private IEnumerator FadeOutLoadingScreen()
    {
        if (canvasGroup == null)
        {
            // ���� ��� CanvasGroup, ������ �������� ������
            loadingPanel.SetActive(false);
            yield break;
        }

        // ������ ��������� alpha
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 1f; // �������� ������������
            yield return null;
        }

        // ����� alpha=0 - ��������
        canvasGroup.alpha = 0f;
        loadingPanel.SetActive(false);
        fadeOutCoroutine = null;
    }

    /// <summary>
    /// ������� � ��������� "��������."
    /// </summary>
    private IEnumerator AnimateLoadingText()
    {
        if (loadingText == null)
            yield break;

        string baseText = "��������";
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
