using UnityEngine;
using MySql.Data.MySqlClient;
using System.Collections;

/// <summary>
/// Скрипт на том же GameObject, где PlayerController.
/// Отвечает за сохранение/загрузку в БД.
/// </summary>
public class PlayerDataSaver : MonoBehaviour
{
    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void OnApplicationQuit()
    {
        // Финальное сохранение
        SaveToDB(MySQLLogin.LoggedUserId);
    }

    public void SaveToDB(int userId)
    {
        if (userId <= 0) return;
        if (string.IsNullOrEmpty(MySQLLogin.ConnectionString)) return;

        StartCoroutine(SavePlayerDataCoroutine(userId));
    }

    public void LoadFromDB(int userId)
    {
        if (userId <= 0) return;
        if (string.IsNullOrEmpty(MySQLLogin.ConnectionString)) return;

        StartCoroutine(LoadPlayerDataCoroutine(userId));
    }

    private IEnumerator SavePlayerDataCoroutine(int userId)
    {
        using (MySqlConnection conn = new MySqlConnection(MySQLLogin.ConnectionString))
        {
            try
            {
                conn.Open();

                // Сохраняем все поля
                string query = @"
                    UPDATE player_data
                    SET
                        hp = @hp,
                        stamina = @st,
                        mood = @md,
                        pos_x = @px,
                        pos_y = @py,
                        day_count = @dc,
                        current_hour = @ch,
                        kills = @kl,
                        resources_gathered = @rg,
                        crafts = @cr,
                        prayers = @pr,
                        playtime_minutes = @pt
                    WHERE user_id = @uid;
                ";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);

                    cmd.Parameters.AddWithValue("@hp", player.health);
                    cmd.Parameters.AddWithValue("@st", player.stamina);
                    cmd.Parameters.AddWithValue("@md", player.mood);
                    cmd.Parameters.AddWithValue("@px", player.transform.position.x);
                    cmd.Parameters.AddWithValue("@py", player.transform.position.y);
                    cmd.Parameters.AddWithValue("@dc", player.dayCount);
                    cmd.Parameters.AddWithValue("@ch", player.currentHour);

                    cmd.Parameters.AddWithValue("@kl", player.kills);
                    cmd.Parameters.AddWithValue("@rg", player.resourcesGathered);
                    cmd.Parameters.AddWithValue("@cr", player.crafts);
                    cmd.Parameters.AddWithValue("@pr", player.prayers);
                    cmd.Parameters.AddWithValue("@pt", player.playtimeMinutes);

                    int rows = cmd.ExecuteNonQuery();
                    Debug.Log($"[PlayerDataSaver] Сохранено {rows} строк. HP={player.health}, STA={player.stamina}, Mood={player.mood}, Kills={player.kills}, PT={player.playtimeMinutes}");
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError("[PlayerDataSaver] Ошибка сохранения: " + ex.Message);
            }
        }
        yield return null;
    }

    private IEnumerator LoadPlayerDataCoroutine(int userId)
    {
        using (MySqlConnection conn = new MySqlConnection(MySQLLogin.ConnectionString))
        {
            try
            {
                conn.Open();
                string query = @"
                    SELECT
                        hp, stamina, mood,
                        pos_x, pos_y,
                        day_count, current_hour,
                        kills, resources_gathered,
                        crafts, prayers,
                        playtime_minutes
                    FROM player_data
                    WHERE user_id = @uid
                    LIMIT 1;
                ";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            float hp = reader.GetFloat("hp");
                            float st = reader.GetFloat("stamina");
                            float md = reader.GetFloat("mood");
                            float px = reader.GetFloat("pos_x");
                            float py = reader.GetFloat("pos_y");
                            int dc = reader.GetInt32("day_count");
                            float ch = reader.GetFloat("current_hour");
                            int kl = reader.GetInt32("kills");
                            int rg = reader.GetInt32("resources_gathered");
                            int cr = reader.GetInt32("crafts");
                            int pr = reader.GetInt32("prayers");
                            float pt = reader.GetFloat("playtime_minutes");

                            Debug.Log($"[PlayerDataSaver] LoadFromDB => HP={hp}, STA={st}, Mood={md}, Kills={kl}, PT={pt}");

                            // Передаём данные в PlayerController
                            player.SetPlayerData(
                                hp, st, md,
                                px, py,
                                dc, ch,
                                kl, rg, cr, pr,
                                pt
                            );
                        }
                        else
                        {
                            Debug.LogWarning("[PlayerDataSaver] user_id не найден в player_data!");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError("[PlayerDataSaver] Ошибка загрузки: " + ex.Message);
            }
        }
        LoadingScreenManager.Hide();
        yield return null;
    }
}
