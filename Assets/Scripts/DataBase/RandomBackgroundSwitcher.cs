using UnityEngine;
using UnityEngine.UI;

public class RandomBackgroundSwitcher : MonoBehaviour
{
    // ������ ������� ��������, ������� ����� �������� ����������.
    public Sprite[] backgrounds;

    // ������ �� ��������� Image ������.
    [SerializeField] private Image panelImage;

    void Awake()
    {
        // �������� ��������� Image, �����������, ��� ������ ����� �� ��� �� �������, ��� � ������.
        panelImage = GetComponent<Image>();
        if (panelImage == null)
        {
            Debug.LogError("��������� Image �� ������ �� ���� �������!");
        }
    }

    // �����, ������� ���������� ������ ��� ��� ��������� ������.
    void OnEnable()
    {
        SwitchBackground();
    }

    // ����� ��� ������ ���������� ����.
    public void SwitchBackground()
    {
        if (backgrounds == null || backgrounds.Length == 0)
        {
            Debug.LogWarning("������ ����� ������!");
            return;
        }

        int randomIndex = Random.Range(0, backgrounds.Length);
        panelImage.sprite = backgrounds[randomIndex];
    }
}
