using System;
using ReactiveUI;
using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace AIRPG.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly NavigationService _navigation;
    private readonly CompositeDisposable _disposables = new();

    private ViewModelBase _currentViewModel = NavigationService.Instance.CurrentViewModel;
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public MainWindowViewModel()
    {
        _navigation = NavigationService.Instance;

        _navigation.WhenAnyValue(x => x.CurrentViewModel)
                   .Subscribe(vm => CurrentViewModel = vm)
                   .DisposeWith(_disposables);
    }

    public void Dispose() => _disposables.Dispose();
}

