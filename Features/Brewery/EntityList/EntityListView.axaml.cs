using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace AIRPG.Features.Brewery.EntityList;

public partial class EntityListView : ReactiveUserControl<EntityListViewModel>
{
    public EntityListView() => InitializeComponent();
}
