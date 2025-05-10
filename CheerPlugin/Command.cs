using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CheerPlugin;

public partial class CheerPlugin
{
    [ConsoleCommand("css_cheer", "Disable cheer sound")]
    [CommandHelper(usage: "Toggle cheer sound", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ToggleCheerCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        // Check if the player is valid
        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return;
        }

        // Toggle cheer sound
        if (commandInfo.ArgCount == 0)
        {
            // Remove player from the list if they are disable cheer sound
            if (_playerList.Contains(player))
            {
                _playerList = new HashSet<CCSPlayerController>(_playerList.Where(p => p != player));
                UpdateCheerMode(player, 1);

                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Enable cheer sound");

                return;
            }

            _playerList.Add(player);
            UpdateCheerMode(player, 0);
            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Disable cheer sound");

            return;
        }

        // Disable cheer sound
        if (commandInfo.GetArg(1) == "0")
        {
            if (_playerList.Contains(player))
            {
                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}You already disable cheer sound");

                return;
            }

            _playerList.Add(player);
            UpdateCheerMode(player, 0);

            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Disable cheer sound");

            return;
        }

        // Enable cheer sound
        if (commandInfo.GetArg(1) == "1")
        {

            if (!_playerList.Contains(player))
            {
                _playerList.Add(player);
                UpdateCheerMode(player, 1);

                player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Disable cheer sound");

                return;
            }

            player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}You already enable cheer sound");
            return;

        }

        player.PrintToChat($" {ChatColors.Green}[Cheer] {ChatColors.White}Invalid input | 0 = disable cheer sound, 1 = enable cheer sound");
        
    }


}