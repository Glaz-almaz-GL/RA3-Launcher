using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RA3_Launcher.Items
{
    public class SettingsItem
    {
        [JsonPropertyName("gamePath")]
        public string GamePath { get; set; } = string.Empty;

        [JsonPropertyName("launchOptions")]
        public string[] LaunchOptions { get; set; } = [];

        [JsonPropertyName("checkUpdatesForMods")]
        public bool CheckUpdatesForMods { get; set; } = true;

        [JsonPropertyName("checkUpdatesForApp")]
        public bool CheckUpdatesForApp { get; set; } = true;

        public SettingsItem(string rA3Path, string[] launchOptions, bool checkUpdatesForMods, bool checkUpdatesForApp)
        {
            GamePath = rA3Path;
            LaunchOptions = launchOptions;
            CheckUpdatesForMods = checkUpdatesForMods;
            CheckUpdatesForApp = checkUpdatesForApp;
        }

        public SettingsItem()
        {
        }
    }
}
