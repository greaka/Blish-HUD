using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls
{
    public enum ControlFlowDirection
    {
        LeftToRight,
        TopToBottom
    }

    public class FlowPanel : Panel
    {
        protected Vector2 _controlPadding = Vector2.Zero;

        protected ControlFlowDirection _flowDirection = ControlFlowDirection.LeftToRight;

        protected bool _padLeftBeforeControl;

        protected bool _padTopBeforeControl;

        public Vector2 ControlPadding
        {
            get => this._controlPadding;
            set => SetProperty(ref this._controlPadding, value, true);
        }

        public bool PadLeftBeforeControl
        {
            get => this._padLeftBeforeControl;
            set => SetProperty(ref this._padLeftBeforeControl, value, true);
        }

        public bool PadTopBeforeControl
        {
            get => this._padTopBeforeControl;
            set => SetProperty(ref this._padTopBeforeControl, value, true);
        }

        public ControlFlowDirection FlowDirection
        {
            get => this._flowDirection;
            set => SetProperty(ref this._flowDirection, value, true);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);
            OnChildrenChanged(e);

            e.ChangedChild.Resized += ChangedChildOnResized;
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            base.OnChildRemoved(e);
            OnChildrenChanged(e);

            e.ChangedChild.Resized -= ChangedChildOnResized;
        }

        private void ChangedChildOnResized(object sender, ResizedEventArgs e)
        {
            ReflowChildLayout(this._children);
        }

        private void OnChildrenChanged(ChildChangedEventArgs e)
        {
            ReflowChildLayout(e.ResultingChildren);
        }

        public override void RecalculateLayout()
        {
            ReflowChildLayout(this._children);

            base.RecalculateLayout();
        }

        public void FilterChildren<TControl>(Func<TControl, bool> filter) where TControl : Control
        {
            this._children.Cast<TControl>().ToList().ForEach(tc => tc.Visible = filter(tc));
            ReflowChildLayout(this._children);
        }

        public void SortChildren<TControl>(Comparison<TControl> comparison) where TControl : Control
        {
            var tempChildren = this._children.Cast<TControl>().ToList();
            tempChildren.Sort(comparison);

            this._children = tempChildren.Cast<Control>().ToList();

            ReflowChildLayout(this._children);
        }

        private void ReflowChildLayoutLeftToRight(List<Control> allChildren)
        {
            var nextBottom = this._padTopBeforeControl ? this._controlPadding.Y : 0;
            var currentBottom = this._padTopBeforeControl ? this._controlPadding.Y : 0;
            var lastRight = this._padLeftBeforeControl ? this._controlPadding.X : 0;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next line
                if (child.Width >= this.Width - lastRight)
                {
                    // TODO: Consider a more graceful alternative (like just stick it on its own line)
                    // Prevent stack overflow
                    if (child.Width > this.ContentRegion.Width)
                        throw new Exception("Control is too large to flow in FlowPanel");

                    currentBottom = nextBottom + this._controlPadding.Y;
                    lastRight = this._padLeftBeforeControl ? this._controlPadding.X : 0;
                }

                child.Location = new Point((int) lastRight, (int) currentBottom);

                lastRight = child.Right + this._controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(List<Control> allChildren)
        {
            // TODO: Implement FlowPanel FlowDirection.TopToBottom
        }

        private void ReflowChildLayout(List<Control> allChildren)
        {
            if (this.FlowDirection == ControlFlowDirection.LeftToRight)
            {
                ReflowChildLayoutLeftToRight(allChildren.Where(c => c.GetType() != typeof(Scrollbar)).ToList());
            }
            else
            {
                ReflowChildLayoutTopToBottom(allChildren.Where(c => c.GetType() != typeof(Scrollbar)).ToList());
            }
        }
    }
}