using System;
using System.IO;
using NAudio.Wave;
using NAudio.Lame;

namespace AudioProcessorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\nМеню обработки аудио:");
                Console.WriteLine("1. Обрезать аудио");
                Console.WriteLine("2. Изменить громкость");
                Console.WriteLine("3. Получить информацию об аудио");
                Console.WriteLine("4. Выход");
                Console.Write("Выберите опцию (1-4): ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            TrimAudio();
                            break;
                        case "2":
                            ChangeVolume();
                            break;
                        case "3":
                            GetAudioInfo();
                            break;
                        case "4":
                            return;
                        default:
                            Console.WriteLine("Неверная опция. Попробуйте снова.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        static void TrimAudio()
        {
            Console.Write("Введите путь к входному аудиофайлу (MP3 или WAV): ");
            string inputPath = Console.ReadLine();
            Console.Write("Введите путь для выходного аудиофайла: ");
            string outputPath = Console.ReadLine();

            Console.Write("Введите время начала (в секундах): ");
            if (!double.TryParse(Console.ReadLine(), out double startTime))
            {
                Console.WriteLine("Неверное время начала!");
                return;
            }
            Console.Write("Введите длительность (в секундах): ");
            if (!double.TryParse(Console.ReadLine(), out double duration))
            {
                Console.WriteLine("Неверная длительность!");
                return;
            }

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Входной файл не существует!");
                return;
            }

            bool isMp3 = Path.GetExtension(inputPath).Equals(".mp3", StringComparison.OrdinalIgnoreCase);
            if (isMp3 && !Path.GetExtension(outputPath).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                outputPath = Path.ChangeExtension(outputPath, ".mp3");
                Console.WriteLine($"Выходной файл будет сохранен как: {outputPath}");
            }
            else if (!isMp3 && !Path.GetExtension(outputPath).Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                outputPath = Path.ChangeExtension(outputPath, ".wav");
                Console.WriteLine($"Выходной файл будет сохранен как: {outputPath}");
            }

            using (var reader = new AudioFileReader(inputPath))
            {
                if (isMp3)
                {
                    using (var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, 128))
                    {
                        reader.CurrentTime = TimeSpan.FromSeconds(startTime);
                        var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
                        long bytesToRead = (long)(duration * reader.WaveFormat.AverageBytesPerSecond);
                        long bytesRead;

                        while (bytesToRead > 0 && (bytesRead = reader.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesToRead))) > 0)
                        {
                            writer.Write(buffer, 0, (int)bytesRead);
                            bytesToRead -= bytesRead;
                        }
                    }
                }
                else
                {
                    using (var writer = new WaveFileWriter(outputPath, reader.WaveFormat))
                    {
                        reader.CurrentTime = TimeSpan.FromSeconds(startTime);
                        var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                        int samplesToRead = (int)(duration * reader.WaveFormat.SampleRate);
                        int samplesRead;

                        while ((samplesRead = reader.Read(buffer, 0, Math.Min(buffer.Length, samplesToRead))) > 0)
                        {
                            writer.WriteSamples(buffer, 0, samplesRead);
                            samplesToRead -= samplesRead;
                            if (samplesToRead <= 0) break;
                        }
                    }
                }
            }
            Console.WriteLine("Аудио успешно обрезано!");
        }

        static void ChangeVolume()
        {
            Console.Write("Введите путь к входному аудиофайлу (MP3 или WAV): ");
            string inputPath = Console.ReadLine();
            Console.Write("Введите путь для выходного аудиофайла: ");
            string outputPath = Console.ReadLine();

            Console.Write("Введите коэффициент громкости (от 0.1 до 2.0): ");
            if (!float.TryParse(Console.ReadLine(), out float volume) || volume < 0.1f || volume > 2.0f)
            {
                Console.WriteLine("Неверный коэффициент громкости!");
                return;
            }

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Входной файл не существует!");
                return;
            }

            bool isMp3 = Path.GetExtension(inputPath).Equals(".mp3", StringComparison.OrdinalIgnoreCase);
            if (isMp3 && !Path.GetExtension(outputPath).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                outputPath = Path.ChangeExtension(outputPath, ".mp3");
                Console.WriteLine($"Выходной файл будет сохранен как: {outputPath}");
            }
            else if (!isMp3 && !Path.GetExtension(outputPath).Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                outputPath = Path.ChangeExtension(outputPath, ".wav");
                Console.WriteLine($"Выходной файл будет сохранен как: {outputPath}");
            }

            using (var reader = new AudioFileReader(inputPath))
            {
                // Создаем временный WAV-файл для обработки громкости
                string tempWavPath = Path.GetTempFileName();
                try
                {
                    using (var tempWriter = new WaveFileWriter(tempWavPath, reader.WaveFormat))
                    {
                        var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                        int samplesRead;

                        while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < samplesRead; i++)
                            {
                                buffer[i] *= volume;
                            }
                            tempWriter.WriteSamples(buffer, 0, samplesRead);
                        }
                    }

                    // Если нужен MP3, конвертируем временный WAV в MP3
                    if (isMp3)
                    {
                        using (var tempReader = new WaveFileReader(tempWavPath))
                        using (var mp3Writer = new LameMP3FileWriter(outputPath, tempReader.WaveFormat, 128))
                        {
                            tempReader.CopyTo(mp3Writer);
                        }
                    }
                    else
                    {
                        File.Move(tempWavPath, outputPath);
                    }
                }
                finally
                {
                    // Удаляем временный файл, если он остался
                    if (File.Exists(tempWavPath))
                    {
                        File.Delete(tempWavPath);
                    }
                }
            }
            Console.WriteLine("Громкость успешно изменена!");
        }

        static void GetAudioInfo()
        {
            Console.Write("Введите путь к аудиофайлу (MP3 или WAV): ");
            string filePath = Console.ReadLine();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не существует!");
                return;
            }

            using (var reader = new AudioFileReader(filePath))
            {
                Console.WriteLine("\nИнформация об аудио:");
                Console.WriteLine($"Формат: {reader.WaveFormat.Encoding}");
                Console.WriteLine($"Частота дискретизации: {reader.WaveFormat.SampleRate} Гц");
                Console.WriteLine($"Каналы: {reader.WaveFormat.Channels}");
                Console.WriteLine($"Бит на сэмпл: {reader.WaveFormat.BitsPerSample}");
                Console.WriteLine($"Длительность: {reader.TotalTime}");
                Console.WriteLine($"Размер файла: {new FileInfo(filePath).Length / 1024.0:F2} КБ");
            }
        }
    }
}