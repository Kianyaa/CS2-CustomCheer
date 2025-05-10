using System.Collections.Concurrent;
using CounterStrikeSharp.API.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace CheerPlugin;



public partial class CheerPlugin : BasePlugin, IPluginConfig<CheerConfig>
{
    public override string ModuleName => "CheerPlugin";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Kianya";
    public override string ModuleDescription => "Cheer sound when a player pressed the cheer button";

    // Default Configurable settings
    private int CheerCooldown = 30; // Cooldown in seconds
    private int CheerLimit = 3; // Max cheers per cooldown period

    public CheerConfig Config { get; set; } = null!;

    public override void Load(bool hotReload)
    {
        // Register the command listener for the "cheer" command
        AddCommandListener("cheer", CommandPlayerCheer);
        RegisterEventHandler<EventPlayerConnectFull>(WhenPlayerConnected);

        // Load the configuration
        if (Config != null)
        {
            CheerCooldown = Config._cheerCooldown;
            CheerLimit = Config._cheerLimit;
        }

        else
        {
            Logger.LogWarning("Config is null use default setting");
        }

        // Load the user preferences from the database
        LoadDatabase();
    }

    public override void Unload(bool hotReload)
    {
        // Unregister the command listener for the "cheer" command
        RemoveCommandListener("cheer", CommandPlayerCheer, HookMode.Pre);
        DeregisterEventHandler<EventPlayerConnectFull>(WhenPlayerConnected);
    }
}
