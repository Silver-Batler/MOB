using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace comparison_of_images.Utilities
{
    public static class WpfUtilities
    {
        public static void LimitTranslation(TranslateTransform translate, Image image, Grid container, ScaleTransform scale)
        {
            double scaledWidth = image.ActualWidth * scale.ScaleX;
            double scaledHeight = image.ActualHeight * scale.ScaleY;
            double containerWidth = container.ActualWidth;
            double containerHeight = container.ActualHeight;

            double maxX = (scaledWidth > containerWidth)
                ? (scaledWidth - containerWidth) / 2
                : (containerWidth - scaledWidth) / 2;
            double maxY = (scaledHeight > containerHeight)
                ? (scaledHeight - containerHeight) / 2
                : (containerHeight - scaledHeight) / 2;

            translate.X = Math.Min(maxX, Math.Max(-maxX, translate.X));
            translate.Y = Math.Min(maxY, Math.Max(-maxY, translate.Y));
        }
    }
}