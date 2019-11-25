using System;
using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;

namespace Blish_HUD.Pathing.Behaviors
{
    [PathingBehavior("notification")]
    public class Notification<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        private bool _canResendMessage = true;
        private float _notificationDistance = 2f;

        private ScreenNotification.NotificationType _notificationType = ScreenNotification.NotificationType.Info;

        /// <inheritdoc />
        public Notification(TPathable managedPathable) : base(managedPathable)
        {
        }

        public string NotificationMessage { get; set; } = string.Empty;

        public float NotificationDistance
        {
            get => this._notificationDistance;
            set
            {
                this._notificationDistance = value;
                Load();
            }
        }

        public ScreenNotification.NotificationType NotificationType
        {
            get => this._notificationType;
            set => this._notificationType = value;
        }

        /// <inheritdoc />
        public override void Load()
        {
            if (this.Activator != null)
            {
                this.Activator.Activated -= ActivatorOnActivated;
                this.Activator.Deactivated -= ActivatorOnDeactivated;

                this.Activator.Dispose();
            }

            this.Activator = new ZoneActivator<TPathable, TEntity>(this)
            {
                ActivationDistance = this._notificationDistance,
                DistanceFrom = DistanceFrom.Player
            };

            this.Activator.Activated += ActivatorOnActivated;
            this.Activator.Deactivated += ActivatorOnDeactivated;
        }

        /// <inheritdoc />
        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes)
        {
            foreach (var attr in attributes)
            {
                switch (attr.Name.ToLowerInvariant())
                {
                    case "notification":
                        this.NotificationMessage = attr.Value;
                        break;
                    case "notification-distance":
                        InvariantUtil.TryParseFloat(attr.Value, out this._notificationDistance);
                        break;
                    case "notification-type":
                        Enum.TryParse(attr.Value, true, out this._notificationType);
                        break;
                }
            }
        }

        private void ActivatorOnActivated(object sender, EventArgs e)
        {
            if (this._canResendMessage)
                ScreenNotification.ShowNotification(this.NotificationMessage, this._notificationType);

            this._canResendMessage = false;
        }

        private void ActivatorOnDeactivated(object sender, EventArgs e)
        {
            this._canResendMessage = true;
        }
    }
}