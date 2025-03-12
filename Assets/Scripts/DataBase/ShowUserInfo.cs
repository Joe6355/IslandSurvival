using UnityEngine;
using TMPro;

public class ShowUserInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text userInfoText;

    private void Start()
    {
        // Выводим данные из MySQLLogin (статические поля)
        int userId = MySQLLogin.LoggedUserId;
        string nick = MySQLLogin.LoggedNickname;

        if (userId > 0)
        {
            userInfoText.text = $"Добро пожаловать, {nick}!\nВаш ID: {userId}";
        }
        else
        {
            userInfoText.text = "Ошибка: пользователь не авторизован или данные не получены.";
        }
    }
}
