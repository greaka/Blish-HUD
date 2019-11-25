using Blish_HUD.Content;

namespace Blish_HUD.Controls
{
    public class WindowTab
    {
        public WindowTab(string name, AsyncTexture2D icon) : this(name, icon, name.GetHashCode())
        {
            /* NOOP */
        }

        public WindowTab(string name, AsyncTexture2D icon, int priority)
        {
            this.Name = name;
            this.Icon = icon;
            this.Priority = priority;
        }

        public string Name { get; set; }
        public AsyncTexture2D Icon { get; set; }
        public int Priority { get; set; }
    }
}