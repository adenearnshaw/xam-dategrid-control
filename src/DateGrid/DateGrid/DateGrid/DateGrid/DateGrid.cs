using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DateGrid.DateGrid
{
    public class DateGrid : ContentView, IDisposable
    {
        public static readonly BindableProperty StartDateProperty
            = BindableProperty.Create(nameof(StartDate), typeof(DateTime), typeof(DateGrid), DateTime.UtcNow, propertyChanged: HandlePropertyChanged);
        public static readonly BindableProperty EndDateProperty
            = BindableProperty.Create(nameof(EndDate), typeof(DateTime), typeof(DateGrid), DateTime.UtcNow.AddDays(7), propertyChanged: HandlePropertyChanged);
        public static readonly BindableProperty FirstDayOfWeekProperty
            = BindableProperty.Create(nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(DateGrid), DayOfWeek.Sunday, propertyChanged: HandlePropertyChanged);
        public static readonly BindableProperty CommandProperty
            = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(DateGrid), default(ICommand), propertyChanged: HandlePropertyChanged);
        public static readonly BindableProperty ValueProperty
            = BindableProperty.Create(nameof(Value), typeof(DateTime?), typeof(DateGrid), null, BindingMode.TwoWay, propertyChanged: HandleValuePropertyChanged);

        public static readonly BindableProperty ReadonlyDateStyleProperty
            = BindableProperty.Create(nameof(ReadonlyDateStyle), typeof(Style), typeof(DateGrid), defaultValue: DateGrid.DefaultReadonlyStyle, defaultBindingMode: BindingMode.OneWay, propertyChanged: HandleStylePropertyChanged);
        public static readonly BindableProperty SelectedDateStyleProperty
            = BindableProperty.Create(nameof(SelectedDateStyle), typeof(Style), typeof(DateGrid), defaultValue: DateGrid.DefaultSelectedStyle, defaultBindingMode: BindingMode.OneWay, propertyChanged: HandleStylePropertyChanged);
        public static readonly BindableProperty UnselectedDateStyleProperty
            = BindableProperty.Create(nameof(UnselectedDateStyle), typeof(Style), typeof(DateGrid), defaultValue: DateGrid.DefaultUnselectedStyle, defaultBindingMode: BindingMode.OneWay, propertyChanged: HandleStylePropertyChanged);

        public DateTime StartDate
        {
            get { return (DateTime)GetValue(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }
        public DateTime EndDate
        {
            get { return (DateTime)GetValue(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }
        public DayOfWeek FirstDayOfWeek
        {
            get { return (DayOfWeek)GetValue(FirstDayOfWeekProperty); }
            set { SetValue(FirstDayOfWeekProperty, value); }
        }
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public DateTime? Value
        {
            get => (DateTime?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public Style ReadonlyDateStyle
        {
            get { return (Style)base.GetValue(ReadonlyDateStyleProperty); }
            set { base.SetValue(ReadonlyDateStyleProperty, value); }
        }
        public Style SelectedDateStyle
        {
            get { return (Style)base.GetValue(SelectedDateStyleProperty); }
            set { base.SetValue(SelectedDateStyleProperty, value); }
        }
        public Style UnselectedDateStyle
        {
            get { return (Style)base.GetValue(UnselectedDateStyleProperty); }
            set { base.SetValue(UnselectedDateStyleProperty, value); }
        }

        public DateGrid()
        {
            RenderContent();
        }

        private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((DateGrid)bindable).RenderContent();
        }

        private static async void HandleValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            await Task.Delay(20); //Trying to tackle Race Condition
            ((DateGrid)bindable).UpdateChildStyles();
        }

        private static void HandleStylePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((DateGrid)bindable).UpdateChildStyles();
        }

        private bool _isRendering;
        private void RenderContent()
        {
            _isRendering = true;
            DestroyOldContent();

            if (StartDate > EndDate)
            {
                Debug.WriteLine($"{nameof(DateGrid)}: {nameof(StartDate)} must not be greater than {nameof(EndDate)}");
                return;
            }


            var paddedDateRange = new PaddedDateRange(StartDate, EndDate, FirstDayOfWeek);

            var layout = CreateBaseLayout();

            AddMonthHeader(ref layout, paddedDateRange);
            AddDayHeaders(ref layout, paddedDateRange);
            AddChildDates(ref layout, paddedDateRange);

            UpdateChildStyles();

            Content = layout;
            _isRendering = false;
        }

        private FlexLayout CreateBaseLayout()
        {
            var layout = new FlexLayout
            {
                AlignItems = FlexAlignItems.Center,
                AlignContent = FlexAlignContent.SpaceEvenly,
                JustifyContent = FlexJustify.SpaceBetween,
                Wrap = FlexWrap.Wrap,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };

            return layout;
        }

        private void AddMonthHeader(ref FlexLayout layout, PaddedDateRange dateRange)
        {
            var headerLayout = new StackLayout();
            FlexLayout.SetBasis(headerLayout, new FlexBasis(1f, true));
            FlexLayout.SetShrink(headerLayout, 0);

            var titleLabel = new Label()
            {
                Style = DateGrid.DefaultHeaderStyle,
                WidthRequest = -1,
                HorizontalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Center,
                Text = DateExtensions.MonthHeader(dateRange.StartDate, dateRange.EndDate, "MMMM", "MMMM"),
            };
            headerLayout.Children.Add(titleLabel);

            layout.Children.Add(headerLayout);
        }

        private void AddDayHeaders(ref FlexLayout layout, PaddedDateRange dateRange)
        {
            var dayHeaders = dateRange.StartDate.DayHeaders(dateRange.WeekStartsOn);

            foreach (var day in dayHeaders)
            {
                var label = new Label
                {
                    Style = DateGrid.DefaultHeaderStyle,
                    Text = day.Substring(0, 1).ToUpper(),
                };
                FlexLayout.SetBasis(label, new FlexBasis(0.1428f, true));
                FlexLayout.SetShrink(label, 0);

                layout.Children.Add(label);
            }
        }

        private void AddChildDates(ref FlexLayout layout, PaddedDateRange dateRange)
        {
            foreach (var d in dateRange.PaddingBefore.OrderBy(d => d))
            {
                var label = new ReadonlyLabel
                {
                    Style = ReadonlyDateStyle,
                    ClassId = GetDateAsId(d),
                    Text = d.Day.ToString(),
                };
                FlexLayout.SetBasis(label, new FlexBasis(0.1428f, true));
                FlexLayout.SetShrink(label, 0);

                layout.Children.Add(label);
            }

            foreach (var d in dateRange.Days.OrderBy(d => d))
            {
                var button = new Button
                {
                    Style = UnselectedDateStyle,
                    ClassId = GetDateAsId(d),
                    Command = this.Command,
                    CommandParameter = d,
                    BindingContext = d,
                    Text = d.Day.ToString(),
                };
                button.Clicked += ButtonClicked;

                layout.Children.Add(new ButtonContainer(button));
            }

            foreach (var d in dateRange.PaddingAfter.OrderBy(d => d))
            {
                var label = new ReadonlyLabel
                {
                    Style = ReadonlyDateStyle,
                    ClassId = GetDateAsId(d),
                    Text = d.Day.ToString()
                };
                FlexLayout.SetBasis(label, new FlexBasis(0.1428f, true));
                FlexLayout.SetShrink(label, 0);

                layout.Children.Add(label);
            }
        }

        private void UpdateChildStyles()
        {
            var selectedId = GetDateAsId(Value);

            foreach (var child in (Content as FlexLayout)?.Children ?? new List<View>())
            {
                if (child is ButtonContainer btnContainer)
                {
                    Debug.WriteLine(btnContainer.ClassId);
                    btnContainer.Button.Style = btnContainer.Button.ClassId == selectedId
                        ? SelectedDateStyle
                        : UnselectedDateStyle;
                }
                else if (child is ReadonlyLabel lbl)
                {
                    lbl.Style = ReadonlyDateStyle;
                }
            }
        }



        private void DestroyOldContent()
        {
            foreach (var child in (Content as FlexLayout)?.Children ?? new List<View>())
            {
                if (child is ButtonContainer btnContainer)
                {
                    btnContainer.Button.Clicked -= ButtonClicked;
                }
            }

            Content = null;
        }

        private void ButtonClicked(object sender, EventArgs args)
        {
            Value = ((Button)sender).BindingContext as DateTime?;
        }

        private string GetDateAsId(DateTime? date) => date?.ToString("yyyyMMdd") ?? string.Empty;

        public static Style DefaultHeaderStyle => new Style(typeof(Label))
        {
            ApplyToDerivedTypes = true,
            Setters =
            {
                new Setter
                {
                    Property = Label.BackgroundColorProperty,
                    Value = Color.Transparent
                },
                new Setter
                {
                    Property = Label.TextColorProperty,
                    Value = Color.FromHex("#212121")
                },
                new Setter
                {
                    Property = Label.FontAttributesProperty,
                    Value = FontAttributes.Bold
                },
                new Setter
                {
                    Property = VisualElement.HeightRequestProperty,
                    Value = -1
                },
                new Setter
                {
                    Property = VisualElement.WidthRequestProperty,
                    Value = 44
                },
                new Setter
                {
                    Property = View.HorizontalOptionsProperty,
                    Value = LayoutOptions.Fill
                },
                new Setter
                {
                    Property = Label.HorizontalTextAlignmentProperty,
                    Value = TextAlignment.Center
                },
                new Setter
                {
                    Property = Label.VerticalTextAlignmentProperty,
                    Value = TextAlignment.Center
                },
                new Setter
                {
                    Property = View.MarginProperty,
                    Value = new Thickness(4,4,4,8)
                },
                new Setter
                {
                    Property = Label.FontSizeProperty,
                    Value= 16
                }
            }
        };

        public static Style DefaultReadonlyStyle => new Style(typeof(Label))
        {
            ApplyToDerivedTypes = true,
            Setters =
            {
                new Setter
                {
                    Property = Label.BackgroundColorProperty,
                    Value = Color.Transparent
                },
                new Setter
                {
                    Property = Label.TextColorProperty,
                    Value = Color.FromHex("#808080")
                },
                new Setter
                {
                    Property = VisualElement.HeightRequestProperty,
                    Value = 44
                },
                new Setter
                {
                    Property = VisualElement.WidthRequestProperty,
                    Value = 44
                },
                new Setter
                {
                    Property = View.HorizontalOptionsProperty,
                    Value = LayoutOptions.Fill
                },
                new Setter
                {
                    Property = Label.HorizontalTextAlignmentProperty,
                    Value = TextAlignment.Center
                },
                new Setter
                {
                    Property = Label.VerticalTextAlignmentProperty,
                    Value = TextAlignment.Center
                },
                new Setter
                {
                    Property = View.MarginProperty,
                    Value = new Thickness(4)
                },
                new Setter
                {
                    Property = Label.FontSizeProperty,
                    Value= 14
                }
            }
        };

        public static Style DefaultUnselectedStyle => new Style(typeof(Button))
        {
            ApplyToDerivedTypes = true,
            Setters =
            {
                new Setter
                {
                    Property = VisualElement.BackgroundColorProperty,
                    Value = Color.Transparent
                },
                new Setter
                {
                    Property = Button.TextColorProperty,
                    Value = Color.FromHex("#212121")
                },
                new Setter
                {
                    Property = Button.CornerRadiusProperty,
                    Value = 22
                },
                new Setter
                {
                    Property = VisualElement.HeightRequestProperty,
                    Value = 44
                },
                new Setter
                {
                    Property = VisualElement.WidthRequestProperty,
                    Value = 44
                },
                new Setter
                {
                    Property = View.MinimumWidthRequestProperty,
                    Value = 0
                },
                new Setter
                {
                    Property = View.HorizontalOptionsProperty,
                    Value = LayoutOptions.Center
                },
                new Setter
                {
                    Property = Button.PaddingProperty,
                    Value = new Thickness(0)
                },
                new Setter
                {
                    Property = Button.PaddingProperty,
                    Value = new Thickness(4)
                },
                new Setter
                {
                    Property = Button.FontSizeProperty,
                    Value= 14
                },
                new Setter
                {
                    Property = Button.FontAttributesProperty,
                    Value = FontAttributes.Bold
                },
                new Setter
                {
                    Property = VisualElement.VisualProperty,
                    Value = VisualMarker.Default
                }
            }
        };

        public static Style DefaultSelectedStyle => new Style(typeof(Button))
        {
            ApplyToDerivedTypes = true,
            BasedOn = DefaultUnselectedStyle,
            Setters =
            {
                new Setter
                {
                    Property = VisualElement.BackgroundColorProperty,
                    Value = Color.FromHex("#212121")

                },
                new Setter
                {
                    Property = Button.TextColorProperty,
                    Value = Color.FromHex("#f5f5f5")
                },
            }
        };

        #region IDisposable
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                DestroyOldContent();
            }

            disposed = true;
        }
        #endregion

        private class ButtonContainer : ContentView
        {
            public ButtonContainer(Button button)
            {
                HorizontalOptions = LayoutOptions.Fill;
                Content = button;

                FlexLayout.SetBasis(this, new FlexBasis(0.1428f, true));
                FlexLayout.SetShrink(this, 0);
                FlexLayout.SetGrow(this, 0);
            }

            public Button Button => Content as Button;
        }

        private class ReadonlyLabel : Label { }

        public class DateRange
        {
            public DateTime StartDate { get; }
            public DateTime EndDate { get; }
            public IReadOnlyList<DateTime> Days { get; }

            public DateRange(DateTime startDate, DateTime endDate)
            {
                StartDate = startDate;
                EndDate = endDate;

                Days = Enumerable.Range(0, EndDate.Subtract(StartDate).Days + 1)
                                 .Select(offset => startDate.AddDays(offset))
                                 .ToList();
            }
        }

        public class PaddedDateRange : DateRange
        {
            public DayOfWeek WeekStartsOn { get; }

            public IReadOnlyList<DateTime> PaddingBefore { get; } = new List<DateTime>();
            public IReadOnlyList<DateTime> PaddingAfter { get; } = new List<DateTime>();

            public PaddedDateRange(DateTime startDate, DateTime endDate, DayOfWeek weekStartsOn)
                : base(startDate, endDate)
            {
                WeekStartsOn = weekStartsOn;
                PaddingBefore = GetPaddingDays(startDate, endDate, weekStartsOn, true);
                PaddingAfter = GetPaddingDays(startDate, endDate, weekStartsOn, false);
            }

            private List<DateTime> GetPaddingDays(DateTime startDate, DateTime endDate, DayOfWeek weekStartsOn, bool isBefore)
            {
                var paddedDays = new List<DateTime>();

                if (isBefore)
                {
                    var startOfWeek = startDate.FirstDateOfWeek(weekStartsOn);
                    for (DateTime d = startDate.Date.AddDays(-1); d >= startOfWeek; d = d.AddDays(-1))
                    {
                        paddedDays.Add(d);
                    }
                }
                else
                {
                    var endOfWeek = endDate.FirstDateOfWeek(weekStartsOn).AddDays(6);
                    for (DateTime d = EndDate.Date.AddDays(1); d <= endOfWeek; d = d.AddDays(1))
                    {
                        paddedDays.Add(d);
                    }
                }

                return paddedDays;
            }
        }
    }
}


