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
    private  string itemPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items");

    private ViewModelBase _workspace;
    private EntityVM? _currentEntity;
    private MetaItem _itemState;

    public EntityVM? CurrentEntity
    {
        get => _currentEntity;
        set
        {
            if (_currentEntity != value && value != null)
            {
                Save();
                LoadItem(value);
            }
            this.RaiseAndSetIfChanged(ref _currentEntity,value);
        }
            
    }
    public ObservableCollection<EntityVM> EntityList { get; set;} = new();
    public ViewModelBase Workspace { get => _workspace; set => this.RaiseAndSetIfChanged(ref _workspace, value); }
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
                "Create" => new ItemWorkAreaCreateViewModel(new WeaponData()), 
                "Image" => new ItemWorkAreaImageViewModel(),
                _ =>  new ItemWorkAreaCreateViewModel(new WeaponData())
            };
        });

        SaveCommand = ReactiveCommand.Create(() => Save());
        CreateNewItemCommand = ReactiveCommand.Create(() => CreateNewItem());
        DeleteItemCommand = ReactiveCommand.Create<EntityVM>(DeleteItem);

        LoadItems();

    }

    private void CreateNewItem()
    {
        if (CurrentEntity != null && _itemState != null) Save();

        _itemState = Activator.CreateInstance( _itemState?.GetType() ?? typeof(WeaponData)) as MetaItem ?? new WeaponData();

        Workspace = new ItemWorkAreaCreateViewModel(_itemState);

        _currentEntity = new EntityVM(_itemState);

        EntityList.Add(CurrentEntity!);
    }
    private void Save()
    {
        string json = JsonSerializer.Serialize(_itemState, new JsonSerializerOptions { WriteIndented = true});
        File.WriteAllText(Path.Combine(itemPath,$"{_itemState.ItemID}.json"), json);
        if (_currentEntity != null) _currentEntity!.Name = _itemState.Name; 
    }
    private void LoadItems()
    {
        EntityList.Clear();

        var items = Directory.GetFiles(itemPath, "*.json", SearchOption.AllDirectories)
            .Select(file => JsonSerializer.Deserialize<MetaItem>(File.ReadAllText(file), new JsonSerializerOptions {IncludeFields = true }))
            .Where(item => item != null)
            .Select(item => item!);

        foreach (var item in items)
        {
            EntityList.Add(new EntityVM(item));
        }
    }
    private void DeleteItem(EntityVM item)
    {
        string IPath = Path.Combine(itemPath, $"{item.ID}.json");
        if (File.Exists(IPath))
        {
            File.Delete(IPath);
            EntityList.Remove(item);
        }
    }
    private void LoadItem(EntityVM Item)
    {
        if (File.Exists(Item.jsonPath))
        {
            string json = File.ReadAllText(Item.jsonPath);
            _itemState = JsonSerializer.Deserialize<MetaItem>(json, new JsonSerializerOptions {WriteIndented = true})!;
            Workspace = new ItemWorkAreaCreateViewModel(_itemState);
        }
        else
        {
            _itemState = new WeaponData
            {
                ItemID = Item.ID,
                Description = _itemState.Description = "This item was not found. It may have been deleted or moved. You can edit this placeholder item and save it to create a new item."
            };
            Workspace = new ItemWorkAreaCreateViewModel(_itemState);
        }
    }
}

public class EntityVM : ViewModelBase,IDataTemplateOnly
{
    private string _name;
    private int _id;
    public string Name{
    get => _name;
    set
        {
            this.RaiseAndSetIfChanged(ref _name,value);
        }
    }
    public int ID {
    get => _id;
    set
        {
            this.RaiseAndSetIfChanged(ref _id,value);
        }
    }
    public string jsonPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items", $"{ID}.json");
    private Bitmap? _image;
    public Bitmap? Image
    {
        get => _image;
        set => this.RaiseAndSetIfChanged(ref _image, value);
    }
    public EntityVM( MetaItem ItemState)
    {
        Name = ItemState.Name;
        ID = ItemState.ItemID;
        Image = ItemState.ItemImage;
    }
}
