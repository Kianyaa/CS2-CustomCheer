using System.Collections.Concurrent;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace CheerPlugin;

public class PositionConfig
{
    public byte CheerEnable { get; set; } = 0;
}

public class Config : BasePluginConfig
{
    public Dictionary<ulong, PositionConfig> CheerPlugin { get; set; } = new();
}

public class CheerPlugin : BasePlugin
{
    public override string ModuleName => "CheerPlugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kianya";
    public override string ModuleDescription => "Cheer sound when a player pressed the cheer button";

    private HashSet<CCSPlayerController> _playerList = new HashSet<CCSPlayerController>();

    public Config Config { get; set; } = new Config();
    private const string ConfigFilePath = "../../csgo/addons/counterstrikesharp/configs/cheer_config.json";

    private static readonly Random Random = new Random();
    private readonly ConcurrentDictionary<string, (int CheerCount, double LastCheerTime)> _playerCheerData = new();
    private const int CheerCooldown = 50; // Cooldown in seconds
    private const int CheerLimit = 3; // Max cheers per cooldown period
    private string? _lastHumanDie = null;
    private int _countDeath = 0;
    private bool _detectone = true;

    private Timer? _clearTimer = null;

    public override void Load(bool hotReload)
    {
        AddCommandListener("cheer", CommandListener_RadioCommands);
        RegisterEventHandler<EventPlayerConnectFull>(WhenPlayerConnected);
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("cheer", CommandListener_RadioCommands, HookMode.Pre);
        DeregisterEventHandler<EventPlayerConnectFull>(WhenPlayerConnected);

        _clearTimer?.Kill();
    }

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }

    [GameEventHandler(HookMode.Post)]
    private HookResult CommandListener_RadioCommands(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || player is not { IsValid: true, PlayerPawn.IsValid: true } ||
            player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return HookResult.Continue;
        }

        var steamId = player.SteamID.ToString();
        var currentTime = Server.CurrentTime;

        if (_playerCheerData.TryGetValue(steamId, out var cheerData))
        {
            if (currentTime - cheerData.LastCheerTime > CheerCooldown)
            {
                cheerData = (0, currentTime);
            }

            if (cheerData.CheerCount >= CheerLimit)
            {
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.Default}You have reached the cheer limit. Wait for cooldown.");
                return HookResult.Stop;
            }

            cheerData = (cheerData.CheerCount + 1, currentTime);
        }
        else
        {
            cheerData = (1, currentTime);
        }

        _playerCheerData[steamId] = cheerData;

        if (_lastHumanDie == null)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}{player.PlayerName} {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard!");
        }

        if (_countDeath > 1)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}{player.PlayerName} {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard!");


            if (_detectone)
            {
                _detectone = false;
                _clearTimer = new Timer(5f, ReSetDetectKill, TimerFlags.STOP_ON_MAPCHANGE);
            }
        }

        if (_countDeath == 1)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}{player.PlayerName} {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard with {ChatColors.Red}{_lastHumanDie} {ChatColors.Default}death");

            if (_detectone)
            {
                _detectone = false;
                _clearTimer = new Timer(5f, ReSetDetectKill, TimerFlags.STOP_ON_MAPCHANGE);
            }
        }

        foreach (var eachPlayer in Utilities.GetPlayers())
        {
            if (eachPlayer == null || eachPlayer is not { IsValid: true, PlayerPawn.IsValid: true })
            {
                continue;
            }

            if (_playerList.Contains(eachPlayer))
            {
                continue;
            }

            var cheerNumber = Random.Next(1, 16);
            var cheerSound = $@"\sounds\enemydown\cheer\cheer_{cheerNumber}";

            eachPlayer.ExecuteClientCommand("play " + cheerSound);
        }

        return HookResult.Stop;
    }

    // Reset player cheer data when new round start
    [GameEventHandler(HookMode.Post)]
    public HookResult OnNewRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        var gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
        if (gameRulesProxy?.GameRules == null) return HookResult.Continue;

        var gameRules = gameRulesProxy.GameRules;

        if (gameRules.WarmupPeriod == true)
        {

            LoadConfigFromFile(ConfigFilePath);
            
            if (!ValidateConfig(Config))
            {
                Logger.LogError("Invalid configuration detected. Please check the configuration file.");
            }
            else
            {
                Logger.LogInformation("Cheer Config loaded successfully.");
                OnConfigParsed(Config);
            }

        }

        foreach (var key in _playerCheerData.Keys)
        {
            if (_playerCheerData.TryGetValue(key, out var cheerData))
            {
                _playerCheerData[key] = (0, cheerData.LastCheerTime);
            }
        }

        _lastHumanDie = null;
        _countDeath = 0;
        _detectone = true;

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult WhenPlayerConnected(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid == null || @event.Userid is not { IsValid: true, PawnIsAlive: true } ||
            @event.Userid.Connected != PlayerConnectedState.PlayerConnected)
        {
            return HookResult.Continue;
        }

        var steamID = @event.Userid.SteamID;

        if (Config.CheerPlugin.TryGetValue(steamID, out var positionConfig) && positionConfig.CheerEnable == 0)
        {
            var player = Utilities.GetPlayerFromSteamId(steamID);
            if (player != null)
            {
                _playerList.Add(player);
            }
        }

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerHumanDie(EventPlayerDeath @event, GameEventInfo info)
    {

        if (@event.Userid == null || !@event.Userid.IsValid)
        {
            return HookResult.Continue;
        }

        if (@event.Userid.Team == CsTeam.CounterTerrorist)
        {
            _lastHumanDie = @event.Userid.PlayerName;
            _countDeath++;
        }
        
        return HookResult.Continue;
    }

    [ConsoleCommand("css_cheer", "Disable cheer sound")]
    [CommandHelper(usage: "Expect player name in alive human-side", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void NoCheerCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        // Check if the player is valid
        if (player == null || !player.IsValid || !player.PlayerPawn.IsValid ||
            player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return;
        }


        // Disable cheer sound
        if (commandInfo.GetArg(1) == null || commandInfo.GetArg(1) == "" || commandInfo.GetArg(1) == "0")
        {

            // Check player data json is disable now turn it on
            if (Config.CheerPlugin.TryGetValue(player.SteamID, out var positionConfig) &&
                positionConfig.CheerEnable == 0)
            {
                Config.CheerPlugin[player.SteamID] = new PositionConfig
                {
                    CheerEnable = 1,
                };

                SaveConfigToFile(ConfigFilePath);

                _playerList = new HashSet<CCSPlayerController>(_playerList.Where(p => p != player));

                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Enable cheer sound");

                return;
            }


            if (_playerList.Contains(player))
            {
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}You already disable cheer sound");

                return;
            }
            
            var playerSteamID = player.SteamID;

            Config.CheerPlugin[playerSteamID] = new PositionConfig
            {
                CheerEnable = 0,
            };

            SaveConfigToFile(ConfigFilePath);

            _playerList.Add(player);

            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Disable cheer sound");

            return;
        }

        // Enable cheer sound
        if (commandInfo.GetArg(1) == "1")
        {

            if (_playerList.Contains(player))
            {
                var playerSteamID = player.SteamID;

                Config.CheerPlugin[playerSteamID] = new PositionConfig
                {
                    CheerEnable = 1,
                };

                SaveConfigToFile(ConfigFilePath);

                _playerList = new HashSet<CCSPlayerController>(_playerList.Where(p => p != player));

                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Enable cheer sound");

                return;
            }

            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}You already enable cheer sound");

        }

        else
        {
            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Invalid input | 0 = disable cheer sound, 1 = enable cheer sound");
        }
    }

    public void SaveConfigToFile(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(Config, options);
        File.WriteAllText(filePath, jsonString);
    }

    public void LoadConfigFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var jsonString = File.ReadAllText(filePath);
            Config = JsonSerializer.Deserialize<Config>(jsonString) ?? new Config();
        }
    }

    public bool ValidateConfig(Config config)
    {
        foreach (var entry in config.CheerPlugin)
        {
            var positionConfig = entry.Value;
        }

        return true;
    }

    public void  ReSetDetectKill()
    {
        _countDeath = 0;
        _lastHumanDie = null;
        _detectone = true;

        _clearTimer?.Kill();
    }
}