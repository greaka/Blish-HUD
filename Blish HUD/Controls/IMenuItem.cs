using System;
using System.Collections.Generic;

namespace Blish_HUD.Controls
{
    public enum MenuItemType
    {
        Root,
        Item
    }

    public interface IMenuItem
    {
        int MenuItemHeight { get; set; }

        bool Selected { get; }

        MenuItem SelectedMenuItem { get; }

        bool ShouldShift { get; set; }

        event EventHandler<ControlActivatedEventArgs> ItemSelected;

        void Select();

        void Select(MenuItem menuItem);

        void Select(MenuItem menuItem, List<IMenuItem> itemPath);

        void Deselect();
    }
}