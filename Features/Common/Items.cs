
using System;
using System.Collections.Generic;
using System.IO;
using AIRPG.Core.IDGenerationService;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIRPG.Features;

public class ItemData
{
    private Bitmap? _itemImage;

    [NotMapped]
    public Bitmap? ItemImage
    {
        get
        {
            if (_itemImage != null) return _itemImage;

            if (!string.IsNullOrWhiteSpace(ImgPath) && File.Exists(ImgPath))
                _itemImage = new Bitmap(ImgPath);
            else
                _itemImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "item_placeholder.png"));

            return _itemImage;
        }
    }
    [NotMapped]
    public virtual ItemType Type { get; }
    public string ImgPath {get;set;}  = string.Empty;
    public string Name {get;set;} = string.Empty;
    public double Weight {get;set;} = 0;
    public double Value {get;set;} = 0;
    public string Description {get;set;}  = string.Empty;
    public string Source {get;set;}  = string.Empty;
    public int Id { get; set; } 
    public void ResetImageCache() => _itemImage = null;
    }       

public class WeaponData : ItemData
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

public class ArmorData : ItemData
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
