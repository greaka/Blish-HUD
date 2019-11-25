using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Settings;

namespace Blish_HUD.Pathing.Behaviors
{
    public class DisplayInteractionInfo<TPathable, TEntity> : Interactable<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        protected static SettingEntry<Dictionary<string, bool>> _userStateStore;

        //private const string BEHAVIOR_NAME = "ReappearOnMapChange";

        protected readonly InteractionInfo Indicator;

        //private string GetBehaviorStoreEntry(string characterName) {
        //    var toonName = GameService.Player.CharacterName;

        //    if (_userStateStore.Value.ContainsKey(characterName)) {

        //    }
        //}

        public DisplayInteractionInfo(TPathable managedPathable) : base(managedPathable)
        {
            //_userStateStore = _userStateStore ?? PersistentBehaviorStore.Value.DefineSetting(
            //                                                              BEHAVIOR_NAME,
            //                                                              new Dictionary<string, bool>(),
            //                                                              new Dictionary<string, bool>()
            //                                                             );

            this.Indicator = new InteractionInfo();
        }

        public string InfoText
        {
            get => this.Indicator.Text;
            set => this.Indicator.Text = value;
        }
    }
}