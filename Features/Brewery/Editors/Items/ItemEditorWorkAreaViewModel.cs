using AIRPG.Core.ViewModels;
using AIRPG.Features.Brewery.Editors.Settings;
using DynamicData.Kernel;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AIRPG.Features.Brewery.Editors.Items;

public class ItemCreateViewModel : ViewModelBase, IEditorWorkSpaceViewModel
{ 
    // PRIVATE FIELDS
    private string _name = string.Empty;
    private string _source = "PHB";
    private string _description = string.Empty;
    private ItemType? _type = null;
    private ViewModelBase _settings = new ItemCreateSettingsViewModel();
    private ObservableCollection<ViewModelBase> _secondaryProperties = new();
    // PUBLIC PROPERTIES
    public string Source
    {
        get => _source;
        set => this.RaiseAndSetIfChanged(ref _source,value);
    }
    public ObservableCollection<ViewModelBase> SecondaryProperties
    {
        get => _secondaryProperties;
        set => this.RaiseAndSetIfChanged(ref _secondaryProperties,value);
    }
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description,value);
    }
    public ItemType[] AllItemTypes => Enum.GetValues<ItemType>();
    public ItemType? Type{
        get => _type;
        set
        {
            if(_type != value) ShowSecondaryProperties(value);
            this.RaiseAndSetIfChanged(ref _type, value);
        }
    }
    public ViewModelBase Settings{
        get => _settings;
        set => this.RaiseAndSetIfChanged(ref _settings, value);
    }

    private void ShowSecondaryProperties(ItemType? itemType)
    {

        IItem? item = itemType switch
        {
            ItemType.Weapon => new Weapon(),
            ItemType.Armor => new Armor(),
            _ =>  null 
        };
        if (item == null)
        {
            Description = $"Error, {itemType} is not in ChangeCurrentItem switch statment";
            return;
        }
        SecondaryProperties.Clear();
        var properties = item.GetType().GetProperties();

        ObservableCollection<ViewModelBase> DummySecondaryProperties = new();

        foreach(PropertyInfo prop in properties)
        {   
            if (prop.Name == "Weight" || prop.Name == "Value")
            {
                continue;
            }
            else if (prop.PropertyType == typeof(int))
            {
                DummySecondaryProperties.Add(new IntPropertyVM(prop));
            }
            else if (prop.PropertyType == typeof(double))
            {
                DummySecondaryProperties.Add(new DoublePropertyVM(prop));
            }
            else if (prop.PropertyType == typeof(bool))
            {
                DummySecondaryProperties.Add(new BoolPropertyVM(prop));
            }
            else if (prop.PropertyType == typeof(Damage))
            {
                DummySecondaryProperties.Add(new DamagePropertyVM());
            }
            else if (prop.PropertyType.IsEnum && !prop.PropertyType.IsDefined(typeof(FlagsAttribute), false))
            {
                DummySecondaryProperties.Add(new EnumPropertyVM(prop));
            }
            else if (prop.PropertyType.IsEnum && prop.PropertyType.IsDefined(typeof(FlagsAttribute), false))
            {
                DummySecondaryProperties.Add(new EnumMultiplePropertiesVM(prop));
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