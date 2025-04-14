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
    /// Класс для обрезки аудиофайлов и управления визуальными элементами обрезки.
    /// </summary>
    public class AudioTrimmer
    {
        private readonly Canvas _canvas;
        private bool _isTrimming;
        private bool _isDragging;
        private Line _trimStartLine;
        private Line _trimEndLine;
        private Rectangle _trimHighlight;
        private double _trimStartX;
        private double _trimEndX;
        private Line _draggedTrimLine;

        /// <summary>
        /// Получает значение, указывающее, включён ли режим обрезки.
        /// </summary>
        public bool IsTrimming => _isTrimming;

        /// <summary>
        /// Инициализирует новый экземпляр обрезчика аудио.
        /// </summary>
        /// <param name="canvas">Канва для отрисовки элементов обрезки.</param>
        public AudioTrimmer(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        /// <summary>
        /// Выполняет обрезку аудиофайла или включает режим обрезки.
        /// </summary>
        /// <param name="window">Главное окно приложения для отображения диалогов.</param>
        /// <param name="currentFilePath">Путь к текущему аудиофайлу.</param>
        /// <param name="playbackController">Контроллер воспроизведения для обновления UI.</param>
        public void TrimAudio(Window window, string currentFilePath, PlaybackController playbackController)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                throw new InvalidOperationException("No audio file loaded.");
            }

            try
            {
                if (!_isTrimming)
                {
                    EnableTrimMode(playbackController);
                }
                else
                {
                    SaveTrimmedAudio(window, currentFilePath);
                    DisableTrimMode(playbackController);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error trimming audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Включает режим обрезки, добавляя линии и выделение на канву.
        /// </summary>
        private void EnableTrimMode(PlaybackController playbackController)
        {
            _isTrimming = true;
            double canvasWidth = _canvas.ActualWidth;
            double canvasHeight = _canvas.ActualHeight;

            _trimStartLine = new Line
            {
                X1 = 0,
                X2 = 0,
                Y1 = 0,
                Y2 = canvasHeight,
                Stroke = Brushes.Yellow,
                StrokeThickness = 4
            };
            _trimEndLine = new Line
            {
                X1 = canvasWidth,
                X2 = canvasWidth,
                Y1 = 0,
                Y2 = canvasHeight,
                Stroke = Brushes.Yellow,
                StrokeThickness = 4
            };
            _trimStartX = 0;
            _trimEndX = canvasWidth;

            _trimHighlight = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)),
                Width = canvasWidth,
                Height = canvasHeight
            };
            Canvas.SetLeft(_trimHighlight, 0);

            _canvas.Children.Add(_trimHighlight);
            _canvas.Children.Add(_trimStartLine);
            _canvas.Children.Add(_trimEndLine);
            playbackController.MovePlaybackLineToTop();
        }

        /// <summary>
        /// Сохраняет обрезанный аудиофайл.
        /// </summary>
        private void SaveTrimmedAudio(Window window, string currentFilePath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "WAV Files|*.wav",
                FileName = "trimmed_" + System.IO.Path.GetFileNameWithoutExtension(currentFilePath) + ".wav"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                double canvasWidth = _canvas.ActualWidth;
                double totalSeconds;

                using (var reader = new AudioFileReader(currentFilePath))
                {
                    totalSeconds = reader.TotalTime.TotalSeconds;
                }

                double startSeconds = (_trimStartX / canvasWidth) * totalSeconds;
                double endSeconds = (_trimEndX / canvasWidth) * totalSeconds;

                using (var reader = new AudioFileReader(currentFilePath))
                {
                    reader.CurrentTime = TimeSpan.FromSeconds(startSeconds);
                    var trimDuration = TimeSpan.FromSeconds(endSeconds - startSeconds);
                    using (var writer = new WaveFileWriter(saveFileDialog.FileName, reader.WaveFormat))
                    {
                        byte[] buffer = new byte[1024];
                        long bytesToRead = (long)(reader.WaveFormat.AverageBytesPerSecond * trimDuration.TotalSeconds);
                        long bytesRead = 0;

                        while (bytesRead < bytesToRead)
                        {
                            int bytes = reader.Read(buffer, 0, Math.Min(buffer.Length, (int)(bytesToRead - bytesRead)));
                            if (bytes == 0) break;
                            writer.Write(buffer, 0, bytes);
                            bytesRead += bytes;
                        }
                    }
                }

                MessageBox.Show("Audio trimmed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Отключает режим обрезки, удаляя элементы с канвы.
        /// </summary>
        private void DisableTrimMode(PlaybackController playbackController)
        {
            _isTrimming = false;
            _canvas.Children.Remove(_trimStartLine);
            _canvas.Children.Remove(_trimEndLine);
            _canvas.Children.Remove(_trimHighlight);
            _trimStartLine = null;
            _trimEndLine = null;
            _trimHighlight = null;
            playbackController.MovePlaybackLineToTop();
        }

        /// <summary>
        /// Сбрасывает состояние обрезки.
        /// </summary>
        public void Reset()
        {
            if (_isTrimming)
            {
                _isTrimming = false;
                _canvas.Children.Remove(_trimStartLine);
                _canvas.Children.Remove(_trimEndLine);
                _canvas.Children.Remove(_trimHighlight);
                _trimStartLine = null;
                _trimEndLine = null;
                _trimHighlight = null;
            }
        }

        /// <summary>
        /// Обрабатывает нажатие мыши на канве для перемещения линий обрезки.
        /// </summary>
        public void WaveformCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isTrimming) return;

            
            Point mousePosition = e.GetPosition(_canvas);
            double clickX = mousePosition.X;

            if (_trimStartLine != null && Math.Abs(clickX - _trimStartLine.X1) < 10)
            {
                _draggedTrimLine = _trimStartLine;
                _isDragging = true;
                _canvas.CaptureMouse();
            }
            else if (_trimEndLine != null && Math.Abs(clickX - _trimEndLine.X1) < 10)
            {
                _draggedTrimLine = _trimEndLine;
                _isDragging = true;
                _canvas.CaptureMouse();
            }
        }

        /// <summary>
        /// Обрабатывает движение мыши для перемещения линий обрезки.
        /// </summary>
        public void WaveformCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedTrimLine != null)
            {
                Point mousePosition = e.GetPosition(_canvas);
                double clickX = Math.Max(0, Math.Min(mousePosition.X, _canvas.ActualWidth));

                _draggedTrimLine.X1 = clickX;
                _draggedTrimLine.X2 = clickX;

                _trimStartX = Math.Min(_trimStartLine.X1, _trimEndLine.X1);
                _trimEndX = Math.Max(_trimStartLine.X1, _trimEndLine.X1);

                UpdateTrimHighlight();
            }
        }

        /// <summary>
        /// Обрабатывает отпускание мыши, завершая перемещение линий обрезки.
        /// </summary>
        public void WaveformCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _draggedTrimLine = null;
                _canvas.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Обновляет область выделения между линиями обрезки.
        /// </summary>
        private void UpdateTrimHighlight()
        {
            if (_trimHighlight == null) return;

            double canvasWidth = _canvas.ActualWidth;
            double canvasHeight = _canvas.ActualHeight;

            Canvas.SetLeft(_trimHighlight, _trimStartX);
            _trimHighlight.Width = _trimEndX - _trimStartX;
            _trimHighlight.Height = canvasHeight;
        }
    }
}