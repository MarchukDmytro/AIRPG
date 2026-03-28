using ReactiveUI;
using AIRPG.Core.ViewModels;
using System.Reactive;
using System.Collections.ObjectModel;
using AIRPG.Features.Brewery.Editors.Items;
using System.Text.Json;
using System.IO;
using System;
using System.Linq;
using Avalonia.Media.Imaging;
using System.Collections.Generic;

namespace AIRPG.Features.Brewery.Editors;

public class ItemEditorViewModel : ViewModelBase, IBreweryTabViewModel
{
    private IEditorWorkSpaceViewModel _workspace;
    private  string itemPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items");
    private EntityVM _curentEntity;
    private MetaItem _itemState;
    public EntityVM CurentEntity
    {
        get => _curentEntity;
        set
        {
            if (_curentEntity != value)
            {
                loadItem(value);
            }
            this.RaiseAndSetIfChanged(ref _curentEntity,value);
        }
            
    }
    public ObservableCollection<EntityVM> EntityList { get; set;} = new();
    public IEditorWorkSpaceViewModel Workspace { get => _workspace; set => this.RaiseAndSetIfChanged(ref _workspace, value); }

    public ReactiveCommand<string, Unit> SetCurrentWorkTabCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateNewItemCommand { get; }
    public ReactiveCommand<EntityVM, Unit> DeleteItemCommand { get; }

    public ItemEditorViewModel()
    {
        if (!Directory.Exists(itemPath))
        {
            Directory.CreateDirectory(itemPath);
        }
        SetCurrentWorkTabCommand = ReactiveCommand.Create<string>(workTab => 
        {
            Workspace = workTab switch
            {
                "Create" => new ItemCreateViewModel(_itemState), 
                "Image" => new ItemImgViewModel(),
                "Card" => new ItemCardViewModel(),
                _ =>  new ItemCreateViewModel(_itemState)
            };
        });
        SaveCommand = ReactiveCommand.Create(() => Save());
        CreateNewItemCommand = ReactiveCommand.Create(() => CreateNewItem());
        DeleteItemCommand = ReactiveCommand.Create<EntityVM>(DeleteItem);

        LoadItems();
        if (EntityList.Count == 0)
        {
            CreateNewItem();
        }
    }

    private void CreateNewItem()
    {
        _itemState = new Weapon();
        Workspace = new ItemCreateViewModel(_itemState);
        // _curentEntity = new EntityVM(name:_itemState.Name,
        // id: _itemState.itemID,
        // imagePath: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "item_placeholder.png"));
    }
    private void Save()
    {
        HashSet<int> existingIDs = Directory.GetFiles(itemPath, "*.json", SearchOption.AllDirectories)
            .Select(file => JsonSerializer.Deserialize<MetaItem>(File.ReadAllText(file), new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }))
            .Where(item => item != null)
            .Select(item => item!.itemID)
            .ToHashSet();

        Random rng = new();
        int newID;
        do { newID = rng.Next(0, int.MaxValue); } 
        while (existingIDs.Contains(newID));
        string safeName = string.Concat(_itemState.Name
        .Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
        .Trim();
        _itemState.itemID = newID;
        string newFolder = Path.Combine(itemPath, $"{safeName}_{newID}");
        Directory.CreateDirectory(newFolder);
        _itemState.MyType = _itemState.GetType().AssemblyQualifiedName!;
        string json = JsonSerializer.Serialize(_itemState, _itemState.GetType(), new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });

        File.WriteAllText(Path.Combine(newFolder, $"{safeName}_{newID}.json"), json);
        EntityList.Add(new EntityVM(
                name: _itemState.Name,
                id: newID,
                imagePath: Path.Combine(itemPath, $"{safeName}_{newID}", $"{safeName}_{newID}")
            ));;
        CurentEntity = EntityList.Last();
    }
    private void LoadItems()
    {
        EntityList.Clear();
        var items = Directory.GetFiles(itemPath, "*.json", SearchOption.AllDirectories)
            .Select(file => JsonSerializer.Deserialize<MetaItem>(File.ReadAllText(file), new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }))
            .Where(item => item != null)
            .Select(item => item!);

        foreach (var item in items)
        {
            EntityList.Add(new EntityVM(
                name: item.Name,
                id: item.itemID,
                imagePath: Path.Combine(itemPath, $"{item.Name}_{item.itemID}", $"{item.Name}_{item.itemID}")
            ));
        }

    }
    private void DeleteItem(EntityVM item)
    {
        string itemFolder = Path.Combine(itemPath, $"{item.Name}_{item.ID}");
        if (Directory.Exists(itemFolder))
        {
            Directory.Delete(itemFolder, true);
            EntityList.Remove(item);
        }
    }
    private void loadItem(EntityVM Item)
    {
    if (File.Exists(Item.jsonPath))
    {
        string json = File.ReadAllText(Item.jsonPath);
        var meta = JsonSerializer.Deserialize<MetaItem>(json, new JsonSerializerOptions { WriteIndented = true,  IncludeFields = true });
        Type type = Type.GetType(meta!.MyType) ?? typeof(Weapon);
        _itemState = (MetaItem)JsonSerializer.Deserialize(json, type, new JsonSerializerOptions {  WriteIndented = true, IncludeFields = true })!;
        Workspace = new ItemCreateViewModel(_itemState);
    }
         else
        {
            CreateNewItem();
            _itemState.Description = "This item was not found. It may have been deleted or moved. You can edit this placeholder item and save it to create a new item.";
        }
    }
}

public class EntityVM : ViewModelBase,IDataTemplateOnly
{
    public string Name { get; set; }
    public int ID { get; }
    public string jsonPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items", $"{Name}_{ID}", $"{Name}_{ID}.json");
    private Bitmap? _image;
    public Bitmap? Image
    {
        get => _image;
        set => this.RaiseAndSetIfChanged(ref _image, value);
    }
    public EntityVM(string name, int id,string imagePath)
    {
        string[] supportedExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp" };

        string? foundImage = supportedExtensions
            .Select(ext => imagePath + ext)
            .FirstOrDefault(File.Exists);
        Name = name;
        ID = id;
        var ImagePath = foundImage ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "item_placeholder.png");
        Image = new Bitmap(ImagePath);
    }
}