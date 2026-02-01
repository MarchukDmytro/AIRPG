using ReactiveUI;
using System.Reactive;
using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;

namespace AIRPG.Features.Brewery;

public class BreweryViewModel : ViewModelBase
{

    public BreweryViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        //CurrentWorkZone = new CrateWorldViewMap();

        OpenMainMenuCommand = ReactiveCommand.Create(() => _navigation.ToMainMenu());
    }
    /* private ViewModelBase? _currentWorkZone;
     public ViewModelBase? CurrentWorkZone
     {
         get;
         set => this.RaiseAndSetIfChanged(ViewModelBase? _currentWorkZone, value)
     }
 */
    private readonly INavigationService _navigation;
    public ReactiveCommand<Unit, Unit> OpenMainMenuCommand { get; }
}

