using System;
using System.Collections.Generic;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;
using Glide;

namespace Blish_HUD.Pathing.Behaviors
{
    [PathingBehavior("bounce")]
    public class Bounce<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        private Tween _bounceAnimation;

        private float _bounceDelay;
        private float _bounceDistance = 2f;
        private float _bounceDuration = 1f;
        private float _bounceHeight = 2f;

        private BehaviorWhen _bounceWhen = BehaviorWhen.Always;

        private readonly float _originalVerticalOffset;

        public Bounce(TPathable managedPathable) : base(managedPathable)
        {
            this._originalVerticalOffset = managedPathable.ManagedEntity.VerticalOffset;
        }

        public BehaviorWhen BounceWhen
        {
            get => this._bounceWhen;
            set
            {
                this._bounceWhen = value;
                UpdateBounceState();
            }
        }

        public float BounceHeight
        {
            get => this._bounceHeight;
            set
            {
                this._bounceHeight = value;
                UpdateBounceState();
            }
        }

        public float BounceDelay
        {
            get => this._bounceDelay;
            set
            {
                this._bounceDelay = value;
                UpdateBounceState();
            }
        }

        public float BounceDuration
        {
            get => this._bounceDuration;
            set
            {
                this._bounceDuration = value;
                UpdateBounceState();
            }
        }

        public float BounceDistance
        {
            get => this._bounceDistance;
            set
            {
                this._bounceDistance = value;
                UpdateBounceState();
            }
        }

        public override void Load()
        {
            this._bounceAnimation?.CancelAndComplete();
            this._bounceAnimation = null;

            UpdateBounceState();
        }

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes)
        {
            float fOut;

            foreach (var attr in attributes)
            {
                switch (attr.Name.ToLowerInvariant())
                {
                    case "bounce":
                    case "bounce-height":
                        InvariantUtil.TryParseFloat(attr.Value, out this._bounceHeight);
                        break;
                    case "bounce-delay":
                        InvariantUtil.TryParseFloat(attr.Value, out this._bounceDelay);
                        break;
                    case "bounce-duration":
                        InvariantUtil.TryParseFloat(attr.Value, out this._bounceDuration);
                        break;
                    case "bounce-distance":
                        InvariantUtil.TryParseFloat(attr.Value, out this._bounceDistance);
                        break;
                    case "bounce-when":
                        Enum.TryParse(attr.Value, true, out this._bounceWhen);
                        break;
                }
            }
        }

        private void UpdateBounceState()
        {
            if (this.Activator != null)
            {
                this.Activator.Activated -= BounceActivatorOnActivated;
                this.Activator.Deactivated -= BounceActivatorOnDeactivated;

                this.Activator.Dispose();
            }

            StopBouncing();

            switch (this.BounceWhen)
            {
                case BehaviorWhen.InZone:
                    this.Activator = new ZoneActivator<TPathable, TEntity>(this)
                    {
                        ActivationDistance = this._bounceDistance,
                        DistanceFrom = DistanceFrom.Player
                    };

                    this.Activator.Activated += BounceActivatorOnActivated;
                    this.Activator.Deactivated += BounceActivatorOnDeactivated;

                    break;
                case BehaviorWhen.Always:
                    this.Activator = new Always<TPathable, TEntity>(this);
                    break;
                default:
                    StartBouncing();
                    break;
            }
        }

        private void BounceActivatorOnActivated(object sender, EventArgs e)
        {
            StartBouncing();
        }

        private void BounceActivatorOnDeactivated(object sender, EventArgs e)
        {
            StopBouncing();
        }

        private void StartBouncing()
        {
            this._bounceAnimation?.CancelAndComplete();

            this._bounceAnimation = GameService.Animation.Tweener.Tween(this.ManagedPathable.ManagedEntity,
                    new {VerticalOffset = this._originalVerticalOffset + this._bounceHeight}, this._bounceDuration,
                    this._bounceDelay)
                .From(new {VerticalOffset = this._originalVerticalOffset})
                .Ease(Ease.QuadInOut)
                .Repeat()
                .Reflect();
        }

        private void StopBouncing()
        {
            this._bounceAnimation?.Cancel();

            this._bounceAnimation = GameService.Animation.Tweener.Tween(this.ManagedPathable.ManagedEntity,
                    new {VerticalOffset = this._originalVerticalOffset},
                    this.ManagedPathable.ManagedEntity.VerticalOffset / 2f)
                .Ease(Ease.BounceOut);
        }
    }
}