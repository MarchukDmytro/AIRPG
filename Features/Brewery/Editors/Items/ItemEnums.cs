using System;
namespace AIRPG.Features.Brewery.Editors.Items;



public enum ArmorType
{
    Light,
    Medium,
    Heavy
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
public enum WeaponProperty
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
        None,
        Weapon, 
        Armor
    }