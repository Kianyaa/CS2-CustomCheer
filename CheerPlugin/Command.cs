using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CheerPlugin;

public partial class CheerPlugin
{
    [ConsoleCommand("css_cheer", "Disable-Enable cheer sound")]
    [CommandHelper(usage: "<cheer 0-1>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ToggleCheerCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return;
        }

        string arg = commandInfo.ArgCount > 0 ? commandInfo.GetArg(1) : "";

        if (arg == "0") // Disable cheer sound
        {
            if (!_playerList.Add(player))
            {
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}You already disabled cheer sound");
                return;
            }

            _ = InsertPlayerData(player, 0);
            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Disabled cheer sound");
            return;
        }
        else if (arg == "1") // Enable cheer sound
        {
            if (_playerList.Remove(player))
            {
                _ = InsertPlayerData(player, 1);
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Enabled cheer sound");
                return;
            }

            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}You already enabled cheer sound");
            return;
        }

        // Toggle cheer with no argument
        if (arg == "")
        {
            if (_playerList.Remove(player))
            {
                _ = InsertPlayerData(player, 1);
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Enabled cheer sound");
            }
            else
            {
                _playerList.Add(player);
                _ = InsertPlayerData(player, 0);
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Disabled cheer sound");
            }
            return;
        }

        player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Invalid input | 0 = disable cheer sound, 1 = enable cheer sound");
    }

}