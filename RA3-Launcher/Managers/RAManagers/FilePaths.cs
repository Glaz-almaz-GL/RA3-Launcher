using System;
using System.IO;

namespace Managers.RAManagers
{
    public static class FilePaths
    {
        public const string PatchesDirPath = "Patches";
        public static readonly string FourGBPatchPath = Path.Combine(PatchesDirPath, "4GBPatch.exe");

        public const string RegistryDirPath = "Registry";
        public static readonly string DocumentsDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static readonly string ModsDirPath = Path.Combine(DocumentsDirPath, "Red Alert 3", "Mods");

        public const string ModLaunchersPath = "ModLaunchers";
        public const string DownloadFilesDirPath = "Downloads";

        public static readonly string Fix32RegistryPath = Path.Combine(RegistryDirPath, "Fix_RA3_x86.reg");
        public static readonly string Fix64RegistryPath = Path.Combine(RegistryDirPath, "Fix_RA3_x64.reg");

        public const string RA3BattleNetUrl = "https://web.file.cor-games.com/srv/ra3battlenet/launcher/";
        public const string RA3CnCUrl = "https://cnc-online.net/en/download/";
        public const string RadminVpnUrl = "https://www.radmin-vpn.com/ru/";
    }
}
