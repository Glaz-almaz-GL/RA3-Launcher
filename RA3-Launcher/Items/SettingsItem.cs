using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Items
{
    public class SettingsItem
    {
        [JsonPropertyName("gamePath")]
        [JsonProperty("gamePath")]
        public string GamePath { get; set; } = string.Empty;

        [JsonPropertyName("launchOptions")]
        [JsonProperty("launchOptions")]
        public string LaunchOptions { get; set; } = string.Empty;

        [JsonPropertyName("checkUpdatesForMods")]
        [JsonProperty("checkUpdatesForMods")]
        public bool CheckUpdatesForMods { get; set; } = true;

        [JsonPropertyName("checkUpdatesForApp")]
        [JsonProperty("checkUpdatesForApp")]
        public bool CheckUpdatesForApp { get; set; } = true;

        public SettingsItem(string gamePath, string launchOptions, bool checkUpdatesForMods, bool checkUpdatesForApp)
        {
            GamePath = gamePath;
            LaunchOptions = launchOptions;
            CheckUpdatesForMods = checkUpdatesForMods;
            CheckUpdatesForApp = checkUpdatesForApp;
        }

        public SettingsItem()
        {
        }
    }
}
