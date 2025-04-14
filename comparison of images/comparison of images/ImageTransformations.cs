using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace comparison_of_images
{
    public class ImageTransformations
    {
        private readonly ScaleTransform[] _imageScales;
        private readonly RotateTransform[] _imageRotates;
        private readonly TranslateTransform[] _imageTranslates;
        private Point _startMousePos;
        private Point _startTranslate;
        private bool _isDragging = false;
        private bool _altPressed = false;

        public ImageTransformations(ScaleTransform[] scales, RotateTransform[] rotates, TranslateTransform[] translates)
        {
            _imageScales = scales;
            _imageRotates = rotates;
            _imageTranslates = translates;
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                _altPressed = true;
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                _altPressed = false;
            }
        }

        public void HandleMouseWheel(object sender, MouseWheelEventArgs e, ImageManager imageManager)
        {
            var image = sender as Image;
            int imageIndex = Array.IndexOf(imageManager.Images, image);
            if (imageIndex == -1) return;

            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;
            double newScale = _imageScales[imageIndex].ScaleX * zoomFactor;

            if (newScale < 1) return;

            if (_altPressed)
            {
                for (int i = 0; i < imageManager.Images.Length; i++)
                {
                    if (imageManager.Images[i].Source != null)
                    {
                        Point mousePosInContainer = e.GetPosition(imageManager.ImageContainers[i]);
                        double relativeX = mousePosInContainer.X - (imageManager.ImageContainers[i].ActualWidth / 2);
                        double relativeY = mousePosInContainer.Y - (imageManager.ImageContainers[i].ActualHeight / 2);
                        double scaleChange = newScale / _imageScales[i].ScaleX;

                        _imageScales[i].ScaleX = newScale;
                        _imageScales[i].ScaleY = newScale;
                        _imageTranslates[i].X = (_imageTranslates[i].X - relativeX) * scaleChange + relativeX;
                        _imageTranslates[i].Y = (_imageTranslates[i].Y - relativeY) * scaleChange + relativeY;

                        LimitTranslation(i, imageManager.Images[i], imageManager.ImageContainers[i]);
                    }
                }
            }
            else
            {
                Point mousePosInContainer = e.GetPosition(imageManager.ImageContainers[imageIndex]);
                double relativeX = mousePosInContainer.X - (imageManager.ImageContainers[imageIndex].ActualWidth / 2);
                double relativeY = mousePosInContainer.Y - (imageManager.ImageContainers[imageIndex].ActualHeight / 2);
                double scaleChange = newScale / _imageScales[imageIndex].ScaleX;

                _imageScales[imageIndex].ScaleX = newScale;
                _imageScales[imageIndex].ScaleY = newScale;
                _imageTranslates[imageIndex].X = (_imageTranslates[imageIndex].X - relativeX) * scaleChange + relativeX;
                _imageTranslates[imageIndex].Y = (_imageTranslates[imageIndex].Y - relativeY) * scaleChange + relativeY;

                LimitTranslation(imageIndex, image, imageManager.ImageContainers[imageIndex]);
            }
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e, ImageManager imageManager)
        {
            if (e.ChangedButton != MouseButton.Middle) return;

            var image = sender as Image;
            int imageIndex = Array.IndexOf(imageManager.Images, image);
            if (imageIndex == -1) return;

            if (_altPressed)
            {
                for (int i = 0; i < imageManager.Images.Length; i++)
                {
                    if (imageManager.Images[i].Source != null)
                    {
                        _imageRotates[i].Angle = (_imageRotates[i].Angle + 90) % 360;
                    }
                }
            }
            else
            {
                _imageRotates[imageIndex].Angle = (_imageRotates[imageIndex].Angle + 90) % 360;
            }

            e.Handled = true;
        }

        public void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e, ImageManager imageManager)
        {
            var image = sender as Image;
            int imageIndex = Array.IndexOf(imageManager.Images, image);
            if (imageIndex == -1) return;

            if (_imageScales[imageIndex].ScaleX > 1 || _imageScales[imageIndex].ScaleY > 1)
            {
                _isDragging = true;
                _startMousePos = e.GetPosition(imageManager.ImageContainers[0].Parent as IInputElement);
                _startTranslate = new Point(_imageTranslates[imageIndex].X, _imageTranslates[imageIndex].Y);
                image.CaptureMouse();
                imageManager.Images[imageIndex].Tag = imageIndex; // Temporary storage for dragging
            }
        }

        public void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (image == null) return;

            _isDragging = false;
            image.ReleaseMouseCapture();
        }

        public void HandleMouseMove(object sender, MouseEventArgs e, ImageManager imageManager)
        {
            var image = sender as Image;
            int imageIndex = Array.IndexOf(imageManager.Images, image);
            if (imageIndex == -1) return;

            if (_isDragging && (imageManager.CurrentImageIndex == imageIndex || _altPressed))
            {
                Point currentMousePos = e.GetPosition(imageManager.ImageContainers[0].Parent as IInputElement);
                Vector delta = currentMousePos - _startMousePos;

                if (_altPressed)
                {
                    for (int i = 0; i < imageManager.Images.Length; i++)
                    {
                        if (imageManager.Images[i].Source != null)
                        {
                            ApplyTranslation(i, delta, imageManager.Images[i], imageManager.ImageContainers[i]);
                        }
                    }
                }
                else
                {
                    ApplyTranslation(imageIndex, delta, image, imageManager.ImageContainers[imageIndex]);
                }
            }
        }

        private void ApplyTranslation(int index, Vector delta, Image image, Grid container)
        {
            double scaledWidth = image.ActualWidth * _imageScales[index].ScaleX;
            double scaledHeight = image.ActualHeight * _imageScales[index].ScaleY;
            double containerWidth = container.ActualWidth;
            double containerHeight = container.ActualHeight;

            double maxX = (scaledWidth > containerWidth)
                ? (scaledWidth - containerWidth) / 2
                : (containerWidth - scaledWidth) / 2;
            double maxY = (scaledHeight > containerHeight)
                ? (scaledHeight - containerHeight) / 2
                : (containerHeight - scaledHeight) / 2;

            double newX = _startTranslate.X + delta.X;
            double newY = _startTranslate.Y + delta.Y;

            _imageTranslates[index].X = Math.Min(maxX, Math.Max(-maxX, newX));
            _imageTranslates[index].Y = Math.Min(maxY, Math.Max(-maxY, newY));
        }

        private void LimitTranslation(int index, Image image, Grid container)
        {
            double scaledWidth = image.ActualWidth * _imageScales[index].ScaleX;
            double scaledHeight = image.ActualHeight * _imageScales[index].ScaleY;
            double containerWidth = container.ActualWidth;
            double containerHeight = container.ActualHeight;

            double maxX = (scaledWidth > containerWidth)
                ? (scaledWidth - containerWidth) / 2
                : (containerWidth - scaledWidth) / 2;
            double maxY = (scaledHeight > containerHeight)
                ? (scaledHeight - containerHeight) / 2
                : (containerHeight - scaledHeight) / 2;

            _imageTranslates[index].X = Math.Min(maxX, Math.Max(-maxX, _imageTranslates[index].X));
            _imageTranslates[index].Y = Math.Min(maxY, Math.Max(-maxY, _imageTranslates[index].Y));
        }

        public void ResetTransforms(int index)
        {
            _imageScales[index].ScaleX = 1;
            _imageScales[index].ScaleY = 1;
            _imageRotates[index].Angle = 0;
            _imageTranslates[index].X = 0;
            _imageTranslates[index].Y = 0;
        }

        public bool IsAltPressed => _altPressed;
    }
}