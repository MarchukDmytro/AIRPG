
using ReactiveUI;
using System.Reactive;
using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;

namespace AIRPG.Features.Campaigns;

public class CampaignMenuViewModel : ViewModelBase
{

    public CampaignMenuViewModel(INavigationService navigation)
    {
        _navigation = navigation;

        OpenMainMenuCommand = ReactiveCommand.Create(() => _navigation.ToMainMenu());
        OpenGameViewCommand = ReactiveCommand.Create(() => _navigation.ToGameMenu());
    }

    private readonly INavigationService _navigation;

    //Commands
    public ReactiveCommand<Unit, Unit> OpenMainMenuCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenGameViewCommand { get; }
}

