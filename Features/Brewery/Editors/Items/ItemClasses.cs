
using System;
using System.Collections.Generic;

namespace AIRPG.Features.Brewery.Editors.Items;

public class MetaItem
{
    public string Name = string.Empty;
    public double Weight = 0;
    public double Value = 0;
    public string Description = string.Empty;
    public string Source = "PHB";
    public ItemType ItemType;
    public int itemID = 0;
    public string  MyType;

    public MetaItem()
    {
        MyType = GetType().AssemblyQualifiedName!;
    }

}

public class Weapon : MetaItem
{
    public List<Damage> Damage = new(); 
    public int Range = 0;
    public int MaxRange= 0;
    public WeaponCategory Category = WeaponCategory.Simple;
    public WeaponProperty Properties = WeaponProperty.None;
    public  MasteryProperty Mastery = MasteryProperty.None;
    public WeaponType Type  = WeaponType.Melee;


}

public class Armor : MetaItem
{
    public int AC = 0;
    public bool Disadvantage  = false;
    public ArmorType Type = ArmorType.Light;
    public int Strength = 0;
    public int PutOn = 0;
    public int TakeOff = 0; 
}