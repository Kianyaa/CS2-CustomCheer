using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace CheerPlugin;

public class CheerConfig : BasePluginConfig
{
    [JsonPropertyName("_cheerCooldown")] public int CheerCooldown { get; set; } = 60;
    [JsonPropertyName("_cheerLimit")] public int CheerLimit { get; set; } = 3;
}