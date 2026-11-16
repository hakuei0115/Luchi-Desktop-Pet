using System.Windows;
using System.Windows.Input;

namespace Luchi.DesktopPet
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PetImage_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                DragMove();
            }
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