using ReactiveUI;
using AIRPG.Core.ViewModels;
using System.Reactive;
using System.Collections.ObjectModel;
using System;

namespace AIRPG.Features.Brewery.Editors;

public class SpellEditorViewModel : ViewModelBase
{
    public ObservableCollection<string> EntityList { get; } = new();
    private string _workTab = "Create";

    private string _workspace = "Create";

    private string _controlPanel = "Create";

    public string WorkTab
    {
        get => _workTab;
        set => this.RaiseAndSetIfChanged(ref _workTab, value);
    }
    public string Workspace
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
    public SpellEditorViewModel()
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
                "Create" => "Create",
                "Lore" => "Lore",
                "Abstract" => "Abstract",
                "Map" => "Map",
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
