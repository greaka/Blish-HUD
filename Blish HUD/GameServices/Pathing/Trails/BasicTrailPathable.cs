using System;
using Blish_HUD.Pathing.Entities;

namespace Blish_HUD.Pathing.Trails
{
    public class BasicTrailPathable : ManagedPathable<Trail>
    {
        private bool _active;

        private float _scale = 1.0f;

        public BasicTrailPathable(Trail managedEntity) : base(managedEntity)
        {
        }

        public override float Scale
        {
            get => this._scale;
            set
            {
                if (SetProperty(ref this._scale, value)) UpdateBounds();
            }
        }

        public override bool Active
        {
            get => this._active;
            set => SetProperty(ref this._active, value);
        }

        private void UpdateBounds()
        {
            Console.WriteLine($"{nameof(UpdateBounds)} was called despite it not being implemented yet!");
        }
    }
}