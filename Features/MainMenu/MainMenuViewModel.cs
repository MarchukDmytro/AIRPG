using ReactiveUI;
using System.Reactive;
using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;

namespace AIRPG.Features.MainMenu;

public class MainMenuViewModel : ViewModelBase
{

    public MainMenuViewModel(INavigationService navigation)
    {
        _navigation = navigation;

        OpenCampaignMenuCommand = ReactiveCommand.Create(() => _navigation.ToCampaignMenu());
        OpenBreweryCommand = ReactiveCommand.Create(() => _navigation.ToBrewery());
        ExitCommand = ReactiveCommand.Create(() => _navigation.Exit());
    }

    private readonly INavigationService _navigation;
    public ReactiveCommand<Unit, Unit> OpenCampaignMenuCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenBreweryCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
}
