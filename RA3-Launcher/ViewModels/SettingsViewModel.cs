using RA3_Launcher.Managers;
using RA3_Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RA3_Launcher.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        [JsonIgnore]
        public string LaunchOptions { get; set; } = string.Join(" ", SettingsManager.CurrentSettings.LaunchOptions) ?? string.Empty;

        [JsonIgnore]
        public string GamePath { get; set; } = SettingsManager.CurrentSettings.GamePath;

        [JsonIgnore]
        public bool CheckUpdatesForMods { get; set; } = SettingsManager.CurrentSettings.CheckUpdatesForMods;

        [JsonIgnore]
        public bool CheckUpdatesForApp { get; set; } = SettingsManager.CurrentSettings.CheckUpdatesForApp;

        public const string FourGBPatchDescription = "Увеличивает доступную память для 32-битных приложений до 4 ГБ, что может улучшить стабильность игры Red Alert 3.";
        public const string RegistryFixDescription = "Исправляет проблемы с реестром Windows, которые могут мешать работе игры Red Alert 3, также полезно если ваш Red Alert 3 не видит установленные карты.";
        public const string BattleNetDescription = "Устанавливает RA3 BattleNet, позволяя играть онлайн с другими игроками.";
        public const string CncOnlineDescription = "Устанавливает модуль подключения к CnC Online, альтернативной платформе для многопользовательской игры.";
        public const string RadminVPNDescription = "Устанавливает Radmin VPN, позволяющий создавать виртуальную локальную сеть для игры по LAN.";

        [JsonIgnore]
        public static string FourGBPatchDescriptionProperty => FourGBPatchDescription;

        [JsonIgnore]
        public static string RegistryFixDescriptionProperty => RegistryFixDescription;

        [JsonIgnore]
        public static string BattleNetDescriptionProperty => BattleNetDescription;

        [JsonIgnore]
        public static string CnCOnlineDescriptionProperty => CncOnlineDescription;

        [JsonIgnore]
        public static string RadminVPNDescriptionProperty => RadminVPNDescription;
    }
}
