using System;

namespace AIRPG.Features;

public enum Dice
{
    D4,D6,D8,D10,D12,D20,D100
}


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
    Weapon, 
    Armor
}

public readonly struct Damage
{
    public Damage(int diceNumber,Dice die, int flatDamage,DamageType type){
        DiceNumber=diceNumber;
        Die=die;
        FlatDamage=flatDamage;
        Type=type;
    }
    public int DiceNumber { get; }
    public int FlatDamage { get; }
    public Dice Die { get;}
    public DamageType Type { get;}
}

public enum WeaponType
{
    Melee,
    Ranged
}