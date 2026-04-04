using System;

namespace AIRPG.Features;

public class EnumArrays
{

     static EnumArrays? _instance;
    public static EnumArrays Instance => _instance ??= new EnumArrays();
    public Array GetDice
    {
        get => Enum.GetValues(typeof(Dice));
    }
    public Array ArmorType
    {
        get => Enum.GetValues(typeof(ArmorType));
    }
    public Array DamageType
    {
        get => Enum.GetValues(typeof(DamageType));
    }
    public Array WeaponCategory
    {
        get => Enum.GetValues(typeof(WeaponCategory));
    }
    public Array MasteryProperty
    {
        get => Enum.GetValues(typeof(MasteryProperty));
    }
    public Array WeaponProperties
    {
        get => Enum.GetValues(typeof(WeaponProperties));
    }
    public Array ItemType
    {
        get => Enum.GetValues(typeof(ItemType));
    }
    public Array WeaponType
    {
        get => Enum.GetValues(typeof(WeaponType));
    }

}

public enum Dice
{
    D4,D6,D8,D10,D12,D20,D100
}

public enum ArmorType
{
    Light,
    Medium,
    Heavy,
    Shield
}

public enum DamageType
{
    Blueberry, 
    Slashing, Piercing, Bludgeoning, 
    Slashing_Magical, Piercing_Magical, Bludgeoning_Magical,
    Acid, Cold, Fire, Thunder, Lightning, Poison,
    Forec, Radiant, Psychic, Necrotic
}

public enum WeaponCategory
{
    Simple,
    Martial
}

public enum MasteryProperty
{
    None,
    Cleave,
    Graze,
    Nick,
    Push,
    Sap,
    Slow,
    Topple,
    Vex
}

[Flags]
public enum WeaponProperties
{
    None        = 0,
    Ammunition  = 1 << 0,  // 1
    Finesse     = 1 << 1,  // 2
    Heavy       = 1 << 2,  // 4
    Light       = 1 << 3,  // 8
    Loading     = 1 << 4,  // 16
    Range       = 1 << 5,  // 32
    Reach       = 1 << 6,  // 64
    Thrown      = 1 << 7,  // 128
    TwoHanded   = 1 << 8,  // 256
    Versatile   = 1 << 9   // 512
}
public enum ItemType
{
    Common,
    Weapon, 
    Armor
}

public enum WeaponType
{
    Melee,
    Ranged
}