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
    [SerializeField] private TMP_Text errorText;      // ����� ��� �����������
    [SerializeField] private TMP_Text errorTextLog;   // ����� ��� ������

    [Header("MySQL Connection Settings")]
    [SerializeField] private string server = "185.180.231.180";
    [SerializeField] private string database = "my_game_db";
    [SerializeField] private string userID = "my_game_user";
    [SerializeField] private string passwordDB = "StrongPass123!";
    [SerializeField] private string port = "3306";

    public static string ConnectionString { get; private set; }

    // ���� � ������������ (ID � ���), ����� ������ ������� �����, ��� ���������
    public static int LoggedUserId = -1;
    public static string LoggedNickname = "";

    private static MySQLLogin instance;

    private void Awake()
    {
        // ��������� Singleton ��� MySQLLogin
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
        // ��������� ������ �����������
        ConnectionString = $"Server={server};Database={database};User ID={userID};" +
                           $"Password={passwordDB};Port={port};SslMode=none;CharSet=utf8mb4;";

        TestConnection();
        Debug.Log("[MySQLLogin] ������ ��������. ID=" + gameObject.GetInstanceID());
    }

    /// <summary>
    /// ��������� ���������� � �� (����)
    /// </summary>
    private void TestConnection()
    {
        try
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                Debug.Log("����������� � ���� ������ �������!");
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError("������ �����������: " + ex.Message);
        }
    }

    /// <summary>
    /// ������ "Register": ������ ������ � users � player_data
    /// </summary>
    public void OnRegisterButton()
    {
        // ��������� ����
        string email = regEmailField.text;
        string nick = regNicknameField.text;
        string pass = regPasswordField.text;

        // ��������
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nick) || string.IsNullOrEmpty(pass))
        {
            errorText.text = "��������� ��� ���� ��� �����������!";
            return;
        }
        if (!email.Contains("@"))
        {
            errorText.text = "������������ �����!";
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            try
            {
                conn.Open();

                // 1) ���������, ��� �� ������������ � ����� �����/������
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
                            // ��� ����
                            errorText.text = "����� ��� ��� ����� ��� ����������������!";
                            return;
                        }
                    }
                }

                // 2) ���� ���, ��������� � users
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
                    long newUserId = insertCmd.LastInsertedId; // id ������ ������������

                    // 3) ������ ������ � player_data
                    //    �������������� hp=100, stamina=100, mood=100, pos=(0,0),
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

                    errorText.text = "����������� ������ �������!";
                }
            }
            catch (MySqlException ex)
            {
                errorText.text = "������ ��� �����������: " + ex.Message;
            }
        }
    }

    /// <summary>
    /// ������ "Login": ������������
    /// </summary>
    public void OnLoginButton()
    {
        Debug.Log("[MySQLLogin] ������ ������ �����������.");

        string login = loginEmailOrNickField.text;
        string pass = loginPasswordField.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
        {
            Debug.LogWarning("[MySQLLogin] ������: �� ��� ���� ���������!");
            errorTextLog.text = "��������� ��� ����!";
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            try
            {
                conn.Open();
                Debug.Log("[MySQLLogin] ����������� � ���� �������!");

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

                            // ���������� ������ (� �������� ������� ���������� ���)
                            if (dbPass == pass)
                            {
                                LoggedUserId = userId;
                                LoggedNickname = dbNick;

                                Debug.Log("[MySQLLogin] ����������� �������!");
                                errorTextLog.text = "����������� �������!";
                                StartCoroutine(DelayedSceneLoad(1, userId));
                            }
                            else
                            {
                                Debug.LogWarning("[MySQLLogin] �������� ������!");
                                errorTextLog.text = "�������� ������!";
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[MySQLLogin] ������������ �� ������!");
                            errorTextLog.text = "������������ �� ������!";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError("[MySQLLogin] ������ �����������: " + ex.Message);
                errorTextLog.text = "������ �����������: " + ex.Message;
            }
        }
    }

    /// <summary>
    /// �������� ����� ��������� ����� (1) � ����� LoadFromDB � PlayerDataSaver
    /// </summary>
    private IEnumerator DelayedSceneLoad(int sceneIndex, int userId)
    {
        Debug.Log($"[MySQLLogin] �������� �������� ����� {sceneIndex} ��� user_id={userId}");

        SceneManager.LoadScene(sceneIndex);
        yield return new WaitForSeconds(1.5f);

        Debug.Log("[MySQLLogin] ����� ���������. ���� PlayerDataSaver...");

        // ���� PlayerDataSaver, ����� ��������� ��� ���������
        PlayerDataSaver saver = FindObjectOfType<PlayerDataSaver>();
        if (saver != null)
        {
            Debug.Log("[MySQLLogin] PlayerDataSaver ������! ��������� �� ��...");
            saver.LoadFromDB(userId); // ��������� ��� ���������
        }
        else
        {
            Debug.LogError("[MySQLLogin] ������: PlayerDataSaver �� ������!");
        }
    }
}
