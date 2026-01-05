using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using NeonTracer.Logic;
using NeonTracer.Models;
using NeonTracer.ViewModels;

namespace NeonTracer.Controls;

public class GameCanvas : Control
{
    #region Avalonia 属性定义

    /// <summary>
    /// 直接属性 得分
    /// </summary>
    public static readonly DirectProperty<GameCanvas, double> ScoreProperty = AvaloniaProperty.RegisterDirect<GameCanvas, double>(
        nameof(Score), o => o.Score, (o, v) => o.Score = v ,defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// 样式属性 是否运行
    /// </summary>
    public static readonly StyledProperty<bool> IsRunningProperty = AvaloniaProperty.Register<GameCanvas, bool>(
        nameof(IsRunning));

    /// <summary>
    /// 直接属性 绑定游戏引擎
    /// </summary>
    public static readonly DirectProperty<GameCanvas, GameEngine> GameEngineProperty = AvaloniaProperty.RegisterDirect<GameCanvas, GameEngine>(
        nameof(GameEngine),
        o => o.GameEngine,
        (o, v) => o.GameEngine = v,
        defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region 属性封装

    private GameEngine _gameEngine;
    public GameEngine GameEngine
    {
        get => _gameEngine;
        set => SetAndRaise(GameEngineProperty, ref _gameEngine, value);
    }

    private double _score;
    public double Score
    {
        get => _score;
        set => SetAndRaise(ScoreProperty, ref _score, value);
    }

    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    #endregion

    #region 私有字段

    /// <summary>
    /// 性能计时器
    /// </summary>
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// 游戏主循环定时器
    /// </summary>
    private readonly DispatcherTimer _gameTimer;

    /// <summary>
    /// 总运行时间
    /// </summary>
    private double _totalTime;

    /// <summary>
    /// 鼠标是否按下
    /// </summary>
    private bool _isPressed;

    /// <summary>
    /// 存储每一帧应该生成的线段
    /// </summary>
    private readonly Queue<Point> _moveQueue = new();

    #endregion

    public GameCanvas()
    {
        
        ClipToBounds = true;
        Focusable = true;

        _gameTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            // 60 FPS 
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameTimer.Tick += GameTick;
    }

    /// <summary>
    /// 启动计时器
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == IsRunningProperty)
        {
            if (IsRunning)
            {
                _stopwatch.Restart();
                _gameTimer.Start();
                Focus();
            }
            else
            {
                _stopwatch.Stop();
                _gameTimer.Stop();
            }
        }
        else if (change.Property == GameEngineProperty)
        {
            var newEngine = change.NewValue as GameEngine;

            if (change.OldValue is GameEngine oldEngine)
            {
                oldEngine.ScoreChanged -= OnEngineScoreChanged;
            }

            if (newEngine != null)
            {
                newEngine.Bounds = Bounds;
                newEngine.ScoreChanged += OnEngineScoreChanged;
            }
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        GameEngine.Bounds = Bounds;
    }

    /// <summary>
    /// 引擎分数变化回调
    /// </summary>
    private void OnEngineScoreChanged(double newScore)
    {
        Score = newScore;
    }

    /// <summary>
    /// 游戏循环 
    /// </summary>
    private void GameTick(object? sender, EventArgs e)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;
        
        var elapsedMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;
        
        _totalTime += elapsedMilliseconds;
        _stopwatch.Restart();
        GameEngine.Update(elapsedMilliseconds, _moveQueue);
        
        InvalidateVisual();
    }

    /// <summary>
    /// 重写渲染方法，基于Skia自定义渲染元素
    /// </summary>
    public override void Render(DrawingContext context)
    {
        // 绘制透明背景以确保击中测试正常工作
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
        
        base.Render(context);

        // 调用自定义渲染操作
        context.Custom(new NeonRenderOperation(
            GameEngine.ActivePhotons, 
            GameEngine.ActiveSegments, 
            GameEngine.ActiveExplosions, 
            Bounds, 
            _totalTime));
    }

    #region 交互事件

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsRunning) return;
        
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPressed = true;
            _moveQueue.Enqueue(e.GetPosition(this));
        }
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!IsRunning || !_isPressed) return;

        var endPoint = e.GetPosition(this);
        _moveQueue.Enqueue(endPoint);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!IsRunning) return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPressed = false;
            _moveQueue.Clear();
            e.Handled = true;
        }
    }
    


    #endregion
}