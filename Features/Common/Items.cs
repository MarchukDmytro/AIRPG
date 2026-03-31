
using System;
using System.Collections.Generic;
using System.IO;
using AIRPG.Core.IDGenerationService;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Text.Json.Serialization;

namespace AIRPG.Features;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(WeaponData), "Weapon")]
[JsonDerivedType(typeof(ArmorData), "Armor")]
public abstract class MetaItem
{
    public Bitmap? ItemImage
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ImgPath)&& File.Exists(ImgPath))
            {
                Console.WriteLine($"Loading image from path: {ImgPath}");
                return new Bitmap(ImgPath);
            }
            else
            {
                return  new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "item_placeholder.png"));
            }
        }
    }
    public string ImgPath {get;set;}  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "item_placeholder.png");
    public string Name {get;set;} = string.Empty;
    public double Weight {get;set;} = 0;
    public double Value {get;set;} = 0;
    public string Description {get;set;}  = string.Empty;
    public string Source {get;set;}  = string.Empty;
    public virtual ItemType Type { get; }
    public int ItemID { get; set; } 

    public MetaItem(){
        ItemID = IdGenerationService.Instance.GetNextIdPlaceholder();
    }
    }       

public class WeaponData : MetaItem
{
    public List<Damage> Damage { get;set;} = new List<Damage>();
    public int Range { get;set;} = 0;
    public int MaxRange{ get;set;} = 0;
    public WeaponCategory Category { get ;set;} = WeaponCategory.Simple;
    public WeaponProperties Properties { get;set;}  = WeaponProperties.None;
    public  MasteryProperty Mastery { get;set;} = MasteryProperty.None;
    public WeaponType WType  { get;set;} = WeaponType.Melee;
    public override ItemType Type => ItemType.Weapon;
}

public class ArmorData : MetaItem
{
    public int AC { get;set; } = 0;
    public bool Disadvantage { get;set;} = false;
    public ArmorType AType { get;set;} = ArmorType.Light;
    public int Strength{ get;set;} = 0;
    public int PutOn { get;set;} = 0;
    public int TakeOff { get;set;} = 0;
    public override ItemType Type => ItemType.Armor;
}

public class Damage
{
    public int DiceAmount = 0;
    public int FlatDamage = 0;
    public Dice Die = Dice.D4;
    public DamageType Type = DamageType.Blueberry;
}
