using Avalonia.Data;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AIRPG.Features.Brewery.Editors.Items;

public interface IItem
{
    public string Name { get; set; }
    public float Weight { get; set; }
    public float Value { get; set; }   
    public string Description { get; set; }

}


public class Weapon : IItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float Weight { get; set; } = 0;
    public float Value { get; set; } = 0;

    private string _damage = string.Empty; 
    public int Range {get;set;}= 0;
    public int EfectiveRange {get;set;}= 0;
    public WeaponCategory Category {get;set;} = WeaponCategory.Simple;
    public WeaponProperty Properties = WeaponProperty.None;
    public  MasteryProperty masteryProperty = MasteryProperty.None;

    public DamageType DamageType { get; set; } = DamageType.Blueberry;
    public string Damage 
    { 
        get=>_damage; 
        set=>_damage = CheckDamageFormat(value);
    }

    private string CheckDamageFormat(string damage)
    {
        string[] damage_instances = damage.Split('+');
        foreach (string instance in damage_instances)
        {
            if(!Regex.IsMatch(instance, @"^\d+") || !Regex.IsMatch(instance, @"^\d+d\d+"))
            {
               return string.Empty;
            }
        }
        return damage;
    }
}

public class Armor : IItem
{
    public string Name { get; set; } = string.Empty;
    public float Weight { get; set; } = 0;
    public float Value { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public int AC { get; set; } = 0;
    public bool Disadvantage{ get; set; }  = false;
    public ArmorType armorType{get;set;} = ArmorType.Light;
    public int StrengthRequirement{get;set;} = 0;
    public int TimeToPutOn{get;set;} = 0;
    public int TimeToTakeOff{get;set;} = 0; 
}