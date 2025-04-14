using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace comparison_of_images
{
    public partial class MainWindow : Window
    {
        private readonly ImageManager _imageManager;
        private readonly ImageTransformations _imageTransformations;
        private readonly DiagonalGuideHandler _diagonalGuideHandler;

        public MainWindow()
        {
            InitializeComponent();
            _imageManager = new ImageManager(
                new[] { DisplayedImage1, DisplayedImage2, DisplayedImage3, DisplayedImage4 },
                new[] { ImageContainer1, ImageContainer2, ImageContainer3, ImageContainer4 },
                new[] { FileNameTextBlock1, FileNameTextBlock2, FileNameTextBlock3, FileNameTextBlock4 },
                new[] { ImageSizeTextBlock1, ImageSizeTextBlock2, ImageSizeTextBlock3, ImageSizeTextBlock4 },
                new[] { Container1, Container2, Container3, Container4 }
            );
            _imageTransformations = new ImageTransformations(
                new[] { ImageScale1, ImageScale2, ImageScale3, ImageScale4 },
                new[] { ImageRotate1, ImageRotate2, ImageRotate3, ImageRotate4 },
                new[] { ImageTranslate1, ImageTranslate2, ImageTranslate3, ImageTranslate4 }
            );
            _diagonalGuideHandler = new DiagonalGuideHandler(
                new[] { DiagonalLine1_1, DiagonalLine2_1, DiagonalLine3_1, DiagonalLine4_1 },
                new[] { DiagonalLine1_2, DiagonalLine2_2, DiagonalLine3_2, DiagonalLine4_2 }
            );

            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.PreviewKeyUp += MainWindow_PreviewKeyUp;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _imageTransformations.HandleKeyDown(e);
            _diagonalGuideHandler.HandleKeyDown(e);
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _imageTransformations.HandleKeyUp(e);
            _diagonalGuideHandler.HandleKeyUp(e);
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            _imageManager.LoadImage();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _imageTransformations.HandleMouseWheel(sender, e, _imageManager);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _imageTransformations.HandleMouseDown(sender, e, _imageManager);
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _imageManager.RemoveImage(sender, _imageTransformations, _diagonalGuideHandler);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _imageTransformations.HandleMouseLeftButtonDown(sender, e, _imageManager);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _imageTransformations.HandleMouseLeftButtonUp(sender, e);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            _imageTransformations.HandleMouseMove(sender, e, _imageManager);
            _diagonalGuideHandler.HandleMouseMove(sender, e, _imageManager, DiagonalGuidesCheckBox.IsChecked == true);
        }
    }
}