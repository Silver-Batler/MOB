using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Введите путь к изображению:");
        string inputPath = Console.ReadLine();

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("Файл не найден!");
            return;
        }

        try
        {
            // Загружаем исходное изображение
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

                    switch (choice)
                    {
                        case "1":
                            ResizeImage(originalImage, outputDir);
                            break;
                        case "2":
                            ConvertToGrayscale(originalImage, outputDir);
                            break;
                        case "3":
                            RotateImage(originalImage, outputDir);
                            break;
                        case "4":
                            AddWatermark(originalImage, outputDir);
                            break;
                        case "5":
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

    static void ResizeImage(Bitmap original, string outputDir)
    {
        Console.WriteLine("Введите ширину (пиксели):");
        if (!int.TryParse(Console.ReadLine(), out int width))
        {
            Console.WriteLine("Неверный формат!");
            return;
        }

        Console.WriteLine("Введите высоту (пиксели):");
        if (!int.TryParse(Console.ReadLine(), out int height))
        {
            Console.WriteLine("Неверный формат!");
            return;
        }

        using (Bitmap resized = new Bitmap(width, height))
        using (Graphics g = Graphics.FromImage(resized))
        {
            g.DrawImage(original, 0, 0, width, height);
            string outputPath = Path.Combine(outputDir, $"resized_{DateTime.Now.Ticks}.jpg");
            resized.Save(outputPath, ImageFormat.Jpeg);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }

    static void ConvertToGrayscale(Bitmap original, string outputDir)
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

            string outputPath = Path.Combine(outputDir, $"grayscale_{DateTime.Now.Ticks}.jpg");
            grayscale.Save(outputPath, ImageFormat.Jpeg);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }

    static void RotateImage(Bitmap original, string outputDir)
    {
        Console.WriteLine("Введите угол поворота (90, 180, 270):");
        if (!int.TryParse(Console.ReadLine(), out int angle) || (angle != 90 && angle != 180 && angle != 270))
        {
            Console.WriteLine("Неверный угол!");
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
            string outputPath = Path.Combine(outputDir, $"rotated_{DateTime.Now.Ticks}.jpg");
            rotated.Save(outputPath, ImageFormat.Jpeg);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }

    static void AddWatermark(Bitmap original, string outputDir)
    {
        Console.WriteLine("Введите текст водяного знака:");
        string watermarkText = Console.ReadLine();

        using (Bitmap watermarked = new Bitmap(original))
        using (Graphics g = Graphics.FromImage(watermarked))
        {
            Font font = new Font("Arial", 20);
            Brush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
            PointF point = new PointF(10, 10);
            g.DrawString(watermarkText, font, brush, point);

            string outputPath = Path.Combine(outputDir, $"watermarked_{DateTime.Now.Ticks}.jpg");
            watermarked.Save(outputPath, ImageFormat.Jpeg);
            Console.WriteLine($"Изображение сохранено: {outputPath}");
        }
    }
}