using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RatingControl.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private int numOfStars;
    
    [ObservableProperty] 
    [NotifyDataErrorInfo]
    [Range(0,5)]
    private int value;

    [ObservableProperty] private int previewValue;
    
    [RelayCommand]
    private void Submit()
    {
        ValidateAllProperties();
    }
}