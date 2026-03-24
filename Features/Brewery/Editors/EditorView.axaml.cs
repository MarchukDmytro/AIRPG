using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AIRPG.Features.Brewery.Editors;

public partial class EditorView : UserControl
{
    public EditorView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}