using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities
{
    public class ChildChangedEventArgs<TContainer, TChild> : CancelEventArgs
        where TContainer : EntityContainer<TChild>
        where TChild : Entity
    {
        public ChildChangedEventArgs(TContainer sender, ICollection<TChild> changedChildren, bool adding)
        {
            this.ChangedChildren = new ReadOnlyCollection<TChild>(changedChildren.ToList());
            this.Added = adding;

            var pendingChildren = new List<TChild>(sender.Children.ToList());

            if (adding)
                pendingChildren.AddRange(changedChildren);
            else
                pendingChildren.RemoveAll(changedChildren.Contains);

            this.ResultingChildren = new ReadOnlyCollection<TChild>(pendingChildren);
        }

        public IReadOnlyCollection<TChild> ChangedChildren { get; }
        public bool Added { get; }
        public IReadOnlyCollection<TChild> ResultingChildren { get; }
    }

    public abstract class EntityContainer<T> : Entity, ICollection<T> where T : Entity
    {
        private readonly List<T> _children;

        public EntityContainer(IEnumerable<T> children)
        {
            this._children = children?.ToList() ?? new List<T>();
        }

        public IReadOnlyCollection<T> Children => this._children.AsReadOnly();

        public IEnumerator<T> GetEnumerator()
        {
            return this._children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => this._children.Count;

        public event EventHandler<ChildChangedEventArgs<EntityContainer<T>, T>> ChildrenAdded;
        public event EventHandler<ChildChangedEventArgs<EntityContainer<T>, T>> ChildrenRemoved;

        public override void Update(GameTime gameTime)
        {
            this._children.ForEach(c => c.Update(gameTime));
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            foreach (var child in this._children.Where(c => c.Visible))
            {
                child.Draw(graphicsDevice);
            }
        }

        protected virtual void OnChildrenAdded(ChildChangedEventArgs<EntityContainer<T>, T> e)
        {
            ChildrenAdded?.Invoke(this, e);
        }

        protected virtual void OnChildrenRemoved(ChildChangedEventArgs<EntityContainer<T>, T> e)
        {
            ChildrenRemoved?.Invoke(this, e);
        }

        public void AddChildren(IEnumerable<T> children)
        {
            // Limit our list down to just items not already within the list
            // We're just ignoring any items that already children of this container
            var childrenToAdd = children.Except(this._children).ToList();

            if (!childrenToAdd.Any()) return;

            var eventArgs = new ChildChangedEventArgs<EntityContainer<T>, T>(this, childrenToAdd, true);
            OnChildrenAdded(eventArgs);

            if (!eventArgs.Cancel)
            {
                this._children.AddRange(childrenToAdd);

                OnPropertyChanged(nameof(this.Children));
                OnPropertyChanged(nameof(this.Count));
            }
        }

        public void AddChild(T child)
        {
            AddChildren(new[] {child});
        }

        public void RemoveChildren(IEnumerable<T> children)
        {
            /* Limit our list down to just items within the list
               We'll just ignore any items that aren't actual children */
            var childrenToRemove = children.Intersect(this._children).ToList();

            if (!childrenToRemove.Any()) return;

            var eventArgs = new ChildChangedEventArgs<EntityContainer<T>, T>(this, childrenToRemove, false);
            OnChildrenRemoved(eventArgs);

            if (!eventArgs.Cancel)
            {
                this._children.RemoveAll(childrenToRemove.Contains);

                OnPropertyChanged(nameof(this.Children));
                OnPropertyChanged(nameof(this.Count));
            }
        }

        public void RemoveChild(T child)
        {
            RemoveChildren(new[] {child});
        }

        public void ClearChildren()
        {
            RemoveChildren(this._children);
        }

        public bool ContainsChild(T child)
        {
            return this._children.Contains(child);
        }

        #region ICollection Explicit Implementations

        /* To better indicate what is being added or removed, these
           are hidden with an explicit implementation so that we 
           can provide a method with a more intuitive name */

        // See: AddChild()
        void ICollection<T>.Add(T child)
        {
            AddChild(child);
        }

        // See: ClearChildren()
        void ICollection<T>.Clear()
        {
            ClearChildren();
        }

        // See: RemoveChild()
        bool ICollection<T>.Remove(T child)
        {
            var beforeCount = this._children.Count;

            RemoveChild(child);

            return beforeCount == this._children.Count + 1;
        }

        // See: ContainsChild()
        public bool Contains(T child)
        {
            return ContainsChild(child);
        }

        // Not applicable to our current implementation
        public bool IsReadOnly => false;

        // Not applicable to our current implementation
        public void CopyTo(T[] array, int arrayIndex)
        {
            this._children.CopyTo(array, arrayIndex);
        }

        #endregion
    }
}