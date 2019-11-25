using System.ComponentModel;
using Newtonsoft.Json;

namespace Blish_HUD.PersistentStore
{
    [JsonObject]
    public class StoreValue<T> : StoreValue, INotifyPropertyChanged
    {
        public StoreValue()
        {
            /* NOOP */
        }

        public StoreValue(T defaultValue)
        {
            this._value = defaultValue;
        }

        [JsonIgnore]
        public T Value
        {
            get => (T) this._value;
            set
            {
                if (Equals(this._value, value)) return;

                this._value = value;

                OnPropertyChanged();
            }
        }
    }
}