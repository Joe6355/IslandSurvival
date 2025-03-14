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
    // ������ ������ ��������� ������������ (��� ������������).
    // � �������� ������� ����� ������� Singleton GameManager ��� PlayerSession.
    public static int LoggedUserId = -1;
    public static string LoggedNickname = "";

    private static MySQLLogin instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ��������� ������ ��������
        }
        else
        {
            Debug.Log("[MySQLLogin] ������ ��������� ���������. ������� ����� ������.");
            Destroy(gameObject); // ������� ��������� ������
            return;
        }
    }
    private void Start()
    {

        showUserInfo = FindObjectOfType<ShowUserInfo>();
        // ��������� ������ ����������� ���� ���.
        ConnectionString = $"Server={server};Database={database};User ID={userID};Password={passwordDB};Port={port};SslMode=none;CharSet=utf8mb4;";

        TestConnection();
        Debug.Log("[MySQLLogin] ������ ��������. ID �������: " + gameObject.GetInstanceID());
    }


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
    /// ���������� ��� ������� ������ "Register".
    /// </summary>
    public void OnRegisterButton()
    {
        // ��������� ������ �� �����
        string email = regEmailField.text;
        string nick = regNicknameField.text;
        string pass = regPasswordField.text;

        // ������� ��������
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

                // 1) ���������, ��� �� ��� ������ ���� ��� �����
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
                            // ��� ���� ����� ������������
                            errorText.text = "����� ��� ��� ����� ��� ����������������!";
                            return;
                        }
                    }
                }

                // 2) ���� ���, ��������� ������ ������������
                //    (� �������� ������� ����� ���������� ������)
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

                    // 3) ������ ������ � player_data c ���������� ����������
                    string insertDataQuery = @"
                        INSERT INTO player_data (user_id, hp, stamina, mood, pos_x, pos_y)
                        VALUES (@userId, 100, 100, 100, 0, 0);
";
                    using (MySqlCommand insertDataCmd = new MySqlCommand(insertDataQuery, conn))
                    {
                        insertDataCmd.Parameters.AddWithValue("@userId", newUserId);
                        insertDataCmd.ExecuteNonQuery();
                    }

                    // ��������� �� ������
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
    /// ���������� ��� ������� ������ "Login".
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

                                Debug.Log("[MySQLLogin] ����������� �������!");
                                errorTextLog.text = "����������� �������!";
                                StartCoroutine(DelayedSceneLoad(1, userId));
                            }
                            else
                            {
                                Debug.LogWarning("[MySQLLogin] ������: �������� ������!");
                                errorTextLog.text = "�������� ������!";
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[MySQLLogin] ������: ������������ �� ������!");
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

    //����� ���������� �� �������� ����� ���������(������ ����� ����������)
    private IEnumerator DelayedSceneLoad(int sceneIndex, int userId)
    {
        Debug.Log($"[MySQLLogin] �������� �������� ����� {sceneIndex} ��� ������������ {userId}");

        SceneManager.LoadScene(sceneIndex);
        yield return new WaitForSeconds(1.5f); // ��� ����� �����������

        Debug.Log("[MySQLLogin] ����� ���������. ���� PlayerController...");

        // ��� ��������� PlayerController
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
            Debug.Log("[MySQLLogin] PlayerController ������! ��������� ������...");
            StartCoroutine(LoadPlayerData(userId));
        }
        else
        {
            Debug.LogError("[MySQLLogin] ������: PlayerController �� ������!");
        }
    }



    private IEnumerator LoadPlayerData(int userId)
    {
        Debug.Log($"[MySQLLogin] ��������� ������ ��� user_id={userId}");

        // ���������� ����������� �����
        LoadingScreenManager.Instance.ShowLoadingScreen();

        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            try
            {
                conn.Open();
                Debug.Log("[MySQLLogin] ����������� � �� �������!");

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

                            Debug.Log($"[MySQLLogin] ������ �� ��: HP={hp}, Stamina={stamina}, Mood={mood}, PosX={posX}, PosY={posY}");

                            PlayerController player = FindObjectOfType<PlayerController>();
                            if (player != null)
                            {
                                Debug.Log("[MySQLLogin] ������� ������ � PlayerController.");
                                player.LoadPlayerData(hp, stamina, mood, posX, posY);
                            }
                            else
                            {
                                Debug.LogError("[MySQLLogin] ������: PlayerController �� ������!");
                            }
                        }
                        else
                        {
                            Debug.LogError("[MySQLLogin] ������: ������ ������ �� �������!");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError("[MySQLLogin] ������ �������� ������: " + ex.Message);
            }
        }

        // �������� ����������� �����
        LoadingScreenManager.Instance.HideLoadingScreen();

        yield return null;
    }

}
