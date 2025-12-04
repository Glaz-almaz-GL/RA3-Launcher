using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Items.Mod;
using Managers.AvaloniaManagers;
using Managers.Github;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ViewModels
{
    public partial class DownloadViewModel : ViewModelBase
    {
        public event Action? GoToMainRequested;

        public DownloadViewModel() { }

        [ObservableProperty]
        private ObservableCollection<ModViewModel> _availableMods = [];

        [ObservableProperty]
        private bool _isModsLoading = false;

        [ObservableProperty]
        private bool _isMapsLoading = false;

        [ObservableProperty]
        private bool _isModsSelected = true;

        [ObservableProperty]
        private bool _isMapsSelected = false;

        [ObservableProperty]
        private ObservableCollection<object> _availableMaps = []; // временно object

        [RelayCommand]
        private void SelectMods()
        {
            IsModsSelected = true;
        }

        [RelayCommand]
        private void SelectMaps()
        {
            IsMapsSelected = false;
        }

        [RelayCommand]
        private void GoToMain()
        {
            GoToMainRequested?.Invoke();
        }

        partial void OnIsModsSelectedChanged(bool value)
        {
            if (value)
            {
                IsMapsSelected = false;
                SelectMods();
            }
        }

        partial void OnIsMapsSelectedChanged(bool value)
        {
            if (value)
            {
                IsModsSelected = false;
                SelectMaps();
            }
        }

        [RelayCommand]
        public async Task LoadModsAsync()
        {
            IsModsLoading = true;
            try
            {
                List<ModMetadata> modMetadatas = await GitHubModManager.GetModsAsync();

                // Выполняем тяжёлую работу в фоне
                ObservableCollection<ModViewModel> vms = await Task.Run(() =>
                {
                    return new ObservableCollection<ModViewModel>(
                        modMetadatas.ConvertAll(m => new ModViewModel(m)));
                });

                AvailableMods = vms; // Присваиваем уже на UI-потоке (автоматически через MVVM)
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg(ex);
            }
            finally
            {
                IsModsLoading = false;
            }
        }
    }
}