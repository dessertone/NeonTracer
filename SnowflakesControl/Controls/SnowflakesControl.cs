using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using SnowflakesControl.Models;

namespace SnowflakesControl.Controls;

public class SnowflakesControl:Control, ICustomHitTest
{
    static SnowflakesControl()
    {
        AffectsRender<SnowflakesControl>(SnowflakesProperty, IsRunningProperty);
    }

    /// <summary>   
    /// define <see cref="IsRunning"/> StyledProperty
    /// </summary>
    public static readonly StyledProperty<bool> IsRunningProperty = AvaloniaProperty.Register<SnowflakesControl, bool>(
        nameof(IsRunning));
    
    /// <summary>
    /// define <see cref="Score"/> DirectProperty
    /// </summary>
    public static readonly DirectProperty<SnowflakesControl, double> ScoreProperty = AvaloniaProperty.RegisterDirect<SnowflakesControl, double>(
        nameof(Score), o => o.Score, (o, v) => o.Score = v, defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// define <see cref="Snowflakes"/> DirectProperty
    /// </summary>
    public static readonly DirectProperty<SnowflakesControl, IList<Snowflake>> SnowflakesProperty = AvaloniaProperty.RegisterDirect<SnowflakesControl, IList<Snowflake>>(
        nameof(Snowflakes), o => o.Snowflakes, (o, v) => o.Snowflakes = v);

    private ICollection<SnowHint> _snowHints = [];
    
    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }
    
    private IList<Snowflake> _snowflakes  = new List<Snowflake>();
    public IList<Snowflake> Snowflakes
    {
        get => _snowflakes;
        set => SetAndRaise(SnowflakesProperty, ref _snowflakes, value);
    }
    
    private double _score;
    public double Score
    {
        get => _score;
        set => SetAndRaise(ScoreProperty, ref _score, value);
    }

    private readonly Stopwatch _stopwatch = new();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsRunningProperty)
        {
            if (change.GetNewValue<bool>())
            {
                _stopwatch.Restart();
            }
            else
            {
                _stopwatch.Stop();
            }
        }
    }

        public override void Render(DrawingContext context)
        {
            double elapsed = _stopwatch.Elapsed.TotalMilliseconds;

            // render snowflakes
            foreach (var snowflake in Snowflakes)
            { 
                if (IsRunning)
                {
                    snowflake.Move(elapsed);
                }
                
                context.DrawEllipse(Brushes.AliceBlue, null, snowflake.GetCenterForViewport(Bounds), snowflake.Radius, snowflake.Radius);
            }

            var scoreText = new FormattedText(
                $"Score: {Score:F1}",
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                40,
                new SolidColorBrush(Colors.Bisque)
            );
            scoreText.SetFontWeight(FontWeight.Bold);
            context.DrawText(scoreText, new Point(0, 0));
            // render snowHints
            foreach (var snowHint in _snowHints.ToArray())
            {
                if (IsRunning)
                {
                    snowHint.Update(elapsed);
                }
                var formattedText = new FormattedText(snowHint.ToString(),
                    CultureInfo.InvariantCulture, 
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    20,
                    new SolidColorBrush(Colors.Gold, snowHint.Opacity));
                context.DrawText(formattedText, snowHint.GetTopLeftForViewport(Bounds, new Size(formattedText.Width, formattedText.Height)));
            }
            
            
            
            base.Render(context);

            if (IsRunning)
            {
                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
                _stopwatch.Restart();
            }
        }


    public bool HitTest(Point point)
    {
        var snowflake = Snowflakes.FirstOrDefault(s => s.IsHit(point, Bounds));
        if (snowflake is not null)
        {
            Snowflakes.Remove(snowflake);
            Score += snowflake.Score;
            _snowHints.Add(new SnowHint(snowflake, _snowHints));
        }
        return snowflake != null;
    }
}