using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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

        public MainWindow()
        {
            InitializeComponent();

            _bubbleTimer.Tick += BubbleTimer_Tick;
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

        private void ShowClickReaction()
        {
            BubbleText.Text = "主人，怎麼了？";
            SpeechBubble.Visibility = Visibility.Visible;

            // 重新點擊時，從現在開始重新計時。
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

        private void Window_Closed(object? sender, EventArgs e)
        {
            EndPress();

            _bubbleTimer.Stop();
            _bubbleTimer.Tick -= BubbleTimer_Tick;
        }

        private void ExitMenuItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
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