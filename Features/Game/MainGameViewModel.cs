using Avalonia.ReactiveUI;
using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;
using System;

namespace AIRPG.Features.Game;

public class MainGameViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public MainGameViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }


}
