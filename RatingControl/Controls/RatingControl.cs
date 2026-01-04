using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;

namespace RatingControl.Controls;

[TemplatePart("PART_StarsPresenter", typeof(ItemsControl))]

public class RatingControl:TemplatedControl
{

    
    
    public RatingControl()
    {
        UpdateStars();
    }
    
    private ItemsControl? _starsPresenter;
    
    /// <summary>
    /// define the star counts as StyledProperty so that we can set it inside our style settings 
    /// </summary>
    public static readonly StyledProperty<int> NumberOfStarsProperty = AvaloniaProperty.Register<RatingControl, int>(
        nameof(NumberOfStars),
        5,
        coerce: (_, v) => v < 1 ? 1 : v
        );
    public int NumberOfStars
    {
        get => GetValue(NumberOfStarsProperty);
        set => SetValue(NumberOfStarsProperty, value);
    }


    private int _previewValue;

    public static readonly DirectProperty<RatingControl, int> PreviewValueProperty = AvaloniaProperty.RegisterDirect<RatingControl, int>(
        nameof(PreviewValue), o => o.PreviewValue, (o, v) => o.PreviewValue = v,defaultBindingMode:BindingMode.TwoWay);

    public int PreviewValue
    {
        get => _previewValue;
        set => SetAndRaise(PreviewValueProperty, ref _previewValue, value);
    }

    /// <summary>
    /// we can use this property to draw the stars
    /// </summary>
    public static readonly DirectProperty<RatingControl, IEnumerable<int>> StarsProperty = AvaloniaProperty.RegisterDirect<RatingControl, IEnumerable<int>>(
        nameof(Stars), o => o.Stars);
    
    private IEnumerable<int> _stars;
    public IEnumerable<int> Stars
    {
        get => _stars;
        set => SetAndRaise(StarsProperty, ref _stars, value);
    }


    /// <summary>
    /// user choose the stars' value and we can dynamically update behind the code
    /// </summary>
    public static readonly DirectProperty<RatingControl, int> ValueProperty = AvaloniaProperty.RegisterDirect<RatingControl, int>(
        nameof(Value), o => o.Value, (o, v) => o.Value = v,defaultBindingMode:BindingMode.TwoWay, enableDataValidation:true);
    
    private int _value;
    public int Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    /// <summary>
    /// register ItemsControl's event 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_starsPresenter is not null)
        {
            _starsPresenter.PointerReleased -= StarsPresenter_PointerReleased;
            _starsPresenter.PointerExited -= StarsPresenter_PointerExited;
        }
        
        _starsPresenter = e.NameScope.Find<ItemsControl>("PART_StarsPresenter");
        if (_starsPresenter is not null)
        {
            _starsPresenter.PointerReleased += StarsPresenter_PointerReleased;
            _starsPresenter.PointerExited += StarsPresenter_PointerExited;
        }
    }

    private void StarsPresenter_PointerExited(object? sender, PointerEventArgs e)
    {
        if (e.Source is Path star)
        {
            PreviewValue =  Value;
        }
    }

    private void Stars_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (e.Source is Path star)
        {
            PreviewValue = star.DataContext as int? ?? Value;
        }
    }

    private void StarsPresenter_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is Path star)
        {
            Value = star.DataContext as int? ?? 0;     
        }
    }

    /// <summary>
    /// update Stars size with NumberOfStars
    /// </summary>
    /// <param name="change"></param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == NumberOfStarsProperty)
        {
            UpdateStars();
        }
    }

    private void UpdateStars()
    {
        Stars = Enumerable.Range(1, NumberOfStars);
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        base.UpdateDataValidation(property, state, error);
        
        if(property == ValueProperty)
            DataValidationErrors.SetError(this, error);
    }
}