using System;
using System.Collections.ObjectModel;
using AIRPG.Features;
using System.Reflection;
using AIRPG.Core.ViewModels;
using ReactiveUI;
using System.ComponentModel;
using System.Reactive;
using System.Diagnostics;


namespace AIRPG.Features.Brewery.Editors.Items;


public class IntPropertyVM : ViewModelBase
{
    public string Name {get;} = string.Empty;
    private int _property = 0;

    public int Property
    {
        get => _property;
        set => this.RaiseAndSetIfChanged(ref _property,value);
    }
        public IntPropertyVM(PropertyInfo info)
    {
        Name = info.Name;
    }
}

public class DoublePropertyVM : ViewModelBase
{
    public string Name {get;} = string.Empty;
    private double _property = 0;
    public double Property
    {
        get => _property;
        set => this.RaiseAndSetIfChanged(ref _property, value);
    }
    public DoublePropertyVM(PropertyInfo info)
    {
        Name = info.Name;
    }
}

public class BoolPropertyVM : ViewModelBase
{
    public string Name {get;} = string.Empty;
    private bool _property = false;
    public bool Property
    {
        get => _property;
        set => this.RaiseAndSetIfChanged(ref _property,value);
    }
    public BoolPropertyVM(PropertyInfo info)
    {
        Name = info.Name;
    }
}

public class EnumPropertyVM : ViewModelBase
{
    public string Name { get; }
    
    public object Property { get; set; } 
    
    public Array Options { get; } 

    public EnumPropertyVM(PropertyInfo info)
    {
        Name = info.Name;
        
        Options = Enum.GetValues(info.PropertyType);
    }
}

public class EnumMultiplePropertiesVM : ViewModelBase
{
    private readonly Type _enumType; // We save the type so we can convert it back later

    public string Name { get; }
    public ObservableCollection<PropertyOptionVM> Options { get; } = new();

    public EnumMultiplePropertiesVM(PropertyInfo info)
    {
        Name = info.Name;
        _enumType = info.PropertyType;

        // Get all values dynamically without <TEnum>
        Array optionProps = Enum.GetValues(_enumType);
        
        foreach (object prop in optionProps)
        {
            // PRO TIP: Skip the "None = 0" flag so it doesn't create a useless CheckBox
            if (Convert.ToInt64(prop) == 0) continue; 

            Options.Add(new PropertyOptionVM(prop));
        }
    }

    public object Value
    {
        get
        {
            long result = 0;
            foreach (var option in Options)
            {
                if (option.IsObtained)
                {
                    result |= Convert.ToInt64(option.Property);
                }
            }
            // Use the saved _enumType to convert the number back to the Enum
            return Enum.ToObject(_enumType, result); 
        }
        set
        {
            long input = Convert.ToInt64(value);
            foreach (var option in Options)
            {
                long flag = Convert.ToInt64(option.Property);
                // Safe check to see if the flag is inside the input
                option.IsObtained = (input & flag) == flag; 
            }
        }
    }
    
}
public class PropertyOptionVM : ViewModelBase 
    {
        private bool _isObtained = false;
        
        public object Property { get; set; } // Stores the enum value (e.g., WeaponProperty.Light)
        
        // Added this so Avalonia has a clean string to show next to the CheckBox

        public bool IsObtained
        {
            get => _isObtained;
            set => this.RaiseAndSetIfChanged(ref _isObtained, value);
        }

        public PropertyOptionVM(object property)
        {
            Property = property;
        }
    }
public class DamagePropertyVM : ViewModelBase
{
    private ObservableCollection<DamageInstance> _damageInstances = new();
    public ReactiveCommand<Unit, Unit> AddDamageInstanceCommand {get;}
    public ReactiveCommand<Unit, Unit> RemoveDamageInstanceCommand{get;}
    public ObservableCollection<DamageInstance> DamageInstances
    {
        get => _damageInstances;
        set => this.RaiseAndSetIfChanged(ref _damageInstances,value);
    }
    public DamagePropertyVM(){
        AddDamageInstanceCommand = ReactiveCommand.Create(() => 
            {
                try
                {
                    AddDamageInstance();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        RemoveDamageInstanceCommand = ReactiveCommand.Create(() => RemoveDamageInstance());
    }
    public void RemoveDamageInstance(){}
    public void AddDamageInstance()
    {Console.WriteLine("button cldwedwicked");
        Debug.WriteLine("button clicked");
        try
                {
                     DamageInstances.Add(new DamageInstance());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
    }
    
}
public class DamageInstance : ViewModelBase
    {
        private Dice _dice = Dice.D4;
        private int _diceAmount = 0;
        private int _flatDamage = 0;
        private DamageType _damageType = DamageType.Blueberry;
        public Dice[] DiceOptions => Enum.GetValues<Dice>();
        public DamageType[] DamageTypes => Enum.GetValues<DamageType>();
        public Dice Die
        {
            get => _dice;
            set => this.RaiseAndSetIfChanged(ref _dice,value);
        }
        public int DiceAmount
        {
            get => _diceAmount;
            set => this.RaiseAndSetIfChanged(ref _diceAmount,value);
        }
        public int FlatDamage
        {
            get => _flatDamage;
            set => this.RaiseAndSetIfChanged(ref _flatDamage,value);
        }
        public DamageType TypeOfDamage
        {
            get => _damageType;
            set => this.RaiseAndSetIfChanged(ref _damageType,value);
        }
    }