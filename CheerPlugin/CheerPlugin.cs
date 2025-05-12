using CounterStrikeSharp.API.Core;

namespace CheerPlugin;

public partial class CheerPlugin : BasePlugin, IPluginConfig<CheerConfig>
{
    public override string ModuleName => "CheerPlugin";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Kianya";
    public override string ModuleDescription => "Cheer sound when a player pressed the cheer button";

    // Default Configurable settings
    private int _cheerCooldown; // Cooldown in seconds
    private int _cheerLimit; // Max cheers per cooldown period

    public CheerConfig Config { get; set; } = null!;

    public override void Load(bool hotReload)
    {
        // Register the command listener for the "cheer" command
        AddCommandListener("cheer", CommandPlayerCheer);
        RegisterEventHandler<EventPlayerConnectFull>(WhenPlayerConnected);

        // Load the plugin configuration file
        if (Config != null)
        {
            _cheerCooldown = Config.CheerCooldown;
            _cheerLimit = Config.CheerLimit;
        }

        else
        {
            _cheerCooldown = 45;
            _cheerLimit = 3;
        }

        // Load the user preferences from the database
        LoadDatabase();
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("cheer", CommandPlayerCheer, HookMode.Pre);
        DeregisterEventHandler<EventPlayerConnectFull>(WhenPlayerConnected);
    }
}
