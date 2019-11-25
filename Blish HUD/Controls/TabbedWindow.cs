using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class TabbedWindow : WindowBase
    {
        private const int TAB_HEIGHT = 52;
        private const int TAB_WIDTH = 104;
        private const int TAB_ICON_SIZE = 32;

        private const int TAB_SECTION_WIDTH = 46;

        private const int WINDOWCONTENT_WIDTH = 1024;
        private const int WINDOWCONTENT_HEIGHT = 700;

        private static readonly Texture2D _textureDefaultBackround;
        private static readonly Texture2D _textureSplitLine;
        private static readonly Texture2D _textureBlackFade;
        private static readonly Texture2D _textureTabActive;

        private static readonly Rectangle StandardTabBounds =
            new Rectangle(TAB_SECTION_WIDTH, 24, TAB_WIDTH, TAB_HEIGHT);

        private readonly Dictionary<WindowTab, Rectangle> _tabRegions = new Dictionary<WindowTab, Rectangle>();

        private int _hoveredTabIndex;

        protected int _selectedTabIndex = -1;
        private List<WindowTab> _tabs = new List<WindowTab>();

        public TabbedWindow()
        {
            var tabWindowTexture = _textureDefaultBackround;
            tabWindowTexture = tabWindowTexture.Duplicate()
                .SetRegion(0, 0, 64, _textureDefaultBackround.Height, Color.Transparent);

            ConstructWindow(tabWindowTexture, new Vector2(25, 33), new Rectangle(0, 0, 1100, 745),
                new Thickness(60, 75, 45, 25), 40);

            this._contentRegion = new Rectangle(TAB_WIDTH / 2, 48, WINDOWCONTENT_WIDTH, WINDOWCONTENT_HEIGHT);
        }

        public int SelectedTabIndex
        {
            get => this._selectedTabIndex;
            set
            {
                if (SetProperty(ref this._selectedTabIndex, value))
                {
                    OnTabChanged(EventArgs.Empty);
                }
            }
        }

        public WindowTab SelectedTab =>
            this._tabs.Count > this._selectedTabIndex ? this._tabs[this._selectedTabIndex] : null;

        private int HoveredTabIndex
        {
            get => this._hoveredTabIndex;
            set => SetProperty(ref this._hoveredTabIndex, value);
        }

        // TODO: Remove public access to _panels - only kept currently as it is used by KillProof.me module (need more robust "Navigate()" call for panel history)
        public Dictionary<WindowTab, Panel> Panels { get; } = new Dictionary<WindowTab, Panel>();

        public event EventHandler<EventArgs> TabChanged;

        protected virtual void OnTabChanged(EventArgs e)
        {
            if (this._visible)
            {
                Content.PlaySoundEffectByName($"audio\\tab-swap-{RandomUtil.GetRandom(1, 5)}");
            }

            this.Subtitle = this.SelectedTab.Name;

            Navigate(this.Panels[this.SelectedTab], false);

            TabChanged?.Invoke(this, e);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            this.HoveredTabIndex = -1;

            base.OnMouseLeft(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var newSet = false;

            if ((this.RelativeMousePosition.X < StandardTabBounds.Right) &&
                (this.RelativeMousePosition.Y > StandardTabBounds.Y))
            {
                var tabList = this._tabRegions.ToList();
                for (var tabIndex = 0; tabIndex < this._tabs.Count; tabIndex++)
                {
                    var tab = this._tabs[tabIndex];
                    if (this._tabRegions[tab].Contains(this.RelativeMousePosition))
                    {
                        this.HoveredTabIndex = tabIndex;
                        newSet = true;
                        this.BasicTooltipText = tab.Name;

                        break;
                    }
                }

                tabList.Clear();
            }

            if (!newSet)
            {
                this.HoveredTabIndex = -1;
                this.BasicTooltipText = null;
            }

            base.OnMouseMoved(e);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if ((this.RelativeMousePosition.X < StandardTabBounds.Right) &&
                (this.RelativeMousePosition.Y > StandardTabBounds.Y))
            {
                var tabList = this._tabs.ToList();
                for (var tabIndex = 0; tabIndex < this._tabs.Count; tabIndex++)
                {
                    var tab = tabList[tabIndex];
                    if (this._tabRegions[tab].Contains(this.RelativeMousePosition))
                    {
                        this.SelectedTabIndex = tabIndex;

                        break;
                    }
                }

                tabList.Clear();
            }

            base.OnLeftMouseButtonPressed(e);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (this._tabs.Count == 0) return;

            var firstTabBounds = TabBoundsFromIndex(0);
            var selectedTabBounds = this._tabRegions[this.SelectedTab];
            var lastTabBounds = TabBoundsFromIndex(this._tabRegions.Count - 1);

            this._layoutTopTabBarBounds = new Rectangle(0, 0, TAB_SECTION_WIDTH, firstTabBounds.Top);
            this._layoutBottomTabBarBounds = new Rectangle(0, lastTabBounds.Bottom, TAB_SECTION_WIDTH,
                this._size.Y - lastTabBounds.Bottom);

            var topSplitHeight = selectedTabBounds.Top - this.ContentRegion.Top;
            var bottomSplitHeight = this.ContentRegion.Bottom - selectedTabBounds.Bottom;

            this._layoutTopSplitLineBounds = new Rectangle(this.ContentRegion.X - _textureSplitLine.Width + 1,
                this.ContentRegion.Y,
                _textureSplitLine.Width,
                topSplitHeight);

            this._layoutTopSplitLineSourceBounds = new Rectangle(0, 0, _textureSplitLine.Width, topSplitHeight);

            this._layoutBottomSplitLineBounds = new Rectangle(this.ContentRegion.X - _textureSplitLine.Width + 1,
                selectedTabBounds.Bottom,
                _textureSplitLine.Width,
                bottomSplitHeight);

            this._layoutBottomSplitLineSourceBounds = new Rectangle(0, _textureSplitLine.Height - bottomSplitHeight,
                _textureSplitLine.Width, bottomSplitHeight);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            // Draw black block for tab bar
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, this._layoutTopTabBarBounds,
                Color.Black);

            // Draw black fade for tab bar
            spriteBatch.DrawOnCtrl(this, _textureBlackFade, this._layoutBottomTabBarBounds);

            // Draw tabs
            var i = 0;
            foreach (var tab in this._tabs)
            {
                var active = i == this.SelectedTabIndex;
                var hovered = i == this.HoveredTabIndex;

                var tabBounds = this._tabRegions[tab];
                var subBounds = new Rectangle(tabBounds.X + tabBounds.Width / 2, tabBounds.Y, TAB_WIDTH / 2,
                    tabBounds.Height);

                if (active)
                {
                    spriteBatch.DrawOnCtrl(this, _textureDefaultBackround,
                        tabBounds,
                        tabBounds.OffsetBy(this._windowBackgroundOrigin.ToPoint()).OffsetBy(-5, -13).Add(0, -35, 0, 0)
                            .Add(tabBounds.Width / 3, 0, -tabBounds.Width / 3, 0),
                        Color.White);

                    spriteBatch.DrawOnCtrl(this, _textureTabActive, tabBounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                        new Rectangle(0, tabBounds.Y, TAB_SECTION_WIDTH, tabBounds.Height), Color.Black);
                }

                spriteBatch.DrawOnCtrl(this, tab.Icon,
                    new Rectangle(TAB_WIDTH / 4 - TAB_ICON_SIZE / 2 + 2,
                        TAB_HEIGHT / 2 - TAB_ICON_SIZE / 2,
                        TAB_ICON_SIZE,
                        TAB_ICON_SIZE).OffsetBy(subBounds.Location),
                    active || hovered
                        ? Color.White
                        : ContentService.Colors.DullColor);

                i++;
            }

            // Draw top of split
            spriteBatch.DrawOnCtrl(this, _textureSplitLine, this._layoutTopSplitLineBounds,
                this._layoutTopSplitLineSourceBounds);

            // Draw bottom of split
            spriteBatch.DrawOnCtrl(this, _textureSplitLine, this._layoutBottomSplitLineBounds,
                this._layoutBottomSplitLineSourceBounds);
        }

        #region Load Static

        static TabbedWindow()
        {
            _textureDefaultBackround = Content.GetTexture("502049");
            _textureSplitLine = Content.GetTexture("605024");
            _textureBlackFade = Content.GetTexture("fade-down-46");
            _textureTabActive = Content.GetTexture("window-tab-active");
        }

        #endregion

        #region Tab Handling

        public WindowTab AddTab(string name, AsyncTexture2D icon, Panel panel, int priority)
        {
            var tab = new WindowTab(name, icon, priority);
            AddTab(tab, panel);
            return tab;
        }

        public WindowTab AddTab(string name, AsyncTexture2D icon, Panel panel)
        {
            var tab = new WindowTab(name, icon);
            AddTab(tab, panel);
            return tab;
        }

        public void AddTab(WindowTab tab, Panel panel)
        {
            if (!this._tabs.Contains(tab))
            {
                var prevTab = this._tabs.Count > 0 ? this._tabs[this.SelectedTabIndex] : tab;

                panel.Visible = false;
                panel.Parent = this;

                this._tabs.Add(tab);
                this._tabRegions.Add(tab, TabBoundsFromIndex(this._tabRegions.Count));
                this.Panels.Add(tab, panel);

                this._tabs = this._tabs.OrderBy(t => t.Priority).ToList();

                for (var i = 0; i < this._tabs.Count; i++)
                {
                    this._tabRegions[this._tabs[i]] = TabBoundsFromIndex(i);
                }

                // Update tab index without making tab switch noise
                if (this._selectedTabIndex == -1)
                {
                    this._subtitle = prevTab.Name;
                    this._selectedTabIndex = this._tabs.IndexOf(prevTab);
                    this.ActivePanel = panel;
                }
                else
                {
                    this._selectedTabIndex = this._tabs.IndexOf(prevTab);
                }

                Invalidate();
            }
        }

        public void RemoveTab(WindowTab tab)
        {
            // TODO: If the last tab is for some reason removed, this will crash the application
            var prevTab = this._tabs.Count > 0 ? this._tabs[this.SelectedTabIndex] : this._tabs[0];

            if (this._tabs.Contains(tab))
            {
                this._tabs.Remove(tab);
                this._tabRegions.Remove(tab);
                this.Panels.Remove(tab);
            }

            this._tabs = this._tabs.OrderBy(t => t.Priority).ToList();

            for (var tabIndex = 0; tabIndex < this._tabRegions.Count; tabIndex++)
            {
                var curTab = this._tabs[tabIndex];
                this._tabRegions[curTab] = TabBoundsFromIndex(tabIndex);
            }

            if (this._tabs.Contains(prevTab))
            {
                this._selectedTabIndex = this._tabs.IndexOf(prevTab);
            }

            Invalidate();
        }

        private Rectangle TabBoundsFromIndex(int index)
        {
            return StandardTabBounds.OffsetBy(-TAB_WIDTH, this.ContentRegion.Y + index * TAB_HEIGHT);
        }

        #endregion

        #region Calculated Layout

        private Rectangle _layoutTopTabBarBounds;
        private Rectangle _layoutBottomTabBarBounds;

        private Rectangle _layoutTopSplitLineBounds;
        private Rectangle _layoutBottomSplitLineBounds;

        private Rectangle _layoutTopSplitLineSourceBounds;
        private Rectangle _layoutBottomSplitLineSourceBounds;

        #endregion
    }
}