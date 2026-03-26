using System.Collections.Generic;
using AIRPG.Core.ViewModels;
using AIRPG.Features.Brewery.Editors.Settings;
using ReactiveUI;
using System;
using Tmds.DBus.Protocol;

namespace AIRPG.Features.Brewery.Editors.Items;

public class ItemCreateViewModel : ViewModelBase, IEditorWorkSpaceViewModel
{ 
    private string _description = string.Empty;
    private string _name = string.Empty;
    private ItemType _type = ItemType.None;
    private IItem _currentItem = new Weapon();
    public ItemType[] AllItemTypes => Enum.GetValues<ItemType>();
    public string Description{
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
    public string Name{
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    public IItem CurrentItem
    {
        get => _currentItem;
        set => this.RaiseAndSetIfChanged(ref _currentItem,value);
    }
    public ItemType Type{
        get => _type;
        set
        {
            if (_type != value){
                CurrentItem = value switch
                {
                    ItemType.Weapon => new Weapon(),
                    ItemType.Armor => new Armor(),
                    _ => CurrentItem
                };
            }
            this.RaiseAndSetIfChanged(ref _type, value);
        }
    }
    private ViewModelBase _settings = new ItemCreateSettingsViewModel();
    public ViewModelBase Settings{
        get => _settings;
        set => this.RaiseAndSetIfChanged(ref _settings, value);
    }


}