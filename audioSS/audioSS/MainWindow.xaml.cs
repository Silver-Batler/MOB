using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Wave;

namespace audioSS
{
    /// <summary>
    /// Главное окно приложения для работы с аудиофайлами.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AudioPlayer _audioPlayer;
        private readonly WaveformRenderer _waveformRenderer;
        private readonly AudioTrimmer _audioTrimmer;
        private readonly PlaybackController _playbackController;

        /// <summary>
        /// Инициализирует новое главное окно приложения и объекты для управления аудио.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _audioPlayer = new AudioPlayer();
            _waveformRenderer = new WaveformRenderer(WaveformCanvas);
            _audioTrimmer = new AudioTrimmer(WaveformCanvas);
            _playbackController = new PlaybackController(WaveformCanvas, AudioTimerText);
        }

        /// <summary>S
        /// Обрабатывает выбор и загрузку аудиофайла.
        /// </summary>
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Audio Files|*.mp3;*.wav|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _audioPlayer.LoadFile(openFileDialog.FileName);
                    StatusText.Text = $"Loaded: {System.IO.Path.GetFileName(openFileDialog.FileName)}";
                    _waveformRenderer.DrawWaveform(_audioPlayer.GetAudioReader());
                    _playbackController.Reset(_audioPlayer.GetAudioReader());
                    _audioTrimmer.Reset();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading audio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки воспроизведения.
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _audioPlayer.Play(_playbackController);
                StatusText.Text = $"Playing: {System.IO.Path.GetFileName(_audioPlayer.CurrentFilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки паузы.
        /// </summary>
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _audioPlayer.Pause();
                _playbackController.StopTimer();
                StatusText.Text = $"Paused: {System.IO.Path.GetFileName(_audioPlayer.CurrentFilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки остановки.
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _audioPlayer.Stop(_playbackController);
                StatusText.Text = $"Stopped: {System.IO.Path.GetFileName(_audioPlayer.CurrentFilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки обрезки.
        /// </summary>
        private void TrimButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _audioTrimmer.TrimAudio(this, _audioPlayer.CurrentFilePath, _playbackController);
                if (!_audioTrimmer.IsTrimming)
                {
                    StatusText.Text = $"Trimmed: {System.IO.Path.GetFileName(_audioPlayer.CurrentFilePath)}";
                }
                else
                {
                    StatusText.Text = "Select trim area and click Trim Audio again to save.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обрабатывает изменение значения слайдера громкости.
        /// </summary>
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.SetVolume((float)e.NewValue);
            }
        }

        /// <summary>
        /// Обрабатывает нажатие мыши на канве для изменения позиции воспроизведения или обрезки.
        /// </summary>
        private void WaveformCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _audioTrimmer.WaveformCanvas_MouseDown(sender, e);
            // Обрабатываем PlaybackController только если не перетаскиваем линию обрезки
            if (!_audioTrimmer.IsTrimming)
            {
                _playbackController.WaveformCanvas_MouseDown(sender, e);
            }
        }

        /// <summary>
        /// Обрабатывает движение мыши для перемещения линии воспроизведения или линий обрезки.
        /// </summary>
        private void WaveformCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            _audioTrimmer.WaveformCanvas_MouseMove(sender, e);
            // Обрабатываем PlaybackController только если не перетаскиваем линию обрезки
            if (!_audioTrimmer.IsTrimming)
            {
                _playbackController.WaveformCanvas_MouseMove(sender, e);
            }
        }

        /// <summary>
        /// Обрабатывает отпускание мыши, завершая перемещение.
        /// </summary>
        private void WaveformCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _audioTrimmer.WaveformCanvas_MouseUp(sender, e);
            _playbackController.WaveformCanvas_MouseUp(sender, e);
        }
    }
}