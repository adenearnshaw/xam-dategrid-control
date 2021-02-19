using MvvmHelpers;
using System;
using Xamarin.Forms;

namespace DateGrid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
        }


        public class MainViewModel : BaseViewModel
        {
            public DateTime StartDate { get; set; } = new DateTime(2021, 2, 3);
            public DateTime EndDate { get; set; } = new DateTime(2021, 2, 27);

            private DateTime _selectedDate;
            public DateTime SelectedDate 
            {
                get => _selectedDate;
                set => SetProperty(ref _selectedDate, value);
            }
        }
    }
}
