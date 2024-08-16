using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using EpubCSharp.code;
using Microsoft.UI.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static EpubCSharp.code.FileManagement;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubCSharp.app_pages
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EbookWindow : Window
    {
        //  the current ebook being read.
        private Ebook _ebook;
        private static readonly HttpClient Client = new HttpClient();

        // path to the XHTML file of the ebook
        private string _xhtmlPath = "";

        // tuple passed from the main window holding the ebook's play order and folder path.
        private (string ebookPlayOrder, string ebookFolderPath) _navValueTuple;

        // the last scroll position of the WebView
        private string _lastScroll = "0";

        private bool _chapterNavigated = false;

        private readonly string _emptyMessage = "*783kd4HJsn";

        // text selected by javascript
        private string _selectedText = "";

        private static Process _flaskProcess;

        private int _startUp = 0;

        private static bool _isArgosReady = false;

        /// <summary>
        /// Event handler for the WindowClosed event.
        /// </summary>
        public event EventHandler WindowClosed;

        /// <summary>
        /// Constructor initializes the component and subscribes to Loaded and Unloaded events.
        /// </summary>
        public EbookWindow((string ebookPlayOrder, string ebookFolderPath) data)
        {
            this.InitializeComponent();
            StartFlaskServerThread();
            PageStartup();
            //this.Closed += MainWindow_Closed;
            ChangeCommandBarColors();
            _navValueTuple = data;
            OpenEbookMessage(_navValueTuple);
            EbookViewer_Loaded();
            MyWebView.Focus(FocusState.Programmatic);
            ChangeBooksStatus();
        }

        /// <summary>
        /// Not working correctly.
        /// Handles the event when the size of the eBook window changes. This method adjusts the line height 
        /// of the text displayed in the WebView to ensure that the text is properly aligned and spaced based 
        /// on the new window size.
        /// </summary>
        /// <param name="sender">The source of the event, typically the eBook window.</param>
        /// <param name="args">The <see cref="WindowSizeChangedEventArgs"/> containing the event data, such as the new window size.</param>
        /// <remarks>
        /// The method performs the following tasks:
        /// <list type="bullet">
        /// <item><description>Ensures that the WebView's CoreWebView2 instance is initialized.</description></item>
        /// <item><description>Executes JavaScript in the WebView to obtain the document height, window height, and current scroll position.</description></item>
        /// <item><description>Calculates the optimal line height for the text based on the new window height, ensuring consistent text spacing.</description></item>
        /// <item><description>Logs relevant information for debugging purposes, such as document height, window height, and the calculated line height.</description></item>
        /// <item><description>Calls <see cref="UpdateLineWidth"/> to apply the new line height and reloads the WebView to reflect the changes.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="FormatException">Thrown if the height values returned from the WebView are not in a valid numeric format.</exception>
        private async void EbookWindow_OnSizeChanged(object sender, WindowSizeChangedEventArgs args)
        {

            /*
            int fontSize = 20;  // in px
            int lineHeight = 20; // in px

            MyWebView.EnsureCoreWebView2Async();
            string documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
            string  windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");
            string scrollValue = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");

            string bodyHeightScript = "document.body.scrollHeight;";
            string bodyHeightValue = await MyWebView.CoreWebView2.ExecuteScriptAsync(bodyHeightScript);


            double windowHeightValue = double.Parse(windowHeight);

            double diff = windowHeightValue / (double)lineHeight;
            int lines = (int)(Math.Floor(diff));

            double newLineHeight = lineHeight + (windowHeightValue - lines * lineHeight) / (double)lines;
            string newLineHeightString = $"{newLineHeight.ToString()}px";

            Debug.WriteLine("EbookWindowSize:");
            Debug.WriteLine($"Document Height = {documentHeight} | Window Height = {windowHeight}");
            Debug.WriteLine($"Scroll Value = {scrollValue}");
            Debug.WriteLine($"New Line Height = {newLineHeightString}");
            Debug.WriteLine("****************************************************");

            await UpdateLineWidth(newLineHeightString);

            MyWebView.Reload();
            //await RestorePositionAsync();
            */

        }

        /// <summary>
        /// Updates the line height in the CSS file used by the eBook viewer to the specified value.
        /// </summary>
        /// <param name="newFontFamily">The new line-height value to be set in the CSS file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="number">
        /// <item><description>Retrieves the path of the CSS file using <see cref="FileManagement.GetEbookViewerStyleFilePath"/>.</description></item>
        /// <item><description>Reads the existing content of the CSS file.</description></item>
        /// <item><description>Uses a regular expression to locate and replace the current <c>line-height</c> value in the <c>body</c> selector with the new value.</description></item>
        /// <item><description>If a <c>line-height</c> declaration is not found within the <c>body</c> selector, the method adds a new <c>line-height</c> property.</description></item>
        /// <item><description>Writes the modified CSS content back to the file.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="IOException">Thrown if there is an error reading from or writing to the CSS file.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="newFontFamily"/> is null or empty.</exception>
        /// <exception cref="RegexMatchTimeoutException">Thrown if the regular expression operation times out.</exception>
        private static async Task UpdateLineWidth(string newFontFamily)
        {

            string cssFilePath = FileManagement.GetEbookViewerStyleFilePath();

            // Read the existing CSS file
            string cssContent = File.ReadAllText(cssFilePath);

            // Regular expression to find the body Font-family declaration
            string pattern = @"(?<=body\s*{[^}]*?line-height:\s*).*?(?=;)";
            string replacement = newFontFamily;

            // Replace the existing Font-family for body with the new one
            string modifiedCssContent = Regex.Replace(cssContent, pattern, replacement, RegexOptions.Singleline);

            // If no Font-family was found, add it
            if (!Regex.IsMatch(cssContent, @"body\s*{[^}]*?line-height:"))
            {
                modifiedCssContent = Regex.Replace(modifiedCssContent, @"body\s*{", $"body {{\n    line-height: {newFontFamily};\n", RegexOptions.Singleline);
            }

            // Write the modified content back to the CSS file
            File.WriteAllText(cssFilePath, modifiedCssContent);
        }

        /// <summary>
        /// Handles the closing event of the main window. This method performs cleanup operations 
        /// and ensures that necessary actions are taken before the window is fully closed.
        /// </summary>
        /// <param name="sender">The source of the event, typically the main window.</param>
        /// <param name="args">The <see cref="WindowEventArgs"/> containing the event data.</param>
        /// <remarks>
        /// The method first detaches the <c>MainWindow_Closed</c> event handler to prevent any potential recursive calls. 
        /// It then stops the Flask server thread, saves the current position, and calculates the time difference.
        /// If these operations succeed, it invokes the <c>WindowClosed</c> event, signaling that the window has been closed.
        /// Finally, the window is closed. If an exception occurs during any of these operations, it is caught, and an error 
        /// message is logged for debugging purposes.
        /// </remarks>
        /// <exception cref="Exception">Thrown if there is an issue during the cleanup process or while performing any of the asynchronous tasks.</exception>
        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                this.Closed -= MainWindow_Closed;

                await StopFlaskServerThread();
                await SavePosition();
                CalculateTimeDifference();
                WindowClosed?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow_Closed() - Fail - {ex.Message}\n");
            }
        }

        /// <summary>
        /// Initializes the page settings by loading global configurations and setting up UI elements 
        /// like combo boxes and text boxes with the appropriate values from the settings.
        /// </summary>
        /// <remarks>
        /// This method deserializes the global settings from a JSON file and applies the values to the corresponding 
        /// UI elements on the page. It sets the selected index of the fonts and themes combo boxes, adjusts the padding, 
        /// and parses the font size from the settings. If the font size is valid, it is applied to the font size text box; 
        /// otherwise, a default message is set. Finally, the method calls <c>comboBoxesSetup()</c> to perform any additional 
        /// setup required for the combo boxes.
        /// </remarks>
        /// <exception cref="JsonException">Thrown if there is an error during the deserialization of the global settings JSON file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the global settings file is not found.</exception>
        /// <exception cref="FormatException">Thrown if the font size cannot be parsed to a double.</exception>
        private void PageStartup()
        {
            GlobalSettingsJson globalSettings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));

            FontsComboBox.SelectedIndex = SettingsPage.BookReadingFonts.IndexOf(globalSettings.Font);
            ThemesComboBox.SelectedIndex = SettingsPage.Themes.Keys.ToList().IndexOf(globalSettings.Theme);

            PaddingBox.Text = globalSettings.Padding;

            if (double.TryParse(globalSettings.FontSize.Split("rem")[0], out double fontSizeValue))
            {
                int fontSizeValueInt = (int)(fontSizeValue * 10);
                if (fontSizeValueInt > 0)
                {
                    FontSizeBox.Text = fontSizeValueInt.ToString();
                }
            }

            else
            {
                FontSizeBox.Text = "Provide valid fontsize = whole numbers > 0";
            }

            if (globalSettings.TranslationService == "argos")
            {
                CheckArgosState();
            }

            ComboBoxesSetup();
        }

        /// <summary>
        /// Populates the combo boxes on the page with available font and theme options.
        /// </summary>
        /// <remarks>
        /// This method iterates through the list of available book reading fonts and theme names 
        /// defined in the SettingsPage class. It adds each font to the FontsComboBox 
        /// and each theme to the <c>ThemesComboBox</c>. This setup ensures that users can select 
        /// from the predefined options when configuring the page settings.
        /// </remarks>
        private void ComboBoxesSetup()
        {
            foreach (var font in SettingsPage.BookReadingFonts)
            {
                FontsComboBox.Items.Add(font);
            }

            foreach (var theme in SettingsPage.Themes.Keys.ToList())
            {
                ThemesComboBox.Items.Add(theme);
            }
        }

        /// <summary>
        /// Updates the status of the eBook based on its current status. If the eBook status is "Not Started",
        /// it changes the status to "Reading". The updated status is then saved back to the JSON file.
        /// </summary>
        /// <remarks>
        /// This method reads the eBook data from a JSON file specified by the path in <c>_navValueTuple</c>. 
        /// It checks the current status of the eBook and performs the following updates:
        /// <list type="bullet">
        /// <item>
        /// <description>If the status is "Not Started", it changes the status to "Reading".</description>
        /// </item>
        /// <item>
        /// <description>If the status is "Finished" or "Reading", no changes are made.</description>
        /// </item>
        /// </list>
        /// After updating the status, the method serializes the modified eBook object and writes it back to the JSON file.
        /// </remarks>
        /// <exception cref="IOException">Thrown if there is an error reading from or writing to the eBook JSON file.</exception>
        /// <exception cref="JsonException">Thrown if there is an error during the serialization or deserialization of the eBook data.</exception>
        private void ChangeBooksStatus()
        {
            _ebook = JsonHandler.ReadEbookJsonFile(FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath));
            if (_ebook.Status == "Finished")
            {
            }
            else if (_ebook.Status == "Reading")
            {
            }
            else if (_ebook.Status == "Not Started")
            {
                _ebook.Status = "Reading";
            }
            File.WriteAllText(FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath), JsonSerializer.Serialize(_ebook));
        }

        /// <summary>
        /// Handles the event when the eBook window is closed. This method triggers a custom event 
        /// to notify subscribers that the window has been closed.
        /// </summary>
        /// <param name="sender">The source of the event, typically the eBook window.</param>
        /// <param name="e">The <see cref="EventArgs"/> containing the event data.</param>
        /// <remarks>
        /// This method raises the <c>WindowClosed</c> event, if there are any subscribers, to signal that 
        /// the eBook window has been closed. The event can be used to perform any additional cleanup 
        /// or notify other parts of the application about the window closure.
        /// </remarks>
        /// <exception cref="Exception">Thrown if there are issues with raising the event or if event handlers have errors.</exception>
        private void EbookWindow_Closed(object sender, EventArgs e)
        {
            // Trigger the custom WindowClosed event
            WindowClosed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Saves the current date and time as the time when the book was opened.
        /// </summary>
        /// <remarks>
        /// This asynchronous method updates the <c>BookOpenTime</c> property of the <c>_ebook</c> 
        /// object with the current date and time in string format. This information can be used 
        /// for tracking when the book was last opened or for other time-related features in the application.
        /// </remarks>
        /// <exception cref="Exception">Thrown if there is an issue accessing or modifying the <c>_ebook</c> object.</exception>
        private void SaveBookOpenTime()
        {
            // Save the time the book was opened
            _ebook.BookOpenTime = DateTime.Now.ToString();
        }

        /// <summary>
        /// Saves the current position and related information of the eBook. This includes 
        /// the current scroll position, in-book position, and the date/time when the book was last opened and closed.
        /// </summary>
        /// <param name="debug">Optional. If true, enables debug logging to track the success or failure of each operation.</param>
        /// <remarks>
        /// This asynchronous method performs the following actions:
        /// <list type="number">
        /// <item>
        /// <description>Attempts to retrieve and save the current scroll position from the WebView. If retrieval fails, a default value of "0" is used.</description>
        /// </item>
        /// <item>
        /// <description>Saves the current play order of the eBook to indicate the in-book position. If an error occurs, the operation is logged.</description>
        /// </item>
        /// <item>
        /// <description>Records the current date and time as the last opened date of the eBook. Any errors during this process are logged if debugging is enabled.</description>
        /// </item>
        /// </list>
        /// The method also updates the <c>BookCloseTime</c> property to the current date and time, and saves the updated eBook data to a JSON file.
        /// If any exception occurs during the operations, it is logged if debugging is enabled. The final block ensures that the eBook data is 
        /// saved even if some parts of the operation fail.
        /// </remarks>
        /// <exception cref="Exception">Thrown if there is an issue with retrieving the scroll position, saving the eBook data, or handling the eBook file.</exception>
        private async Task SavePosition(bool debug = false)
        {
            try
            {
                try
                {
                    _ebook.ScrollValue = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");
                    if (debug) { Debug.WriteLine($"SavePosition() 1 - Success"); }
                }
                catch (Exception ex)
                {
                    _ebook.ScrollValue = "0";
                    if (debug) { Debug.WriteLine($"SavePosition() 1 - Fail - {ex.Message}"); }
                }

                try
                {
                    _ebook.InBookPosition = _navValueTuple.ebookPlayOrder;
                    if (debug) { Debug.WriteLine($"SavePosition() 2 - Success"); }
                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine($"SavePosition() 2 - Fail - {ex.Message}"); }
                }

                try
                {
                    _ebook.DateLastOpened = DateTime.Now.ToString();
                    if (debug) { Debug.WriteLine($"SavePosition() 3 - Success"); }
                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine($"SavePosition() 3 - Fail - {ex.Message}"); }
                }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"An error occurred in SavePosition: {ex.Message}"); }
            }

            finally
            {
                try
                {

                    _ebook.BookCloseTime = DateTime.Now.ToString();

                    // Get the file path
                    string filePath = FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath);
                    filePath = Path.GetDirectoryName(filePath);

                    // Store the JSON ebook file
                    JsonHandler.StoreJsonEbookFile(_ebook, filePath);
                    if (debug) { Debug.WriteLine($"SavePosition() - Success"); }

                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine($"SavePosition() - Fail - {ex.Message}"); }
                }
            }
        }

        /// <summary>
        /// Calculates the difference between the time the book was opened and the time it was closed. 
        /// It updates the total reading time and stores the updated information in the eBook data.
        /// </summary>
        /// <param name="debug">Optional. If true, enables debug logging to track the success or failure of the operations.</param>
        /// <remarks>
        /// This asynchronous method performs the following actions:
        /// <list type="number">
        /// <item>
        /// <description>Attempts to parse and set the <c>openTime</c> from the <c>BookOpenTime</c> property. If parsing fails, the current time is used.</description>
        /// </item>
        /// <item>
        /// <description>Attempts to parse and set the <c>closeTime</c> from the <c>BookCloseTime</c> property. If parsing fails, the current time is used.</description>
        /// </item>
        /// <item>
        /// <description>Calculates the time difference between <c>closeTime</c> and <c>openTime</c>. It then adds this difference to the previously recorded reading time.</description>
        /// </item>
        /// <item>
        /// <description>Updates the <c>BookReadTime</c> property with the total reading time, which includes the time difference.</description>
        /// </item>
        /// <item>
        /// <description>Saves the updated eBook data to a JSON file and logs the success or failure of the operation if debugging is enabled.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">Thrown if there is an issue with parsing date and time values, calculating time differences, or storing the eBook data.</exception>
        private void CalculateTimeDifference(bool debug = false)
        {
            // Calculate the time the book was open
            try
            {
                DateTime openTime = DateTime.Now;
                DateTime closeTime = DateTime.Now;
                try
                {
                    openTime = DateTime.Parse(_ebook.BookOpenTime);
                }

                catch
                {
                    openTime = DateTime.Now;
                }

                try
                {
                    closeTime = DateTime.Parse(_ebook.BookCloseTime);

                }

                catch
                {
                    closeTime = DateTime.Now;
                }

                TimeSpan readTime = new TimeSpan(0, 0, 0, 0);
                TimeSpan timeDifference = new TimeSpan(0, 0, 0, 0);
                TimeSpan totalTime;

                try
                {
                    readTime = readTime + TimeSpan.Parse(_ebook.BookReadTime);
                }

                catch
                {
                    readTime = new TimeSpan(0, 0, 0, 0);
                }

                finally
                {
                    timeDifference = closeTime - openTime;
                    totalTime = readTime + timeDifference;
                    _ebook.BookReadTime = totalTime.ToString();

                    StoreEbookStats(timeDifference);
                    // Get the file path
                    string filePath = FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath);
                    filePath = Path.GetDirectoryName(filePath);

                    // Store the JSON ebook file
                    JsonHandler.StoreJsonEbookFile(_ebook, filePath);
                }

                if (debug) { Debug.WriteLine($"CalculateTimeDifference() - Success\n"); }
            }

            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"CalculateTimeDifference() - Fail - {ex.Message}\n"); }
            }

        }

        /// <summary>
        /// Stores the time difference for the current date in the eBook's statistics record.
        /// </summary>
        /// <param name="timeDifference">The <see cref="TimeSpan"/> representing the duration to be recorded.</param>
        /// <param name="debug">Optional. If true, enables debug logging to track the success or failure of the operation.</param>
        /// <remarks>
        /// This asynchronous method updates the eBook's statistics record by storing the given <paramref name="timeDifference"/> 
        /// for the current date. If there is an existing record for the current date, it adds the time difference to the existing 
        /// value. If no record exists for the current date, it creates a new entry.
        ///
        /// If <c>StatsRecord1</c> is null, it is initialized with a new dictionary and the time difference is added.
        /// If <c>StatsRecord1</c> is empty, a new dictionary is created and the time difference is added.
        /// If <c>StatsRecord1</c> contains an entry for the current date, the existing value is updated with the new time difference.
        ///
        /// The method also handles exceptions that may occur during this process and logs them if debugging is enabled.
        /// </remarks>
        /// <exception cref="Exception">Thrown if there is an issue with updating the eBook's statistics record or handling the time span.</exception>
        private void StoreEbookStats(TimeSpan timeDifference, bool debug = false)
        {
            try
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // Check if StatsRecord1 is null or empty
                if (_ebook.StatsRecord1 != null && _ebook.StatsRecord1.Count > 0)
                {
                    if (_ebook.StatsRecord1.ContainsKey(currentDate))
                    {
                        TimeSpan timeSpan;
                        timeSpan = TimeSpan.Parse(_ebook.StatsRecord1[currentDate]);
                        _ebook.StatsRecord1[currentDate] = (timeSpan + timeDifference).ToString();
                    }
                    else
                    {
                        _ebook.StatsRecord1.Add(currentDate, timeDifference.ToString());
                    }

                }
                else if (_ebook.StatsRecord1 == null)
                {
                    _ebook.StatsRecord1 = new Dictionary<string, string>();
                    _ebook.StatsRecord1.Add(currentDate, timeDifference.ToString());
                }

                else
                {
                    _ebook.StatsRecord1 = new Dictionary<string, string>();
                    _ebook.StatsRecord1.Add(currentDate, timeDifference.ToString());
                }

                if (debug) { Debug.WriteLine($"StoreEbookStats() - Success"); }

            }

            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"StoreEbookStats() - Fail - {ex.Message}"); }
            }
        }

        /// <summary>
        /// Restores the reading position of the eBook by scrolling to the previously saved position and loading the appropriate content.
        /// </summary>
        /// <param name="debug">Optional. If true, enables debug logging to track the success or failure of the operation.</param>
        /// <remarks>
        /// This asynchronous method performs the following actions:
        /// <list type="bullet">
        /// <item>
        /// <description>Reads the eBook data from the JSON file located at the path specified in <c>_navValueTuple.ebookFolderPath</c>.</description>
        /// </item>
        /// <item>
        /// <description>Ensures that the WebView2 control is properly initialized.</description>
        /// </item>
        /// <item>
        /// <description>Scrolls the WebView2 control to the vertical position specified by the <c>ScrollValue</c> property of the eBook.</description>
        /// </item>
        /// <item>
        /// <description>Sets the XHTML content file path based on the eBook’s current position using <c>InBookPosition</c>.</description>
        /// </item>
        /// </list>
        /// If an exception occurs during any of these operations, it will be logged if debugging is enabled.
        /// </remarks>
        /// <exception cref="Exception">Thrown if there is an issue with reading the eBook data, initializing WebView2, or executing the script.</exception>
        private async Task RestorePositionAsync(bool debug = false)
        {
            try
            {
                _ebook = JsonHandler.ReadEbookJsonFile(FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath));
                await MyWebView.EnsureCoreWebView2Async(null);
                await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, {_ebook.ScrollValue});");
                _xhtmlPath = FileManagement.GetBookContentFilePath(_navValueTuple.ebookFolderPath, _ebook.InBookPosition);
                if (debug) { Debug.WriteLine($"RestorePositionAsync() - Success"); }
            }

            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"RestorePositionAsync() - Fail - {ex.Message}"); }
            }
        }

        /// <summary>
        /// Asynchronously handles the initialization and positioning of the ebook viewer when it is loaded.
        /// </summary>
        /// <param name="debug">
        /// Optional. A boolean flag indicating whether debug information should be logged. Defaults to <c>false</c>.
        /// </param>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///   <item>
        ///     Initializes the web view asynchronously using <see cref="InitializeWebViewAsync"/>.
        ///   </item>
        ///   <item>
        ///     Restores the previously saved scroll position using <see cref="RestorePositionAsync"/>.
        ///   </item>
        ///   <item>
        ///     Continuously compares the current scroll position of the ebook viewer with the value obtained from the web view script execution.
        ///     If the scroll value is "null", it is set to "0". This comparison and restoration loop continues until the scroll position matches.
        ///   </item>
        /// </list>
        /// In case of an exception, the method logs a failure message if the <paramref name="debug"/> parameter is set to <c>true</c>.
        /// Regardless of success or failure, it ensures that the book's open time is saved by calling <see cref="SaveBookOpenTime"/>.
        /// </remarks>
        /// <exception cref="Exception">
        /// Throws any exceptions that occur during the initialization or scroll restoration processes.
        /// </exception>
        private async Task EbookViewer_Loaded(bool debug = false)
        {
            try
            {
                await InitializeWebViewAsync();
                await RestorePositionAsync();

                while (_ebook.ScrollValue != await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;"))
                {
                    if (_ebook.ScrollValue == "null")
                    {
                        _ebook.ScrollValue = "0";
                    }
                    RestorePositionAsync();
                }
                if (debug) { Debug.WriteLine($"EbookViewer_Loaded() - Success"); }
            }
            catch
            {
                if (debug) { Debug.WriteLine($"EbookViewer_Loaded() - Fail"); }
            }
            finally
            {
                SaveBookOpenTime();

            }
        }

        /// <summary>
        /// Asynchronously initializes the WebView control and configures its settings and content.
        /// </summary>
        /// <param name="debug">
        /// Optional. A boolean flag indicating whether debug information should be logged. Defaults to <c>false</c>.
        /// </param>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///   <item>
        ///     Ensures the WebView's core environment is initialized by calling <see cref="WebView2.EnsureCoreWebView2Async()"/>.
        ///   </item>
        ///   <item>
        ///     Subscribes to the <see cref="CoreWebView2_WebMessageReceived"/> event to handle messages from the web content.
        ///   </item>
        ///   <item>
        ///     Enables JavaScript execution in the WebView by setting MyWebView.CoreWebView2.Settings.IsScriptEnabled = true; to <c>true</c>.
        ///   </item>
        ///   <item>
        ///     Retrieves the path to the ebook content file using <see cref="FileManagement.GetBookContentFilePath"/> and sets it as the source of the WebView.
        ///   </item>
        /// </list>
        /// If the method completes successfully and <paramref name="debug"/> is set to <c>true</c>, a success message is logged.
        /// If an exception occurs, it logs the error message if <paramref name="debug"/> is set to <c>true</c>.
        /// </remarks>
        /// <exception cref="Exception">
        /// Throws any exceptions that occur during the initialization process, such as failure to initialize the WebView or set its source.
        /// </exception>
        private async Task InitializeWebViewAsync(bool debug = false)
        {
            try
            {
                await MyWebView.EnsureCoreWebView2Async(null);
                MyWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                MyWebView.CoreWebView2.Settings.IsScriptEnabled = true;
                _xhtmlPath = FileManagement.GetBookContentFilePath(_navValueTuple.ebookFolderPath, _navValueTuple.ebookPlayOrder);
                MyWebView.Source = new Uri(_xhtmlPath);
                if (debug) { Debug.WriteLine($"InitializeWebViewAsync() - Success\n"); }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"InitializeWebViewAsync() - Fail - {ex.Message}\n"); }
            }
        }

        /// <summary>
        /// Handles the WebMessageReceived event for the CoreWebView2 control, processing messages received from the web content.
        /// </summary>
        /// <param name="sender">
        /// The source of the event, which is expected to be the <see cref="CoreWebView2"/> control.
        /// </param>
        /// <param name="e">
        /// An instance of <see cref="CoreWebView2WebMessageReceivedEventArgs"/> containing data related to the received web message.
        /// </param>
        /// <remarks>
        /// This method processes messages sent from the web content based on their format and content:
        /// <list type="bullet">
        ///   <item>
        ///     If the message contains an equals sign ("="), it is split into a type and content based on the equals sign. If the content is not equal to the value of <see cref="_emptyMessage"/>, 
        ///     it triggers the <see cref="ShowFlyoutAsync"/> method to display a flyout with the message type and content.
        ///   </item>
        ///   <item>
        ///     If the message is "scrolledDown" (after trimming whitespace), the <see cref="MoveForward"/> method is called to handle forward scrolling.
        ///   </item>
        ///   <item>
        ///     If the message is "scrolledUp" (after trimming whitespace), the <see cref="MoveBackward"/> method is called to handle backward scrolling.
        ///   </item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the message content or any required components are unexpectedly null or missing.
        /// </exception>
        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            if (message.Contains("="))
            {
                //Debug.WriteLine($"CoreWebView2_WebMessageReceived = {message}");

                string messageType = message.Split(" = ")[0];
                string messageContent = message.Split(" = ")[1];

                if (messageContent != _emptyMessage)
                {
                    // open flyout
                    // Update the Flyout content
                    ShowFlyoutAsync(messageType, messageContent);

                }
            }
            else if (message.Trim() == "scrolledDown")
            {
                MoveForward();
            }

            else if (message.Trim() == "scrolledUp")
            {
                MoveBackward();
            }
        }

        /// <summary>
        /// Removes specified leading or trailing characters from a given string.
        /// </summary>
        /// <param name="text">
        /// The string from which characters will be removed.
        /// </param>
        /// <param name="charsToRemove">
        /// An array of characters to be removed from the string.
        /// </param>
        /// <param name="removeLeading">
        /// A boolean value indicating whether to remove leading characters (<c>true</c>) or trailing characters (<c>false</c>).
        /// </param>
        /// <returns>
        /// A new string with the specified leading or trailing characters removed.
        /// </returns>
        /// <remarks>
        /// If <paramref name="removeLeading"/> is <c>true</c>, characters from the start of the string that match any character in <paramref name="charsToRemove"/> are removed.
        /// If <paramref name="removeLeading"/> is <c>false</c>, characters from the end of the string that match any character in <paramref name="charsToRemove"/> are removed.
        /// The method stops removing characters when a character not in <paramref name="charsToRemove"/> is encountered or the boundary of the string is reached.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="text"/> or <paramref name="charsToRemove"/> parameters are <c>null</c>.
        /// </exception>
        private static string RemoveLeadingTrailingSymbols(string text, char[] charsToRemove, bool removeLeading)
        {
            int index = removeLeading ? 0 : text.Length - 1;
            int step = removeLeading ? 1 : -1;

            while (index >= 0 && index < text.Length && Array.Exists(charsToRemove, c => c == text[index]))
            {
                text = text.Remove(index, 1);
                index += step;
            }

            return text;
        }

        /// <summary>
        /// Asynchronously removes specified leading and trailing symbols from a given string.
        /// </summary>
        /// <param name="text">
        /// The string from which symbols will be removed.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the string with the specified symbols removed from the start and end.
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Defines an array of symbols to be removed, including punctuation, whitespace, and various other special characters.
        ///   </item>
        ///   <item>
        ///     Calls <see cref="RemoveLeadingTrailingSymbols"/> to remove symbols from the start of the string that match any character in the defined array.
        ///   </item>
        ///   <item>
        ///     Calls <see cref="RemoveLeadingTrailingSymbols"/> again to remove symbols from the end of the string that match any character in the defined array.
        ///   </item>
        ///   <item>
        ///     Returns the modified string with the specified symbols removed from both ends.
        ///   </item>
        /// </list>
        /// This method performs synchronous operations within the asynchronous context and does not actually perform any asynchronous operations.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="text"/> parameter is <c>null</c>.
        /// </exception>

        private static async Task<string> RemoveSymbols(string text)
        {
            // Original text
            // Define an array of characters to remove if they are at the start or end
            char[] charsToRemove = { '.', ',', '!', '?', ':', ';', ' ', '\n', '\r', '\t', '\u2581', '"', '“', '”', '‘', '’', '(', ')', '[', ']', '{', '}', '<', '>', '„', '«', '»' };

            // Remove symbols from the start if they are in the charsToRemove array
            text = RemoveLeadingTrailingSymbols(text, charsToRemove, true);

            // Remove symbols from the end if they are in the charsToRemove array
            text = RemoveLeadingTrailingSymbols(text, charsToRemove, false);

            // Output the final result
            return text;
        }

        /// <summary>
        /// Asynchronously repairs a given text by addressing specific translation issues and removing extraneous whitespace.
        /// </summary>
        /// <param name="text">
        /// The text to be repaired.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the repaired text with specified issues corrected and trailing whitespace removed.
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Replaces occurrences of the Unicode character <c>\u2581</c> (a lower one eighth block) with a space character. This is specifically aimed at fixing translation issues from Argos translate in English to Czech translations.
        ///   </item>
        ///   <item>
        ///     Removes any leading or trailing whitespace characters from the text using <see cref="string.Trim"/>.
        ///   </item>
        /// </list>
        /// Note that the method is defined as asynchronous but does not perform any asynchronous operations. The <c>async</c> keyword is used to ensure compatibility with asynchronous programming patterns.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="text"/> parameter is <c>null</c>.
        /// </exception>
        private static string RepairTranslation(string text)
        {
            text = text.Replace("\u2581", " ");   // argos translate misbehavior in en->cs translations

            // remove trailing whitespaces
            text = text.Trim();

            return text;
        }

        /// <summary>
        /// Retrieves the language code for a given language name from a JSON dictionary file.
        /// </summary>
        /// <param name="endoLanguageName">
        /// The name of the language for which the code is to be retrieved.
        /// </param>
        /// <param name="debug">
        /// Optional. A boolean flag indicating whether debug information should be logged. Defaults to <c>false</c>.
        /// </param>
        /// <returns>
        /// The language code corresponding to the specified language name. Returns "en" if the language name is not found or an error occurs.
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Reads and deserializes the global settings JSON file into a <see cref="GlobalSettingsJson"/> object. (Note: The deserialized object is not used in this method but is part of the initial setup.)
        ///   </item>
        ///   <item>
        ///     Reads and deserializes a JSON file containing a dictionary of language names to language codes. This file is located at a hardcoded path.
        ///   </item>
        ///   <item>
        ///     Retrieves the language code for the specified <paramref name="endoLanguageName"/> from the dictionary.
        ///   </item>
        ///   <item>
        ///     Logs the success or failure of the operation if <paramref name="debug"/> is <c>true</c>, including the retrieved code or the exception message.
        ///   </item>
        /// </list>
        /// If an exception occurs, the method returns "en" as a default language code.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="endoLanguageName"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the specified JSON file at the hardcoded path is not found.
        /// </exception>
        /// <exception cref="JsonException">
        /// Thrown if there is an error in deserializing the JSON files.
        /// </exception>
        private string GetLanguageCode(string endoLanguageName, bool debug = false)
        {
            try
            {

                

                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));

                // Combine the installed location path with the relative path
                string path = Path.Combine(AppContext.BaseDirectory, "Assets\\iso639I_reduced.json");

                string json = File.ReadAllText(path);
                Dictionary<string, string> languageDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                string code = languageDict[endoLanguageName];
                if (debug) { Debug.WriteLine($"GetLanguageCode() - Success - {code}"); }
                return code;
            }

            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"GetLanguageCode() - Fail - {ex.Message}"); }
                return "en";
            }
        }

        /// <summary>
        /// Asynchronously displays a flyout with translation or content based on the provided message type and content.
        /// </summary>
        /// <param name="messageType">
        /// The type of message that determines how the flyout should be presented (e.g., "CLICKED WORD").
        /// </param>
        /// <param name="messageContent">
        /// The content to be displayed and translated in the flyout.
        /// </param>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     If <paramref name="messageContent"/> is different from a stored value (<c>_selectedText</c>), it processes the content based on the <paramref name="messageType"/>.
        ///   </item>
        ///   <item>
        ///     For the "CLICKED WORD" message type, it removes certain punctuation and whitespace characters from <paramref name="messageContent"/> by calling <see cref="RemoveSymbols"/>.
        ///   </item>
        ///   <item>
        ///     Creates and configures a <see cref="Flyout"/> control to display the message content and additional translation results.
        ///   </item>
        ///   <item>
        ///     Configures the flyout's placement, show mode, and event handling (e.g., focusing a text box when closed).
        ///   </item>
        ///   <item>
        ///     Creates a <see cref="StackPanel"/> to hold the content of the flyout, including:
        ///     <list type="bullet">
        ///       <item>
        ///         A <see cref="TextBlock"/> displaying the original message content with bold text and ellipsis if it overflows.
        ///       </item>
        ///       <item>
        ///         A <see cref="ScrollViewer"/> containing another <see cref="TextBlock"/> that will display the translation or processed result.
        ///       </item>
        ///       <item>
        ///         A <see cref="Button"/> labeled "Save" that, when clicked, hides the flyout, saves the translation, and focuses a text box.
        ///       </item>
        ///     </list>
        ///   </item>
        ///   <item>
        ///     Determines the translation service to use based on global settings and retrieves the translation using either Argos or MyMemory.
        ///   </item>
        ///   <item>
        ///     Processes the translation result by removing symbols or repairing the translation based on <paramref name="messageType"/>.
        ///   </item>
        ///   <item>
        ///     Sets the flyout content to the configured stack panel and displays the flyout at a specified control (e.g., <c>MyWebView</c>).
        ///   </item>
        /// </list>
        /// If <paramref name="messageContent"/> matches the stored <c>_selectedText</c>, it simply updates the stored text.
        /// </remarks>
        /// <exception cref="Exception">
        /// Handles exceptions related to file access, JSON deserialization, or translation services, falling back to default behavior in case of errors.
        /// </exception>
        private async Task ShowFlyoutAsync(string messageType, string messageContent)
        {

            if (messageContent != _selectedText)
            {
                //Debug.WriteLine($"ShowFlyoutAsync - {messageType} - {messageContent}");
                if (messageType == "CLICKED WORD")
                {
                    // remove any . , ! ? : ; symbols from the text
                    messageContent = await RemoveSymbols(messageContent);
                }

                // Create a new Flyout
                Flyout flyout = new Flyout();

                // set target


                flyout.Placement = FlyoutPlacementMode.Bottom;
                flyout.ShowMode = FlyoutShowMode.Transient;
                flyout.Closed += (s, e) =>
                {
                    MyWebView.Focus(FocusState.Programmatic);
                };


                // Create content for the Flyout
                StackPanel stackPanel = new StackPanel
                {
                    Width = 200,

                };

                TextBlock textBlock1 = new TextBlock { Text = $"{messageContent}" };
                textBlock1.FontWeight = FontWeights.Bold;
                textBlock1.TextTrimming = TextTrimming.CharacterEllipsis; // Trims at the character level and adds ...



                // Create and configure the second TextBlock
                TextBlock textBlock2 = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = stackPanel.Width
                };

                // Create the ScrollViewer and set its properties
                ScrollViewer scrollViewer = new ScrollViewer
                {
                    Content = textBlock2,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto, // Shows scrollbar if needed
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto // Shows scrollbar if needed

                };


                scrollViewer.Width = stackPanel.Width;
                scrollViewer.Height = stackPanel.Height;

                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                string sourceLanguage = GetLanguageCode(_ebook.Language);
                string targetLanguage = GetLanguageCode(settings.Language);

                string result = "";

                if (settings.TranslationService == "argos")
                {
                    result = await PerformTranslation(messageContent, sourceLanguage, targetLanguage);
                }

                else if (settings.TranslationService == "My Memory")
                {
                    result = await GetTranslationMymemory(messageContent, sourceLanguage, targetLanguage);
                }


                if (messageType == "CLICKED WORD")
                {
                    result = await RemoveSymbols(result);
                }
                else
                {
                    result = RepairTranslation(result);
                }

                textBlock2.Text = result;

                Button closeButton = new Button { Content = "Save" };
                closeButton.Margin = new Thickness(0, 5, 0, 0);


                // Add close functionality
                closeButton.Click += async (s, e) =>
                {
                    flyout.Hide();
                    StoreTranslation(sourceLanguage, targetLanguage, messageContent, result);
                    MyWebView.Focus(FocusState.Programmatic);


                };

                // Add elements to the StackPanel
                stackPanel.Children.Add(textBlock1);
                stackPanel.Children.Add(textBlock2);
                stackPanel.Children.Add(closeButton);

                // Set the content of the Flyout
                flyout.Content = stackPanel;

                // Show the Flyout at a specific position after the Python script has finished
                flyout.ShowAt(MyWebView); // 'MyWebView' refers to the control or page where the Flyout is shown
            }
            else
            {
                _selectedText = messageContent;
            }

            MyWebView.Focus(FocusState.Programmatic);

        }

        /// <summary>
        /// Adds or updates a translation entry in the global dictionary.
        /// </summary>
        /// <param name="sourceLanguage">The source language code.</param>
        /// <param name="targetLanguage">The target language code.</param>
        /// <param name="originalText">The original text to translate.</param>
        /// <param name="translatedText">The translated text.</param>
        /// <remarks>
        /// Reads the global dictionary JSON file, adds or updates the translation entry if it doesn't already exist,
        /// and writes the updated dictionary back to the file. The web view is then focused programmatically.
        /// </remarks>
        /// <exception cref="Exception">Thrown for file access or serialization issues.</exception>

        private void StoreTranslation(string sourceLanguage, string targetLanguage, string originalText, string translatedText)
        {

            string dictPath = FileManagement.GetGlobalDictPath();

            GlobalDictJson globalDict = JsonSerializer.Deserialize<GlobalDictJson>(File.ReadAllText(dictPath));

            if (!globalDict.TranslationsDict.ContainsKey(originalText))
            {
                globalDict.TranslationsDict.Add(originalText, new List<string>() { sourceLanguage, targetLanguage, translatedText });
            }

            File.WriteAllText(FileManagement.GetGlobalDictPath(), JsonSerializer.Serialize(globalDict));
            MyWebView.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Executes a Python script with specified arguments and returns the script's output.
        /// </summary>
        /// <param name="text">The text to be processed by the Python script.</param>
        /// <param name="sourceLanguage">The source language code.</param>
        /// <param name="targetLanguage">The target language code.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the output of the Python script.</returns>
        /// <remarks>
        /// The method starts a Python process using the provided interpreter and script paths, passing the arguments to the script.
        /// It reads the standard output and error streams asynchronously, waits for the process to exit, and then returns the output.
        /// Any errors are logged for debugging purposes.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the Python process fails to start.</exception>

        private static async Task<string> RunPythonScript(string text, string sourceLanguage, string targetLanguage, bool debug = false)
        {
            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));

            // Combine the installed location path with the relative path
            string path = Path.Combine(AppContext.BaseDirectory, "code\\translation_script.py");


            // Path to the Python interpreter and the script
            string pythonPath = settings.PythonPath;
            string scriptPath = path;

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" \"{text}\" \"{sourceLanguage}\" \"{targetLanguage}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(start))
            {
                // Ensure the process is started correctly
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start the Python process.");
                }

                // Read the output and error asynchronously
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                // Wait for the process to exit
                await process.WaitForExitAsync(); // Use WaitForExitAsync for better async handling

                // Retrieve the results
                string output = await outputTask;
                string errors = await errorTask;

                if (!string.IsNullOrEmpty(errors))
                {
                    // Handle errors as needed
                    if (debug) { Debug.WriteLine($"Python errors: {errors}"); }
                }

                return output.Trim();
            }
        }

        /// <summary>
        /// Updates the CSS path in an XHTML document by modifying the href attribute of the stylesheet link element.
        /// </summary>
        /// <param name="xhtmlPath">The path to the XHTML file.</param>
        /// <param name="newCssPath">The new path to the CSS file to be set.</param>
        /// <param name="debug">Optional. A boolean flag indicating whether to log debug information. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// The method loads the XHTML document, searches for a <c>&lt;link&gt;</c> element with a <c>rel</c> attribute of "stylesheet",
        /// updates its <c>href</c> attribute to the <paramref name="newCssPath"/>, and saves the document.
        /// If the element is not found or an error occurs, it logs the appropriate message if <paramref name="debug"/> is <c>true</c>.</remarks>
        /// <exception cref="Exception">Thrown for issues related to file access or XML processing.</exception>
        private void UpdateCssPath(string xhtmlPath, string newCssPath, bool debug = false)
        {
            try
            {
                XDocument xhtmlDocument = XDocument.Load(xhtmlPath);
                var linkElement = xhtmlDocument.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == "link" && e.Attribute("rel")?.Value == "stylesheet");

                if (linkElement != null)
                {
                    linkElement.SetAttributeValue("href", newCssPath);
                    xhtmlDocument.Save(xhtmlPath);
                    if (debug) { Debug.WriteLine("UpdateCssPath() - Success\n"); }
                }
                else
                {
                    if (debug) { Debug.WriteLine("No <link> element with rel=\"stylesheet\" found."); }
                }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"UpdateCssPath() - Fail - {ex.Message}\n"); }
            }
        }

        /// <summary>
        /// Reloads the WebView to apply CSS changes and restores the scroll position after navigation completes.
        /// </summary>
        /// <param name="debug">Optional. A boolean flag indicating whether to log debug information. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// This method reloads the <see cref="MyWebView"/> to apply any CSS updates. It attaches an event handler to <see cref="WebView.NavigationCompleted"/> 
        /// to restore the scroll position once navigation is complete and successful. Logs messages if <paramref name="debug"/> is <c>true</c>.</remarks>
        /// <exception cref="Exception">Handles any exceptions that occur during the reload or event handling.</exception>

        private void UpdateCSSAction(bool debug = false)
        {
            try
            {
                MyWebView.Reload(); // Reload the WebView to apply CSS changes
                MyWebView.NavigationCompleted += async (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        await RestorePositionAsync(); // Restore scroll position
                    }
                    else
                    {
                        if (debug) { Debug.WriteLine("Navigation failed."); }
                    }
                };

                if (debug) { Debug.WriteLine("UpdateCSSAction() - Success\n"); }
            }

            catch
            {
                if (debug) { Debug.WriteLine("UpdateCSSAction() - Fail\n"); }
            }

        }

        /// <summary>
        /// Handles the action to perform when navigating to the home state, including stopping services, saving state, and closing the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///   <item>
        ///     Reads and deserializes global settings to check if the translation service is "argos". If so, it stops the Flask server.
        ///   </item>
        ///   <item>
        ///     Saves the current position and calculates the time difference since the last operation.
        ///   </item>
        ///   <item>
        ///     Triggers the <c>WindowClosed</c> event and closes the window.
        ///   </item>
        /// </list>
        /// Logs any exceptions encountered during the operation if debug mode is enabled.
        /// </remarks>
        /// <exception cref="Exception">Logs the exception message if an error occurs during execution.</exception>
        private async void GoHomeAction(object sender, RoutedEventArgs e)
        {
            try
            {

                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                if (settings.TranslationService == "argos")
                {
                    StopFlaskServer();
                }

                await SavePosition();
                CalculateTimeDifference();
                WindowClosed?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GoHomeAction() - Fail - {ex.Message}\n");
            }
        }

        /// <summary>
        /// Runs when the user clicks the Backward button. Scrolls up in the WebView. If the user has scrolled to the top of the page, navigates to the previous page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Backward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MoveBackward();
            }
            catch
            {
                Debug.WriteLine("Backward_Click() - Fail\n");
            }

        }

        /// <summary>
        /// Runs when the user clicks the Forward button. Scrolls down in the WebView. If the user has scrolled to the bottom of the page, navigates to the next page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Forward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MoveForward();
            }
            catch
            {
                Debug.WriteLine("Forward_Click() - Fail\n");
            }
        }

        /// <summary>
        /// Scrolls the WebView content backward by one viewport height and updates the saved position.
        /// </summary>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        ///   <item>Sets the <c>_chapterNavigated</c> flag to <c>false</c>.</item>
        ///   <item>Scrolls the WebView up by the height of the viewport using JavaScript.</item>
        ///   <item>Saves the current scroll position.</item>
        ///   <item>Checks if further backward navigation is possible.</item>
        /// </list>
        /// </remarks>
        private async void MoveBackward()
        {
            _chapterNavigated = false;
            await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollBy(0, -window.innerHeight);");
            await SavePosition();
            CheckBackward();
        }

        /// <summary>
        /// Scrolls the WebView content forward by one viewport height and updates the saved position.
        /// </summary>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        ///   <item>Sets the <c>_chapterNavigated</c> flag to <c>false</c>.</item>
        ///   <item>Scrolls the WebView down by the height of the viewport using JavaScript.</item>
        ///   <item>Saves the current scroll position.</item>
        ///   <item>Checks if further forward navigation is possible.</item>
        /// </list>
        /// </remarks>
        private async void MoveForward()
        {
            _chapterNavigated = false;
            await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollBy(0, window.innerHeight);");
            await SavePosition();
            CheckForward();
        }

        /// <summary>
        /// Moves to the next chapter in the ebook, handles the end of the book, and updates the ebook status accordingly.
        /// </summary>
        /// <param name="debug">Optional. A boolean flag indicating whether to log debug information. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        ///   <item>
        ///     Checks if <c>_chapterNavigated</c> is <c>false</c> and initializes required components.
        ///   </item>
        ///   <item>
        ///     If the current chapter is the last one, shows a dialog to ask if the user wants to continue or go home.
        ///     <list type="bullet">
        ///       <item>
        ///         If the user chooses to go home, updates the ebook status to "Finished", saves the updated data, and closes the window.
        ///       </item>
        ///     </list>
        ///   </item>
        ///   <item>
        ///     If there are more chapters, updates the <c>ebookPlayOrder</c> to the next chapter, sets the WebView source to the new chapter's path, 
        ///     and scrolls to the top of the document upon navigation completion.
        ///   </item>
        ///   <item>
        ///     Saves the current position and logs debug information if <paramref name="debug"/> is <c>true</c>.</item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">Logs any exception messages if errors occur during the process.</exception>
        private async void MoveToNextChapter(bool debug = false)
        {

            if (!_chapterNavigated)
            {
                try
                {
                    // Ensure _navValueTuple is initialized
                    if (_navValueTuple == default)
                    {
                        throw new InvalidOperationException("_navValueTuple is not initialized.");
                    }

                    int playOrder = int.Parse(_navValueTuple.ebookPlayOrder);

                    // TO-DO fix book finished error

                    List<string> playOrderList = _ebook.NavData.Keys.ToList();
                    int maxPlayOrder = playOrderList.Count;

                    if ((playOrder + 1) > maxPlayOrder)
                    {
                        // Show the ContentDialog after the method executes
                        ContentDialog dialog = new ContentDialog
                        {
                            Title = "Ebook Finished",
                            Content = "Do you want to keep reading?",
                            PrimaryButtonText = "Yes",
                            SecondaryButtonText = "Go Home",
                            XamlRoot = this.Content.XamlRoot // Set the XamlRoot property

                        };

                        ContentDialogResult result = await dialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            // Handle Yes response
                        }
                        else if (result == ContentDialogResult.Secondary)
                        {
                            try
                            {
                                _ebook = JsonHandler.ReadEbookJsonFile(FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath));
                                _ebook.Status = "Finished";
                                File.WriteAllText(FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath), JsonSerializer.Serialize(_ebook));


                                _navValueTuple.ebookPlayOrder = 1.ToString();
                                await SavePosition();
                                CalculateTimeDifference();
                                WindowClosed?.Invoke(this, EventArgs.Empty);
                                this.Close();

                                if (debug) { Debug.WriteLine("GoHomeAction() - Success\n"); }
                            }
                            catch (Exception ex)
                            {
                                if (debug) { Debug.WriteLine($"GoHomeAction() - Fail - {ex.Message}\n"); }
                            }
                        }
                    }

                    else
                    {
                        _navValueTuple.ebookPlayOrder = (playOrder + 1).ToString();

                        _xhtmlPath =
                            FileManagement.GetBookContentFilePath(_navValueTuple.ebookFolderPath, _navValueTuple.ebookPlayOrder);

                        // Ensure MyWebView is initialized
                        if (MyWebView == null)
                        {
                            throw new InvalidOperationException("MyWebView is not initialized.");
                        }

                        MyWebView.Source = new Uri(_xhtmlPath); // Set the Source property

                        string documentHeight = null;
                        string windowHeight = null;

                        MyWebView.NavigationCompleted += async (s, args) =>
                        {
                            if (args.IsSuccess)
                            {
                                if (debug) { Debug.WriteLine("MoveToNextChapter - Navigation completed."); }

                                documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
                                windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");
                                await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, 0);");
                                _chapterNavigated = true;
                                return;
                            }
                            else
                            {
                                if (debug) { Debug.WriteLine("Navigation failed."); }
                            }
                        };
                    }
                    await SavePosition();
                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine($"MoveToNextChapter() - Fail - {ex.Message}\n"); }
                }
            }


        }

        /// <summary>
        /// Moves to the previous chapter in the ebook and scrolls to the bottom of the page upon navigation completion.
        /// </summary>
        /// <param name="debug">Optional. A boolean flag indicating whether to log debug information. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        ///   <item>
        ///     Checks if <c>_chapterNavigated</c> is <c>false</c> and initializes required components.
        ///   </item>
        ///   <item>
        ///     Decreases the <c>ebookPlayOrder</c> if it's greater than 1 and updates the WebView source to the previous chapter's path.
        ///   </item>
        ///   <item>
        ///     Upon navigation completion, retrieves the document's height, scrolls to the bottom, and sets <c>_chapterNavigated</c> to <c>true</c>.
        ///   </item>
        ///   <item>
        ///     Saves the current position and logs debug information if <paramref name="debug"/> is <c>true</c>.</item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">Logs any exception messages if errors occur during the process.</exception>
        private async void MoveToPreviousChapter(bool debug = false)
        {
            if (!_chapterNavigated)
            {
                try
                {
                    // Ensure _navValueTuple is initialized
                    if (_navValueTuple == default)
                    {
                        throw new InvalidOperationException("_navValueTuple is not initialized.");
                    }

                    int playOrder = int.Parse(_navValueTuple.ebookPlayOrder);

                    if (playOrder > 1)
                    {
                        playOrder--;
                    }

                    else
                    {
                        return;
                    }

                    _navValueTuple.ebookPlayOrder = (playOrder).ToString();
                    _xhtmlPath =
                        FileManagement.GetBookContentFilePath(_navValueTuple.ebookFolderPath, _navValueTuple.ebookPlayOrder);

                    // Ensure MyWebView is initialized
                    if (MyWebView == null)
                    {
                        throw new InvalidOperationException("MyWebView is not initialized.");
                    }

                    MyWebView.Source = new Uri(_xhtmlPath); // Set the Source property

                    string documentHeight = null;
                    string windowHeight = null;
                    string wantedScroll = null;

                    // scrolls to the bottom of the page
                    MyWebView.NavigationCompleted += async (s, args) =>
                    {
                        if (args.IsSuccess)
                        {
                            if (debug) { Debug.WriteLine("MoveToPreviousChapter - Navigation completed."); }
                            documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
                            windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");
                            wantedScroll = (float.Parse(documentHeight)).ToString();
                            await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, {wantedScroll});");
                            _chapterNavigated = true;

                            return;
                        }
                        else
                        {
                            if (debug) { Debug.WriteLine("Navigation failed."); }
                        }
                    };
                    await SavePosition();
                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine($"MoveToPreviousChapter() - Fail - {ex.Message}\n"); }
                }
            }
        }

        /// <summary>
        /// Checks if the user has scrolled to the bottom of the current chapter and moves to the next chapter if so.
        /// </summary>
        /// <param name="debug">Optional. A boolean flag indicating whether to log debug information. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// This method retrieves the current scroll position, document height, and viewport height. It compares the current scroll position 
        /// with the last recorded position to determine if the user has scrolled to the bottom. If so, it triggers navigation to the next chapter.
        /// Logs debug information if <paramref name="debug"/> is <c>true</c>.</remarks>
        /// <exception cref="Exception">Logs any exception messages if errors occur during the process.</exception>
        private async void CheckForward(bool debug = false)
        {
            // Check if the user has scrolled to the bottom
            var scrollY = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");
            var documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
            var windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");

            try
            {
                if (scrollY == _lastScroll)
                {
                    Debug.WriteLine($"scrollY = {scrollY} - _lastScroll = {_lastScroll}");
                    MoveToNextChapter();
                }

                if (debug) { Debug.WriteLine("CheckForward() - Success\n"); }
            }

            catch
            {
                if (debug) { Debug.WriteLine("CheckForward() - Fail\n"); }
            }
            _lastScroll = scrollY;
        }

        /// <summary>
        /// Checks if the user has scrolled to the top of the current chapter and moves to the previous chapter if so.
        /// </summary>
        /// <param name="debug">Optional. A boolean flag indicating whether to log debug information. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// This method retrieves the current scroll position, document height, and viewport height. It compares the current scroll position 
        /// with the last recorded position to determine if the user has scrolled to the top. If so, it triggers navigation to the previous chapter.
        /// Logs debug information if <paramref name="debug"/> is <c>true</c>.</remarks>
        /// <exception cref="Exception">Logs any exception messages if errors occur during the process.</exception>
        private async void CheckBackward(bool debug = false)
        {
            var scrollY = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");
            var documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
            var windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");

            try
            {
                if (scrollY == _lastScroll)
                {
                    Debug.WriteLine($"scrollY = {scrollY} - _lastScroll = {_lastScroll}");

                    MoveToPreviousChapter();
                }
                if (debug) { Debug.WriteLine("CheckBackward() - Success\n"); }
            }

            catch
            {
                if (debug) { Debug.WriteLine("CheckBackward() - Fail\n"); }
            }

            _lastScroll = scrollY;
        }

        /// <summary>
        /// Handles key down events to navigate through the ebook based on arrow key inputs.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data, which contains information about the key pressed.</param>
        /// <remarks>
        /// This method checks if the left or right arrow key is pressed:
        /// <list type="bullet">
        ///   <item>
        ///     If the left arrow key is pressed, the method invokes <see cref="MoveBackward"/> to navigate to the previous chapter.
        ///   </item>
        ///   <item>
        ///     If the right arrow key is pressed, the method invokes <see cref="MoveForward"/> to navigate to the next chapter.
        ///   </item>
        /// </list>
        /// </remarks>
        private async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Left)
            {
                MoveBackward();
            }
            else if (e.Key == Windows.System.VirtualKey.Right)
            {
                MoveForward();
            }
        }

        /// <summary>
        /// Updates the colors and styles of the command bar and viewer based on the user's settings.
        /// </summary>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        ///   <item>
        ///     Loads color settings from a JSON configuration file based on the user's theme.
        ///   </item>
        ///   <item>
        ///     Updates the background color of the viewer grid and command bar, as well as the foreground color of command bar buttons.
        ///   </item>
        ///   <item>
        ///     Adjusts the margin of the WebView depending on the ebook viewer setting and updates the CSS accordingly.
        ///   </item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">Handles exceptions related to file reading and color parsing. If the settings or colors are invalid, default values are used.</exception>
        private void ChangeCommandBarColors()
        {
            string color_string = "#efe0cd";
            string font_string;
            Windows.UI.Color _backgroundColor = ParseHexColor("#eed2ae");
            Windows.UI.Color _foregroundColor = ParseHexColor("#000000");
            Windows.UI.Color _buttonColor;

            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));


            try
            {
                color_string = SettingsPage.Themes[settings.Theme]["background-color"];
                font_string = (SettingsPage.LoadFontComboBox());
            }

            finally
            {
                Windows.UI.Color _viewerBackgroundColor = ParseHexColor(color_string);
                ViewerGrid.Background = new SolidColorBrush(_viewerBackgroundColor);
            }

            _backgroundColor = ParseHexColor(SettingsPage.Themes[settings.Theme]["header-color"]);
            _foregroundColor = ParseHexColor(SettingsPage.Themes[settings.Theme]["button-color"]);
            _buttonColor = ParseHexColor(SettingsPage.Themes[settings.Theme]["button-color"]);

            if (settings.EbookViewer == "epubjs")
            {
                MyWebView.Margin = new Thickness(Int32.Parse("0"));
                UpdateCSSAction();
            }
            else
            {
                MyWebView.Margin = new Thickness(Int32.Parse(settings.Padding));
                UpdateCSSAction();

            }

            MyCommandBar.Background = new SolidColorBrush(_backgroundColor);
            MyCommandBar.Foreground = new SolidColorBrush(_foregroundColor);
            Forward.Foreground = Backward.Foreground = Settings.Foreground = Home.Foreground = new SolidColorBrush(_buttonColor);

        }

        /// <summary>
        /// Handles the click event for the padding button, updating the padding setting and UI accordingly.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data, which contains information about the click event.</param>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves the text from the padding TextBox and attempts to convert it to a double.
        ///   </item>
        ///   <item>
        ///     If the conversion is successful and the value is positive, updates the global settings with the new padding value, saves the settings to a file, and updates the UI to reflect the new padding with a success background color.
        ///   </item>
        ///   <item>
        ///     If the conversion fails or the value is not valid, displays an error message in the TextBox and updates the background color to indicate an error.
        ///   </item>
        /// </list>
        /// </remarks>
        private void PaddingButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox
            string padding = PaddingBox.Text;

            // Try to convert the string to a double
            if (double.TryParse(padding, out double paddingValue))
            {
                if (!string.IsNullOrWhiteSpace(padding) && paddingValue > 0)
                {
                    GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                    settings.Padding = padding;
                    File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                    PaddingBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#c9ffad"));
                    ChangeCommandBarColors();
                }
            }
            else
            {
                PaddingBox.Text = "Type a number bigger than 0...";
                PaddingBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#f2aeb4"));
            }
        }

        /// <summary>
        /// Handles the click event for the font size button, updating the font size setting and UI accordingly.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data, which contains information about the click event.</param>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves the text from the font size TextBox and attempts to convert it to a double.
        ///   </item>
        ///   <item>
        ///     If the conversion is successful and the value is positive, updates the global settings with the new font size, saves the settings to a file, and updates the UI with a success background color.
        ///   </item>
        ///   <item>
        ///     Updates the font size in the settings page and applies the new CSS to reflect the updated font size.
        ///   </item>
        ///   <item>
        ///     If the conversion fails or the value is not valid, displays an error message in the TextBox and updates the background color to indicate an error.
        ///   </item>
        /// </list>
        /// </remarks>
        private void FontSizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox
            string fontSize = FontSizeBox.Text;

            // Try to convert the string to a double
            if (double.TryParse(fontSize, out double paddingValue))
            {
                // Perform your action with the message here
                // For example, display it in a message box
                if (!string.IsNullOrWhiteSpace(fontSize) && paddingValue > 0)
                {
                    GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                    settings.FontSize = $"{(paddingValue / 10).ToString()}rem";
                    File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                    FontSizeBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#c9ffad"));
                    SettingsPage.UpdateBodyFontSize(settings.FontSize);
                    UpdateCSSAction();
                }
            }
            else
            {
                FontSizeBox.Text = "Type a number bigger than 0...";
                FontSizeBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#f2aeb4"));

            }
        }

        /// <summary>
        /// Converts a hex color string to a <see cref="Windows.UI.Color"/> object.
        /// </summary>
        /// <param name="hexColor">A string representing the color in hexadecimal format, optionally prefixed with '#'.</param>
        /// <returns>A <see cref="Windows.UI.Color"/> object representing the color.</returns>
        /// <remarks>
        /// The method assumes the hex color string is in the format "RRGGBB". The alpha (opacity) value is set to fully opaque (255).
        /// </remarks>
        /// <exception cref="FormatException">Thrown if the hex color string is not in a valid format.</exception>
        /// <example>
        /// <code>
        /// Windows.UI.Color color = ParseHexColor("#FF5733");
        /// </code>
        /// </example>
        public static Windows.UI.Color ParseHexColor(string hexColor)
        {
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }

            byte a = 255;
            byte r = byte.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Opens and logs information about an ebook based on the provided play order and folder path.
        /// </summary>
        /// <param name="data">A tuple containing the play order and folder path of the ebook.</param>
        /// <remarks>
        /// The method retrieves the path to the ebook content and reads the ebook data using the provided 
        /// play order and folder path. It then logs details about the ebook such as its title, content path,
        /// play order, and scroll value for debugging purposes.
        /// </remarks>
        /// <example>
        /// <code>
        /// OpenEbookMessage(("01", "MyEbooks/Folder1"));
        /// </code>
        /// </example>
        private void OpenEbookMessage((string ebookPlayOrder, string ebookFolderPath) data)
        {
            _xhtmlPath = FileManagement.GetBookContentFilePath(_navValueTuple.ebookFolderPath, _navValueTuple.ebookPlayOrder);
            _ebook = JsonHandler.ReadEbookJsonFile(FileManagement.GetEbookDataJsonFile(_navValueTuple.ebookFolderPath));

            Debug.WriteLine("");
            Debug.WriteLine("******************************");
            Debug.WriteLine($"Path: {_ebook.Title}");
            Debug.WriteLine($"Path: {_xhtmlPath}");
            Debug.WriteLine($"PlayOrder: {_navValueTuple.ebookPlayOrder}");
            Debug.WriteLine($"Scroll: {_ebook.ScrollValue}");
            Debug.WriteLine("******************************");
            Debug.WriteLine("");
        }

        /// <summary>
        /// Starts the Flask server by executing a Python script using the specified Python interpreter.
        /// </summary>
        /// <param name="debug">Optional flag to enable debugging output. Default is <c>false</c>.</param>
        /// <remarks>
        /// This method reads the global settings to obtain the path of the Python interpreter and the
        /// Python script to execute. It then starts a process to run the Flask server. The script path
        /// is constructed from the working directory combined with the script file name. If debugging is
        /// enabled, the method logs the script path, output, and any errors to help troubleshoot issues.
        /// </remarks>
        /// <example>
        /// <code>
        /// await StartFlaskServer(true);
        /// </code>
        /// </example>
        private void StartFlaskServer(bool debug = false)
        {
            try
            {

                GlobalSettingsJson settings =
                    JsonSerializer.Deserialize<GlobalSettingsJson>(
                        File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));

                // Combine the working directory with the script file name
                string scriptPath = Path.Combine(AppContext.BaseDirectory, "code\\translation_script.py");

                if (debug)
                {
                    Debug.WriteLine($"Python script path: {scriptPath}");
                }

                _flaskProcess = new Process();
                _flaskProcess.StartInfo.FileName = settings.PythonPath; // Command to run Python
                _flaskProcess.StartInfo.Arguments = scriptPath; // Your Python script
                _flaskProcess.StartInfo.UseShellExecute = false;
                _flaskProcess.StartInfo.RedirectStandardOutput = true;
                _flaskProcess.StartInfo.RedirectStandardError = true;
                _flaskProcess.StartInfo.CreateNoWindow = true; // Run without creating a new window
                _flaskProcess.Start();

                if (debug)
                {
                    Debug.WriteLine($"StartFlaskServer() - Success");
                }

                // Optionally, you can read the output or error streams to log or display:
                string output = _flaskProcess.StandardOutput.ReadToEnd();
                string error = _flaskProcess.StandardError.ReadToEnd();

                if (debug)
                {
                    Debug.WriteLine(output);
                    Debug.WriteLine(error);
                }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"StartFlaskServer() - Fail - {ex.Message}"); }
            }

        }

        /// <summary>
        /// Starts the Flask server on a separate thread if the translation service is set to "argos".
        /// </summary>
        /// <remarks>
        /// This method reads the global settings to determine if the translation service is "argos". 
        /// If so, it creates and starts a new thread to execute the <see cref="StartFlaskServer"/> method.
        /// The threading ensures that the Flask server starts asynchronously and does not block the main thread.
        /// </remarksx
        /// <example>
        /// <code>
        /// StartFlaskServerThread();
        /// </code>
        /// </example>
        private async void StartFlaskServerThread()
        {

            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));


            if (settings.TranslationService == "argos")
            {
                await Task.Run(() => StartFlaskServer(debug: false));


            }
        }

        /// <summary>
        /// Stops the Flask server if the translation service is set to "argos".
        /// </summary>
        /// <remarks>
        /// This method reads the global settings to determine if the translation service is "argos". 
        /// If so, it asynchronously calls the <see cref="StopFlaskServer"/> method to stop the Flask server.
        /// The use of `await` ensures that the server shutdown process completes before the method exits.
        /// </remarks>
        /// <example>
        /// <code>
        /// await StopFlaskServerThread();
        /// </code>
        /// </example>
        private async Task StopFlaskServerThread()
        {
            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
            if (settings.TranslationService == "argos")
            {
                StopFlaskServer();
            }
        }

        /// <summary>
        /// Stops the Flask server by terminating its process.
        /// </summary>
        /// <param name="debug">If true, enables debug logging to trace method execution.</param>
        /// <remarks>
        /// This method checks if the Flask process is running and has not already exited. 
        /// If so, it forcefully terminates the process and disposes of it to release system resources.
        /// Debug messages are logged based on the <paramref name="debug"/> parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// await StopFlaskServer(true);
        /// </code>
        /// </example>
        private static void StopFlaskServer(bool debug = false)
        {
            try
            {
                if (_flaskProcess != null && !_flaskProcess.HasExited)
                {
                    _flaskProcess.Kill(); // Forcefully terminate the process
                    _flaskProcess.Dispose();
                    if (debug) { Debug.WriteLine($"StopFlaskServer() - Success"); }
                }
            }

            catch (Exception e)
            {
                if (debug) { Debug.WriteLine($"StopFlaskServer() - Fail - {e.Message}"); }
            }
        }

        /// <summary>
        /// Asynchronously initiates the initialization process by sending a POST request to a specified URL 
        /// with the provided source and target languages in JSON format.
        /// </summary>
        /// <param name="sourceLanguage">The source language for the initialization process.</param>
        /// <param name="targetLanguage">The target language for the initialization process.</param>
        /// <param name="debug">
        /// Optional parameter to enable or disable debug output. 
        /// If true, debug messages will be written to the debug output stream. Default is true.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, 
        /// with a result of type <see cref="string"/>. The result will be a message indicating 
        /// whether the initialization request was sent successfully or an error occurred.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown if the HTTP request fails, providing detailed error information in the returned string.
        /// </exception>
        /// <exception cref="Exception">
        /// Catches all other exceptions that may occur during the execution, 
        /// returning a message that includes the exception details.
        /// </exception>
        /// <remarks>
        /// This method constructs a JSON payload with the specified source and target languages, 
        /// and sends it to the initialization endpoint. If the request is successful, 
        /// a success message is returned. Otherwise, an error message is returned.
        /// </remarks>
        private static async Task<string> StartInitialization(string sourceLanguage, string targetLanguage, bool debug = true)
        {
            try
            {
                var initUrl = "http://127.0.0.1:5000/initialize";
                var initRequestData = new
                {
                    source_language = sourceLanguage,
                    target_language = targetLanguage
                };

                var initJson = JsonSerializer.Serialize(initRequestData);
                var initContent = new StringContent(initJson, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage initResponse = await Client.PostAsync(initUrl, initContent);
                    initResponse.EnsureSuccessStatusCode();

                    if (debug) { Debug.WriteLine("Initialization request sent."); }
                    return "Initialization request sent.";
                }
                catch (HttpRequestException e)
                {
                    if (debug) { Debug.WriteLine($"Initialization request error: {e.Message}"); }
                    return $"ERROR: Initialization request failed: {e.Message}";
                }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"StartInitialization() - Fail - {ex.Message}"); }
                return $"ERROR: Unexpected failure: {ex.Message}";
            }
        }

        /// <summary>
        /// Sends a translation request to the server and returns the translated text.
        /// </summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="sourceLanguage">The source language of the text.</param>
        /// <param name="targetLanguage">The target language for the translation.</param>
        /// <param name="debug">If true, enables debug output. Default is true.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> containing the translated text or an error message if the translation fails.
        /// </returns>
        /// <remarks>
        /// The method sends a POST request with the text and language parameters in JSON format to the translation server.
        /// It handles potential errors and returns appropriate error messages if the translation fails or if the server response is malformed.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="Exception">Catches all other exceptions that may occur during execution.</exception>
        private static async Task<string> GetTranslation(string text, string sourceLanguage, string targetLanguage, bool debug = true)
        {
            try
            {
                var translationUrl = "http://127.0.0.1:5000/translate";
                var translationRequestData = new
                {
                    text = text,
                    source_language = sourceLanguage,
                    target_language = targetLanguage
                };

                var translationJson = JsonSerializer.Serialize(translationRequestData);
                var translationContent = new StringContent(translationJson, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage translationResponse = await Client.PostAsync(translationUrl, translationContent);
                    translationResponse.EnsureSuccessStatusCode();
                    var translationResponseString = await translationResponse.Content.ReadAsStringAsync();

                    using (JsonDocument doc = JsonDocument.Parse(translationResponseString))
                    {
                        if (doc.RootElement.TryGetProperty("translated_text", out JsonElement translatedTextElement))
                        {
                            return translatedTextElement.GetString();
                        }
                        else
                        {
                            return "ERROR: Translation response format error.";
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    if (debug) { Debug.WriteLine($"Translation request error: {e.Message}"); }
                    return "ERROR: Translation request failed.";
                }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"GetTranslation() - Fail - {ex.Message}"); }
                return "ERROR: Unexpected failure.";
            }
        }

        /// <summary>
        /// Asynchronously sends a translation request to a specified URL and returns the translated text.
        /// </summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="sourceLanguage">The source language of the text.</param>
        /// <param name="targetLanguage">The target language for the translation.</param>
        /// <param name="debug">If true, enables debug output. Default is true.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> with the translated text, or an error message if the translation fails.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="Exception">Catches all other exceptions that may occur during execution.</exception>
        private static async Task<string> CheckInitializationStatus(string sourceLanguage, string targetLanguage, bool debug = true)
        {
            try
            {
                string status = "not started";
                while (status.Contains("not started") || status.Contains("in progress"))
                {
                    var statusUrl = $"http://127.0.0.1:5000/initialize/status?source_language={sourceLanguage}&target_language={targetLanguage}";
                    HttpResponseMessage statusResponse = await Client.GetAsync(statusUrl);
                    statusResponse.EnsureSuccessStatusCode();

                    var statusString = await statusResponse.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(statusString))
                    {
                        if (doc.RootElement.TryGetProperty("initialization_status", out JsonElement statusElement))
                        {
                            status = statusElement.GetString();
                            Debug.WriteLine($"Initialization status: {status}");
                            if (!(status.Contains("not started") || status.Contains("in progress")))
                            {
                                break;
                            }
                        }
                        else
                        {
                            status = "ERROR: Status response format error.";
                        }
                    }
                    await Task.Delay(5000);
                }

                if (status.Contains("failed"))
                {
                    return $"ERROR: Initialization failed: {status}";
                }

                return "Initialization completed successfully.";
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"CheckInitializationStatus() - Fail - {ex.Message}"); }
                return $"ERROR: Unexpected failure: {ex.Message}";
            }
        }

        /// <summary>
        /// Updates the visual state of the ArgosEllipse based on the readiness of Argos.
        /// </summary>
        /// <remarks>
        /// Changes the color and visibility of ArgosEllipse depending on the _isArgosReady flag.
        /// A green color indicates readiness, while a red color indicates that Argos is not ready.
        /// </remarks>
        private void CheckArgosState()
        {
            if (_isArgosReady)
            {
                Debug.WriteLine("Ellipse visible");
                ArgosEllipse.Fill = new SolidColorBrush(ParseHexColor("#c2ffd2"));
                ArgosEllipse.Visibility = Visibility.Visible;
            }
            else
            {
                Debug.WriteLine("Ellipse visible");
                ArgosEllipse.Fill = new SolidColorBrush(ParseHexColor("#ff9cb3"));
                ArgosEllipse.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Asynchronously performs a translation, initializing the translation service if necessary.
        /// </summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="sourceLanguage">The source language of the text.</param>
        /// <param name="targetLanguage">The target language for the translation.</param>
        /// <param name="debug">If true, enables debug output. Default is true.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> with the translated text, or an error message if the translation fails.
        /// </returns>
        /// <remarks>
        /// If the translation service is not ready, the method initializes it, checks its status,
        /// and attempts the translation again until successful or an error occurs.
        /// </remarks>
        private async Task<string> PerformTranslation(string text, string sourceLanguage, string targetLanguage, bool debug = true)
        {
            Debug.WriteLine($"Source Language = {sourceLanguage} | Target Language = {targetLanguage}");
            string translationResult = "ERROR: Translation request failed.";
            if (!_isArgosReady)
            {
                var initResult = await StartInitialization(sourceLanguage, targetLanguage, debug);

                while (true)
                {
                    string statusResult = await CheckInitializationStatus(sourceLanguage, targetLanguage, debug);

                    Debug.WriteLine($"statusResult = {statusResult}");
                    if (!statusResult.Contains("ERROR"))
                    {
                        _isArgosReady = true;
                        translationResult = await GetTranslation(text, sourceLanguage, targetLanguage, debug);
                        if (!(translationResult.Contains("ERROR") && !(translationResult.Contains("INTERNAL SERVER ERROR"))))
                        {
                            break;
                        }
                    }
                    _isArgosReady = false;
                    await Task.Delay(10000);
                    CheckArgosState();

                }
            }
            CheckArgosState();
            translationResult = await GetTranslation(text, sourceLanguage, targetLanguage, debug);
            Debug.WriteLine($"translationResult = {translationResult}");
            return translationResult;
        }

        /// <summary>
        /// Retrieves a translated text from the MyMemory translation service.
        /// </summary>
        /// <param name="textToTranslate">The text that needs to be translated.</param>
        /// <param name="sourceLanguage">The language code of the source text (e.g., "en" for English).</param>
        /// <param name="targetLanguage">The language code to which the text should be translated (e.g., "es" for Spanish).</param>
        /// <param name="developerEmail">The email address used for API key identification and support requests. Defaults to "david.valek17@gmail.com".</param>
        /// <returns>A task representing the asynchronous operation, with a string result containing the translated text or an error message if the request fails.</returns>
        /// <remarks>
        /// This method calls the `TranslateText` function to perform the translation using the MyMemory service. The email address provided is used
        /// for API key identification and can be customized as needed. The `TranslateText` function handles the communication with the MyMemory API
        /// and processes the translation response.
        /// </remarks>
        /// <example>
        /// <code>
        /// string translatedText = await GetTranslationMymemory("Hello", "en", "fr");
        /// Console.WriteLine(translatedText); // Output will be the translated text in French.
        /// </code>
        /// </example>
        private static async Task<string> GetTranslationMymemory(string textToTranslate, string sourceLanguage, string targetLanguage, string developerEmail = "david.valek17@gmail.com")
        {

            var translatedText = await TranslateText(textToTranslate, sourceLanguage, targetLanguage, developerEmail);
            return translatedText;
        }

        /// <summary>
        /// Translates the given text using the MyMemory translation service API.
        /// </summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="sourceLanguage">The language code of the source text (e.g., "en" for English).</param>
        /// <param name="targetLanguage">The language code to which the text should be translated (e.g., "es" for Spanish).</param>
        /// <param name="developerEmail">The email address used for API key identification and support requests.</param>
        /// <returns>A task representing the asynchronous operation, with a string result containing the translated text.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <remarks>
        /// This method constructs a URL to request a translation from the MyMemory API, sends a GET request, and parses the response.
        /// The email address provided is used for API key identification and support requests. Ensure that the email address
        /// used is registered with the MyMemory API for proper functioning.
        /// </remarks>
        private static async Task<string> TranslateText(string text, string sourceLanguage, string targetLanguage, string developerEmail)
        {
            var url = $"https://api.mymemory.translated.net/get?q={HttpUtility.UrlEncode(text)}&langpair={sourceLanguage}|{targetLanguage}&de={HttpUtility.UrlEncode(developerEmail)}";

            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
            string responseBody = Encoding.UTF8.GetString(responseBytes);

            // Parse the JSON response
            var jsonDocument = JsonDocument.Parse(responseBody);
            var translatedText = jsonDocument.RootElement.GetProperty("responseData").GetProperty("translatedText").GetString();
            return translatedText;
        }

        /// <summary>
        /// Handles the event when the selected item in the fonts ComboBox changes.
        /// Updates the application's font settings based on the user's selection and applies the new font to the text content.
        /// </summary>
        /// <param name="sender">The source of the event, typically the ComboBox control.</param>
        /// <param name="e">The event arguments containing the details of the selection change.</param>
        /// <remarks>
        /// This method checks if the application startup process is complete (i.e., `_startUp` is at least 2) before updating the font settings.
        /// If the startup process is not yet complete, it increments the `_startUp` counter. Once the startup is complete, it:
        /// - Retrieves the selected font family from the ComboBox.
        /// - Deserializes the global settings JSON file to get the current settings.
        /// - Updates the `Font` property with the new font family and saves the updated settings back to the JSON file.
        /// - Calls `SettingsPage.UpdateBodyFontFamily` to apply the new font to the text content and `UpdateCSSAction` to refresh the CSS with the new font.
        /// </remarks>
        private void FontsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_startUp >= 2)
            {
                string newFontFamily = SettingsPage.BookReadingFonts[FontsComboBox.SelectedIndex];

                // store to json
                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                settings.Font = newFontFamily;
                File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                SettingsPage.UpdateBodyFontFamily(newFontFamily);
                UpdateCSSAction();
            }

            else
            {
                _startUp++;
            }

        }

        /// <summary>
        /// Handles the event when the selected item in the themes ComboBox changes.
        /// Updates the application's theme settings based on the user's selection and applies the new theme to the UI components.
        /// </summary>
        /// <param name="sender">The source of the event, typically the ComboBox control.</param>
        /// <param name="e">The event arguments containing the details of the selection change.</param>
        /// <remarks>
        /// This method checks if the application startup process is complete (i.e., `_startUp` is at least 2) before updating the theme settings.
        /// If the startup process is not yet complete, it increments the `_startUp` counter. Once the startup is complete, it:
        /// - Retrieves the selected theme from the ComboBox.
        /// - Deserializes the global settings JSON file to get the current settings.
        /// - Updates the `Theme` property with the new theme and saves the updated settings back to the JSON file.
        /// - Calls `SettingsPage.UpdateBodyTextColor` and `SettingsPage.UpdateBodyBackgroundColor` to apply the new theme's colors to the text and background respectively.
        /// - Refreshes the CSS to apply the new theme styles and updates the command bar colors accordingly.
        /// </remarks>
        private void ThemesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_startUp >= 2)
            {
                string theme = SettingsPage.Themes.Keys.ToList()[ThemesComboBox.SelectedIndex];
                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                settings.Theme = theme;
                File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                SettingsPage.UpdateBodyTextColor(SettingsPage.Themes[theme]["text-color"]);
                SettingsPage.UpdateBodyBackgroundColor(SettingsPage.Themes[theme]["background-color"]);
                UpdateCSSAction();
                ChangeCommandBarColors();
            }

            else
            {
                _startUp++;
            }
        }

        /// <summary>
        /// Handles the click event for the settings button.
        /// Toggles the visibility of the settings popup control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the button control that was clicked.</param>
        /// <param name="e">The event arguments containing the details of the click event.</param>
        /// <remarks>
        /// This method toggles the `IsOpen` property of the `PopupControl` to show or hide the settings popup.
        /// If the popup is currently closed, it will open; if it is currently open, it will close.
        /// This provides a way for users to access or hide the settings panel based on their interaction with the settings button.
        /// </remarks>
        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            PopupControl.IsOpen = !PopupControl.IsOpen;
        }

        /*
        deprecated
        /// <summary>
        /// Executes the JavaScript code in the WebView.
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteJavaScriptAsync(string javaScriptPath)
        {

            _script = await File.ReadAllTextAsync(javaScriptPath);


            try
            {
                if (MyWebView.CoreWebView2 != null)
                {
                    //var result = await MyWebView.CoreWebView2.ExecuteScriptAsync(_script);
                    //Debug.WriteLine($"JavaScript executed with result: {result}");
                }
                else
                {
                    //Debug.WriteLine("CoreWebView2 is not initialized.");
                }

                //Debug.WriteLine("ExecuteJavaScriptAsync() - Success\n");
            }

            catch
            {
                Debug.WriteLine("ExecuteJavaScriptAsync() - Fail\n");
            }
        }
        */


    }

}
