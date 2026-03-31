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
using Avalonia.Media.Imaging;
using System.Security.Cryptography.X509Certificates;


namespace AIRPG.Features;

public class EnumMultiplePropertiesVM : ViewModelBase
{
    protected readonly Func<object> _get;
    protected readonly Action<object> _set;
    
    public ObservableCollection<PropertyOptionVM> Options { get;} = new();

    public EnumMultiplePropertiesVM(Func<object> get, Action<object> set, Array optionProps)
    {
        _get = get;
        _set = set;
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
    public object Property
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
public class DamagePropertyVM : ViewModelBase
{
    private Func<List<Damage>> _get;
    private Action<List<Damage>> _set;
    private ObservableCollection<DamageInstance> _damageInstances = new();
    public ReactiveCommand<Unit, Unit> AddDamageInstanceCommand {get;}
    public ObservableCollection<DamageInstance> DamageInstances
    {
        get => _damageInstances;
        set => this.RaiseAndSetIfChanged(ref _damageInstances,value);
    }
    public List<Damage> Property
    {
        get => _get();
        set
        {
            _set(value);
        }
    }
    public DamagePropertyVM( Func<List<Damage>> get, Action<List<Damage>> set)
    {
        _get = get;
        _set = set;
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

public abstract class ItemWrapperVM : ViewModelBase
{
    protected readonly MetaItem _item;
    public ItemType Type => _item.Type;
    public string Name {get => _item.Name;set {_item.Name = value;this.RaisePropertyChanged();}}
    public double Weight{get => _item.Weight;set {_item.Weight = value;this.RaisePropertyChanged();}}
    
    public double Value {get => _item.Value;set {_item.Value = value;this.RaisePropertyChanged();}}
    public string Description {get => _item.Description;set {_item.Description = value;this.RaisePropertyChanged();}}    
    public string ImgPath {get => _item.ImgPath;set {_item.ImgPath = value;this.RaisePropertyChanged();}}
    public string Source {get => _item.Source;set {_item.Source = value;this.RaisePropertyChanged();}}
    public EnumArrays Options{get;} = EnumArrays.Instance;
    public Bitmap? ItemImage {get => _item.ItemImage;}

    public ItemWrapperVM(MetaItem item){
        _item=item;   
    }
}

public class WeaponWrapperVM : ItemWrapperVM
{
    private WeaponData _weapon => (WeaponData)_item;
    public List<Damage> Damage { get => _weapon.Damage; set { _weapon.Damage = value; this.RaisePropertyChanged(); } }
    public int Range { get => _weapon.Range; set { _weapon.Range = value; this.RaisePropertyChanged(); } }
    public int MaxRange { get => _weapon.MaxRange; set { _weapon.MaxRange = value; this.RaisePropertyChanged(); } }
    public WeaponCategory Category { get => _weapon.Category; set { _weapon.Category = value; this.RaisePropertyChanged(); } }
    public WeaponProperties Properties { get => _weapon.Properties; set { _weapon.Properties = value; this.RaisePropertyChanged(); } }
    public MasteryProperty Mastery { get => _weapon.Mastery; set { _weapon.Mastery = value; this.RaisePropertyChanged(); } }
    public WeaponType WType { get => _weapon.WType; set { _weapon.WType = value; this.RaisePropertyChanged(); } }
    public EnumMultiplePropertiesVM PropertiesVM { get; }
    public DamagePropertyVM DamageProperty { get; }
    public WeaponWrapperVM(WeaponData weapon) : base(weapon)
    {
        PropertiesVM = new EnumMultiplePropertiesVM(() => _weapon.Properties, (value) => _weapon.Properties = (WeaponProperties)value, Options.WeaponProperties);
        DamageProperty = new DamagePropertyVM(() => _weapon.Damage, (value) => _weapon.Damage = value);

    }
}
public class ArmorWrapperVM : ItemWrapperVM
{
    private ArmorData _armor => (ArmorData)_item;
    public int AC {get => _armor.AC;set {_armor.AC = value;this.RaisePropertyChanged();}}
    public bool Disadvantage {get => _armor.Disadvantage;set {_armor.Disadvantage = value;this.RaisePropertyChanged();}}
    public ArmorType AType {get => _armor.AType;set {_armor.AType = value;this.RaisePropertyChanged();}}
    public int Strength{get => _armor.Strength;set {_armor.Strength = value;this.RaisePropertyChanged();}}
    public int PutOn {get => _armor.PutOn;set {_armor.PutOn = value;this.RaisePropertyChanged();}}
    public int TakeOff {get => _armor.TakeOff;set {_armor.TakeOff = value;this.RaisePropertyChanged();}}

    public ArmorWrapperVM(ArmorData armor) : base(armor)
    {
    }
}

