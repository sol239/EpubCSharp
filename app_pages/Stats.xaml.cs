using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using EpubReader.app_pages;
using EpubReader.code;

using System.Diagnostics;


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
            //SetupChart(DateTime.Now.ToString("yyyy-MM-dd"));

        }

        public async Task LoadBooks()
        {

            try
            {
                // combined dict

                foreach (string ebookFolderPath in app_controls.GetListOfAllEbooks())
                {

                    Ebook ebook = new Ebook();
                    string jsonDataFilePath = string.Empty;

                    try
                    {
                        jsonDataFilePath = FileManagement.GetEbookDataJsonFile(ebookFolderPath);
                        Debug.WriteLine($"LoadBooks() 1 - Success\n");
                    }

                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LoadBooks() 1 - Fail - {ex.Message}\n");
                    }

                    try
                    {
                        ebook = JsonHandler.ReadEbookJsonFile(jsonDataFilePath);
                        Debug.WriteLine($"LoadBooks() 1.1 - Success\n");
                    }
                    catch
                    {
                        Debug.WriteLine($"LoadBooks() 1.1 - Fail\n");
                    }

                    try
                    {
                        _timeSpan += TimeSpan.Parse(ebook.BookReadTime);
                        Debug.WriteLine($"LoadBooks() 1.2 - Success\n");
                    }

                    catch
                    {
                        Debug.WriteLine($"LoadBooks() 1.2 - Fail\n");
                        _timeSpan += TimeSpan.Parse("00:00:00");
                    }

                    finally
                    {
                        foreach (var entry in ebook.StatsRecord1)
                        {
                            try
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

                                Debug.WriteLine($"LoadBooks() 2 - Success\n");
                            }

                            catch (Exception ex)
                            {
                                Debug.WriteLine($"LoadBooks() 2 - Fail - {ex.Message}\n");
                            }
                        }

                    }

                    //Books.Add(new Book { Name = "Auto Nomní", TimeDict = ebook.StatsRecord1 });

                }

                try
                {
                    Books.Add(new Book { Name = "Combined", TimeDict = _combinedDict });
                    timeSpan = $"Total time: {_timeSpan.Days}d {_timeSpan.Hours}h {_timeSpan.Minutes}m {_timeSpan.Seconds}s";
                    // use today's date as the default selected date
                    //TimeSpentPerBookTextBlock.Text = _combinedDict[DateTime.Now.ToString("yyyy-MM-dd")];
                    TimeSpan _timeSpan2 = TimeSpan.Parse(_combinedDict[DateTime.Now.ToString("yyyy-MM-dd")]);

                    TimeSpentPerBookTextBlock.Text = $"{DateTime.Now.ToString("yy-MMM-dd ddd")}: {_timeSpan2.Hours}h {_timeSpan2.Minutes}m {_timeSpan2.Seconds}s";


                    Debug.WriteLine($"LoadBooks() 3 - Success\n");
                }

                catch (Exception ex)
                {
                    Debug.WriteLine($"LoadBooks() 3 - Fail - {ex.Message}\n");

                    TimeSpentPerBookTextBlock.Text = $"{DateTime.Now.ToString("yy-MMM-dd ddd")}: 0s";

                }

                Debug.WriteLine($"LoadBooks() - Success\n");
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"LoadBooks() - Fail - {ex.Message}\n");
            }

        }


        private Color GetColorFromValue(string value)
        {
            try
            {
                TimeSpan timeSpan = TimeSpan.Parse(value);

                TimeSpan interval0 = new TimeSpan(0, 0, 0, 0);
                TimeSpan interval1 = new TimeSpan(0, 0, 5, 0);
                TimeSpan interval2 = new TimeSpan(0, 0, 15, 0);
                TimeSpan interval3 = new TimeSpan(0, 0, 30, 0);
                TimeSpan interval4 = new TimeSpan(0, 0, 60, 0);
                TimeSpan interval5 = new TimeSpan(0, 0, 90, 0);

                var color0 = EbookWindow.ParseHexColor("#ffffff");
                var color1 = EbookWindow.ParseHexColor("#EEFCE9");
                var color2 = EbookWindow.ParseHexColor("#7ceb55");
                var color3 = EbookWindow.ParseHexColor("#4ce019");
                var color4 = EbookWindow.ParseHexColor("#36a012");
                var color5 = EbookWindow.ParseHexColor("#2b800e");
                var color6 = EbookWindow.ParseHexColor("#154007");


                if (timeSpan <= interval0)
                {
                    return color0;
                    Debug.WriteLine("Color 0");
                }
                else if (timeSpan > interval0 && timeSpan <= interval1)
                {
                    Debug.WriteLine("Color 1");
                    return color1;
                }
                else if (timeSpan > interval1 && timeSpan <= interval2)
                {
                    Debug.WriteLine("Color 2");
                    return color2;
                }

                else if (timeSpan > interval2 && timeSpan <= interval3)
                {
                    Debug.WriteLine("Color 3");
                    return color3;
                }

                else if (timeSpan > interval3 && timeSpan <= interval4)
                {
                    Debug.WriteLine("Color 4");
                    return color4;
                }

                else if (timeSpan > interval4 && timeSpan <= interval5)
                {
                    Debug.WriteLine("Color 5");
                    return color5;
                }

                else if (timeSpan > interval5)
                {
                    Debug.WriteLine("Color 6");
                    return color6;
                }
                else
                {
                    Debug.WriteLine("Color No Interval");
                    return EbookWindow.ParseHexColor("#000000");
                } 
                Debug.WriteLine($"GetColorFromValue() - Success\n");
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"GetColorFromValue() - Fail - {ex.Message}\n");
                return EbookWindow.ParseHexColor("#000000");
            }


        }

        private int _currentMonth;
        private bool cange = true;

        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            try
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

                if (day.Date.Month != _currentMonth)
                {
                    //SetupChart(dateKey);
                    _currentMonth = day.Date.Month;
                }


                //Debug.WriteLine($"CalendarView_CalendarViewDayItemChanging() - Success\n");
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"CalendarView_CalendarViewDayItemChanging() - Fail - {ex.Message}\n");
            }

        }


        private void CalendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            try
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
                        TimeSpentPerBookTextBlock.Text = $"{selectedDate.ToString("yy-MMM-dd ddd")}: {_timeSpan2.Hours}h {_timeSpan2.Minutes}m {_timeSpan2.Seconds}s";
                    }
                    else
                    {
                        TimeSpentPerBookTextBlock.Text = $"{selectedDate.ToString("yy-MMM-dd ddd")}: 0s";
                    }

                    //SetupChart(formattedDate);


                    Debug.WriteLine(formattedDate);

                    // print all keys
                    foreach (var entry in _combinedDict)
                    {
                        Debug.WriteLine(entry.Key);
                    }

                }

                Debug.WriteLine($"CalendarView_SelectedDatesChanged() - Success\n");
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"CalendarView_SelectedDatesChanged() - Fail - {ex.Message}\n");
            }
           
        }

        public static List<string> GetAllValidDates(int year, int month)
        {
            List<string> dates = new List<string>();

            // Validate the input year and month
            if (year < 1 || month < 1 || month > 12)
            {
                throw new ArgumentException("Invalid year or month");
            }

            // Determine the number of days in the month
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Generate all dates for the month
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(year, month, day);
                dates.Add(date.ToString("yyyy-MM-dd"));
            }

            return dates;
        }

        private void SetupChart(string selectedDate)
        {
            /*
            DateTime date = DateTime.Parse(selectedDate);
            List<string> dates = GetAllValidDates(date.Year, date.Month);
            Debug.WriteLine(String.Join(", ", dates));

            var lineSeries = new LineSeries<double>
            {
                Values = new double[] { 3, 5, 7, 4, 2, 6, 8, 5, 9, 3 }

                // line plot containg reading times per month
                //Value = new
            };
                        // Assign the series to the chart
            lineChart.Series = new ISeries[] { lineSeries };

            // Optionally configure axes
            lineChart.XAxes = new Axis[] { new Axis { Labeler = value => value.ToString() } };
            lineChart.YAxes = new Axis[] { new Axis { Labeler = value => value.ToString() } };
            */
        }
    }

    public class Book
    {
        public string Name { get; set; }
        public Dictionary<string, string> TimeDict { get; set; }
    }
}

