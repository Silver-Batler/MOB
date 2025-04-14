using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
#pragma warning disable CA1416 // Проверка совместимости платформы
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Введите путь к изображению (поддерживаются JPG, PNG):");
        string inputPath = Console.ReadLine()?.Trim();

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("Файл не найден!");
            return;
        }

        string extension = Path.GetExtension(inputPath).ToLower();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            Console.WriteLine("Неподдерживаемый формат! Используйте JPG или PNG.");
            return;
        }

        try
        {
            using (Bitmap originalImage = new Bitmap(inputPath))
            {
                string outputDir = Path.Combine(Path.GetDirectoryName(inputPath), "Processed");
                Directory.CreateDirectory(outputDir);

                while (true)
                {
                    Console.WriteLine("\nВыберите действие:");
                    Console.WriteLine("1. Изменить размер");
                    Console.WriteLine("2. Превратить в черно-белое");
                    Console.WriteLine("3. Повернуть изображение");
                    Console.WriteLine("4. Добавить водяной знак");
                    Console.WriteLine("5. Выход");

                    string choice = Console.ReadLine();

                    Console.WriteLine("Выберите формат сохранения (1 - JPG, 2 - PNG):");
                    string formatChoice = Console.ReadLine();
                    ImageFormat saveFormat = formatChoice == "2" ? ImageFormat.Png : ImageFormat.Jpeg;
                    string saveExtension = formatChoice == "2" ? "png" : "jpg";

                    switch (choice)
                    {
                        case "1":
                            ResizeImage(originalImage, outputDir, saveFormat, saveExtension);
                            break;
                        case "2":
                            ConvertToGrayscale(originalImage, outputDir, saveFormat, saveExtension);
                            break;
                        case "3":
                            RotateImage(originalImage, outputDir, saveFormat, saveExtension);
                            break;
                        case "4":
                            AddWatermark(originalImage, outputDir, saveFormat, saveExtension);
                            break;
                        case "5":
                            Console.WriteLine("Программа завершена.");
                            return;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void ResizeImage(Bitmap original, string outputDir, ImageFormat format, string extension)
    {
        Console.WriteLine("Введите ширину (пиксели):");
        if (!int.TryParse(Console.ReadLine(), out int width) || width <= 0)
        {
            Console.WriteLine("Неверная ширина!");
            return;
        }

        Console.WriteLine("Введите высоту (пиксели):");
        if (!int.TryParse(Console.ReadLine(), out int height) || height <= 0)
        {
            Console.WriteLine("Неверная высота!");
            return;
        }

        using (Bitmap resized = new Bitmap(width, height))
        using (Graphics g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(original, 0, 0, width, height);
            string outputPath = Path.Combine(outputDir, $"resized_{DateTime.Now.Ticks}.{extension}");
            resized.Save(outputPath, format);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }

    static void ConvertToGrayscale(Bitmap original, string outputDir, ImageFormat format, string extension)
    {
        using (Bitmap grayscale = new Bitmap(original.Width, original.Height))
        {
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color pixelColor = original.GetPixel(x, y);
                    int grayValue = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                    grayscale.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            string outputPath = Path.Combine(outputDir, $"grayscale_{DateTime.Now.Ticks}.{extension}");
            grayscale.Save(outputPath, format);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }

    static void RotateImage(Bitmap original, string outputDir, ImageFormat format, string extension)
    {
        Console.WriteLine("Введите угол поворота (90, 180, 270):");
        if (!int.TryParse(Console.ReadLine(), out int angle) || (angle != 90 && angle != 180 && angle != 270))
        {
            Console.WriteLine("Неверный угол! Используйте 90, 180 или 270.");
            return;
        }


        RotateFlipType rotation = angle switch
        {
            90 => RotateFlipType.Rotate90FlipNone,
            180 => RotateFlipType.Rotate180FlipNone,
            270 => RotateFlipType.Rotate270FlipNone,
            _ => RotateFlipType.RotateNoneFlipNone
        };


        using (Bitmap rotated = new Bitmap(original))
        {
            rotated.RotateFlip(rotation);
            string outputPath = Path.Combine(outputDir, $"rotated_{DateTime.Now.Ticks}.{extension}");
            rotated.Save(outputPath, format);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }

    static void AddWatermark(Bitmap original, string outputDir, ImageFormat format, string extension)
    {
        Console.WriteLine("Введите текст водяного знака:");
        string watermarkText = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(watermarkText))
        {
            Console.WriteLine("Текст не может быть пустым!");
            return;
        }

        using (Bitmap watermarked = new Bitmap(original))
        using (Graphics g = Graphics.FromImage(watermarked))
        {
            Font font = new Font("Arial", 20, FontStyle.Bold);
            Brush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
            PointF point = new PointF(10, 10);
            g.DrawString(watermarkText, font, brush, point);

            string outputPath = Path.Combine(outputDir, $"watermarked_{DateTime.Now.Ticks}.{extension}");
            watermarked.Save(outputPath, format);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }
#pragma warning restore CA1416 // Проверка совместимости платформы
}