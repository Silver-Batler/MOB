using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace comparison_of_images
{
    public class DiagonalGuideHandler
    {
        private readonly Line[] _diagonalLines1;
        private readonly Line[] _diagonalLines2;
        private bool _altPressed = false;

        public DiagonalGuideHandler(Line[] diagonalLines1, Line[] diagonalLines2)
        {
            _diagonalLines1 = diagonalLines1;
            _diagonalLines2 = diagonalLines2;
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
                for (int i = 0; i < _diagonalLines1.Length; i++)
                {
                    _diagonalLines1[i].Visibility = Visibility.Collapsed;
                    _diagonalLines2[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        public void HandleMouseMove(object sender, MouseEventArgs e, ImageManager imageManager, bool isDiagonalGuidesChecked)
        {
            var image = sender as Image;
            int imageIndex = Array.IndexOf(imageManager.Images, image);
            if (imageIndex == -1) return;

            if (!isDiagonalGuidesChecked)
            {
                for (int i = 0; i < _diagonalLines1.Length; i++)
                {
                    _diagonalLines1[i].Visibility = Visibility.Collapsed;
                    _diagonalLines2[i].Visibility = Visibility.Collapsed;
                }
                return;
            }

            if (image.Source != null)
            {
                Point pos = e.GetPosition(imageManager.ImageContainers[imageIndex]);
                double width = imageManager.ImageContainers[imageIndex].ActualWidth;
                double height = imageManager.ImageContainers[imageIndex].ActualHeight;

                if (_altPressed)
                {
                    for (int i = 0; i < imageManager.Images.Length; i++)
                    {
                        if (imageManager.Images[i].Source != null)
                        {
                            double w = imageManager.ImageContainers[i].ActualWidth;
                            double h = imageManager.ImageContainers[i].ActualHeight;
                            Point posInContainer = e.GetPosition(imageManager.ImageContainers[i]);

                            if (posInContainer.X >= 0 && posInContainer.X <= w &&
                                posInContainer.Y >= 0 && posInContainer.Y <= h)
                            {
                                UpdateDiagonalLines(i, posInContainer, w);
                            }
                            else
                            {
                                _diagonalLines1[i].Visibility = Visibility.Collapsed;
                                _diagonalLines2[i].Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
                else
                {
                    if (pos.X >= 0 && pos.X <= width && pos.Y >= 0 && pos.Y <= height)
                    {
                        UpdateDiagonalLines(imageIndex, pos, width);
                    }
                    else
                    {
                        _diagonalLines1[imageIndex].Visibility = Visibility.Collapsed;
                        _diagonalLines2[imageIndex].Visibility = Visibility.Collapsed;
                    }

                    for (int i = 0; i < _diagonalLines1.Length; i++)
                    {
                        if (i != imageIndex)
                        {
                            _diagonalLines1[i].Visibility = Visibility.Collapsed;
                            _diagonalLines2[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void UpdateDiagonalLines(int index, Point pos, double width)
        {
            _diagonalLines1[index].X1 = 0;
            _diagonalLines1[index].Y1 = pos.Y - pos.X;
            _diagonalLines1[index].X2 = width;
            _diagonalLines1[index].Y2 = pos.Y - pos.X + width;
            _diagonalLines1[index].Visibility = Visibility.Visible;

            _diagonalLines2[index].X1 = 0;
            _diagonalLines2[index].Y1 = pos.Y + pos.X;
            _diagonalLines2[index].X2 = width;
            _diagonalLines2[index].Y2 = pos.Y + pos.X - width;
            _diagonalLines2[index].Visibility = Visibility.Visible;
        }

        public void HideDiagonalLines(int index)
        {
            _diagonalLines1[index].Visibility = Visibility.Collapsed;
            _diagonalLines2[index].Visibility = Visibility.Collapsed;
        }
    }
}
