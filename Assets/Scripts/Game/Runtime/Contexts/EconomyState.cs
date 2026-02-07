using System;
using System.Collections.Generic;

namespace Game.Runtime.Contexts
{
    [Serializable]
    public class EconomyState
    {
        public List<CurrencyEntry> currenciesList = new List<CurrencyEntry>();
        private Dictionary<string, int> _currencies;
        private Action _onChanged;

        public void SetOnChanged(Action callback)
        {
            _onChanged = callback;
        }

        private void EnsureCurrencies()
        {
            if (_currencies != null) return;
            _currencies = new Dictionary<string, int>();
            if (currenciesList == null) return;
            foreach (var e in currenciesList)
            {
                if (e == null || string.IsNullOrEmpty(e.id)) continue;
                _currencies[e.id] = e.amount;
            }
        }

        private void SyncToList()
        {
            currenciesList.Clear();
            if (_currencies == null) return;
            foreach (var kv in _currencies)
                currenciesList.Add(new CurrencyEntry { id = kv.Key, amount = kv.Value });
        }

        public void AddCurrency(string id, int amount)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureCurrencies();
            if (_currencies.TryGetValue(id, out int current))
                _currencies[id] = current + amount;
            else
                _currencies[id] = amount;
            SyncToList();
            _onChanged?.Invoke();
        }

        public void SpendCurrency(string id, int amount)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureCurrencies();
            if (_currencies.TryGetValue(id, out int current))
            {
                int next = Math.Max(0, current - amount);
                if (next == 0)
                    _currencies.Remove(id);
                else
                    _currencies[id] = next;
                SyncToList();
                _onChanged?.Invoke();
            }
        }

        public int GetAmount(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            EnsureCurrencies();
            return _currencies.TryGetValue(id, out int amount) ? amount : 0;
        }

        [Serializable]
        public class CurrencyEntry
        {
            public string id;
            public int amount;
        }
    }
}
