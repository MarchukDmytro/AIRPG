using ReactiveUI;
using AIRPG.Core.ViewModels;
using System.Reactive;
using System.Collections.ObjectModel;
using System;

namespace AIRPG.Features.Brewery.Editors;

public class SpellEditorViewModel : ViewModelBase, IBreweryTabViewModel
{
    public ObservableCollection<string> EntityList { get; } = new();
    public IEditorWorkSpaceViewModel Workspace { get; set; } = new PlaceholderViewModel();
    public ReactiveCommand<string, Unit> SetCurrentWorkTabCommand { get; } = 
        ReactiveCommand.Create<string>(_ => { });
}