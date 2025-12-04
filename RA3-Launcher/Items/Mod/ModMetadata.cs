using System;
using System.Collections.Generic;

namespace Items.Mod
{
    public class ModMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public string? GameVersion { get; set; }
        public string? Website { get; set; }
        public string? RepositoryUrl { get; set; }
        public List<string> Screenshots { get; set; } = [];
        public long TotalDownloads { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? LatestVersion { get; set; }
        public List<string> AvailableLanguages { get; set; } = [];
        public List<ModVersionMetadata> Versions { get; set; } = [];
        public bool IsFeatured { get; set; }
        public List<ModFileInfo> MainFiles { get; set; } = [];
    }
}
