using System;
using System.Collections.ObjectModel;
using AIRPG.Features;
using System.Reflection;
using AIRPG.Core.ViewModels;
using ReactiveUI;
using System.ComponentModel;
using System.Reactive;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;


namespace AIRPG.Features.Brewery.Editors.Items;

public class MetapropertyVM<T> : ViewModelBase
{
    public string Name {get;} = string.Empty;
    protected readonly Func<T> _get;
    protected readonly Action<T> _set;
    public virtual T Property
    {
        get => _get();
        set
        {
            _set(value);
            this.RaisePropertyChanged();
        }
    }
    public MetapropertyVM(string name,Func<T> get,Action<T> set)
    {
        _get = get;
        _set = set;
        Name=name;
    }
}
public class IntPropertyVM : MetapropertyVM<int>
{
        public IntPropertyVM(string name, Func<int> get, Action<int> set)
        : base(name, get, set){}
}
public class DoublePropertyVM : MetapropertyVM<double>
{
    public DoublePropertyVM(string name, Func<double> get, Action<double> set)
        : base(name, get, set){}
}
public class BoolPropertyVM : MetapropertyVM<bool>
{
    public BoolPropertyVM(string name, Func<bool> get, Action<bool> set)
        : base(name,get, set){}
}
public class EnumPropertyVM : MetapropertyVM<object>
{
    public Array Options { get; } 
    public EnumPropertyVM(string name, Func<object> get, Action<object> set, Type PropertyType)
        : base(name, get, set)
    {        
        Options = Enum.GetValues(PropertyType);
    }
}
public class EnumMultiplePropertiesVM :  MetapropertyVM<object>
{
    private readonly Type _propertyType; // We save the type so we can convert it back later

    public ObservableCollection<PropertyOptionVM> Options { get;} = new();

    public EnumMultiplePropertiesVM(string name, Func<object> get, Action<object> set, Type PropertyType)
        : base(name, get, set){
        _propertyType = PropertyType;
        // Get all values dynamically without <TEnum>
        Array optionProps = Enum.GetValues(_propertyType);
        
        long input = Convert.ToInt32(Property);
        foreach (object prop in optionProps)
        {
            // PRO TIP: Skip the "None = 0" flag so it doesn't create a useless CheckBox
            long flag = Convert.ToInt32(prop);
            if (flag == 0) continue; 

            PropertyOptionVM NewFlag  = new PropertyOptionVM(prop,get,set);
            NewFlag.IsObtained = (input & flag) == flag;
            Options.Add(NewFlag);
        }
    }
    public override object Property
    {
        get => _get();
        set
        {
            long input = Convert.ToInt32(value);
            foreach (var option in Options)
            {
                long flag = Convert.ToInt32(option.Property);
                // Safe check to see if the flag is inside the input
                option.IsObtained = (input & flag) == flag; 
            }
            _set(value);
            this.RaisePropertyChanged();

        }
    }

    
}
public class DamagePropertyVM : MetapropertyVM<List<Damage>>
{
    private ObservableCollection<DamageInstance> _damageInstances = new();
    public ReactiveCommand<Unit, Unit> AddDamageInstanceCommand {get;}
    public ObservableCollection<DamageInstance> DamageInstances
    {
        get => _damageInstances;
        set => this.RaiseAndSetIfChanged(ref _damageInstances,value);
    }
    public DamagePropertyVM(string name, Func<List<Damage>> get, Action<List<Damage>> set)
        : base(name, get, set){
        AddDamageInstanceCommand = ReactiveCommand.Create(() => AddDamageInstance());
    }
    protected void Suicide(DamageInstance instance, Damage Element)
    {
        DamageInstances.Remove(instance);
        Property.Remove(Element);
    }
    public void AddDamageInstance(){
        var newDamage = new Damage();
        Property.Add(newDamage);
        DamageInstances.Add(new DamageInstance(newDamage,Suicide));
    }
    
    
}
public class PropertyOptionVM : ViewModelBase 
    {
        private Func<object> _get; 
        private Action<object> _set;
        private bool _isObtained = false;
        
        public object Property { get;} 
        
        public bool IsObtained
        {
            get => _isObtained;
            set {
                if (value != _isObtained)
                {
                    int curentValue =  Convert.ToInt32(_get());
                    int intproperty = Convert.ToInt32(Property);
                    if (value)
                    {
                        _set(curentValue |= intproperty);
                    }
                    else
                    {
                        _set(curentValue &= ~intproperty);
                    }
                }
                this.RaiseAndSetIfChanged(ref _isObtained, value);
            }
        }

        public PropertyOptionVM(object property,Func<object> get, Action<object> set)
        {
            _get = get;
            _set = set;
            Property = property;
        }
    }
public class DamageInstance : ViewModelBase
    {
        private Damage damage;
        public Dice[] DiceOptions => Enum.GetValues<Dice>();
        public DamageType[] DamageTypes => Enum.GetValues<DamageType>();
        public Dice Die
        {
            get => damage.Die;
            set => this.RaiseAndSetIfChanged(ref  damage.Die, value);
        }
        public int DiceAmount
        {
            get => damage.DiceAmount;
            set => this.RaiseAndSetIfChanged(ref  damage.DiceAmount, value);
        }
        public int FlatDamage
        {
            get => damage.FlatDamage;
            set => this.RaiseAndSetIfChanged(ref  damage.FlatDamage, value);

        }
        public DamageType TypeOfDamage{
            get => damage.Type;
            set => this.RaiseAndSetIfChanged(ref  damage.Type, value);

        }
        public ReactiveCommand<Unit, Unit> SuicideCommand {get;}

        public DamageInstance(Damage _damage, Action<DamageInstance,Damage> suicide)
        {
            SuicideCommand = ReactiveCommand.Create(() => suicide(this,_damage));
            damage = _damage;
        }
    
    }