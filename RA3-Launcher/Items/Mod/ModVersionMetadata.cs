using System;
using System.Collections.Generic;

namespace Items.Mod
{
    public class ModVersionMetadata
    {
        public string Version { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public List<ModFileInfo> Files { get; set; } = [];
        public List<string> SupportedLanguages { get; set; } = []; // То же, что AvailableLanguages для версии
        public string Changelog { get; set; } = string.Empty;
    }
}
