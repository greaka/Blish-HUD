using System;

namespace Blish_HUD.Controls
{
    public static class CheckableReference
    {
    }

    public interface ICheckable
    {
        bool Checked { get; set; }

        event EventHandler<CheckChangedEvent> CheckedChanged;
    }

    public class CheckChangedEvent : EventArgs
    {
        public CheckChangedEvent(bool @checked)
        {
            this.Checked = @checked;
        }

        public bool Checked { get; }
    }
}