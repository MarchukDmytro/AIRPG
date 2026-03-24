using ReactiveUI;
using AIRPG.Core.ViewModels;
using System.Reactive;
using System.Collections.ObjectModel;
using AIRPG.Features.Brewery.Editors.World;
using System;

namespace AIRPG.Features.Brewery.Editors;

public class WorldEditorViewModel : ViewModelBase
{
    public ObservableCollection<string> EntityList { get; } = new();
    private string _workTab = "Create";

    private ViewModelBase _workspace = new WorldCreateWorkAreaViewModel();

    private string _controlPanel = "Create";

    public string WorkTab
    {
        get => _workTab;
        set => this.RaiseAndSetIfChanged(ref _workTab, value);
    }
    public ViewModelBase Workspace
    {
        get => _workspace;
        set => this.RaiseAndSetIfChanged(ref _workspace, value);
    }
    public string ControlPanel
    {
        get => _controlPanel;
        set => this.RaiseAndSetIfChanged(ref _controlPanel, value);
    }
    public ReactiveCommand<string, Unit> SetCurrentWorkTabCommand { get; }
    public WorldEditorViewModel()
    {
        LoadWorlds();
        SetCurrentWorkTabCommand = ReactiveCommand.Create<string>(workTab => 
        {
            WorkTab = workTab switch
                {
                    "Create" => "Create",
                    "Lore" => "Lore",
                    "Abstract" => "Abstract",
                    "Map" => "Map",
                    _ => WorkTab
                };
            Workspace = workTab switch
            {
                "Create" => new WorldCreateWorkAreaViewModel(),
                "Lore" => new WorldLoreWorkAreaViewModel(),
                "Abstract" => new WorldAbstractWorkAreaViewModel(),
                "Map" => new WorldMapWorkAreaViewModel(),
                _ => Workspace
            };
            ControlPanel = workTab switch
            {
                "Create" => "Create",
                "Lore" => "Lore",
                "Abstract" => "Abstract",
                "Map" => "Map",
                _ => ControlPanel
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
