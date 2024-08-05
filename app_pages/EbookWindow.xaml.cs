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
        private string _script = @"
    function scrollDown() {
        window.scrollBy(0, window.innerHeight);
    }

    function scrollUp() {
        window.scrollBy(0, -window.innerHeight);
    }

    function checkScroll() {
        if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight) {
            // Notify C# when scrolled to the bottom
            window.chrome.webview.postMessage('scrolledToBottom');
        }
    }

    function handleKeyDown(event) {
        if (event.key === 'ArrowDown') {
            scrollDown();
        } else if (event.key === 'ArrowUp') {
            scrollUp();
        }
    }

    function setupEventListeners() {
        window.addEventListener('scroll', checkScroll);
        window.addEventListener('keydown', handleKeyDown);
        window.addEventListener('resize', adjustLayout);
        window.addEventListener('load', adjustLayout);
    }

    function adjustLayout() {
        // Your layout adjustment code here
    }

    // Re-attach event listeners
    setupEventListeners();
    adjustLayout();
    ";

        /// <summary>
        /// Constructor initializes the component and subscribes to Loaded and Unloaded events.
        /// </summary>
        public EbookWindow( (string ebookPlayOrder, string ebookFolderPath) data )
        {
            this.InitializeComponent();
            //NavigationViewService.SetNavigationViewVisibility(Visibility.Collapsed);

            //this.Loaded += EbookViewer_Loaded;
            ChangeCommandBarColors();
            navValueTuple = data;
            OpenEbookMessage(navValueTuple);
            EbookViewer_Loaded();
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
                _ebook.ScrollValue = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollY;");
                _ebook.InBookPosition = navValueTuple.ebookPlayOrder;
                _ebook.DateLastOpened = DateTime.Now.ToString();

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

                    Debug.WriteLine("JSON ebook file stored successfully.");
                    Debug.WriteLine("********************************\n");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An error occurred while storing JSON ebook file: {ex.Message}");
                    Debug.WriteLine("********************************\n");

                }
            }
        }

        public async Task CalculateTimeDifference()
        {
                        // Calculate the time the book was open
            DateTime openTime = DateTime.Parse(_ebook.BookOpenTime);
            DateTime closeTime = DateTime.Parse(_ebook.BookCloseTime);
            TimeSpan readTime = new TimeSpan(0, 0, 0);
            ;
            try
            {
                readTime = TimeSpan.Parse(_ebook.BookReadTime);
            }

            catch
            {
                readTime = new TimeSpan(0, 0, 0);
            }

            finally
            {
                TimeSpan timeDifference = closeTime - openTime;
                TimeSpan totalTime = readTime + timeDifference;
                _ebook.BookReadTime = totalTime.ToString();

                // Get the file path
                string filePath = FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath);
                filePath = Path.GetDirectoryName(filePath);

                // Store the JSON ebook file
                await JsonHandler.StoreJsonEbookFile(_ebook, filePath);
            }
            
        }


        /// <summary>
        /// Loads the playorder and scroll position of the WebView from the ebookData.json file.
        /// </summary>
        /// <returns></returns>
        private async Task RestorePositionAsync()
        {
            _ebook = JsonHandler.ReadEbookJsonFile(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath));
            await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, {_ebook.ScrollValue});");
            _xhtmlPath = FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, _ebook.InBookPosition);

            Debug.WriteLine("\n********************************");
            Debug.WriteLine($"Restore Position: {_xhtmlPath} - Scroll = {_ebook.ScrollValue}");
            Debug.WriteLine("*********************************\n");
        }

        /// <summary>
        /// Executes the JavaScript code in the WebView.
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteJavaScriptAsync()
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
        }

        /// <summary>
        /// Runs when the page is loaded. Initializes the WebView and executes JavaScript.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EbookViewer_Loaded()
        {
            await InitializeWebViewAsync();
            await UpdateCssPath(_xhtmlPath, _cssPath);
            _xhtmlPath = FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, navValueTuple.ebookPlayOrder);
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

            await SaveBookOpenTime();


        }

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
            await MyWebView.EnsureCoreWebView2Async(null);
            await MyWebView.CoreWebView2.ExecuteScriptAsync(_script);
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
                    Debug.WriteLine("CSS path updated successfully.");
                }
                else
                {
                    Debug.WriteLine("No <link> element with rel=\"stylesheet\" found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs when the user clicks the Update CSS button. Updates the CSS path in the XHTML file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateCSSAction(object sender, RoutedEventArgs e)
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
        }

        /// <summary>
        /// Runs when the user clicks the Home button. Navigates to the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoHomeAction(object sender, RoutedEventArgs e)
        {
            await SavePosition();
            await CalculateTimeDifference();

            WindowClosed?.Invoke(this, EventArgs.Empty);

            this.Close();
            //EbookFrame.Navigate(typeof(MyMainWindow));
        }
        
        /// <summary>
        /// Runs when the user clicks the Backward button. Scrolls up in the WebView. If the user has scrolled to the top of the page, navigates to the previous page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Backward_Click(object sender, RoutedEventArgs e)
        {
            await MyWebView.CoreWebView2.ExecuteScriptAsync("scrollUp();");
            await SavePosition();
            CheckBackward();
        }

        /// <summary>
        /// Runs when the user clicks the Forward button. Scrolls down in the WebView. If the user has scrolled to the bottom of the page, navigates to the next page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Forward_Click(object sender, RoutedEventArgs e)
        {
            await MyWebView.CoreWebView2.ExecuteScriptAsync("scrollDown();");
            await SavePosition();
            CheckForward();
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
                navValueTuple.ebookPlayOrder = (playOrder + 1).ToString();
                _xhtmlPath =
                    FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, navValueTuple.ebookPlayOrder);

                Debug.WriteLine($"PlayOrder = {navValueTuple.ebookPlayOrder}");
                Debug.WriteLine("***************");
                Debug.WriteLine("");


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

                await SavePosition();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred in MoveToNext: {ex.Message}");
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
                Debug.WriteLine("***************");
                Debug.WriteLine("");

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


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred in MoveToNext: {ex.Message}");
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
            }

            catch
            {
                Debug.WriteLine("CheckForward() - Fail");
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
            Debug.WriteLine($"scrollY: {scrollY}, documentHeight: {documentHeight}, windowHeight: {windowHeight}\n");
            if (scrollY == lastScroll)
            {
                MoveToPrevious();
            }

            lastScroll = scrollY;


        }
        /// <summary>
        /// Changes the colors of the CommandBar buttons.
        /// </summary>
        private void ChangeCommandBarColors()
        {
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

