using AIRPG.Core.ViewModels;
using AIRPG.Features.Brewery.Editors.Settings;
using DynamicData.Kernel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AIRPG.Features.Brewery.Editors.Items;

public class ItemCreateViewModel : ViewModelBase, IEditorWorkSpaceViewModel
{ 
    // PRIVATE FIELDS
    private MetaItem _curentItem;
    private ViewModelBase _settings = new ItemCreateSettingsViewModel();
    private ObservableCollection<ViewModelBase> _secondaryProperties = new();
    // PUBLIC PROPERTIES
    public string Source
    {
        get => _curentItem.Source;
        set => this.RaiseAndSetIfChanged(ref _curentItem.Source, value);
    }
    public string Name
    {
        get => _curentItem.Name;
        set => this.RaiseAndSetIfChanged(ref _curentItem.Name, value);
    }
        public double Value
    {
        get => _curentItem.Value;
        set => this.RaiseAndSetIfChanged(ref _curentItem.Value, value);
    }
        public double Weight
    {
        get => _curentItem.Weight;
        set => this.RaiseAndSetIfChanged(ref _curentItem.Weight, value);
    }
    public string Description
    {
        get => _curentItem.Description;
        set => this.RaiseAndSetIfChanged(ref _curentItem.Description, value);
    }
    public ObservableCollection<ViewModelBase> SecondaryProperties
    {
        get => _secondaryProperties;
        set => this.RaiseAndSetIfChanged(ref _secondaryProperties,value);
    }
    public ItemType[] AllItemTypes => Enum.GetValues<ItemType>();
    public ItemType Type{
        get => _curentItem.ItemType;
        set
        {
            if(_curentItem.ItemType != value)
            {
            _curentItem = value switch
            {
                ItemType.Weapon => new Weapon(),
                ItemType.Armor => new Armor(),
                _ =>   new MetaItem()
            };
            ShowSecondaryProperties();
            }
            this.RaiseAndSetIfChanged(ref _curentItem.ItemType, value);
            _curentItem.Value = 0;
            _curentItem.Weight = 0;
        }
    }
    public ViewModelBase Settings{
        get => _settings;
        set => this.RaiseAndSetIfChanged(ref _settings, value);
    }

    public ItemCreateViewModel(MetaItem itemState)
    {
        _curentItem = itemState;
        ShowSecondaryProperties();
    }
    private void ShowSecondaryProperties()
    {
        SecondaryProperties.Clear();
        var properties = _curentItem.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        ObservableCollection<ViewModelBase> DummySecondaryProperties = new();

        foreach(FieldInfo prop in properties)
        {   
            var LocalProp = prop;
            if (LocalProp.FieldType == typeof(int))
            {
                DummySecondaryProperties.Add(new IntPropertyVM(LocalProp.Name,
                                                                () => (int)LocalProp.GetValue(_curentItem)!,
                                                                v =>  LocalProp.SetValue(_curentItem,v)));
            }
            else if (LocalProp.FieldType == typeof(double))
            {
                DummySecondaryProperties.Add(new DoublePropertyVM(LocalProp.Name,
                                                                () => (double)LocalProp.GetValue(_curentItem)!,
                                                                v =>  LocalProp.SetValue(_curentItem,v)));
            }
            else if (LocalProp.FieldType == typeof(bool))
            {
                DummySecondaryProperties.Add(new BoolPropertyVM(LocalProp.Name,
                                                                () => (bool)LocalProp.GetValue(_curentItem)!,
                                                                v =>  LocalProp.SetValue(_curentItem,v)));
            }
            else if (LocalProp.FieldType == typeof(List<Damage>))
            {
                DummySecondaryProperties.Add(new DamagePropertyVM(LocalProp.Name,
                                                                () => (List<Damage>)LocalProp.GetValue(_curentItem)!,
                                                                v =>  LocalProp.SetValue(_curentItem,v)));
            }
            else if (LocalProp.FieldType.IsEnum && !LocalProp.FieldType.IsDefined(typeof(FlagsAttribute), false))
            {
                DummySecondaryProperties.Add(new EnumPropertyVM(LocalProp.Name,
                                                                () => LocalProp.GetValue(_curentItem)!,
                                                                v =>  LocalProp.SetValue(_curentItem,v),
                                                                LocalProp.FieldType
                                                                ));
            }
            else if (LocalProp.FieldType.IsEnum && LocalProp.FieldType.IsDefined(typeof(FlagsAttribute), false))
            {
                DummySecondaryProperties.Add(new EnumMultiplePropertiesVM(LocalProp.Name,
                                                                () => LocalProp.GetValue(_curentItem)!,
                                                                v =>  LocalProp.SetValue(_curentItem,v),
                                                                LocalProp.FieldType
                                                                ));
            }

        }
        var sorted = DummySecondaryProperties
            .OrderBy(p => p.GetType().Name)
            .Reverse()
            .ToList();
        foreach (var i in sorted)
            SecondaryProperties.Add(i);
    }

}

public class ItemImgViewModel : ViewModelBase, IEditorWorkSpaceViewModel
{
    public ViewModelBase Settings { get; set; }
}

public class ItemCardViewModel : ViewModelBase, IEditorWorkSpaceViewModel
{
    public ViewModelBase Settings { get; set; }
}