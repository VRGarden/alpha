namespace VRGardenAlpha.Models.Options
{
    public class GardenOptions
    {
        public string? Discord { get; set; }
        public string? BotPassword { get; set; }
        public string? MasterPassword { get; set; }
        public bool RequiresCaptcha { get; set; } = false;
    }
}
