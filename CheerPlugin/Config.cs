using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

public class CheerConfig : BasePluginConfig
{
    [JsonPropertyName("_cheerCooldown")] public int _cheerCooldown { get; set; } = 60;
    [JsonPropertyName("_cheerLimit")] public int _cheerLimit { get; set; } = 3;
}