using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace comparison_of_images
{
    public class ImageManager
    {
        private readonly Image[] _images;
        private readonly Grid[] _imageContainers;
        private readonly TextBlock[] _fileNameTextBlocks;
        private readonly TextBlock[] _imageSizeTextBlocks;
        private readonly StackPanel[] _containers;
        private int _currentImageIndex = -1;

        public ImageManager(Image[] images, Grid[] imageContainers, TextBlock[] fileNameTextBlocks,
            TextBlock[] imageSizeTextBlocks, StackPanel[] containers)
        {
            _images = images;
            _imageContainers = imageContainers;
            _fileNameTextBlocks = fileNameTextBlocks;
            _imageSizeTextBlocks = imageSizeTextBlocks;
            _containers = containers;
        }

        public void LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentImageIndex = GetNextEmptyImageIndex();
                if (_currentImageIndex == -1)
                {
                    MessageBox.Show("Все контейнеры заполнены. Сначала очистите один из них.");
                    return;
                }

                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                _images[_currentImageIndex].Source = bitmap;
                _containers[_currentImageIndex].Visibility = Visibility.Visible;

                string fileName = Path.GetFileName(openFileDialog.FileName);
                int imageWidth = bitmap.PixelWidth;
                int imageHeight = bitmap.PixelHeight;

                _fileNameTextBlocks[_currentImageIndex].Text = fileName;
                _imageSizeTextBlocks[_currentImageIndex].Text = $"({imageWidth} x {imageHeight})";
            }
        }

        public void RemoveImage(object sender, ImageTransformations transformations, DiagonalGuideHandler guideHandler)
        {
            var image = sender as Image;
            int imageIndex = Array.IndexOf(_images, image);
            if (imageIndex == -1) return;

            bool altPressed = transformations.IsAltPressed;
            if (altPressed)
            {
                for (int i = 0; i < _images.Length; i++)
                {
                    ClearImage(i, transformations, guideHandler);
                    _containers[i].Visibility = i == imageIndex ? Visibility.Visible : Visibility.Collapsed;
                }
                _currentImageIndex = -1;
            }
            else
            {
                ClearImage(imageIndex, transformations, guideHandler);
                int loadedImagesCount = _images.Count(img => img.Source != null);
                _containers[imageIndex].Visibility = loadedImagesCount > 0 ? Visibility.Collapsed : Visibility.Visible;
                if (_currentImageIndex == imageIndex)
                {
                    _currentImageIndex = -1;
                }
            }
        }

        private void ClearImage(int index, ImageTransformations transformations, DiagonalGuideHandler guideHandler)
        {
            _images[index].Source = null;
            _fileNameTextBlocks[index].Text = string.Empty;
            _imageSizeTextBlocks[index].Text = string.Empty;
            transformations.ResetTransforms(index);
            guideHandler.HideDiagonalLines(index);
        }

        private int GetNextEmptyImageIndex()
        {
            for (int i = 0; i < _images.Length; i++)
            {
                if (_images[i].Source == null)
                    return i;
            }
            return -1;
        }

        public Image[] Images => _images;
        public Grid[] ImageContainers => _imageContainers;
        public int CurrentImageIndex => _currentImageIndex;
    }
}