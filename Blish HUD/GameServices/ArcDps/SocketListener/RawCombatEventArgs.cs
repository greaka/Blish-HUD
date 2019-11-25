using System;
using Blish_HUD.ArcDps.Models;

namespace Blish_HUD.ArcDps
{
    public class RawCombatEventArgs : EventArgs
    {
        public enum CombatEventType
        {
            Area,
            Local
        }

        public RawCombatEventArgs(CombatEvent combatEvent, CombatEventType eventType)
        {
            this.CombatEvent = combatEvent;
            this.EventType = eventType;
        }

        public CombatEventType EventType { get; }

        public CombatEvent CombatEvent { get; }
    }
}