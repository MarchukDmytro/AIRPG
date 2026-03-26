using ReactiveUI;
using AIRPG.Core.ViewModels;
using System.Reactive;
using System.Collections.ObjectModel;

namespace AIRPG.Features.Brewery.Editors;

public class PlaceholderViewModel : ViewModelBase, IEditorWorkSpaceViewModel
{
    public ViewModelBase Settings { get; set; }

    private class SettingsPlaceholder : ViewModelBase
    {
        public string PlaceholderText { get; } = "This is a placeholder for the actual settings ViewModel.";
    }

    public PlaceholderViewModel()
    {
        Settings = new SettingsPlaceholder(); // Placeholder for actual settings ViewModel
    }

}
public interface IBreweryTabViewModel
{
    public ObservableCollection<string> EntityList { get; }
    public IEditorWorkSpaceViewModel Workspace { get; set; }
    public ReactiveCommand<string, Unit> SetCurrentWorkTabCommand { get; }
}

public interface IEditorWorkSpaceViewModel
{
    public ViewModelBase Settings { get; set; }
}