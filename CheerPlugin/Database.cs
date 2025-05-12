using Microsoft.Data.Sqlite;
using CounterStrikeSharp.API.Core;
using Dapper;

namespace CheerPlugin;

public partial class CheerPlugin
{
    public SqliteConnection? Connection { get; set; } = null;

    private async void LoadDatabase()
    {
        SQLitePCL.Batteries.Init();

        Connection = new SqliteConnection($"Data Source={Path.Join(ModuleDirectory, "cheer_pref.db")}");
        Connection.Open();

        var command = Connection.CreateCommand();
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS 'cheer_userpref' ('steam_id' VARCHAR(64) PRIMARY KEY, 'cheer_enable' INT)";
        await command.ExecuteNonQueryAsync();
    }

    private async Task GetPlayerPrefCheer(CCSPlayerController player)
    {
        if (player == null) return;

        var query = "SELECT cheer_enable FROM cheer_userpref WHERE steam_id = @steamId";

        var param = new
        {
            steamId = player.SteamID
        };

        if (Connection == null) return;

        var result = await Connection.ExecuteReaderAsync(query, param);

        if (result == null) return;

        if (await result.ReadAsync())
            if ((int)result["cheer_enable"] == 0)
            {
                _playerList.Add(player);
            }
            else
                await InsertPlayerData(player, 1);
    }

    private async Task InsertPlayerData(CCSPlayerController player, int cheermode = 1)
    {
        if (player == null) return;

        var query = "INSERT INTO cheer_userpref (steam_id, cheer_enable) VALUES (@steamId, @cheerEnable) ON CONFLICT(steam_id) DO UPDATE SET cheer_enable = EXCLUDED.cheer_enable;";
        var param = new
        {
            steamId = player.SteamID,
            cheerEnable = cheermode
        };

        if (Connection == null) return;

        await Connection.ExecuteAsync(query, param);
    }

}







