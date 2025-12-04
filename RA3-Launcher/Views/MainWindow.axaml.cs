using Huskui.Avalonia.Controls;
using Managers.AvaloniaManagers;
using Managers.Github;

namespace RA3_Launcher.Views;

public partial class MainWindow : AppWindow
{
    private readonly MainView _mainPage = new();
    private DownloadsPage? _downloadsPage;

    public MainWindow()
    {
        InitializeComponent();
        GrowlsManager.Initialize(this);
        DialogsManager.Initialize(this, this);
        GitHubModManager.Initialize();

        // Подписка на навигацию
        _mainPage.ViewModel.OpenModsRequested += NavigateToDownloads;
        _downloadsPage = new();
        _downloadsPage.ViewModel.GoToMainRequested += NavigateToMain;

        MainContent.Content = _mainPage;
    }

    public void NavigateToMain()
    {
        MainContent.Content = _mainPage;
    }

    public void NavigateToDownloads()
    {
        _downloadsPage ??= new DownloadsPage();
        MainContent.Content = _downloadsPage;
    }
}
