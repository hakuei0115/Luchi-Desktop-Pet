using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Luchi.DesktopPet
{
    internal static class PetSpriteFactory
    {
        private const double CanvasSize = 280;
        private const double TargetCharacterHeight = 250;

        private static readonly Point TargetFootAnchor =
            new Point(140, 270);

        public static ImageSource Create(
            string resourcePath,
            int expectedWidth,
            int expectedHeight,
            double headTopY,
            Point footAnchor)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(
                $"pack://application:,,,/{resourcePath}",
                UriKind.Absolute);
            bitmap.EndInit();

            if (bitmap.PixelWidth != expectedWidth
                || bitmap.PixelHeight != expectedHeight)
            {
                throw new InvalidOperationException(
                    $"{resourcePath} 的尺寸為 "
                    + $"{bitmap.PixelWidth} × {bitmap.PixelHeight}，"
                    + $"預期為 {expectedWidth} × {expectedHeight}。"
                    + "請確認使用的素材版本。");
            }

            if (headTopY < 0
                || footAnchor.Y <= headTopY
                || footAnchor.Y >= bitmap.PixelHeight
                || footAnchor.X < 0
                || footAnchor.X >= bitmap.PixelWidth)
            {
                throw new ArgumentException(
                    $"{resourcePath} 的角色高度或腳底錨點設定不正確。");
            }

            bitmap.Freeze();

            double sourceCharacterHeight =
                footAnchor.Y - headTopY;

            double scale =
                TargetCharacterHeight / sourceCharacterHeight;

            double left =
                TargetFootAnchor.X - footAnchor.X * scale;

            double top =
                TargetFootAnchor.Y - footAnchor.Y * scale;

            var imageBounds = new Rect(
                left,
                top,
                bitmap.PixelWidth * scale,
                bitmap.PixelHeight * scale);

            var canvasBounds = new Rect(
                0,
                0,
                CanvasSize,
                CanvasSize);

            var drawing = new DrawingGroup
            {
                ClipGeometry = new RectangleGeometry(canvasBounds)
            };

            using (DrawingContext context = drawing.Open())
            {
                // 固定輸出畫布的尺寸與透明留白。
                context.DrawRectangle(
                    Brushes.Transparent,
                    null,
                    canvasBounds);

                // 將完整原圖等比例繪製到校準後的位置。
                context.DrawImage(bitmap, imageBounds);
            }

            var result = new DrawingImage(drawing);
            result.Freeze();

            return result;
        }
    }
}