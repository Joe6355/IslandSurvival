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
    [SerializeField] private TMP_Text errorText;      // Текст для регистрации
    [SerializeField] private TMP_Text errorTextLog;   // Текст для логина

    [Header("MySQL Connection Settings")]
    [SerializeField] private string server = "185.180.231.180";
    [SerializeField] private string database = "my_game_db";
    [SerializeField] private string userID = "my_game_user";
    [SerializeField] private string passwordDB = "StrongPass123!";
    [SerializeField] private string port = "3306";

    public static string ConnectionString { get; private set; }

    // Инфо о пользователе (ID и ник), чтобы другие скрипты знали, кто залогинен
    public static int LoggedUserId = -1;
    public static string LoggedNickname = "";

    private static MySQLLogin instance;

    private void Awake()
    {
        // Реализуем Singleton для MySQLLogin
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Формируем строку подключения
        ConnectionString = $"Server={server};Database={database};User ID={userID};" +
                           $"Password={passwordDB};Port={port};SslMode=none;CharSet=utf8mb4;";

        TestConnection();
        Debug.Log("[MySQLLogin] Объект загружен. ID=" + gameObject.GetInstanceID());
    }

    /// <summary>
    /// Проверяем соединение с БД (тест)
    /// </summary>
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
    /// Кнопка "Register": создаём запись в users и player_data
    /// </summary>
    public void OnRegisterButton()
    {
        // Считываем поля
        string email = regEmailField.text;
        string nick = regNicknameField.text;
        string pass = regPasswordField.text;

        // Проверки
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

                // 1) Проверяем, нет ли пользователя с таким ником/почтой
                string checkQuery = @"
                    SELECT id FROM users
                    WHERE email = @mail OR nickname = @nick
                    LIMIT 1;
                ";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@mail", email);
                    checkCmd.Parameters.AddWithValue("@nick", nick);

                    using (MySqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // уже есть
                            errorText.text = "Такой ник или почта уже зарегистрированы!";
                            return;
                        }
                    }
                }

                // 2) Если нет, вставляем в users
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
                    long newUserId = insertCmd.LastInsertedId; // id нового пользователя

                    // 3) Создаём запись в player_data
                    //    Инициализируем hp=100, stamina=100, mood=100, pos=(0,0),
                    //    kills=0, resources_gathered=0, crafts=0, prayers=0, playtime_minutes=0
                    string insertDataQuery = @"
                        INSERT INTO player_data (
                            user_id, hp, stamina, mood, pos_x, pos_y,
                            kills, resources_gathered, crafts, prayers, playtime_minutes
                        )
                        VALUES (
                            @userId, 100, 100, 100, 0, 0,
                            0, 0, 0, 0, 0
                        );
                    ";

                    using (MySqlCommand insertDataCmd = new MySqlCommand(insertDataQuery, conn))
                    {
                        insertDataCmd.Parameters.AddWithValue("@userId", newUserId);
                        insertDataCmd.ExecuteNonQuery();
                    }

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
    /// Кнопка "Login": авторизуемся
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
                            string dbPass = reader.GetString("password");
                            string dbNick = reader.GetString("nickname");

                            // Сравниваем пароль (в реальном проекте используем хэш)
                            if (dbPass == pass)
                            {
                                LoggedUserId = userId;
                                LoggedNickname = dbNick;

                                Debug.Log("[MySQLLogin] Авторизация успешна!");
                                errorTextLog.text = "Авторизация успешна!";
                                StartCoroutine(DelayedSceneLoad(1, userId));
                            }
                            else
                            {
                                Debug.LogWarning("[MySQLLogin] Неверный пароль!");
                                errorTextLog.text = "Неверный пароль!";
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[MySQLLogin] Пользователь не найден!");
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

    /// <summary>
    /// Задержка перед загрузкой сцены (1) и вызов LoadFromDB у PlayerDataSaver
    /// </summary>
    private IEnumerator DelayedSceneLoad(int sceneIndex, int userId)
    {
        Debug.Log($"[MySQLLogin] Начинаем загрузку сцены {sceneIndex} для user_id={userId}");

        SceneManager.LoadScene(sceneIndex);
        yield return new WaitForSeconds(1.5f);

        Debug.Log("[MySQLLogin] Сцена загружена. Ищем PlayerDataSaver...");

        // Ищем PlayerDataSaver, чтобы загрузить все параметры
        PlayerDataSaver saver = FindObjectOfType<PlayerDataSaver>();
        if (saver != null)
        {
            Debug.Log("[MySQLLogin] PlayerDataSaver найден! Загружаем из БД...");
            saver.LoadFromDB(userId); // Загружаем все параметры
        }
        else
        {
            Debug.LogError("[MySQLLogin] Ошибка: PlayerDataSaver не найден!");
        }
    }
}
