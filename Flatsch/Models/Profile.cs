using System.Windows.Media;

namespace Flatsch.Models
{
    public class Profile
    {
        public string Name { get; set; }
        public Color BackgroundColor { get; set; }
        public bool IsSoundEnabled { get; set; }
        public float Opacity { get; set; }
        public bool ShowFish { get; set; }
        public int ShowWindowTime { get; set; }
        public int FadeInAnimTime { get; set; }
        public int HideWindowTime { get; set; }
        public string NotificationText { get; set; }
        public int NotificationTextFontSize { get; set; }
        public int NotificationTextMarginTop { get; set; }
        public int Screen { get; set; }
        public bool SpanAcrossAllScreens { get; set; }
        public int FadeOutAnimTime { get; set; }
    }
}
