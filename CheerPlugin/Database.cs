using Microsoft.Data.Sqlite;
using CounterStrikeSharp.API.Core;

namespace CheerPlugin;

public partial class CheerPlugin
{
    public SqliteConnection? Connection { get; set; } = null;

    private async void LoadDatabase()
    {
        Connection = new SqliteConnection($"Data Source={Path.Join(ModuleDirectory, "cheer_pref.db")}");
        Connection.Open();

        var command = Connection.CreateCommand();
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS 'cheer_userpref' ('steam_id' VARCHAR(64) PRIMARY KEY, 'cheer_enable' INT)";
        await command.ExecuteNonQueryAsync();
    }

    private async void GetPlayerPrefCheer(CCSPlayerController player)
    {
        if (player == null) return;

        var query = "SELECT cheer_enable FROM cheer_userpref WHERE steam_id = @steamId";

        using var command = Connection!.CreateCommand();
        command.CommandText = query;
        command.Parameters.Add(new SqliteParameter("@steamId", player.SteamID));

        using var result = await command.ExecuteReaderAsync();

        if (result.HasRows)
        {
            await result.ReadAsync();

            var cheerEnable = result.GetInt32(0); // Use GetInt32 to retrieve the value
            if (cheerEnable == 0)
            {
                _playerList.Add(player);
            }
            else
            {
                InsertPlayerData(player, 1);
            }
        }
    }

    private async void InsertPlayerData(CCSPlayerController player, int cheermode = 1)
    {
        var query = "INSERT INTO cheer_userpref (steam_id, cheer_enable) VALUES (@steamId, @cheerEnable)";

        using var command = Connection!.CreateCommand();
        command.CommandText = query;
        command.Parameters.Add(new SqliteParameter("@steamId", player.SteamID));
        command.Parameters.Add(new SqliteParameter("@cheerEnable", cheermode));

        await command.ExecuteNonQueryAsync();
    }

    private async void UpdateCheerMode(CCSPlayerController player, int newCheerMode)
    {
        var query = "UPDATE cheer_userpref SET cheer_enable = @cheerEnable WHERE steam_id = @steamId";

        using var command = Connection!.CreateCommand();
        command.CommandText = query;
        command.Parameters.Add(new SqliteParameter("@steamId", player.SteamID));
        command.Parameters.Add(new SqliteParameter("@cheerEnable", newCheerMode));

        await command.ExecuteNonQueryAsync();
    }
}







