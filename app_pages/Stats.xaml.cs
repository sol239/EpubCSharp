using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using EpubReader.app_pages;
using EpubReader.code;

using System.Diagnostics;


using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;
using CSharpMarkup.WinUI.LiveChartsCore.SkiaSharpView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Stats : Page
    {
        public List<Book> Books { get; set; } = new List<Book>();
        private string timeSpan;
        private TimeSpan _timeSpan;
        private Dictionary<string, string> _combinedDict = new Dictionary<string, string>();



        public Stats()
        {
            InitializeComponent();

            // Initialize habits
            LoadBooks();
            SetupChart();

        }

        public async Task LoadBooks()
        {

            // combined dict

            foreach (string ebookFolderPath in app_controls.GetListOfAllEbooks())
            {
                string jsonDataFilePath = FileManagment.GetEbookDataJsonFile(ebookFolderPath);
                var ebook = JsonHandler.ReadEbookJsonFile(jsonDataFilePath);

                _timeSpan += TimeSpan.Parse(ebook.BookReadTime);

                //Books.Add(new Book { Name = "Auto Nomní", TimeDict = ebook.StatsRecord1 });

                foreach (var entry in ebook.StatsRecord1)
                {
                    if (!_combinedDict.ContainsKey(entry.Key))
                    {
                        _combinedDict.Add(entry.Key, entry.Value);
                    }
                    else
                    {
                        TimeSpan timeSpan = TimeSpan.Parse(_combinedDict[entry.Key]);
                        TimeSpan timeSpan2 = TimeSpan.Parse(entry.Value);
                        TimeSpan newTimeSpan = timeSpan.Add(timeSpan2);
                        _combinedDict[entry.Key] = newTimeSpan.ToString();
                    }
                }
            }

            Books.Add(new Book { Name = "Combined", TimeDict = _combinedDict });
            timeSpan = $"{_timeSpan.Days}d {_timeSpan.Hours}h {_timeSpan.Minutes}m {_timeSpan.Seconds}s";
            // use today's date as the default selected date
            TimeSpentPerBookTextBlock.Text = _combinedDict[DateTime.Now.ToString("yyyy-MM-dd")];

        }


        private Color GetColorFromValue(string value)
        {
            TimeSpan timeSpan = TimeSpan.Parse(value);
            
            TimeSpan interval0 = new TimeSpan(0, 0, 0, 0);
            TimeSpan interval1 = new TimeSpan(0, 0, 5, 0);
            TimeSpan interval2 = new TimeSpan(0, 0, 15, 0);
            TimeSpan interval3 = new TimeSpan(0, 0, 30, 0);
            TimeSpan interval4 = new TimeSpan(0, 0, 60, 0);
            TimeSpan interval5 = new TimeSpan(0, 0, 90, 0);

            var color0 = EbookWindow.ParseHexColor("#ffffff");
            var color1 = EbookWindow.ParseHexColor("#adf295");
            var color2 = EbookWindow.ParseHexColor("#7ceb55");
            var color3 = EbookWindow.ParseHexColor("#4ce019");
            var color4 = EbookWindow.ParseHexColor("#36a012");
            var color5 = EbookWindow.ParseHexColor("#2b800e");
            var color6 = EbookWindow.ParseHexColor("#154007");


            if (timeSpan <= interval0)
            {
                return color0;
            }
            else if (timeSpan > interval0 && timeSpan <= interval1)
            {
                return color1;
            }
            else if (timeSpan > interval1 && timeSpan <= interval2)
            {
                return color2;
            }

            else if (timeSpan > interval2 && timeSpan <= interval3)
            {
                return color3;
            }

            else if (timeSpan > interval3 && timeSpan <= interval4)
            {
                return color4;
            }

            else if (timeSpan > interval4 && timeSpan <= interval5)
            {
                return color5;
            }

            else if (timeSpan > interval5)
            {
                return color6;
            }
            else
            {
                return EbookWindow.ParseHexColor("#000000");
            }


        }

        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            var day = args.Item as CalendarViewDayItem;
            var dateKey = day.Date.ToString("yyyy-MM-dd");

            // Check if the date exists in any book's TimeDict and get the corresponding value
            var bookEntry = Books.FirstOrDefault(book => book.TimeDict.ContainsKey(dateKey));
            if (bookEntry != null)
            {
                var value = bookEntry.TimeDict[dateKey];
                // Set day item background color based on the value
                day.Background = new SolidColorBrush(GetColorFromValue(value));
            }
            else
            {
                day.Background = new SolidColorBrush(Colors.White);
            }

        }

        private void CalendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            // Check if there are any selected dates
            if (sender.SelectedDates.Count > 0)
            {
                // Get the first selected date
                DateTime selectedDate = sender.SelectedDates[0].DateTime;
                string formattedDate = selectedDate.ToString("yyyy-MM-dd");

                if (_combinedDict.ContainsKey(formattedDate))
                {

                    TimeSpan _timeSpan2 = TimeSpan.Parse(_combinedDict[formattedDate]);
                    TimeSpentPerBookTextBlock.Text = $"{_timeSpan2.Hours}h {_timeSpan2.Minutes}m {_timeSpan2.Seconds}s";
                }
                else
                {
                    TimeSpentPerBookTextBlock.Text = "No data";
                }


                Debug.WriteLine(formattedDate);

                // print all keys
                foreach (var entry in _combinedDict)
                {
                    Debug.WriteLine(entry.Key);
                }

            }
           
        }

        private void SetupChart()
        {
            // Create a line series
            var lineSeries = new LineSeries<double>
            {
                Values = new double[] { 3, 5, 7, 4, 2, 6, 8, 5, 9, 3 }
            };

            // Assign the series to the chart
            lineChart.Series = new ISeries[] { lineSeries };

            // Optionally configure axes
            lineChart.XAxes = new Axis[] { new Axis { Labeler = value => value.ToString() } };
            lineChart.YAxes = new Axis[] { new Axis { Labeler = value => value.ToString() } };
        }
    }

    public class Book
    {
        public string Name { get; set; }
        public Dictionary<string, string> TimeDict { get; set; }
    }
}

