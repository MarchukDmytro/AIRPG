using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AIRPG.Features.Brewery.Editors;

public partial class EditorShellView : UserControl
{
    public EditorShellView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}