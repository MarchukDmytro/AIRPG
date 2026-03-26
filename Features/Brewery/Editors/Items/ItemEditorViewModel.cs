using ReactiveUI;
using AIRPG.Core.ViewModels;
using System.Reactive;
using System.Collections.ObjectModel;
using AIRPG.Features.Brewery.Editors.Items;

namespace AIRPG.Features.Brewery.Editors;

public class ItemEditorViewModel : ViewModelBase, IBreweryTabViewModel
{
    public ObservableCollection<string> EntityList { get; } = new();
    public IEditorWorkSpaceViewModel Workspace { get; set; } = new ItemCreateViewModel();
    public ReactiveCommand<string, Unit> SetCurrentWorkTabCommand { get; } = 
        ReactiveCommand.Create<string>(_ => { });
}