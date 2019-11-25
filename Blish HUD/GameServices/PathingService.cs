﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Blish_HUD.Entities;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Blish_HUD.PersistentStore;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class PathingService : GameService
    {
        public const string MARKER_DIRECTORY = "markers";

        private const string PATHING_STORENAME = "Pathing";

        private readonly ConcurrentQueue<IPathable<Entity>> _queuedAddPathables =
            new ConcurrentQueue<IPathable<Entity>>();

        private readonly ConcurrentQueue<IPathable<Entity>> _queuedRemovePathables =
            new ConcurrentQueue<IPathable<Entity>>();

        private Store _pathingStore;

        public List<IPathable<Entity>> Pathables { get; set; } = new List<IPathable<Entity>>();

        public SynchronizedCollection<PathableResourceManager> PackManagers { get; set; } =
            new SynchronizedCollection<PathableResourceManager>();

        public Store PathingStore =>
            this._pathingStore ?? (this._pathingStore = Store.RegisterStore(PATHING_STORENAME));

        public event EventHandler<EventArgs> NewMapLoaded;

        protected override void Initialize()
        {
            // Subscribe to map changes so that we can hide or show markers for the new map
            Player.MapIdChanged += PlayerMapIdChanged;
        }

        protected override void Load()
        {
            /* NOOP */
        }

        private void ProcessPathableState(IPathable<Entity> pathable)
        {
            if ((pathable.MapId == Player.MapId) || (pathable.MapId == -1))
            {
                //pathable.Active = true;
                Graphics.World.Entities.Add(pathable.ManagedEntity);
            }
            else if (Graphics.World.Entities.Contains(pathable.ManagedEntity))
            {
                //pathable.Active = false;
                Graphics.World.Entities.Remove(pathable.ManagedEntity);
            }
        }

        private void ProcessAddedPathable(IPathable<Entity> pathable)
        {
            Graphics.World.Entities.Add(pathable.ManagedEntity);
            this.Pathables.Add(pathable);
        }

        private void ProcessRemovedPathable(IPathable<Entity> pathable)
        {
            Graphics.World.Entities.Remove(pathable.ManagedEntity);
            this.Pathables.Remove(pathable);
        }

        private void PlayerMapIdChanged(object sender, EventArgs e)
        {
            NewMapLoaded?.Invoke(this, EventArgs.Empty);

            foreach (var packContext in this.PackManagers)
                packContext.RunTextureDisposal();
        }

        public void RegisterPathable(IPathable<Entity> pathable)
        {
            if (pathable == null) return;

            this._queuedAddPathables.Enqueue(pathable);
        }

        public void UnregisterPathable(IPathable<Entity> pathable)
        {
            if (pathable == null) return;

            this._queuedRemovePathables.Enqueue(pathable);
        }

        public void RegisterPathableResourceManager(PathableResourceManager pathableContext)
        {
            if (pathableContext == null) return;

            this.PackManagers.Add(pathableContext);
        }

        public void UnregisterPathableResourceManager(PathableResourceManager pathableContext)
        {
            this.PackManagers.Remove(pathableContext);
        }

        protected override void Update(GameTime gameTime)
        {
            while (this._queuedAddPathables.TryDequeue(out var queuedPathable))
            {
                ProcessAddedPathable(queuedPathable);
            }

            while (this._queuedRemovePathables.TryDequeue(out var queuedRemovedPathable))
            {
                ProcessRemovedPathable(queuedRemovedPathable);
            }

            foreach (var pathable in this.Pathables)
            {
                if (!pathable.Active) continue;

                pathable.Update(gameTime);
            }
        }

        protected override void Unload()
        {
            /* NOOP */
        }
    }
}