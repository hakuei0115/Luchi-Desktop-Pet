using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luchi.DesktopPet;

public partial class MainWindow
{
    private const int IdleBlinkIntervalMs = 4000;

    private readonly DispatcherTimer _idleBlinkTimer = new();
    private (ImageSource Image, int HoldMilliseconds)[] _idleBlinkFrames = [];
    private int _nextIdleBlinkFrame;
    private bool _isDraggingPet;

    private ImageSource InitializeIdleBlink()
    {
        _idleBlinkFrames =
        [
            (LoadIdleBlinkFrame("open-1"), 35),
            (LoadIdleBlinkFrame("half-1"), 45),
            (LoadIdleBlinkFrame("closed"), 60),
            (LoadIdleBlinkFrame("half-2"), 45),
            (LoadIdleBlinkFrame("open-2"), IdleBlinkIntervalMs)
        ];

        _idleBlinkTimer.Tick += IdleBlinkTimer_Tick;
        Loaded += (_, _) => ScheduleIdleBlink();
        IsVisibleChanged += (_, _) => ScheduleIdleBlink();
        StateChanged += (_, _) => ScheduleIdleBlink();
        Closed += (_, _) => _idleBlinkTimer.Stop();

        return _idleBlinkFrames[^1].Image;
    }

    private static ImageSource LoadIdleBlinkFrame(string frameName)
    {
        return PetSpriteFactory.Create(
            $"Assets/luchi-idle-{frameName}.png",
            1186,
            1327,
            21,
            new Point(580, 1314));
    }

    private void ScheduleIdleBlink()
    {
        PauseIdleBlink();

        if (!IsLoaded ||
            !IsVisible ||
            WindowState == WindowState.Minimized ||
            _isDraggingPet ||
            !ReferenceEquals(PetImage.Source, _idleSprite))
        {
            return;
        }

        _idleBlinkTimer.Interval = TimeSpan.FromMilliseconds(IdleBlinkIntervalMs);
        _idleBlinkTimer.Start();
    }

    private void PauseIdleBlink()
    {
        _idleBlinkTimer.Stop();

        if (_nextIdleBlinkFrame != 0)
        {
            _nextIdleBlinkFrame = 0;
            PetImage.Source = _idleSprite;
        }
    }

    private void IdleBlinkTimer_Tick(object? sender, EventArgs e)
    {
        var frame = _idleBlinkFrames[_nextIdleBlinkFrame];
        PetImage.Source = frame.Image;
        _idleBlinkTimer.Interval = TimeSpan.FromMilliseconds(frame.HoldMilliseconds);
        _nextIdleBlinkFrame = (_nextIdleBlinkFrame + 1) % _idleBlinkFrames.Length;
    }

    private void DragPetWithBlinkPaused()
    {
        _isDraggingPet = true;
        PauseIdleBlink();

        try
        {
            DragMove();
        }
        finally
        {
            _isDraggingPet = false;
            ScheduleIdleBlink();
        }
    }
}