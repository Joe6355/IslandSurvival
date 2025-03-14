using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;     // ������ (GameObject) � ������ �����
    [SerializeField] private TextMeshProUGUI loadingText; // ����� "��������..."
    [Header("����� ������� � ����� ����� ���� //LoadingScreenManager.Show();    //// ... ��������� �������� ...    //LoadingScreenManager.Hide();")]
    private CanvasGroup canvasGroup;                      // ��� ������� �������� ������������


    
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

        // ���������, ��� loadingPanel ��������
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
        canvasGroup.alpha = 1; // ��������� �������� (������ ��������������)
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // �� ��������� ����� �������� ������, ���� ����� � ����������� � ������
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
            Debug.LogError("[LoadingScreenManager] ������: Instance �� ������! ��������, ������ �� � �����?");
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
    // ������ ����������, ������� �������� ��������
    // ----------------------------------------------

    /// <summary>
    /// �������� ������ � ��������� �������� �����
    /// </summary>
    public void ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);
        // ������ ����� � 1 (���� ����� ��� ���� 0)
        if (canvasGroup != null) canvasGroup.alpha = 1;

        // ��������� �������� � ��������� ������
        StartCoroutine(AnimateLoadingText());
    }

    /// <summary>
    /// ������ �������� ������ (����� CanvasGroup)
    /// </summary>
    public void HideLoadingScreen()
    {
        // ������������� �������� � �������, ����� �� �������������
        StopAllCoroutines();

        // ��������� �������� �������� ������������
        StartCoroutine(FadeOutLoadingScreen());
    }

    // ----------------------------------------------
    // ������� � ������� ��������� ������������
    // ----------------------------------------------

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
            canvasGroup.alpha -= Time.deltaTime * 1; // �������� �������� ������������
            yield return null;
        }
        canvasGroup.alpha = 0;
        loadingPanel.SetActive(false);
    }

    // ----------------------------------------------
    // ������� � ��������� "...", ����� ������� �����
    // ----------------------------------------------

    private IEnumerator AnimateLoadingText()
    {
        // ���� �� ����� loadingText, ������ �������
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
    }
}
