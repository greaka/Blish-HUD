namespace Blish_HUD.Input
{
    internal class MouseEvent
    {
        public MouseEvent(MouseHook.MouseMessages message, MouseHook.MSLLHOOKSTRUCT hookdetails)
        {
            this.EventMessage = message;
            this.EventDetails = hookdetails;
        }

        public MouseHook.MouseMessages EventMessage { get; }
        public MouseHook.MSLLHOOKSTRUCT EventDetails { get; }
    }
}