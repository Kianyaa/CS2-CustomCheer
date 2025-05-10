namespace CheerPlugin;

public partial class CheerPlugin
{
    public void OnConfigParsed(CheerConfig config)
    {
        // Apply configuration values if needed
        CheerCooldown = config._cheerCooldown;
        CheerLimit = config._cheerLimit;

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