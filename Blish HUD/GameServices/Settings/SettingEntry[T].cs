using System;
using Newtonsoft.Json;

namespace Blish_HUD.Settings
{
    public sealed class SettingEntry<T> : SettingEntry
    {
        private T _value;

        public SettingEntry()
        {
            /* NOOP */
        }

        /// <summary>
        ///     Creates a new <see cref="SettingEntry" /> of type <see cref="T" />.
        /// </summary>
        /// <param name="value">
        ///     The default value for the <see cref="SettingEntry{T}" /> if a value has not yet been saved in the
        ///     settings.
        /// </param>
        protected SettingEntry(T value)
        {
            this._value = value;
        }

        [JsonProperty(SETTINGVALUE_KEY)]
        [JsonRequired]
        public T Value
        {
            get => this._value;
            set
            {
                if (Equals(this._value, value)) return;

                var prevValue = this.Value;
                this._value = value;

                OnSettingChanged(new ValueChangedEventArgs<T>(prevValue, this._value));
            }
        }

        public event EventHandler<ValueChangedEventArgs<T>> SettingChanged;

        private void OnSettingChanged(ValueChangedEventArgs<T> e)
        {
            GameService.Settings.SettingSave();

            OnPropertyChanged(nameof(this.Value));

            SettingChanged?.Invoke(this, e);
        }

        protected override Type GetSettingType()
        {
            return typeof(T);
        }

        protected override object GetSettingValue()
        {
            return this._value;
        }

        public static SettingEntry<T> InitSetting(T value)
        {
            var newSetting = new SettingEntry<T>(value);

            return newSetting;
        }

        public static SettingEntry<T> InitSetting(string entryKey, T value)
        {
            var newSetting = new SettingEntry<T>(value)
            {
                EntryKey = entryKey,

                _value = value
            };

            return newSetting;
        }
    }
}