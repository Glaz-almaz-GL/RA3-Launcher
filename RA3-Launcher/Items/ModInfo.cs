using System;
using System.Diagnostics;

namespace Items
{
    public class ModInfo
    {
        public string Name { get; set; } // DIVISION_1.00
        public string Version { get; set; } // 1.00
        public string ModPath { get; set; } // DIVISION_1.00.skudef
        public string NameAndVersion => $"{Name} v{Version}";

        public ModInfo(string name, string version, string modPath)
        {
            Debug.WriteLine($"Name: {name}; Version: {version}; ModPath: {modPath}");

            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentException.ThrowIfNullOrEmpty(version);
            ArgumentException.ThrowIfNullOrEmpty(modPath);
            Name = name;
            Version = version;
            ModPath = modPath;
        }
    }
}
