using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD.PersistentStore;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD
{
    [JsonObject]
    public class PersistentStoreService : GameService
    {
        [JsonIgnore] private const string STORE_FILENAME = "persistent.json";

        private const int SAVE_FREQUENCY = 5000; // Time in milliseconds (currently 5 seconds)

        private static readonly Logger Logger = Logger.GetLogger<PersistentStoreService>();

        [JsonIgnore] public static bool StoreChanged;

        [JsonIgnore] private JsonSerializerSettings _jsonSettings;

        private double _lastUpdate;

        [JsonIgnore] private string _persistentStorePath;

        private Store _stores;

        protected override void Initialize()
        {
            this._jsonSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None,
                Converters = new List<JsonConverter>
                {
                    new StoreValue.StoreValueConverter(),
                    new Store.PersistentStoreConverter()
                }
            };

            this._persistentStorePath = Path.Combine(DirectoryUtil.BasePath, STORE_FILENAME);

            // If store isn't there, generate the file
            if (!File.Exists(this._persistentStorePath)) Save();

            // Would prefer to have this under Load(), but PersistentStoreService needs to be ready for other modules and services
            try
            {
                var rawStore = File.ReadAllText(this._persistentStorePath);

                this._stores = JsonConvert.DeserializeObject<Store>(rawStore, this._jsonSettings);
            }
            catch (FileNotFoundException)
            {
                // Likely don't have access to this filesystem
            }
            catch (Exception e)
            {
                Logger.Warn(e, "There was an unexpected error trying to read the persistent store data file.");
            }

            if (this._stores == null)
            {
                Logger.Warn("Persistent store could not be read, so a new one will be generated.");
                this._stores = new Store();
            }
        }

        public Store RegisterStore(string storeName)
        {
            return this._stores.GetSubstore(storeName);
        }

        protected override void Load()
        {
            /* NOOP */
        }

        protected override void Unload()
        {
            /* NOOP */
        }

        protected override void Update(GameTime gameTime)
        {
            this._lastUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (this._lastUpdate > SAVE_FREQUENCY)
            {
                if (StoreChanged)
                    Save();
                this._lastUpdate = 0;
            }
        }

        public void Save()
        {
            try
            {
                var rawStore = JsonConvert.SerializeObject(this._stores, Formatting.None, this._jsonSettings);

                using (var settingsWriter = new StreamWriter(this._persistentStorePath, false))
                {
                    settingsWriter.Write(rawStore);
                }
            }
            catch (InvalidOperationException e)
            {
                // Likely that the collection was modified while we were attempting to serialize the stores
                Logger.Warn(e, "Failed to save persistent store.");
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to write persistent store to file!");
            }

            this._lastUpdate = 0;
            StoreChanged = false;
        }
    }
}