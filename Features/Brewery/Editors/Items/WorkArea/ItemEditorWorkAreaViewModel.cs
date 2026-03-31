using AIRPG.Core.ViewModels;
using AIRPG.Features.Brewery.Editors.Settings;
using DynamicData.Kernel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reflection;

namespace AIRPG.Features.Brewery.Editors.Items;

public class ItemWorkAreaCreateViewModel : ViewModelBase
{ 
    // PRIVATE FIELDS
    private ItemWrapperVM _currentItem;
    public ItemWrapperVM CurrentItem { get => _currentItem; set => this.RaiseAndSetIfChanged(ref _currentItem, value); }
    // PUBLIC PROPERTIES
    public ItemType[] AllItemTypes => Enum.GetValues<ItemType>();
    public ItemType Type{
        get => _currentItem.Type;
        set
        {
            if(CurrentItem.Type != value)
            {
            CurrentItem = value switch
            {
                ItemType.Weapon => new WeaponWrapperVM(new WeaponData()),
                ItemType.Armor => new ArmorWrapperVM(new ArmorData()),
                _ => new WeaponWrapperVM(new WeaponData())
            };
            }
            this.RaisePropertyChanged();
        }
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }


    public ItemWorkAreaCreateViewModel(MetaItem itemState)
    {
        _currentItem = itemState switch
        {
            WeaponData weapon => new WeaponWrapperVM(weapon),
            ArmorData armor => new ArmorWrapperVM(armor),
            _ => new WeaponWrapperVM(new WeaponData())
        };
        SaveCommand = ReactiveCommand.Create(() => Save());
    }
    private void Save()
    {}

    
}

public class ItemWorkAreaImageViewModel : ViewModelBase
{
}

