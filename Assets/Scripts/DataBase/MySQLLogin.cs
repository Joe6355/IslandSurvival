using UnityEngine;
using TMPro;
using MySql.Data.MySqlClient;
using UnityEngine.SceneManagement;
using System.Collections;

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

    public static string ConnectionString { get; private set; }

    public ShowUserInfo showUserInfo;
    // Храним данные вошедшего пользователя (для демонстрации).
    // В реальном проекте можно сделать Singleton GameManager или PlayerSession.
    public static int LoggedUserId = -1;
    public static string LoggedNickname = "";

    private static MySQLLogin instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Оставляем объект навсегда
        }
        else
        {
            Debug.Log("[MySQLLogin] Второй экземпляр обнаружен. Удаляем новый объект.");
            Destroy(gameObject); // Удаляем повторный объект
            return;
        }
    }
    private void Start()
    {

        showUserInfo = FindObjectOfType<ShowUserInfo>();
        // Формируем строку подключения один раз.
        ConnectionString = $"Server={server};Database={database};User ID={userID};Password={passwordDB};Port={port};SslMode=none;CharSet=utf8mb4;";

        TestConnection();
        Debug.Log("[MySQLLogin] Объект загружен. ID объекта: " + gameObject.GetInstanceID());
    }


    private void TestConnection()
{
    try
    {
        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
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

        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
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
                        INSERT INTO player_data (user_id, hp, stamina, mood, pos_x, pos_y)
                        VALUES (@userId, 100, 100, 100, 0, 0);
";
                    using (MySqlCommand insertDataCmd = new MySqlCommand(insertDataQuery, conn))
                    {
                        insertDataCmd.Parameters.AddWithValue("@userId", newUserId);
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
        Debug.Log("[MySQLLogin] Нажата кнопка авторизации.");

        string login = loginEmailOrNickField.text;
        string pass = loginPasswordField.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
        {
            Debug.LogWarning("[MySQLLogin] Ошибка: Не все поля заполнены!");
            errorTextLog.text = "Заполните все поля!";
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            try
            {
                conn.Open();
                Debug.Log("[MySQLLogin] Подключение к базе успешно!");

                string query = "SELECT id, password, nickname FROM users WHERE email = @login OR nickname = @login LIMIT 1;";

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

                            if (dbPassword == pass)
                            {
                                LoggedUserId = userId;
                                LoggedNickname = dbNick;

                                Debug.Log("[MySQLLogin] Авторизация успешна!");
                                errorTextLog.text = "Авторизация успешна!";
                                StartCoroutine(DelayedSceneLoad(1, userId));
                            }
                            else
                            {
                                Debug.LogWarning("[MySQLLogin] Ошибка: Неверный пароль!");
                                errorTextLog.text = "Неверный пароль!";
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[MySQLLogin] Ошибка: Пользователь не найден!");
                            errorTextLog.text = "Пользователь не найден!";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError("[MySQLLogin] Ошибка подключения: " + ex.Message);
                errorTextLog.text = "Ошибка подключения: " + ex.Message;
            }
        }
    }

    //Метод отвечающий за задержку перед загрузкой(нужжно будет доработать)
    private IEnumerator DelayedSceneLoad(int sceneIndex, int userId)
    {
        Debug.Log($"[MySQLLogin] Начинаем загрузку сцены {sceneIndex} для пользователя {userId}");

        SceneManager.LoadScene(sceneIndex);
        yield return new WaitForSeconds(1.5f); // Даём сцене загрузиться

        Debug.Log("[MySQLLogin] Сцена загружена. Ищем PlayerController...");

        // Ждём появления PlayerController
        PlayerController player = null;
        float timeout = 5f;
        while (player == null && timeout > 0)
        {
            player = FindObjectOfType<PlayerController>();
            yield return new WaitForSeconds(0.5f);
            timeout -= 0.5f;
        }

        if (player != null)
        {
            Debug.Log("[MySQLLogin] PlayerController найден! Загружаем данные...");
            StartCoroutine(LoadPlayerData(userId));
        }
        else
        {
            Debug.LogError("[MySQLLogin] Ошибка: PlayerController НЕ найден!");
        }
    }



    private IEnumerator LoadPlayerData(int userId)
    {
        Debug.Log($"[MySQLLogin] Загружаем данные для user_id={userId}");

        // Показываем загрузочный экран
        LoadingScreenManager.Instance.ShowLoadingScreen();

        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            try
            {
                conn.Open();
                Debug.Log("[MySQLLogin] Подключение к БД успешно!");

                string query = @"
                SELECT hp, stamina, mood, pos_x, pos_y
                FROM player_data
                WHERE user_id = @userId
                LIMIT 1;
            ";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            float hp = reader.GetFloat("hp");
                            float stamina = reader.GetFloat("stamina");
                            float mood = reader.GetFloat("mood");
                            float posX = reader.GetFloat("pos_x");
                            float posY = reader.GetFloat("pos_y");

                            Debug.Log($"[MySQLLogin] Данные из БД: HP={hp}, Stamina={stamina}, Mood={mood}, PosX={posX}, PosY={posY}");

                            PlayerController player = FindObjectOfType<PlayerController>();
                            if (player != null)
                            {
                                Debug.Log("[MySQLLogin] Передаём данные в PlayerController.");
                                player.LoadPlayerData(hp, stamina, mood, posX, posY);
                            }
                            else
                            {
                                Debug.LogError("[MySQLLogin] Ошибка: PlayerController не найден!");
                            }
                        }
                        else
                        {
                            Debug.LogError("[MySQLLogin] Ошибка: Данные игрока не найдены!");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError("[MySQLLogin] Ошибка загрузки данных: " + ex.Message);
            }
        }

        // Скрываем загрузочный экран
        LoadingScreenManager.Instance.HideLoadingScreen();

        yield return null;
    }

}
