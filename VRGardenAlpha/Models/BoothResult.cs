namespace VRGardenAlpha.Models
{
    public class BoothResult
    {
        public string? Name { get; set; }
        public BoothShop? Shop { get; set; }
        public BoothImage[] Images { get; set; } = Array.Empty<BoothImage>();
    }

    public class BoothShop
    {
        public string? Name { get; set; }
    }

    public class BoothImage
    {
        public string? Original { get; set; }
        public string? Resized { get; set; }
    }
}
