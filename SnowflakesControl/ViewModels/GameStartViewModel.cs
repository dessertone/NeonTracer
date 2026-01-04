using System;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SnowflakesControl.Models;

namespace SnowflakesControl.ViewModels;

public partial class GameStartViewModel: ViewModelBase
{
    
    public AvaloniaList<Snowflake> Snowflakes { get;} = new();
    
    
    public double GameDurationMilliseconds => GameDuration.TotalMilliseconds;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GameDurationMilliseconds))]
    private TimeSpan gameDuration;
    
    public double MillisecondsRemaining => (GameDuration - _stopwatch.Elapsed).TotalMilliseconds;

    [ObservableProperty]
    private bool isGameRunning;

    [ObservableProperty]
    private double score;
    
    private readonly DispatcherTimer _gameTimer;
    private readonly Stopwatch _stopwatch = new ();


    public GameStartViewModel()
    {
        _gameTimer = new(TimeSpan.FromMilliseconds(10), DispatcherPriority.Background, OnGameTimerTick);
    }

    private void OnGameTimerTick(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(MillisecondsRemaining));
        if (MillisecondsRemaining <= 0)
        {
            _gameTimer.Stop();
            _stopwatch.Stop();
            Snowflakes.Clear();
            IsGameRunning = false;
        }
    }

    [RelayCommand]
    private void StartGame()
    {
        // Clear all snowflakes.
        Snowflakes.Clear();
        
        // Reset game score.
        Score = 0;
        // Add some snowflakes at random positions, having random diameters and speed. 
        
        for (int i = 0; i < 20; i++)
        {
            Snowflakes.Add(new Snowflake(
                Random.Shared.NextDouble(),
                Random.Shared.NextDouble(),
                Random.Shared.NextDouble() * 7 + 5,
                Random.Shared.NextDouble() / 5 + 0.1));
        }

        // Set game time.
        GameDuration = TimeSpan.FromMinutes(0.3);
        
        // Indicate that game has started.
        IsGameRunning = true;
        
        // Start the timer and stopwatch.
        _stopwatch.Restart();
        _gameTimer.Start();
    }
    
}