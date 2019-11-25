using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Common.UI.Views
{
    public class RepeatedView<TModel> : View<IPresenter<RepeatedView<TModel>>>
    {
        private readonly FlowPanel _categoryFlowPanel = new FlowPanel();

        private readonly Dictionary<IView, ViewContainer> _viewContainers = new Dictionary<IView, ViewContainer>();

        public RepeatedView()
        {
            this.Views.CollectionChanged += ViewsOnCollectionChanged;
        }

        public ObservableCollection<IView> Views { get; } = new ObservableCollection<IView>();

        public int ViewHeight { get; set; } = 325;

        public ControlFlowDirection FlowDirection
        {
            get => this._categoryFlowPanel.FlowDirection;
            set => this._categoryFlowPanel.FlowDirection = value;
        }

        private void ViewsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    HandleAddViews(e.NewItems.Cast<IView>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    HandleRemoveViews(e.OldItems.Cast<IView>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    // TODO: Implement replace collection changed
                    break;

                case NotifyCollectionChangedAction.Move:
                    // TODO: Implement move collection changed
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this._categoryFlowPanel.ClearChildren();
                    this._viewContainers.Clear();
                    break;
            }
        }

        private void ReflowViews()
        {
            var lastBottom = 0;

            foreach (var view in this.Views)
            {
                var viewContainter = this._viewContainers[view];

                viewContainter.Location = new Point(0, lastBottom);

                lastBottom = viewContainter.Bottom;
            }
        }

        private void HandleAddViews(IEnumerable<IView> views)
        {
            foreach (var view in views)
            {
                var viewContainer = new ViewContainer
                {
                    Size = new Point(this._categoryFlowPanel.Width, this.ViewHeight),
                    Parent = this._categoryFlowPanel
                };

                viewContainer.Show(view);

                this._viewContainers.Add(view, viewContainer);
            }

            ReflowViews();
        }

        private void HandleRemoveViews(IEnumerable<IView> views)
        {
            foreach (var view in views)
            {
                if (this._viewContainers.ContainsKey(view))
                {
                    this._viewContainers[view].Dispose();
                    this._viewContainers.Remove(view);
                }
            }

            ReflowViews();
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel)
        {
            this._categoryFlowPanel.Size = buildPanel.ContentRegion.Size;
            this._categoryFlowPanel.Parent = buildPanel;
        }
    }
}