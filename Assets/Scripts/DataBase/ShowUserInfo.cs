using UnityEngine;
using TMPro;

public class ShowUserInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text userInfoText;

    private void Start()
    {
        // ������� ������ �� MySQLLogin (����������� ����)
        int userId = MySQLLogin.LoggedUserId;
        string nick = MySQLLogin.LoggedNickname;

        if (userId > 0)
        {
            userInfoText.text = $"����� ����������, {nick}!\n��� ID: {userId}";
        }
        else
        {
            userInfoText.text = "������: ������������ �� ����������� ��� ������ �� ��������.";
        }
    }
}
