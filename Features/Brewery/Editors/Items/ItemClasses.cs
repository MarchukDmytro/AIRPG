
namespace AIRPG.Features.Brewery.Editors.Items;

public interface IItem
{
    public string Name { get; set; }
    public double Weight { get; set; }
    public double Value { get; set; }   
    public string Description { get; set; }

}

public class Weapon : IItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Damage Damage {get;set;} 
    public double Weight { get; set; } = 0;
    public double Value { get; set; } = 0;
    public int Range {get;set;}= 0;
    public int MaxRange {get;set;}= 0;
    public WeaponCategory Category {get;set;} = WeaponCategory.Simple;
    public WeaponProperty Properties{get;set;} = WeaponProperty.None;
    public  MasteryProperty Mastery{get;set;} = MasteryProperty.None;
    public WeaponType Type {get;set;} = WeaponType.Melee;


}

public class Armor : IItem
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; } = 0;
    public double Value { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public int AC { get; set; } = 0;
    public bool Disadvantage{ get; set; }  = false;
    public ArmorType Type{get;set;} = ArmorType.Light;
    public int Strength{get;set;} = 0;
    public int PutOn{get;set;} = 0;
    public int TakeOff{get;set;} = 0; 
}