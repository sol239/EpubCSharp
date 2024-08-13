using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EpubReader.app_pages;
using EpubReader.code;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class Stats : Page
    {
        private int _currentMonth;
        private string _timeSpan2;
        private TimeSpan _timeSpan;
        private readonly Dictionary<string, string> _combinedDict = new Dictionary<string, string>();

        /// <summary>
        /// The collection of items to display.
        /// </summary>
        public List<Book> Books { get; set; } = new List<Book>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Stats"/> class.
        /// </summary>
        public Stats()
        {
            InitializeComponent();
            LoadBooks();
        }

        /// <summary>
        /// Loads ebook data from multiple folders, aggregates the reading time, and updates the UI with the 
        /// combined statistics. The method also logs detailed debug information if the <paramref name="debug"/> 
        /// parameter is set to true.
        /// </summary>
        /// <param name="debug">Indicates whether detailed debug information should be logged. Defaults to false.</param>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="number">
        /// <item>
        /// <description>
        /// Iterates over all ebook folders returned by <see cref="AppControls.GetListOfAllEbooks"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// For each folder, attempts to retrieve the associated JSON data file using 
        /// <see cref="FileManagement.GetEbookDataJsonFile"/> and parses it into an <see cref="Ebook"/> object 
        /// using <see cref="JsonHandler.ReadEbookJsonFile"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Aggregates the total reading time across all ebooks by summing the <see cref="Ebook.BookReadTime"/> 
        /// values, handling any parsing errors by defaulting to zero time.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Combines the reading statistics from all ebooks into a single dictionary, merging the times for 
        /// duplicate keys.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Adds the combined statistics as a new <see cref="Book"/> to the application's list of books, and updates 
        /// the UI to display the total reading time for the current day.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">
        /// Catches any exceptions that occur during the processing of ebooks and logs 
        /// them if debug mode is enabled. The method attempts to handle errors gracefully, setting defaults where necessary.
        /// </exception>
        public void LoadBooks(bool debug = false)
        {

            try
            {
                // combined dict

                foreach (string ebookFolderPath in AppControls.GetListOfAllEbooks())
                {

                    Ebook ebook = new Ebook();
                    string jsonDataFilePath = string.Empty;

                    try
                    {
                        jsonDataFilePath = FileManagement.GetEbookDataJsonFile(ebookFolderPath);
                        if (debug) { Debug.WriteLine($"LoadBooks() - 1 Success");}
                    }

                    catch (Exception ex)
                    {
                        if (debug) { Debug.WriteLine($"LoadBooks() - 1 Fail - {ex.Message}");}
                    }

                    try
                    {
                        ebook = JsonHandler.ReadEbookJsonFile(jsonDataFilePath);
                        if (debug) { Debug.WriteLine($"LoadBooks() - 2 Success");}
                    }
                    catch
                    {
                        if (debug) { Debug.WriteLine($"LoadBooks() - 2 Fail");}
                    }

                    try
                    {
                        _timeSpan += TimeSpan.Parse(ebook.BookReadTime);
                        if (debug) { Debug.WriteLine($"LoadBooks() - 3 Success");}
                    }

                    catch
                    {
                        _timeSpan += TimeSpan.Parse("00:00:00");
                        if (debug) { Debug.WriteLine($"LoadBooks() - 3 Fail");}
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
if (debug) { Debug.WriteLine($"LoadBooks() 4 - Success");}
                            }

                            catch (Exception ex)
                            {
                                if (debug) { Debug.WriteLine($"LoadBooks() 4 - Fail - {ex.Message}");}
                            }
                        }

                    }

                }

                try
                {
                    Books.Add(new Book { Name = "Combined", TimeDict = _combinedDict });
                    this._timeSpan2 = $"Total time: {_timeSpan.Days}d {_timeSpan.Hours}h {_timeSpan.Minutes}m {_timeSpan.Seconds}s";
                    // use today's date as the default selected date
                    //TimeSpentPerBookTextBlock.Text = _combinedDict[DateTime.Now.ToString("yyyy-MM-dd")];
                    TimeSpan _timeSpan2 = TimeSpan.Parse(_combinedDict[DateTime.Now.ToString("yyyy-MM-dd")]);

                    TimeSpentPerBookTextBlock.Text = $"{DateTime.Now.ToString("yy-MMM-dd ddd")}: {_timeSpan2.Hours}h {_timeSpan2.Minutes}m {_timeSpan2.Seconds}s";
if (debug) { Debug.WriteLine($"LoadBooks() 5 - Success");}

                }

                catch (Exception ex)
                {
                    TimeSpentPerBookTextBlock.Text = $"{DateTime.Now.ToString("yy-MMM-dd ddd")}: 0s";
if (debug) { Debug.WriteLine($"LoadBooks() 5 - Fail - {ex.Message}");}
                }

                if (debug) { Debug.WriteLine($"LoadBooks() - Success");}
            }

            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"LoadBooks() - Fail - {ex.Message}");}
            }

        }

        /// <summary>
        /// Converts a time span represented as a string into a corresponding color based on predefined time intervals.
        /// </summary>
        /// <param name="value">A string representation of a time span. The string should be in a format that <see cref="TimeSpan.Parse(string)"/> can handle.</param>
        /// <returns>
        /// A <see cref="Color"/> that corresponds to the time span. The color is chosen based on the time intervals defined in the method.
        /// Returns a default color of black if the input string is invalid or if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This method parses the input string into a <see cref="TimeSpan"/> and then compares it against a series of predefined intervals:
        /// - Interval 0: 0 seconds
        /// - Interval 1: 5 seconds
        /// - Interval 2: 15 seconds
        /// - Interval 3: 30 seconds
        /// - Interval 4: 60 seconds
        /// - Interval 5: 90 seconds
        /// If the time span falls within one of these intervals, the corresponding color is returned. 
        /// If the time span does not fall into any of the intervals, the default color is returned.
        /// If the input string cannot be parsed into a <see cref="TimeSpan"/> or an exception is thrown, the method returns a default color of black.
        /// </remarks>
        /// <exception cref="FormatException">Thrown when the input string is not in a valid <see cref="TimeSpan"/> format.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
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

            catch 
            {
                return EbookWindow.ParseHexColor("#000000");
            }
        }

        /// <summary>
        /// Handles the event when a day item in the <see cref="CalendarView"/> is changing. 
        /// Updates the background color of the day item based on associated data and checks for month changes.
        /// </summary>
        /// <param name="sender">The instance of <see cref="CalendarView"/> that is raising the event.</param>
        /// <param name="args">The event arguments containing the item being changed and other related data.</param>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///   <item>
        ///     Casts the args,Item to a <see cref="CalendarViewDayItem"/> to access its properties.
        ///   </item>
        ///   <item>
        ///     Retrieves the date key in the format "yyyy-MM-dd" for the current day item.
        ///   </item>
        ///   <item>
        ///     Checks if the date exists in any book's TimeDict dictionary. If it does, retrieves the associated value.
        ///   </item>
        ///   <item>
        ///     Sets the background color of the day item based on the value retrieved using <see cref="GetColorFromValue(string)"/>. 
        ///     If no matching entry is found, sets the background color to white.
        ///   </item>
        ///   <item>
        ///     Checks if the current day item's month is different from the previously tracked month. If so, updates the month 
        ///     and optionally performs additional setup such as updating charts for the new month.
        ///   </item>
        /// </list>
        /// If an exception occurs during processing, logs the error message to the debug output.
        /// </remarks>
        /// <exception cref="InvalidCastException">Thrown if args.Item cannot be cast to <see cref="CalendarViewDayItem"/>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the date key is not found in TimeDict.</exception>
        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            try
            {
                var day = args.Item;
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
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"CalendarView_CalendarViewDayItemChanging() - Fail - {ex.Message}\n");
            }

        }

        /// <summary>
        /// Handles the event when the selected dates in the <see cref="CalendarView"/> change.
        /// Updates the <see cref="TimeSpentPerBookTextBlock"/> with the time spent on the selected date and logs relevant information.
        /// </summary>
        /// <param name="sender">The instance of <see cref="CalendarView"/> that is raising the event.</param>
        /// <param name="args">The event arguments containing the newly selected dates.</param>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///   <item>
        ///     Checks if there are any selected dates in the calendar view.
        ///   </item>
        ///   <item>
        ///     Retrieves the first selected date and formats it as "yyyy-MM-dd".
        ///   </item>
        ///   <item>
        ///     Checks if the formatted date exists in the <see cref="_combinedDict"/> dictionary. If it does, parses the associated 
        ///     time span value and updates the <see cref="TimeSpentPerBookTextBlock"/> to display the time spent in hours, minutes, and seconds.
        ///     If the date is not found, sets the text to show "0s" for time spent.
        ///   </item>
        ///   <item>
        ///     Logs the formatted date and prints all keys from the <see cref="_combinedDict"/> dictionary to the debug output.
        ///   </item>
        /// </list>
        /// If an exception occurs during processing, the error message is logged to the debug output.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">Thrown if the formatted date is not found in <see cref="_combinedDict"/>.</exception>
        /// <exception cref="FormatException">Thrown if the time span string in <see cref="_combinedDict"/> is not in a valid format.</exception>
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
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"CalendarView_SelectedDatesChanged() - Fail - {ex.Message}\n");
            }
           
        }

            /* Deprecated
        private void SetupChart(string selectedDate)
        {
            
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
            
        }
        */
    }


    /// <summary>
    /// Represents a book with a name and a dictionary of time entries associated with specific dates.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Gets or sets the name of the book.
        /// </summary>
        /// <value>
        /// A string representing the name of the book. 
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that maps dates to time entries for the book.
        /// </summary>
        /// <value>
        /// A <see cref="Dictionary{String, String}"/> where the key is a date in "yyyy-MM-dd" format, 
        /// and the value is a string representing the time spent on that date.
        /// </value>
        public Dictionary<string, string> TimeDict { get; set; }
    }
}

