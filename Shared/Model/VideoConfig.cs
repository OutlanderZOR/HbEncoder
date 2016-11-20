using System.ComponentModel;

namespace Shared.Model
{
    public class VideoConfig
    {
        public string VideoFileName { get; set; }
        public BindingList<Clip> Clips { get; set; } = new BindingList<Clip>();
    }
}
