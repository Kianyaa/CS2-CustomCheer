namespace CheerPlugin;

public partial class CheerPlugin
{
    public void OnConfigParsed(CheerConfig config)
    {
        // Apply configuration values if needed
        _cheerCooldown = config.CheerCooldown;
        _cheerLimit = config.CheerLimit;

        Config = config;
    }

    public void ReSetDetectKill()
    {
        _countDeath = 0;
        _lastHumanDie = null;
        _detectOne = true;

        _clearTimer?.Kill();
    }
}