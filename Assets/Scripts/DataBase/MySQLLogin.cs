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

    // ������ ������ ��������� ������������ (��� ������������).
    // � �������� ������� ����� ������� Singleton GameManager ��� PlayerSession.
    public static int LoggedUserId = -1;
    public static string LoggedNickname = "";

    private void Start()
    {
        // ��������� ������ ����������� ���� ���.
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

        using (MySqlConnection conn = new MySqlConnection(connectionString))
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
                        INSERT INTO player_data (user_id)
                        VALUES (@uID);
                    ";

                    using (MySqlCommand insertDataCmd = new MySqlCommand(insertDataQuery, conn))
                    {
                        insertDataCmd.Parameters.AddWithValue("@uID", newUserId);
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
        // ��������� ������ �� ����� ��� �����������
        string login = loginEmailOrNickField.text; // ����� ���� email ��� ���
        string pass = loginPasswordField.text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
        {
            errorTextLog.text = "��������� ��� ���� ��� �����������!";
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // �������� id, ������ � nickname
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

                            if (dbPassword == pass) // � �������� ������� ����� ���������� ����
                            {
                                // ����������� �������!
                                LoggedUserId = userId;
                                LoggedNickname = dbNick;

                                errorTextLog.text = "����������� �������!";

                                // ��������� �� ����� (1) � ������ ���� ��������� � Build Settings
                                SceneManager.LoadScene(1);
                            }
                            else
                            {
                                errorTextLog.text = "�������� ������!";
                            }
                        }
                        else
                        {
                            errorTextLog.text = "������������ �� ������!";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                errorTextLog.text = "������ �����������: " + ex.Message;
            }
        }
    }
}
