# DateGrid Control for Xamarin

A sample app with a custom DateGrid control

## Features

- Allow user to set min and max dates and automatically pad dates to make whole weeks
- Custom start day of week
- Styleable options for selected date


![Demo video](/assets/sample.gif)


## Control code

- [DateGrid.cs](/src/DateGrid/DateGrid/DateGrid/DateGrid/DateGrid.cs)
- [DateExtensions.cs](/src/DateGrid/DateGrid/DateGrid/DateGrid/DateExtensions.cs)


## Implementing

MainPage.xaml
```xml
<ContentPage x:Class="DateGrid.MainPage"
             xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:dg="clr-namespace:DateGrid.DateGrid">

    <Page.Resources>
        <Style x:Key="DatePickerSelectedStyle"
               BasedOn="{Static dg:DateGrid.DefaultSelectedStyle}"
               TargetType="Button">
            <Setter Property="BackgroundColor" Value="#2196F3" />
            <Setter Property="TextColor" Value="#F5F5F5" />
            <Setter Property="CornerRadius" Value="22" />
        </Style>
    </Page.Resources>

    <StackLayout>
       ...

        <dg:DateGrid x:Name="DatePickerGrid"
                     EndDate="{Binding EndDate}"
                     FirstDayOfWeek="Monday"
                     SelectedDateStyle="{StaticResource DatePickerSelectedStyle}"
                     StartDate="{Binding StartDate}"
                     Value="{Binding SelectedDate}" />
    </StackLayout>

</ContentPage>

```

MainPage.xaml.cs
```csharp
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


```
