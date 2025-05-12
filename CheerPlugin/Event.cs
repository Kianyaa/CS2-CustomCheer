using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using System.Collections.Concurrent;

namespace CheerPlugin;

public partial class CheerPlugin
{
    private string? _lastHumanDie = null;
    private int _countDeath = 0;
    private bool _detectOne = true;

    private static readonly Random Random = new Random();
    private Timer? _clearTimer = null;

    private readonly ConcurrentDictionary<CCSPlayerController, (int CheerCount, double LastCheerTime)> _playerCheerData = new();
    private HashSet<CCSPlayerController> _playerList = new HashSet<CCSPlayerController>();

    [GameEventHandler(HookMode.Post)]
    public HookResult WhenPlayerConnected(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;


        if (player == null || player.IsBot || player.IsHLTV ||
            player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return HookResult.Continue;
        }

        _ = GetPlayerPrefCheer(player);

        return HookResult.Continue;
    }

    // Reset player cheer data when new round start
    [GameEventHandler(HookMode.Post)]
    public HookResult OnNewRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        // Reset player cheer data
        _playerCheerData.Clear();

        // Reset the last human die and count death
        _lastHumanDie = null;
        _countDeath = 0;
        _detectOne = true;

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerHumanDie(EventPlayerDeath @event, GameEventInfo info)
    {

        var player = @event.Userid;

        if (player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        if (player.Team == CsTeam.CounterTerrorist && player.PlayerPawn.Value!.LifeState != (byte)LifeState_t.LIFE_ALIVE)
        {
            _lastHumanDie = player.PlayerName;
            _countDeath++;
        }

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    private HookResult CommandPlayerCheer(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || player is not { IsValid: true, PlayerPawn.IsValid: true } ||
            player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return HookResult.Continue;
        }

        var currentTime = Server.CurrentTime;

        if (_playerCheerData.TryGetValue(player, out var cheerData))
        {
            if (currentTime - cheerData.LastCheerTime > _cheerCooldown)
            {
                cheerData = (0, currentTime);
            }

            if (cheerData.CheerCount >= _cheerLimit)
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

        _playerCheerData[player] = cheerData;

        if (_lastHumanDie == null)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}{player.PlayerName} {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard!");
        }

        if (_countDeath > 1)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}{player.PlayerName} {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard!");


            if (_detectOne)
            {
                _detectOne = false;
                _clearTimer = new Timer(5f, ReSetDetectKill, TimerFlags.STOP_ON_MAPCHANGE);
            }
        }

        if (_countDeath == 1)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}{player.PlayerName} {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard with {ChatColors.Red}{_lastHumanDie} {ChatColors.Default}death");

            if (_detectOne)
            {
                _detectOne = false;
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


}