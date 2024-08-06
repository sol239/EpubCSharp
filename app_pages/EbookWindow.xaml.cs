using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using EpubReader.code;
using HarfBuzzSharp;
using System.Reflection;
using Windows.System;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader.app_pages
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EbookWindow : Window
    {
        //  the current ebook being read.
        private Ebook _ebook;

        // path to the XHTML file of the ebook
        private string _xhtmlPath = "";

        // path to the CSS file for styling the ebook.
        private string _cssPath = FileManagment.GetEbookViewerStyleFilePath();
        private Windows.UI.Color _backgroundColor = ParseHexColor("#eed2ae");
        private Windows.UI.Color _foregroundColor = ParseHexColor("#000000");
        private Windows.UI.Color _buttonColor = ParseHexColor("#000000");

        // tuple passed from the main window holding the ebook's play order and folder path.
        private (string ebookPlayOrder, string ebookFolderPath) navValueTuple;

        // the last scroll position of the WebView
        private string lastScroll = "0";

        // Define the event
        public event EventHandler WindowClosed;

        // JavaScript code to manage scrolling and key events
        private string _script = "0";

        /// <summary>
        /// Constructor initializes the component and subscribes to Loaded and Unloaded events.
        /// </summary>
        public EbookWindow( (string ebookPlayOrder, string ebookFolderPath) data )
        {
            this.InitializeComponent();
            ChangeCommandBarColors();
            navValueTuple = data;
            OpenEbookMessage(navValueTuple);
            EbookViewer_Loaded();
            ViewerGrid.Focus(FocusState.Programmatic);
            //epubjsWindowLoad();
            ChangeBooksStatus();



        }

        private void ChangeBooksStatus()
        {
            _ebook = JsonHandler.ReadEbookJsonFile(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath));
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
            File.WriteAllText(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath), JsonSerializer.Serialize(_ebook));


        }

        // Event handler for the Closed event
        private void EbookWindow_Closed(object sender, EventArgs e)
        {
            // Trigger the custom WindowClosed event
            WindowClosed?.Invoke(this, EventArgs.Empty);
        }

        private async Task SaveBookOpenTime()
        {   
            // Save the time the book was opened
            _ebook.BookOpenTime = DateTime.Now.ToString();
        }

        /// <summary>
        /// Saves playorder and scroll position of the WebView and stores it in the ebookData.json file.
        /// </summary>
        /// <returns></returns>
        private async Task SavePosition()
        {
            try
            {
                try
                {
                    _ebook.ScrollValue = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");
                    Debug.WriteLine($"SavePosition() 1 - Success");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SavePosition() 1 - Fail - {ex.Message}");
                    _ebook.ScrollValue = "0";
                }

                try
                {
                    _ebook.InBookPosition = navValueTuple.ebookPlayOrder;
                    Debug.WriteLine($"SavePosition() 2 - Success");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SavePosition() 2 - Fail - {ex.Message}");
                }

                try
                {
                    _ebook.DateLastOpened = DateTime.Now.ToString();
                    Debug.WriteLine($"SavePosition() 3 - Success");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SavePosition() 3 - Fail - {ex.Message}");
                }

                Debug.WriteLine("********************************");
                Debug.WriteLine("Save Position");
                Debug.WriteLine($"InBookPosition = {_ebook.InBookPosition} | Scroll = {_ebook.ScrollValue}");
                Debug.WriteLine($"Save To: {FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred in SavePosition: {ex.Message}");
            }

            finally
            {
                try
                {

                    _ebook.BookCloseTime = DateTime.Now.ToString();

                    // Get the file path
                    string filePath = FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath);
                    filePath = Path.GetDirectoryName(filePath);

                    // Store the JSON ebook file
                    await JsonHandler.StoreJsonEbookFile(_ebook, filePath);

                    Debug.WriteLine("SavePosition() - Success");
                    Debug.WriteLine("********************************\n");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SavePosition() - Fail - {ex.Message}");
                    Debug.WriteLine("********************************\n");

                }
            }
        }

        public async Task CalculateTimeDifference()
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

                }

                try
                {
                    closeTime = DateTime.Parse(_ebook.BookCloseTime);

                }

                catch
                {

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

                    await Store1(timeDifference);
                    // Get the file path
                    string filePath = FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath);
                    filePath = Path.GetDirectoryName(filePath);

                    // Store the JSON ebook file
                    await JsonHandler.StoreJsonEbookFile(_ebook, filePath);
                }

                Debug.WriteLine($"CalculateTimeDifference() - Success\n");
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"CalculateTimeDifference() - Fail - {ex.Message}\n");
            }

        }

        public async Task Store1(TimeSpan timeDifference)
        {
            try
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // Check if StatsRecord1 is null or empty
                if (_ebook.StatsRecord1 != null && _ebook.StatsRecord1.Count > 0)
                {
                    if (_ebook.StatsRecord1.ContainsKey(currentDate))
                    {
                        TimeSpan _timeSpan = new TimeSpan(0, 0, 0, 0);
                        _timeSpan = TimeSpan.Parse(_ebook.StatsRecord1[currentDate]);
                        _ebook.StatsRecord1[currentDate] = (_timeSpan + timeDifference).ToString();
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

                Debug.WriteLine($"Store1() - Success\n");

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Store1() - Fail - {ex.Message}\n");
            }
        }


        /// <summary>
        /// Loads the playorder and scroll position of the WebView from the ebookData.json file.
        /// </summary>
        /// <returns></returns>
        private async Task RestorePositionAsync()
        {
            try
            {
                _ebook = JsonHandler.ReadEbookJsonFile(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath));
                
                Debug.WriteLine("\n********************************");
                Debug.WriteLine($"Restore Position: {_xhtmlPath} - Scroll = {_ebook.ScrollValue}");
                Debug.WriteLine("*********************************\n");
                Debug.WriteLine($"RestorePositionAsync() - Success\n");
                await MyWebView.EnsureCoreWebView2Async(null);

                await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, {_ebook.ScrollValue});");
                _xhtmlPath = FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, _ebook.InBookPosition);

                
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"RestorePositionAsync() - Fail - {ex.Message}\n");
            }
        }

        /// <summary>
        /// Executes the JavaScript code in the WebView.
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteJavaScriptAsync()
        {

            if (_script == "0")
            {
                _script = await File.ReadAllTextAsync("C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\scripts\\ebook.js");
            }


            try
            {
                if (MyWebView.CoreWebView2 != null)
                {
                    var result = await MyWebView.CoreWebView2.ExecuteScriptAsync(_script);
                    Debug.WriteLine($"JavaScript executed with result: {result}");
                }
                else
                {
                    Debug.WriteLine("CoreWebView2 is not initialized.");
                }

                Debug.WriteLine("ExecuteJavaScriptAsync() - Success\n");
            }

            catch
            {
                Debug.WriteLine("ExecuteJavaScriptAsync() - Fail\n");
            }
        }

        /// <summary>
        /// Runs when the page is loaded. Initializes the WebView and executes JavaScript.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EbookViewer_Loaded()
        {
            try
            {
                await InitializeWebViewAsync();
                await UpdateCssPath(_xhtmlPath, _cssPath);
                _xhtmlPath =
                    FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, navValueTuple.ebookPlayOrder);
                MyWebView.Source = new Uri(_xhtmlPath);
                await RestorePositionAsync(); // Restore scroll position


                MyWebView.NavigationCompleted += async (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        await ExecuteJavaScriptAsync(); // Execute JavaScript after the WebView has loaded
                    }
                    else
                    {
                        Debug.WriteLine("Navigation failed.");
                    }
                };
                MyWebView.Focus(FocusState.Programmatic);
                Debug.WriteLine("EbookViewer_Loaded() - Success\n");
            }

            catch
            {
                Debug.WriteLine("EbookViewer_Loaded() - Fail\n");
            }

            finally
            {
                await RestorePositionAsync();
                await SaveBookOpenTime();

            }
        }

        /// <summary>
        /// Prints log messages to the Output window.
        /// </summary>
        /// <param name="data"></param>
        private void OpenEbookMessage((string ebookPlayOrder, string ebookFolderPath) data)
        {
            _xhtmlPath = FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, navValueTuple.ebookPlayOrder);
            _ebook = JsonHandler.ReadEbookJsonFile(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath));

            Debug.WriteLine("");
            Debug.WriteLine("******************************");
            Debug.WriteLine($"Path: {_ebook.Title}");
            Debug.WriteLine($"Path: {_xhtmlPath}");
            Debug.WriteLine($"PlayOrder: {navValueTuple.ebookPlayOrder}");
            Debug.WriteLine($"Scroll: {_ebook.ScrollValue}");
            Debug.WriteLine("******************************");
            Debug.WriteLine("");
        }

        /// <summary>
        /// Initializes the WebView and executes JavaScript.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeWebViewAsync()
        {
            try
            {
                await MyWebView.EnsureCoreWebView2Async(null);
                MyWebView.CoreWebView2.Settings.IsScriptEnabled = true;
                await MyWebView.CoreWebView2.ExecuteScriptAsync(_script);

                //await MyWebView.CoreWebView2.ExecuteScriptAsync(script);

                // Handle messacges from JavaScript
                MyWebView.CoreWebView2.WebMessageReceived += (sender, e) =>
                {
                    string tappedWord = e.TryGetWebMessageAsString();
                    Debug.WriteLine($"Message = {tappedWord}");
                    if (tappedWord == "scrolledDown")
                    {
                        CheckForward();
                    }

                    else if (tappedWord == "scrolledUp")
                    {
                        CheckBackward();
                    }

                };

                Debug.WriteLine($"InitializeWebViewAsync() - Success\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeWebViewAsync() - Fail - {ex.Message}\n");
            }
        }

        /// <summary>
        /// Updates the CSS path in the XHTML file.
        /// </summary>
        /// <param name="xhtmlPath"></param>
        /// <param name="newCssPath"></param>
        /// <returns></returns>
        public async Task UpdateCssPath(string xhtmlPath, string newCssPath)
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
                    Debug.WriteLine("UpdateCssPath() - Success\n");
                }
                else
                {
                    Debug.WriteLine("No <link> element with rel=\"stylesheet\" found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateCssPath() - Fail - {ex.Message}\n");
            }
        }

        /// <summary>
        /// Runs when the user clicks the Update CSS button. Updates the CSS path in the XHTML file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateCSSAction(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => app_controls.GlobalCssInjector());
                MyWebView.Reload(); // Reload the WebView to apply CSS changes

                // Wait for the WebView to finish loading
                MyWebView.NavigationCompleted += async (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        await ExecuteJavaScriptAsync(); // Re-execute JavaScript after reload
                    }
                    else
                    {
                        Debug.WriteLine("Navigation failed.");
                    }
                };

                Debug.WriteLine("UpdateCSSAction() - Success\n");
            }

            catch
            {
                Debug.WriteLine("UpdateCSSAction() - Fail\n");
            }
        }

        /// <summary>
        /// Runs when the user clicks the Home button. Navigates to the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoHomeAction(object sender, RoutedEventArgs e)
        {
            try
            {
                await SavePosition();
                await CalculateTimeDifference();
                WindowClosed?.Invoke(this, EventArgs.Empty);
                this.Close();

                Debug.WriteLine("GoHomeAction() - Success\n");
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
                await MyWebView.CoreWebView2.ExecuteScriptAsync("scrollUp();");
                await SavePosition();
                CheckBackward();
                Debug.WriteLine("Backward_Click() - Success\n");
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
                await MyWebView.CoreWebView2.ExecuteScriptAsync("scrollDown();");
                await SavePosition();
                CheckForward();

                Debug.WriteLine("Forward_Click() - Success\n");
            }

            catch
            {
                Debug.WriteLine("Forward_Click() - Fail\n");
            }
        }

        private async void MoveToNext()
        {
            // debug message
            Debug.WriteLine("");
            Debug.WriteLine("***************");
            Debug.WriteLine("MoveToNext");


            try
            {
                // Ensure navValueTuple is initialized
                if (navValueTuple == default)
                {
                    throw new InvalidOperationException("navValueTuple is not initialized.");
                }

                int playOrder = int.Parse(navValueTuple.ebookPlayOrder);
                
                // TO-DO fix book finished error

                List<string> playOrderList = _ebook.NavData.Keys.ToList();
                int maxPlayOrder = playOrderList.Count;

                if ( (playOrder + 1) > maxPlayOrder)
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
                            _ebook = JsonHandler.ReadEbookJsonFile(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath));
                            _ebook.Status = "Finished";
                            File.WriteAllText(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath), JsonSerializer.Serialize(_ebook));


                            navValueTuple.ebookPlayOrder = 1.ToString();
                            await SavePosition();
                            await CalculateTimeDifference();
                            WindowClosed?.Invoke(this, EventArgs.Empty);
                            this.Close();

                            Debug.WriteLine("GoHomeAction() - Success\n");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"GoHomeAction() - Fail - {ex.Message}\n");
                        }
                    }
                    


                }

                else
                {
                    navValueTuple.ebookPlayOrder = (playOrder + 1).ToString();

                    _xhtmlPath =
                        FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, navValueTuple.ebookPlayOrder);

                    Debug.WriteLine($"PlayOrder = {navValueTuple.ebookPlayOrder}");



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
                            documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
                            windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");
                            await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, 0);");
                            return;
                        }
                        else
                        {
                            Debug.WriteLine("Navigation failed.");
                        }
                    };
                }


                

                await SavePosition();

                Debug.WriteLine("MoveToNext() - Success");
                Debug.WriteLine("***************");
                Debug.WriteLine("");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MoveToNext() - Fail - {ex.Message}\n");
            }
        }

        private async void MoveToPrevious()
        {
            // debug message
            Debug.WriteLine("");
            Debug.WriteLine("***************");
            Debug.WriteLine("MoveToPrevious");


            try
            {
                // Ensure navValueTuple is initialized
                if (navValueTuple == default)
                {
                    throw new InvalidOperationException("navValueTuple is not initialized.");
                }

                int playOrder = int.Parse(navValueTuple.ebookPlayOrder);

                if (playOrder > 1)
                {
                    playOrder--;
                }

                else
                {
                    return;
                }

                navValueTuple.ebookPlayOrder = (playOrder).ToString();
                _xhtmlPath =
                    FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, navValueTuple.ebookPlayOrder);

                Debug.WriteLine($"PlayOrder = {navValueTuple.ebookPlayOrder}");
                

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
                        documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
                        windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");
                        wantedScroll = (float.Parse(documentHeight)).ToString();
                        await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, {wantedScroll});");
                        return;
                    }
                    else
                    {
                        Debug.WriteLine("Navigation failed.");
                    }
                };

                await SavePosition();

                Debug.WriteLine("MoveToPrevious() - Success");
                Debug.WriteLine("***************");
                Debug.WriteLine("");


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MoveToPrevious() - Fail - {ex.Message}\n");
            }
        }

        /// <summary>
        /// Checks whether the user has scrolled to the bottom of the page.
        /// </summary>
        private async void CheckForward()
        {
            // Check if the user has scrolled to the bottom
            var scrollY = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");
            var documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
            var windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");

            try
            {
                if (scrollY == lastScroll)
                {
                    MoveToNext();
                }

                Debug.WriteLine("CheckForward() - Success\n");
            }

            catch
            {
                Debug.WriteLine("CheckForward() - Fail\n");
            }

            //Debug.WriteLine($"scrollY: {scrollY}, documentHeight: {documentHeight}, windowHeight: {windowHeight}\n");
            lastScroll = scrollY;
        }

        /// <summary>
        /// Checks whether the user has scrolled to the top of the page.
        /// </summary>
        private async void CheckBackward()
        {
            var scrollY = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");
            var documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
            var windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");

            try
            {
                if (scrollY == lastScroll)
                {
                    MoveToPrevious();
                }
                Debug.WriteLine("CheckBackward() - Success\n");
            }

            catch
            {
                Debug.WriteLine("CheckBackward() - Fail\n");
            }

            //Debug.WriteLine($"scrollY: {scrollY}, documentHeight: {documentHeight}, windowHeight: {windowHeight}\n");
            lastScroll = scrollY;
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Left)
            {
                Debug.WriteLine("Left key pressed");
                Backward_Click(sender, e);
            }
            else if (e.Key == Windows.System.VirtualKey.Right)
            {
                Debug.WriteLine("Left key pressed");

                Forward_Click(sender, e);
            }
        }

        /// <summary>
        /// Changes the colors of the CommandBar buttons.
        /// </summary>
        private async void ChangeCommandBarColors()
        {
            string color_string = "#efe0cd";
            string font_string;

            try
            {
                color_string = await (SettingsPage.LoadBackgroundColorComboBox());
                Debug.WriteLine($"Color:{color_string}");
                font_string = await (SettingsPage.LoadFontComboBox());
            }

            finally
            {
                Windows.UI.Color _viewerBackgroundColor = ParseHexColor(color_string);
                ViewerGrid.Background = new SolidColorBrush(_viewerBackgroundColor);
            }

            MyCommandBar.Background = new SolidColorBrush(_backgroundColor);
            MyCommandBar.Foreground = new SolidColorBrush(_foregroundColor);
            Settings.Foreground = Home.Foreground = UpdateCSS.Foreground = new SolidColorBrush(_buttonColor);
        }


        /// <summary>
        /// Parses
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
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
    }
}

