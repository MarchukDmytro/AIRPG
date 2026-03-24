using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;
using AIRPG.Features.Brewery.Settings;
using AIRPG.Features.Brewery.Editors;

using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive;


namespace AIRPG.Features.Brewery;

public class BreweryViewModel : ViewModelBase
{
// FIELDS
    private ViewModelBase _currentSettings = new BreweryWorldSettingsViewModel();
    private ViewModelBase _currentTab = new WorldEditorViewModel();
    public ObservableCollection<string> Models { get; } = new ObservableCollection<string>();
// PROPERTIES
    public ViewModelBase CurrentSettings
    {
        get => _currentSettings;
        set => this.RaiseAndSetIfChanged(ref _currentSettings, value);
    }
    public ViewModelBase CurrentTab
    {
        get => _currentTab;
        set{
            this.RaiseAndSetIfChanged(ref _currentTab, value);
            UpdateCurrentSettings();
        }
    }

    private readonly INavigationService _navigation;

    public ReactiveCommand<Unit, Unit> OpenMainMenuCommand { get; }
    public ReactiveCommand<string, Unit> ChangeTabCommand { get; }

    public BreweryViewModel(INavigationService navigation)
    {
        _navigation = navigation;

        OpenMainMenuCommand = ReactiveCommand.Create(() => { _navigation.ToMainMenu(); });
        ChangeTabCommand = ReactiveCommand.Create<string>(tab =>
        {
            CurrentTab = tab switch
            {
                "World" => new WorldEditorViewModel(),
                "Items" => new ItemEditorViewModel(),
                "Spells" => new SpellEditorViewModel(),
                "Classes" => new ClassEditorViewModel(),
                _ => CurrentTab
            };
        });
    }

    private void UpdateCurrentSettings()
    {
        CurrentSettings = CurrentTab switch
        {
            WorldEditorViewModel => new BreweryWorldSettingsViewModel(),
            ItemEditorViewModel => new BreweryItemSettingsViewModel(),
            SpellEditorViewModel => new BrewerySpellsSettingsViewModel(),
            ClassEditorViewModel => new BreweryClassesSettingsViewModel(),
            _ => new BreweryWorldSettingsViewModel()
        };
    }
}
