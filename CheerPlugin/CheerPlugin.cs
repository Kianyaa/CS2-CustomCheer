using System.Collections.Concurrent;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CheerPlugin;

public class CheerPlugin : BasePlugin
{
    public override string ModuleName => "CheerPlugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kianya";
    public override string ModuleDescription => "Cheer sound when a player pressed the cheer button";

    private static readonly Random Random = new Random();
    private readonly ConcurrentDictionary<string, (int CheerCount, double LastCheerTime)> playerCheerData = new();
    private const int CheerCooldown = 45; // Cooldown in seconds
    private const int CheerLimit = 3; // Max cheers per cooldown period
    private string? _lastHumanDie = null;
    private int _countDeath = 0;
    private bool _detectone = true;

    private List<(string Sentence, double Chance)> laughSentences = new List<(string, double)>
    {
        ($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}name {ChatColors.Default}just {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so loud, the server frame spiked!",0.05),
        ($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}name {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}echoed through the map!",0.05),
        ($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}name {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard, VAC thought it was suspicious behavior!",0.05),
        ($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}name {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard with this situation!",0.05),
        ($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}name {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard!", 0.80)
    };


    public override void Load(bool hotReload)
    {
        AddCommandListener("cheer", CommandListener_RadioCommands);
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

        if (playerCheerData.TryGetValue(steamId, out var cheerData))
        {
            // Can spam or not
            //if (currentTime - cheerData.LastCheerTime < 1.0) // 1 second buffer time to avoid spam laughing
            //{
            //    return HookResult.Stop;
            //}

            // If cooldown expired, reset the cheer count
            if (currentTime - cheerData.LastCheerTime > CheerCooldown)
            {
                cheerData = (0, currentTime);
            }

            if (cheerData.CheerCount >= CheerLimit)
            {
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.Yellow}You have reached the cheer limit. Wait for cooldown.");
                return HookResult.Stop;
            }

            cheerData = (cheerData.CheerCount + 1, currentTime);
        }
        else
        {
            cheerData = (1, currentTime);
        }

        playerCheerData[steamId] = cheerData;

        string message = GetRandomLaughMessage(player.PlayerName);

        if (_lastHumanDie == null)
        {
            Server.PrintToChatAll(message);
        }

        if (_countDeath > 1)
        {

           Server.PrintToChatAll(message);

           if (_detectone) // Only firs person gonna run this
           {
               _detectone = false;

               _ = Task.Run(async () =>
               {
                   await Task.Delay(5000);

                   // Code to run after 5 seconds
                   _countDeath = 0;
                   _lastHumanDie = null;
                   _detectone = true;
               });

           }

        }

        if (_countDeath == 1)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Cheer] {ChatColors.Default}Player {ChatColors.Yellow}{player.PlayerName} {ChatColors.Default}is {ChatColors.Red}la{ChatColors.Yellow}ug{ChatColors.Green}hi{ChatColors.Purple}ng {ChatColors.Default}so hard with {ChatColors.Red}{_lastHumanDie} {ChatColors.Default}death");


            if (_detectone)
            {
                _detectone = false;

                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);

                    // Code to run after 5 seconds
                    _countDeath = 0;
                    _lastHumanDie = null;
                    _detectone = true;
                });
            }

        }

        // New Code here
        foreach (var eachPlayer in Utilities.GetPlayers())
        {
                // Play cheer sound
                var cheerNumber = Random.Next(1, 17); // random round between 1 and 16
                var cheerSound = $@"\sounds\enemydown\cheer\cheer_{cheerNumber}";

                eachPlayer.ExecuteClientCommand("play " + cheerSound);
        }

        return HookResult.Stop;
    }

    // Reset player cheer data when new round start
    [GameEventHandler(HookMode.Post)]
    public HookResult OnNewRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        foreach (var key in playerCheerData.Keys)
        {
            if (playerCheerData.TryGetValue(key, out var cheerData))
            {
                playerCheerData[key] = (0, cheerData.LastCheerTime);
            }
        }

        _lastHumanDie = null;
        _countDeath = 0;
        _detectone = true;

        return HookResult.Continue;
    }


    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerHumanDie(EventPlayerDeath @event, GameEventInfo info)
    {

        if (@event.Userid != null && @event.Userid.Team == CsTeam.CounterTerrorist)
        {
            _lastHumanDie = @event.Userid.PlayerName;
            _countDeath++;
        }
        
        return HookResult.Continue;
    }

    private string GetRandomLaughMessage(string playerName)
    {
        Random rand = new Random();
        double totalWeight = laughSentences.Sum(s => s.Chance);
        double roll = rand.NextDouble() * totalWeight;
        double cumulative = 0.0;

        foreach (var entry in laughSentences)
        {
            cumulative += entry.Chance;
            if (roll < cumulative)
            {
                return entry.Sentence.Replace("name", playerName);
            }
        }

        // fallback (shouldn't usually happen)
        return $"{playerName} is laughing uncontrollably!";
    }


}