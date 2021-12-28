using Xamarin.Forms;

namespace CurrConv.Model
{
    class Curr : BindableObject
    {
        public Curr(string name, double value, int nominal)
        {
            this.Name = name;
            this.Value = value;
            this.Nominal = nominal;
        }

        private int _nominal;
        private string _name;
        private double _value;

        public int Nominal
        {
            get => _nominal;
            set
            {
                _nominal = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
}
