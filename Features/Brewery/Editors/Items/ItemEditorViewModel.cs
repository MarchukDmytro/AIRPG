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
using AIRPG.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AIRPG.Features.Brewery.Editors;

public class ItemEditorViewModel : ViewModelBase, IBreweryTabViewModel
{
    private readonly ItemDbContext _ctx = new ItemDbContext();
    private bool _isNew = false; 
    private ViewModelBase _workspace;
    private EntityVM? _currentEntity;
    private ItemData _itemState;
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

        InitializeAsync();
    }
    private async void InitializeAsync()
    {
        var items = await Task.Run(async () =>
        {
            await _ctx.Database.EnsureCreatedAsync();
            return await _ctx.Items.ToListAsync();
        });

        foreach (var item in items)
            EntityList.Add(new EntityVM(item));

        if (EntityList.Count == 0)
            CreateNewItem();
        else
            CurrentEntity = EntityList.First();
    }
    private void CreateNewItem()
    {
        _isNew = true;
        if (CurrentEntity != null && _itemState != null) Save();

        _itemState = Activator.CreateInstance( _itemState?.GetType() ?? typeof(WeaponData)) as ItemData ?? new WeaponData();

        Workspace = new ItemWorkAreaCreateViewModel(_itemState);

        _currentEntity = new EntityVM(_itemState);

        EntityList.Add(_currentEntity);
        this.RaisePropertyChanged(nameof(CurrentEntity));
        _currentEntity = EntityList.Last();
    }
    private async void Save()
    {
        if (_itemState == null) return;

        if (await _ctx.Items.AnyAsync(x => x.Id == _itemState.Id))
            _ctx.Items.Update(_itemState);
        else
            _ctx.Items.Add(_itemState);

        await _ctx.SaveChangesAsync();

        if (_currentEntity != null)
        {
            _currentEntity.Name = _itemState.Name;
            _currentEntity.Id   = _itemState.Id;
        }
    }
    private void DeleteItem(EntityVM entity)
    {
        int entityIdx = EntityList.IndexOf(entity);
        if(EntityList.Count() == 1)
        {
            CurrentEntity = null;
            _itemState = new WeaponData();
        }
        else if(EntityList.Count() > entityIdx + 1)
        {
            CurrentEntity = EntityList[entityIdx+1];
            
        }
        else if(EntityList.Count() == entityIdx + 1)
        {
            CurrentEntity = EntityList[entityIdx-1];
            
        }
        var item = _ctx.Items.Find(entity.Id);
        
        if (item != null)
        {
            _ctx.Items.Remove(item);
            _ctx.SaveChanges();
        }
        EntityList.Remove(entity);
    }
    private void LoadItem(EntityVM entity)
    {
       var item = _ctx.Items.Find(entity.Id);
       if(item != null)
        {
            _itemState = item;
            _isNew = false;
        }
        else
        {
            _itemState = new WeaponData
            {
                Id = entity.Id,
                Description = "This item was not found. It may have been deleted or moved. You can edit this placeholder item and save it to create a new item."
            };
            _isNew = true;
        }
        Workspace = new ItemWorkAreaCreateViewModel(_itemState);
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
    public int Id {
    get => _id;
    set
        {
            this.RaiseAndSetIfChanged(ref _id,value);
        }
    }
    private Bitmap? _image;
    public Bitmap? Image
    {
        get => _image;
        set => this.RaiseAndSetIfChanged(ref _image, value);
    }
    public EntityVM( ItemData ItemState)
    {
        Name = ItemState.Name;
        Id = ItemState.Id;
        Image = ItemState.ItemImage;
    }
}
