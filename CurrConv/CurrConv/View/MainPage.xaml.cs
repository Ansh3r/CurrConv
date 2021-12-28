
using Xamarin.Forms;
using CurrConv.VM;

namespace CurrConv.View

{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MVVM();
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            ((MVVM)BindingContext).ActuallCurr(e.NewDate);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue)) return;

            if (!double.TryParse(e.NewTextValue, out double value))
            {
                ((Entry)sender).Text = e.OldTextValue;
            }
        }
    }
}
