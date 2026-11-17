using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace Luchi.DesktopPet
{
    public partial class MainWindow : Window
    {
        private Point _pressStartPosition;
        private bool _isPressPending;

        private readonly DispatcherTimer _bubbleTimer =
            new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

        private readonly Forms.ContextMenuStrip _trayMenu;
        private readonly Forms.NotifyIcon _trayIcon;
        private readonly Drawing.Icon _trayIconImage;

        public MainWindow()
        {
            InitializeComponent();

            _bubbleTimer.Tick += BubbleTimer_Tick;

            _trayMenu = CreateTrayMenu();

            // 複製 Windows 內建圖示，作為目前的佔位圖示。
            _trayIconImage =
                (Drawing.Icon)Drawing.SystemIcons.Application.Clone();

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = _trayIconImage,
                Text = "Luchi Desktop Pet",
                ContextMenuStrip = _trayMenu
            };

            _trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick;
            _trayIcon.Visible = true;
        }

        private Forms.ContextMenuStrip CreateTrayMenu()
        {
            var menu = new Forms.ContextMenuStrip();

            var showItem = menu.Items.Add(
                "顯示露希",
                null,
                TrayShowMenuItem_Click);

            var hideItem = menu.Items.Add(
                "隱藏露希",
                null,
                TrayHideMenuItem_Click);

            menu.Items.Add(new Forms.ToolStripSeparator());

            menu.Items.Add(
                "離開",
                null,
                TrayExitMenuItem_Click);

            menu.Opening += (sender, e) =>
            {
                bool needsRestore =
                    !IsVisible || WindowState == WindowState.Minimized;

                showItem.Visible = needsRestore;
                hideItem.Visible = !needsRestore;
            };

            return menu;
        }

        private void ShowPet()
        {
            if (!IsVisible)
            {
                Show();
            }

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
        }

        private void HidePet()
        {
            EndPress();
            HideSpeechBubble();
            ResetPetScale();
            Hide();
        }

        private void TrayShowMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            ShowPet();
        }

        private void TrayHideMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            HidePet();
        }

        private void TrayExitMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            Close();
        }

        private void TrayIcon_MouseDoubleClick(
            object? sender,
            Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ShowPet();
            }
        }

        private void PetImage_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            e.Handled = true;

            _pressStartPosition = e.GetPosition(this);
            _isPressPending = PetImage.CaptureMouse();
        }

        private void PetImage_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (!_isPressPending)
            {
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                EndPress();
                return;
            }

            if (!HasReachedDragThreshold(e.GetPosition(this)))
            {
                return;
            }

            e.Handled = true;

            EndPress();
            HideSpeechBubble();
            ResetPetScale();

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void PetImage_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!_isPressPending)
            {
                return;
            }

            e.Handled = true;

            bool stayedWithinThreshold =
                !HasReachedDragThreshold(e.GetPosition(this));

            bool releasedInsideImage =
                new Rect(PetImage.RenderSize)
                    .Contains(e.GetPosition(PetImage));

            EndPress();

            if (stayedWithinThreshold && releasedInsideImage)
            {
                ShowClickReaction();
            }
        }

        private void PetImage_LostMouseCapture(
            object sender,
            MouseEventArgs e)
        {
            _isPressPending = false;
        }

        private bool HasReachedDragThreshold(Point currentPosition)
        {
            double horizontalDistance =
                Math.Abs(currentPosition.X - _pressStartPosition.X);

            double verticalDistance =
                Math.Abs(currentPosition.Y - _pressStartPosition.Y);

            return horizontalDistance >=
                       SystemParameters.MinimumHorizontalDragDistance
                   || verticalDistance >=
                       SystemParameters.MinimumVerticalDragDistance;
        }

        private void EndPress()
        {
            _isPressPending = false;

            if (PetImage.IsMouseCaptured)
            {
                PetImage.ReleaseMouseCapture();
            }
        }

        private void PlayClickAnimation()
        {
            // 連續點擊時，先取消舊動畫並恢復原始大小。
            ResetPetScale();

            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.04,
                Duration = new Duration(TimeSpan.FromMilliseconds(160)),
                AutoReverse = true,
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new SineEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            PetScale.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                animation,
                HandoffBehavior.SnapshotAndReplace);

            PetScale.BeginAnimation(
                ScaleTransform.ScaleYProperty,
                animation,
                HandoffBehavior.SnapshotAndReplace);
        }

        private void ResetPetScale()
        {
            PetScale.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                null);

            PetScale.BeginAnimation(
                ScaleTransform.ScaleYProperty,
                null);

            PetScale.ScaleX = 1.0;
            PetScale.ScaleY = 1.0;
        }

        private void ShowClickReaction()
        {
            PlayClickAnimation();

            BubbleText.Text = "主人，怎麼了？";
            SpeechBubble.Visibility = Visibility.Visible;

            _bubbleTimer.Stop();
            _bubbleTimer.Start();
        }

        private void HideSpeechBubble()
        {
            _bubbleTimer.Stop();
            SpeechBubble.Visibility = Visibility.Hidden;
        }

        private void BubbleTimer_Tick(object? sender, EventArgs e)
        {
            HideSpeechBubble();
        }

        private void HideMenuItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            HidePet();
        }

        private void ExitMenuItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            EndPress();
            ResetPetScale();

            _bubbleTimer.Stop();
            _bubbleTimer.Tick -= BubbleTimer_Tick;

            _trayIcon.Visible = false;
            _trayIcon.MouseDoubleClick -= TrayIcon_MouseDoubleClick;
            _trayIcon.Dispose();

            _trayMenu.Dispose();
            _trayIconImage.Dispose();
        }

        private void PetImage_ImageFailed(
            object sender,
            ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(
                e.ErrorException.ToString(),
                "露希圖片載入失敗",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}