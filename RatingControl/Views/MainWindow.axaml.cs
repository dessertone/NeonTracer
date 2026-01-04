using Avalonia.Controls;
using RatingControl.ViewModels;

namespace RatingControl.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}