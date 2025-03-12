using UnityEngine;
using TMPro;
using MySql.Data.MySqlClient;
using UnityEngine.SceneManagement;

public class MySQLLogin : MonoBehaviour
{
    [Header("Registration Fields")]
    [SerializeField] private TMP_InputField regEmailField;
    [SerializeField] private TMP_InputField regNicknameField;
    [SerializeField] private TMP_InputField regPasswordField;

    [Header("Login Fields")]
    [SerializeField] private TMP_InputField loginEmailOrNickField;
    [SerializeField] private TMP_InputField loginPasswordField;

    [Header("Error/Status Text")]
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text errorTextLog;

    [Header("MySQL Connection Settings")]
    private string server = "185.180.231.180";
    private string database = "my_game_db";
    private string userID = "my_game_user";
    private string passwordDB = "StrongPass123!";
    private string port = "3306";

    private string connectionString;

    // Храним данные вошедшего пользователя (для демонстрации).
    // В реальном проекте можно сделать Singleton GameManager или PlayerSession.
    public static int LoggedUserId = -1;
    public static string LoggedNickname = "";

    private void Start()
    {
        // Формируем строку подключения один раз.
        connectionString = $"Server={server};Database={database};User ID={userID};Password={passwordDB};Port={port};SslMode=none;";

        TestConnection();
    }


    private void TestConnection()
{
    try
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            Debug.Log("Подключение к базе прошло успешно!");
        }
    }
    catch (MySqlException ex)
    {
        Debug.LogError("Ошибка подключения: " + ex.Message);
    }
}
    /// <summary>
    /// Вызывается при нажатии кнопки "Register".
    /// </summary>
    public void OnRegisterButton()
    {
        // Считываем данные из полей
        string email = regEmailField.text;
        string nick = regNicknameField.text;
        string pass = regPasswordField.text;

        // Простые проверки
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nick) || string.IsNullOrEmpty(pass))
        {
            errorText.text = "Заполните все поля для регистрации!";
            return;
        }
        if (!email.Contains("@"))
        {
            errorText.text = "Некорректная почта!";
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // 1) Проверяем, нет ли уже такого ника или почты
                string checkQuery = @"
                    SELECT id FROM users
                    WHERE email = @email OR nickname = @nick
                    LIMIT 1;
                ";

                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@email", email);
                    checkCmd.Parameters.AddWithValue("@nick", nick);

                    using (MySqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Уже есть такой пользователь
                            errorText.text = "Такой ник или почта уже зарегистрированы!";
                            return;
                        }
                    }
                }

                // 2) Если нет, вставляем нового пользователя
                //    (В реальном проекте нужно хэшировать пароль)
                string insertUserQuery = @"
                    INSERT INTO users (nickname, email, password)
                    VALUES (@nick, @mail, @pass);
                ";

                using (MySqlCommand insertCmd = new MySqlCommand(insertUserQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@nick", nick);
                    insertCmd.Parameters.AddWithValue("@mail", email);
                    insertCmd.Parameters.AddWithValue("@pass", pass);

                    insertCmd.ExecuteNonQuery();
                    long newUserId = insertCmd.LastInsertedId;

                    // 3) Создаём запись в player_data c начальными значениями
                    string insertDataQuery = @"
                        INSERT INTO player_data (user_id)
                        VALUES (@uID);
                    ";

                    using (MySqlCommand insertDataCmd = new MySqlCommand(insertDataQuery, conn))
                    {
                        insertDataCmd.Parameters.AddWithValue("@uID", newUserId);
                        insertDataCmd.ExecuteNonQuery();
                    }

                    // Сообщение об успехе
                    errorText.text = "Регистрация прошла успешно!";
                }
            }
            catch (MySqlException ex)
            {
                errorText.text = "Ошибка при регистрации: " + ex.Message;
            }
        }
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "Login".
    /// </summary>
    public void OnLoginButton()
    {
        // Считываем данные из полей для авторизации
        string login = loginEmailOrNickField.text; // Может быть email или ник
        string pass = loginPasswordField.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
        {
            errorTextLog.text = "Заполните все поля для авторизации!";
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Получаем id, пароль и nickname
                string query = @"
                    SELECT id, password, nickname
                    FROM users
                    WHERE email = @login OR nickname = @login
                    LIMIT 1;
                ";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32("id");
                            string dbPassword = reader.GetString("password");
                            string dbNick = reader.GetString("nickname");

                            if (dbPassword == pass) // В реальном проекте нужно сравнивать хэши
                            {
                                // Авторизация успешна!
                                LoggedUserId = userId;
                                LoggedNickname = dbNick;

                                errorTextLog.text = "Авторизация успешна!";

                                // Переходим на сцену (1) — должна быть добавлена в Build Settings
                                SceneManager.LoadScene(1);
                            }
                            else
                            {
                                errorTextLog.text = "Неверный пароль!";
                            }
                        }
                        else
                        {
                            errorTextLog.text = "Пользователь не найден!";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                errorTextLog.text = "Ошибка подключения: " + ex.Message;
            }
        }
    }
}
