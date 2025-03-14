using UnityEngine;
using TMPro;
using MySql.Data.MySqlClient;
using System.Collections;

public class ShowUserInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text userInfoText;

    [Header("—охраниение данных при выходе из игры")]
    private PlayerController playerController;
    
    private void Start()
    {
        
        playerController = FindObjectOfType<PlayerController>();

        // ¬ыводим данные из MySQLLogin (статические пол€)
        int userId = MySQLLogin.LoggedUserId;
        string nick = MySQLLogin.LoggedNickname;

        if (userId > 0)
        {
            userInfoText.text = $"{nick} -- id {userId}";
        }
        else
        {
            userInfoText.text = "ќшибка: пользователь не авторизован или данные не получены.";
        }
    }

    //ћетод отвечающий за сохраниение в игре
    private void OnApplicationQuit()
    {
        PlayerDataSaver saver = FindObjectOfType<PlayerDataSaver>();
        if (saver != null)
        {
            saver.SaveToDB(MySQLLogin.LoggedUserId);
        }
        else
        {
            Debug.LogWarning("PlayerDataSaver не найден, данные не были сохранены.");
        }
    }
}
