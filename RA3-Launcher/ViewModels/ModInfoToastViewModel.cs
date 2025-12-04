using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Items.Mod;
using System.Diagnostics;
using System.Linq;

namespace RA3_Launcher.ViewModels
{
    public partial class ModInfoToastViewModel : ObservableObject
    {
        [ObservableProperty]
        private ModMetadata _mod = null!;

        public string? LatestChangelog => Mod.Versions?.FirstOrDefault()?.Changelog;

        public bool HasChangelog => !string.IsNullOrWhiteSpace(LatestChangelog);

        public bool HasWebsite => !string.IsNullOrWhiteSpace(Mod.Website);

        public bool HasRepository => !string.IsNullOrWhiteSpace(Mod.RepositoryUrl);

        [RelayCommand]
        private void OpenWebsite()
        {
            OpenUri(Mod.Website);
        }

        [RelayCommand]
        private void OpenRepository()
        {
            OpenUri(Mod.RepositoryUrl);
        }

        private static void OpenUri(string? uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                });
            }
            catch { /* ignored */ }
        }
    }
}
