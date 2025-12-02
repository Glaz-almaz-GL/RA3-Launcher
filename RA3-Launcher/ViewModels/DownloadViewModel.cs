using CommunityToolkit.Mvvm.ComponentModel;
using Huskui.Avalonia.Models;
using Items;
using Items.Mod;
using Managers;
using Managers.Github;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ViewModels
{
    public partial class DownloadViewModel : ViewModelBase
    {
        public DownloadViewModel() { }
        public DownloadViewModel(ObservableCollection<Mod> availableMods)
        {
            AvailableMods = availableMods;
        }

        public async Task LoadModsAsync()
        {
            try
            {
                var mods = await GitHubModManager.GetModsAsync();
                AvailableMods = new ObservableCollection<Mod>(mods);
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg(ex);
            }
        }

        [ObservableProperty]
        private ObservableCollection<Mod> _availableMods = [];


        //[JsonPropertyName("translations")]
        //[JsonProperty("translations")]
        //public List<TranslationItem> Translations { get; set; } = [
        //    new("Corona Mod",
        //        "3.243",
        //        "Мод Corona представляет совершенно новую четвёртую фракцию — Шэньчжоу. Как и три другие фракции в оригинальной игре, Шэньчжоу имеет собственные независимые здания, тщательно проработанные боевые единицы и протоколы командиров. Кроме того, оригинальная система боевых юнитов трёх фракций претерпит серьёзные изменения: передвижение, дальность стрельбы и скорострельность юнитов станут более разумными. Игровые спецэффекты также будут значительно улучшены, а характеристики юнитов будут сбалансированы по результатам боевого тестирования. После завершения разработки базовых юнитов для четырёх фракций, Corona создаст подфракции для каждой из них. Corona разработает большое количество моделей юнитов в разных стилях, предоставляя игрокам выбор из различных визуальных и тактических стилей. Система ведения боевых действий на море претерпит значительные изменения в условиях полуденной короны. Крупные корабли будут иметь более рациональные размеры, более высокую стоимость и более мощное вооружение, а их башни будут работать независимо друг от друга. Первоначальная система технологий схваток для каждой фракции будет расширена, и после технологического обновления 4-го уровня будет ограничено созданием одного эпического юнита. Эпический юнит будет модульным, и большинство его турелей смогут действовать независимо (т.е. атаковать разные цели по отдельности).",
        //        new DateTime(2021, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
        //        new List<string>(["RU", "EN", "ZH"]),
        //        "",
        //        true),

        //    new("Remix Mod",
        //        "0.3.7",
        //        "Remix is a Red Alert 3 mod developed by KnightVVV.Remix remakes all the 45 Top Secret Protocols,The three forces have added new units, including more powerful T4 level units.Fixed a lot of original game bug, and optimized the original unit details.Many new maps have been added, including maps with various special mechanisms.",
        //        new DateTime(2020, 6, 17, 2, 11, 0, DateTimeKind.Unspecified),
        //        new List<string>(["RU", "EN", "ZH"]),
        //        "",
        //        true),

        //    new("Epic War",
        //        "5.2",
        //        "Basis for mod was the source code RA3 Upheaval created by Bibber. This mod brings a significant change in the balance, and adds some new units. I'm very bad speak English and you can do something does not understand, I hope this does not happen. Important note: for the mod to work correctly, you must run \"model detail\" at least at high for the mod to work,it worked even if you put other options at low. However, I would recommend play with maximum graphics settings.",
        //        new DateTime(2019, 10, 28, 0, 0, 0, DateTimeKind.Unspecified),
        //        new List<string>(["RU", "EN", "ZH"]),
        //        "",
        //        true),

        //    new("Armor Rush",
        //        "3.362",
        //        "6 new sub-factions (2 for Allies, Soviet and Japan each). More realistic visual effect, weapon range and damage, the days when a tank takes forever to kill a single infantry is over. More upgrades to unlock powerful weapons. More challenging AI, including ELITE class AI who owns the-one-and-only effect and super units. Much more destructive Ultimate Weapons. Improved faction balance for PvP battles. For more features, check our videos!",
        //        new DateTime(2018, 7, 10, 0, 0, 0, DateTimeKind.Unspecified),
        //        new List<string>(["RU", "EN", "ZH"]),
        //        "",
        //        true),

        //    new("Rejuvenation",
        //        "1.50",
        //        "Greetings, every C&C fans. I've been working on this mod for over 2 years, now it is about time to release it! - Sub factions: GRF(Global Reaction Force), Legion(Russian: Легион), Fujitai(Japanese: 富士隊) for each faction.  - 20 new units of infantries, vehicles, aircrafts and vessels, special abilities.  - New models inheriting the original art styles, special effects, and voices.  - Different upgrades, mind control and mines, the classic CNC elements.  - Epic Units and uprising units.  Special thanks goes @Bibber for providing the source code of Upheaval and some other enlightening! I would appreciate it if you have fun!",
        //        new DateTime(2019, 11, 28, 0, 0, 0, DateTimeKind.Unspecified),
        //        new List<string>(["RU", "EN", "ZH"]),
        //        "",
        //        true),

        //    new("Revival",
        //        "0.0",
        //        "Welcome to Revival a brand new mod for Red alert 3 some of you may have remembered Uprising reborn, a mod that was designed to bring many new units, fun ideas and even a 4th playable faction, unfortunately the mod was cancelled due to a very high volume of negativity within the community and thus losing my enthusiasm with it BUT after time away, I have returned, fine tuned my modelling and modding skills, and now I’m ready to bring a brand new experience to the community !! Revival is a mod that turns Red Alert 3, into what I would envision as a proper expansion to the core game Revival will feature : - New units - New camera settings - Fully functioning Sky box - New structures - New VFX and SFX - New multiplayer maps - Fully edited campaign - and much more Release date is unknown, however I will continue to post updates of photo’s and videos up to the first release this way you know it’s not ….. dead Hope you are looking forward to it as much as I’m looking forward to making it - ItzTeeJaay",
        //        DateTime.MinValue,
        //        new List<string>(["RU", "EN", "ZH"]),
        //        "",
        //        false)
        //    ];
    }
}
