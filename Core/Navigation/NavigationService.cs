using ReactiveUI;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using AIRPG.Core.ViewModels;
using AIRPG.Features.MainMenu;
using AIRPG.Features.Campaigns;
using AIRPG.Features.Brewery;
using AIRPG.Features.Game;

namespace AIRPG.Core.Navigation;

public class NavigationService : ReactiveObject, INavigationService
{
    private static NavigationService? _instance;
    public static NavigationService Instance => _instance ??= new NavigationService();

    private NavigationService()
    {
        _currentViewModel = new MainMenuViewModel(this);
    }

    private ViewModelBase _currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public void ToGameMenu() => CurrentViewModel = new MainGameViewModel(this);
    public void ToMainMenu() => CurrentViewModel = new MainMenuViewModel(this);
    public void ToCampaignMenu() => CurrentViewModel = new CampaignMenuViewModel(this);
    public void ToSettings() { /* TODO */ }
    public void ToGallery() { /* TODO */ }
    public void ToBrewery() => CurrentViewModel = new BreweryViewModel(this);
    public void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) { desktop.Shutdown(); }
    }
}
