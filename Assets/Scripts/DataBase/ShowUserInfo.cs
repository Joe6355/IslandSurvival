using UnityEngine;
using TMPro;
using MySql.Data.MySqlClient;
using System.Collections;

public class ShowUserInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text userInfoText;

    [Header("����������� ������ ��� ������ �� ����")]
    private PlayerController playerController;
    
    private void Start()
    {
        
        playerController = FindObjectOfType<PlayerController>();

        // ������� ������ �� MySQLLogin (����������� ����)
        int userId = MySQLLogin.LoggedUserId;
        string nick = MySQLLogin.LoggedNickname;

        if (userId > 0)
        {
            userInfoText.text = $"{nick} -- id {userId}";
        }
        else
        {
            userInfoText.text = "������: ������������ �� ����������� ��� ������ �� ��������.";
        }
    }

    //����� ���������� �� ����������� � ����
    private void OnApplicationQuit()
    {
        PlayerDataSaver saver = FindObjectOfType<PlayerDataSaver>();
        if (saver != null)
        {
            saver.SaveToDB(MySQLLogin.LoggedUserId);
        }
        else
        {
            Debug.LogWarning("PlayerDataSaver �� ������, ������ �� ���� ���������.");
        }
    }
}
