using NAudio.Wave;
using System;

namespace audioSS
{
    /// <summary>
    /// Класс для управления воспроизведением аудиофайлов.
    /// </summary>
    public class AudioPlayer
    {
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioFileReader;
        private string _currentFilePath;

        /// <summary>
        /// Получает путь к текущему аудиофайлу.
        /// </summary>
        public string CurrentFilePath => _currentFilePath;

        /// <summary>
        /// Загружает аудиофайл для воспроизведения.
        /// </summary>
        /// <param name="filePath">Путь к аудиофайлу.</param>
        public void LoadFile(string filePath)
        {
            _currentFilePath = filePath;
            _audioFileReader?.Dispose();
            _waveOut?.Dispose();
            _audioFileReader = new AudioFileReader(filePath);
        }

        /// <summary>
        /// Воспроизводит аудиофайл.
        /// </summary>
        /// <param name="playbackController">Контроллер воспроизведения для обновления UI.</param>
        public void Play(PlaybackController playbackController)
        {
            if (_audioFileReader == null)
            {
                throw new InvalidOperationException("No audio file loaded.");
            }

            try
            {
                if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
                {
                    _waveOut.Stop();
                    playbackController.StopTimer();
                }
                else if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Paused)
                {
                    _waveOut.Play();
                    playbackController.StartTimer();
                    return;
                }

                _waveOut = new WaveOutEvent();
                _waveOut.Init(_audioFileReader);
                _waveOut.Play();
                playbackController.StartTimer();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error playing audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Приостанавливает воспроизведение.
        /// </summary>
        public void Pause()
        {
            if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                try
                {
                    _waveOut.Pause();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error pausing audio: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Останавливает воспроизведение и сбрасывает позицию.
        /// </summary>
        /// <param name="playbackController">Контроллер воспроизведения для обновления UI.</param>
        public void Stop(PlaybackController playbackController)
        {
            if (_waveOut != null && (_waveOut.PlaybackState == PlaybackState.Playing || _waveOut.PlaybackState == PlaybackState.Paused))
            {
                _waveOut.Stop();
                _audioFileReader.Position = 0;
                playbackController.StopTimer();
                playbackController.UpdateUI();
            }
        }

        /// <summary>
        /// Устанавливает громкость воспроизведения.
        /// </summary>
        /// <param name="volume">Значение громкости (0.0 - 1.0).</param>
        public void SetVolume(float volume)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.Volume = volume;
            }
        }

        /// <summary>
        /// Получает текущий объект AudioFileReader.
        /// </summary>
        /// <returns>Объект AudioFileReader или null, если файл не загружен.</returns>
        public AudioFileReader GetAudioReader()
        {
            return _audioFileReader;
        }

        /// <summary>
        /// Освобождает ресурсы аудиоплеера.
        /// </summary>
        public void Dispose()
        {
            _waveOut?.Dispose();
            _audioFileReader?.Dispose();
        }
    }
}