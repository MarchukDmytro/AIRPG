using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AIRPG.Features.Brewery.Editors;

public partial class ItemEditorView : UserControl
{
    public ItemEditorView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}