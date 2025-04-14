using NAudio.Wave;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace audioSS
{
    /// <summary>
    /// Класс для отрисовки волновой формы аудиофайла на канве.
    /// </summary>
    public class WaveformRenderer
    {
        private readonly Canvas _canvas;

        /// <summary>
        /// Инициализирует новый экземпляр рендера волновой формы.
        /// </summary>
        /// <param name="canvas">Канва для отрисовки.</param>
        public WaveformRenderer(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        /// <summary>
        /// Рисует волновую форму аудиофайла на канве.
        /// </summary>
        /// <param name="audioReader">Объект AudioFileReader для чтения аудиоданных.</param>
        public void DrawWaveform(AudioFileReader audioReader)
        {
            if (audioReader == null) return;

            _canvas.Children.Clear();

            try
            {
                audioReader.Position = 0;

                double canvasWidth = _canvas.ActualWidth;
                double canvasHeight = _canvas.ActualHeight;
                int samplesToShow = 10000;
                float[] sampleBuffer = new float[samplesToShow];
                int samplesRead = 0;

                using (var reader = new AudioFileReader(audioReader.FileName))
                {
                    float[] fullBuffer = new float[reader.Length / sizeof(float)];
                    samplesRead = reader.Read(fullBuffer, 0, fullBuffer.Length);

                    for (int i = 0; i < samplesToShow && i < samplesRead; i++)
                    {
                        int index = (int)((float)i / samplesToShow * samplesRead);
                        sampleBuffer[i] = fullBuffer[index];
                    }
                }

                for (int i = 0; i < samplesToShow - 1; i++)
                {
                    double x1 = (double)i / (samplesToShow - 1) * canvasWidth;
                    double x2 = (double)(i + 1) / (samplesToShow - 1) * canvasWidth;

                    double y1 = (sampleBuffer[i] * (canvasHeight / 2)) + (canvasHeight / 2);
                    double y2 = (sampleBuffer[i + 1] * (canvasHeight / 2)) + (canvasHeight / 2);

                    Line line = new Line
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        Stroke = new SolidColorBrush(Color.FromRgb(48, 32, 131)),
                        StrokeThickness = 0.4
                    };

                    _canvas.Children.Add(line);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error drawing waveform: {ex.Message}");
            }
        }
    }
}