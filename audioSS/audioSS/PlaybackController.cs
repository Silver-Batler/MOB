using NAudio.Wave;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace audioSS
{
    /// <summary>
    /// Класс для управления позицией воспроизведения и обновления UI.
    /// </summary>
    public class PlaybackController
    {
        private readonly Canvas _canvas;
        private readonly TextBlock _audioTimerText;
        private AudioFileReader _audioReader;
        private Line _playbackLine;
        private DispatcherTimer _playbackTimer;
        private bool _isDragging;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера воспроизведения.
        /// </summary>
        /// <param name="canvas">Канва для отрисовки линии воспроизведения.</param>
        /// <param name="audioTimerText">Текстовый блок для отображения времени.</param>
        public PlaybackController(Canvas canvas, TextBlock audioTimerText)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _audioTimerText = audioTimerText ?? throw new ArgumentNullException(nameof(audioTimerText));

            _playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _playbackTimer.Tick += PlaybackTimer_Tick;
        }

        /// <summary>
        /// Обновляет UI каждые 100 мс во время воспроизведения.
        /// </summary>
        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (_audioReader != null && !_isDragging)
            {
                UpdatePlaybackLine();
                UpdateAudioTimer();
            }
        }

        /// <summary>
        /// Обновляет позицию линии воспроизведения.
        /// </summary>
        private void UpdatePlaybackLine()
        {
            if (_audioReader == null || _playbackLine == null) return;

            double canvasWidth = _canvas.ActualWidth;
            double totalSeconds = _audioReader.TotalTime.TotalSeconds;
            double currentSeconds = _audioReader.CurrentTime.TotalSeconds;

            double xPosition = (currentSeconds / totalSeconds) * canvasWidth;

            _playbackLine.X1 = xPosition;
            _playbackLine.X2 = xPosition;
            _playbackLine.Y1 = 0;
            _playbackLine.Y2 = _canvas.ActualHeight;
        }

        /// <summary>
        /// Обновляет текстовый таймер.
        /// </summary>
        private void UpdateAudioTimer()
        {
            if (_audioReader == null) return;

            TimeSpan currentTime = _audioReader.CurrentTime;
            TimeSpan totalTime = _audioReader.TotalTime;

            string currentTimeStr = $"{(int)currentTime.TotalMinutes:D2}:{currentTime.Seconds:D2}";
            string totalTimeStr = $"{(int)totalTime.TotalMinutes:D2}:{totalTime.Seconds:D2}";

            _audioTimerText.Text = $"{currentTimeStr} / {totalTimeStr}";
        }

        /// <summary>
        /// Обрабатывает нажатие мыши на канве для изменения позиции воспроизведения.
        /// </summary>
        public void WaveformCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_audioReader == null) return;

            Point mousePosition = e.GetPosition(_canvas);
            double clickX = mousePosition.X;

            if (_playbackLine != null && Math.Abs(clickX - _playbackLine.X1) < 10)
            {
                _isDragging = true;
                _canvas.CaptureMouse();
                UpdatePlaybackPosition(clickX);
            }
        }

        /// <summary>
        /// Обрабатывает движение мыши для перемещения линии воспроизведения.
        /// </summary>
        public void WaveformCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _audioReader != null)
            {
                Point mousePosition = e.GetPosition(_canvas);
                double clickX = Math.Max(0, Math.Min(mousePosition.X, _canvas.ActualWidth));
                UpdatePlaybackPosition(clickX);
            }
        }

        /// <summary>
        /// Обрабатывает отпускание мыши, завершая перемещение.
        /// </summary>
        public void WaveformCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _canvas.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Обновляет позицию воспроизведения аудио.
        /// </summary>
        private void UpdatePlaybackPosition(double xPosition)
        {
            double canvasWidth = _canvas.ActualWidth;
            double totalSeconds = _audioReader.TotalTime.TotalSeconds;
            double newSeconds = (xPosition / canvasWidth) * totalSeconds;

            _audioReader.CurrentTime = TimeSpan.FromSeconds(newSeconds);
            _playbackLine.X1 = xPosition;
            _playbackLine.X2 = xPosition;
            UpdateAudioTimer();
        }

        /// <summary>
        /// Сбрасывает состояние контроллера и инициализирует новый аудиофайл.
        /// </summary>
        /// <param name="audioReader">Объект AudioFileReader.</param>
        public void Reset(AudioFileReader audioReader)
        {
            _audioReader = audioReader;
            _playbackTimer.Stop();
            _canvas.Children.Remove(_playbackLine);
            _playbackLine = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            _canvas.Children.Add(_playbackLine);
            UpdateUI();
        }

        /// <summary>
        /// Запускает таймер воспроизведения.
        /// </summary>
        public void StartTimer()
        {
            _playbackTimer.Start();
        }

        /// <summary>
        /// Останавливает таймер воспроизведения.
        /// </summary>
        public void StopTimer()
        {
            _playbackTimer.Stop();
        }

        /// <summary>
        /// Обновляет UI (линию воспроизведения и таймер).
        /// </summary>
        public void UpdateUI()
        {
            UpdatePlaybackLine();
            UpdateAudioTimer();
        }

        /// <summary>
        /// Перемещает линию воспроизведения на верхний слой канвы.
        /// </summary>
        public void MovePlaybackLineToTop()
        {
            if (_playbackLine != null)
            {
                _canvas.Children.Remove(_playbackLine);
                _canvas.Children.Add(_playbackLine);
            }
        }
    }
}