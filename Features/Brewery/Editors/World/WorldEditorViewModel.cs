using ReactiveUI;
using AIRPG.Core.ViewModels;
using System.Reactive;
using System.Collections.ObjectModel;
using AIRPG.Features.Brewery.Editors.World;
using System;
using AIRPG.Features.Brewery.Editors.Settings;
namespace AIRPG.Features.Brewery.Editors;

public class WorldEditorViewModel : ViewModelBase, IBreweryTabViewModel
{
    public ObservableCollection<string> EntityList { get; } = new();

    private ViewModelBase _workspace = new WorldCreateViewModel();


    public ViewModelBase Workspace
    {
        get => _workspace;
        set => this.RaiseAndSetIfChanged(ref _workspace, value);
    }
    public ReactiveCommand<string, Unit> SetCurrentWorkTabCommand { get; }
    public WorldEditorViewModel()
    {
        LoadWorlds();
        SetCurrentWorkTabCommand = ReactiveCommand.Create<string>(workTab => 
        {
            Workspace = workTab switch
            {
                "Create" => new WorldCreateViewModel(), 
                "Lore" => new WorldLoreViewModel(),
                "Abstract" => new WorldAbstractViewModel(),
                "Map" => new WorldMapViewModel(),
                _ => Workspace
            };
        });
    }

    private void LoadWorlds()
    {
        EntityList.Add("World 1");
        EntityList.Add("World 2");
        EntityList.Add("World 3");
    }

}
