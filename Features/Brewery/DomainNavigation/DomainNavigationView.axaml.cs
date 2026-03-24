using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace AIRPG.Features.Brewery.DomainNavigation;

public partial class DomainNavigationView : ReactiveUserControl<DomainNavigationViewModel>
{
    public DomainNavigationView() => InitializeComponent();
}
