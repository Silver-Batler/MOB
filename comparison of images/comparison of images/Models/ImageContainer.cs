namespace comparison_of_images.Models
{
    public class ImageContainer
    {
        public int Index { get; set; }
        public string FileName { get; set; }
        public string Dimensions { get; set; }
        public bool IsLoaded => !string.IsNullOrEmpty(FileName);
    }
}