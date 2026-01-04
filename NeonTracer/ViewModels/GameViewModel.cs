using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeonTracer.Logic;
using NeonTracer.Models;

namespace NeonTracer.ViewModels;

public partial class GameViewModel: ViewModelBase
{
    
    [ObservableProperty]
    private GameEngine _gameEngine= new ();
    
    [ObservableProperty] 
    private double _score;

    [ObservableProperty]
    private bool _isRunning;
    
    [ObservableProperty]
    private TimeSpan _gameDuration;
    
    [ObservableProperty]
    private string _gameStateText = "Ready?";

    [ObservableProperty]
    private bool _isStart;
    
    /// <summary>
    /// 供UI选择的霓虹色列表
    /// </summary>
    public List<Color> AvailableColors { get; } = new()
    {
        Colors.Cyan,
        Colors.Magenta,
        Colors.Lime,
        Colors.Yellow,
        Colors.DeepSkyBlue,
        Colors.HotPink,
        Colors.OrangeRed,
        Colors.White
    };
    
    /// <summary>
    /// 当前选中的颜色   
    /// </summary>
    [ObservableProperty]
    private Color _selectedTrackerColor = Colors.Cyan;
    
    
    
    /// <summary>
    /// 计时器
    /// </summary>
    private readonly Stopwatch _stopwatch = new();
    
    private readonly DispatcherTimer _UISyncTimer;
    
    private readonly Random _random = new();
    
    public GameViewModel()
    {
        // 绑定分数
        GameEngine.ScoreChanged += s => Score = s;
        
        _UISyncTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(2000) ,DispatcherPriority.Background, OnSyncTick);
    }
    
    
    [RelayCommand]
    public async Task Start()
    {
        IsRunning = true;
        IsStart = true;
        GameEngine.Reset();
        _stopwatch.Restart();
        _UISyncTimer.Start();
    }

    [RelayCommand]
    public void Pause()
    {
        if (!IsStart) return;
        IsRunning = !IsRunning;
        if (!IsRunning)
        {
            GameStateText = "Resume";
            _stopwatch.Stop();
            _UISyncTimer.Stop();
        }
        else
        {
            GameStateText = "Pause";
            _stopwatch.Start();
            _UISyncTimer.Start();
        }
    }

    [RelayCommand]
    public void Stop()
    {
        GameStateText = "Ready?";
        GameEngine.Reset();
        IsRunning = false;
        IsStart = false;
    }
    
    [RelayCommand]
    public void AddTracker()
    {
        
        GameEngine.AddAutoTracker(SelectedTrackerColor);
    }

    [RelayCommand]
    public void RemoveTracker()
    {
        GameEngine.RemoveLastAutoTracker();
    }
    /// <summary>
    /// 同步数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSyncTick(object? sender, EventArgs e)
    {
        
    }
    

}