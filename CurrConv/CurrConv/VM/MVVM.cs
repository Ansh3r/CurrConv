using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CurrConv.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms.Internals;

namespace CurrConv.VM
{
    internal class MVVM : INotifyPropertyChanged
    {
        private string _Date;
        private ObservableCollection<Curr> _currs;
        private Curr _currLeft;
        private Curr _currRight;
        private string _fromV;
        private string _toV;
        private HttpClient Client { get; }

        public MVVM()
        {
            Client = new HttpClient();
            Currs = new ObservableCollection<Curr>();
            FromV = "0";
            ToV = "0";
            LoadCurr();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string SelectedDate
        {
            get => _Date;
            set
            {
                _Date = value;
                OnPropertyChanged();
            }
        }

        public string FromV
        {
            get => _fromV;
            set
            {
                if (_fromV == value) return;
                _fromV = value;
                CalcRightV(value);
                OnPropertyChanged();
            }
        }

        public string ToV
        {
            get => _toV;
            set
            {
                if (_toV == value) return;
                _toV = value;
                CalcLeftV(value);
                OnPropertyChanged();
            }
        }

        private void CalcLeftV(string value)
        {
            if (CurrLeft == null || CurrRight == null || string.IsNullOrWhiteSpace(value))
            {
                _fromV = "0";
            }
            else
            {
                var res = Convert.ToDouble(value) * CurrLeft.Value / CurrLeft.Nominal /(CurrRight.Value / CurrRight.Nominal);
                _fromV = res.ToString(CultureInfo.InvariantCulture);
            }
            OnPropertyChanged(nameof(FromV));
        }

        private void CalcRightV(string value)
        {
            if (CurrLeft == null || CurrRight == null || string.IsNullOrWhiteSpace(value))
            {
                _toV = "0";
            }
            else
            {
                var res = Convert.ToDouble(value) * CurrRight.Value / CurrRight.Nominal /(CurrLeft.Value / CurrLeft.Nominal);
                _toV = res.ToString(CultureInfo.InvariantCulture);
            }

            OnPropertyChanged(nameof(ToV));
        }

        public Curr CurrLeft
        {
            get => _currLeft;
            set
            {
                _currLeft = value;
                CalcRightV(FromV);
                OnPropertyChanged();
            }
        }

        public Curr CurrRight
        {
            get => _currRight;
            set
            {
                _currRight = value;
                CalcRightV(FromV);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Curr> Currs
        {
            get => _currs;
            set
            {
                _currs = value;
                OnPropertyChanged();
            }
        }

        public async Task ActuallCurr(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                date = date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(-2) : date.AddDays(-1);
            SetNewDate(date);
            var dateString = date.ToString("yyyy/MM/dd");
            var s = $"https://www.cbr-xml-daily.ru/archive/{dateString}/daily_json.js";
            var response = await Client.GetAsync(s);
            var currenciesJson = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(currenciesJson);
            var currencies = (JsonConvert.DeserializeObject<Dictionary<string, Curr>>(jObject["Valute"]?.ToString() ?? "") ?? new Dictionary<string, Curr>()).Select(x => x.Value).ToList();
            if (currencies.Count + 1 == Currs.Count)
                Currs.ForEach(x => x.Value = currencies.First(y => y.Name == x.Name).Value);
            else
                Currs = new ObservableCollection<Curr>(currencies) 
                { 
                    new Curr("Российский рубль", 1, 1) 
                };

            OnPropertyChanged();
        }

        private void SetNewDate(DateTime newDate)
        {
            SelectedDate = "Дата: " + newDate.ToString("dd, MM, yyyy");
        }

        private async Task LoadCurr()
        {
            var dateResult = DateTime.Now;
            var url = "https://www.cbr-xml-daily.ru/daily_json.js";
            if (dateResult.DayOfWeek == DayOfWeek.Saturday || dateResult.DayOfWeek == DayOfWeek.Sunday)
            {
                dateResult = dateResult.DayOfWeek == DayOfWeek.Sunday ? dateResult.AddDays(-2) : dateResult.AddDays(-1);
                var dateString = dateResult.ToString("yyyy/MM/dd");
                url = $"https://www.cbr-xml-daily.ru/archive/{dateString}/daily_json.js";
            }
            SetNewDate(dateResult);
            var response = await Client.GetAsync(url);
            var currenciesJson = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(currenciesJson);
            var currencies = JsonConvert.DeserializeObject<Dictionary<string, Curr>>(jObject["Valute"]?.ToString() ?? "");

            Currs = new ObservableCollection<Curr>(currencies?.Select(x => x.Value) ?? new List<Curr>());
            Currs.Add(new Curr("Российский рубль", 1, 1));

            CurrLeft = Currs.Last();
            CurrRight = Currs.Last();

            OnPropertyChanged();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}