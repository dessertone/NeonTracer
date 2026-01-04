using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NeonTracer.ViewModels;

namespace NeonTracer.Views;

public partial class GameView : UserControl
{
    public GameView()
    {
        InitializeComponent();
        DataContext = new GameViewModel();
    }
}