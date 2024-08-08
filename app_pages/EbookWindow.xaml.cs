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
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using System.Net.Http;
using System.Text;
using Windows.Media.Protection.PlayReady;

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
        private static readonly HttpClient client = new HttpClient();


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

        private bool _chapterNavigated = false;

        // JavaScript code to manage scrolling and key events
        private string _script = "0";
        private string script1Path = "C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\scripts\\ebook.js";
        private string script2Path = "C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\scripts\\script.js";

        private string _emptyMessage = "*783kd4HJsn";

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
                    //Debug.WriteLine($"SavePosition() 1 - Success");
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine($"SavePosition() 1 - Fail - {ex.Message}");
                    _ebook.ScrollValue = "0";
                }

                try
                {
                    _ebook.InBookPosition = navValueTuple.ebookPlayOrder;
                    //Debug.WriteLine($"SavePosition() 2 - Success");
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine($"SavePosition() 2 - Fail - {ex.Message}");
                }

                try
                {
                    _ebook.DateLastOpened = DateTime.Now.ToString();
                    //Debug.WriteLine($"SavePosition() 3 - Success");
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine($"SavePosition() 3 - Fail - {ex.Message}");
                }

                /*
                Debug.WriteLine("********************************");
                Debug.WriteLine("Save Position");
                Debug.WriteLine($"InBookPosition = {_ebook.InBookPosition} | Scroll = {_ebook.ScrollValue}");
                Debug.WriteLine($"Save To: {FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath)}");*/
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

                    /*Debug.WriteLine("SavePosition() - Success");
                    Debug.WriteLine("********************************\n");*/

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
                
                /*
                Debug.WriteLine("\n********************************");
                Debug.WriteLine($"Restore Position: {_xhtmlPath} - Scroll = {_ebook.ScrollValue}");
                Debug.WriteLine("*********************************\n");
                Debug.WriteLine($"RestorePositionAsync() - Success\n");
                */

                await MyWebView.EnsureCoreWebView2Async(null);

                await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, {_ebook.ScrollValue});");
                _xhtmlPath = FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, _ebook.InBookPosition);

                
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"RestorePositionAsync() - Fail - {ex.Message}\n");
            }
        }

        public string script3 = @"
function handleDocumentClick(event) {
    const clickedElement = event.target; {}

    if (clickedElement.nodeType === Node.ELEMENT_NODE) {
        const textContent = clickedElement.textContent;
        const range = document.createRange();

        for (let i = 0; i < clickedElement.childNodes.length; i++) {
            const node = clickedElement.childNodes[i];
            if (node.nodeType === Node.TEXT_NODE) {
                range.selectNodeContents(node);
                const rect = range.getBoundingClientRect();

                if (rect.left <= event.clientX && rect.right >= event.clientX &&
                    rect.top <= event.clientY && rect.bottom >= event.clientY) {
                    const words = node.textContent.split(' ');
                    let clickedWord = '';

                    for (let j = 0; j < words.length; j++) {
                        range.setStart(node, node.textContent.indexOf(words[j]));
                        range.setEnd(node, node.textContent.indexOf(words[j]) + words[j].length);
                        const wordRect = range.getBoundingClientRect();

                        if (wordRect.left <= event.clientX && wordRect.right >= event.clientX &&
                            wordRect.top <= event.clientY && wordRect.bottom >= event.clientY) {
                            clickedWord = words[j];
                            break;
                        }
                    }

                    if (clickedWord) {
                        window.chrome.webview.postMessage(`${clickedWord}`);
                        console.log(clickedWord);
                    }
                    else {
                        window.chrome.webview.postMessage(`*783kd4HJsn`);
                    }

                    break;
                }
            }
        }
    }

    // Display the selected text
    const selectedText = window.getSelection().toString();
    if (selectedText) {
        window.chrome.webview.postMessage(`${selectedText}`);
        console.log(selectedText);

    } else {
        window.chrome.webview.postMessage('*783kd4HJsn');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.body.addEventListener('click', handleDocumentClick);
});
";
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
                    var result = await MyWebView.CoreWebView2.ExecuteScriptAsync(_script);
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

                //await MyWebView.CoreWebView2.ExecuteScriptAsync(script3);
                //MyWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                /*
                MyWebView.NavigationCompleted += async (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        await ExecuteJavaScriptAsync(script1Path); // Execute JavaScript after the WebView has loaded
                        await ExecuteJavaScriptAsync(script2Path); // Execute JavaScript after the WebView has loaded
                        await RestorePositionAsync();
                    }
                    else
                    {
                        Debug.WriteLine("Navigation failed.");
                    }
                };
                */

                MyWebView.Focus(FocusState.Programmatic);
            }
            catch
            {
                //Debug.WriteLine("EbookViewer_Loaded() - Fail\n");
            }
            finally
            {
                await SaveBookOpenTime();
            }
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
                MyWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                MyWebView.CoreWebView2.Settings.IsScriptEnabled = true;
                _xhtmlPath = FileManagment.GetBookContentFilePath(navValueTuple.ebookFolderPath, navValueTuple.ebookPlayOrder);
                MyWebView.Source = new Uri(_xhtmlPath);
                MyWebView.CoreWebView2.WebMessageReceived += (sender, e) =>
                {
                    string message = e.TryGetWebMessageAsString();
                    if (message == "scrolledDown")
                    {
                        //CheckForward();
                    }

                    else if (message == "scrolledUp")
                    {
                        //CheckBackward();
                    }
                };

                Debug.WriteLine($"InitializeWebViewAsync() - Success\n");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeWebViewAsync() - Fail - {ex.Message}\n");
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            Debug.WriteLine($"CoreWebView2_WebMessageReceived = {message}");

            string messageType = message.Split(" = ")[0];
            string messageContent = message.Split(" = ")[1];

            if (messageContent != _emptyMessage)
            {
                // open flyout
                // Update the Flyout content
                ShowFlyoutAsync(messageType, messageContent);

            }

            


        }



        public static string RemoveSymbols(string text)
        {
            // remove any . , ! ? : ; symbols from the text
            text = text.Replace(".", "");
            text = text.Replace(",", "");
            text = text.Replace("!", "");
            text = text.Replace("?", "");
            text = text.Replace(":", "");
            text = text.Replace(";", "");

            return text;
        }

        public async Task ShowFlyoutAsync(string messageType, string messageContent)
        {
            Debug.WriteLine($"ShowFlyoutAsync - {messageType} - {messageContent}");
            if (messageType == "CLICKED WORD")
            {
                // remove any . , ! ? : ; symbols from the text
                messageContent = RemoveSymbols(messageContent);
            }

            // Create a new Flyout
            Flyout flyout = new Flyout();
            flyout.Placement = FlyoutPlacementMode.Bottom;

            // Create content for the Flyout
            StackPanel stackPanel = new StackPanel
            {
                Width = 200,
                Height = 300
            };

            TextBlock textBlock1 = new TextBlock { Text = $"{messageContent}" };
            textBlock1.FontWeight = FontWeights.Bold;

            // Create and configure the second TextBlock
            TextBlock textBlock2 = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = stackPanel.Width
            };

            // Await the result of the Python script before proceeding
            string result = await GetTranslation(messageContent, "en", "cs");
            Debug.WriteLine($"Result: {result}");
            textBlock2.Text = result;

            Button closeButton = new Button { Content = "Save Translation" };

            // Add close functionality
            closeButton.Click += (s, e) => flyout.Hide();

            // Add elements to the StackPanel
            stackPanel.Children.Add(textBlock1);
            stackPanel.Children.Add(textBlock2);
            stackPanel.Children.Add(closeButton);

            // Set the content of the Flyout
            flyout.Content = stackPanel;

            // Show the Flyout at a specific position after the Python script has finished
            flyout.ShowAt(MyWebView); // 'MyWebView' refers to the control or page where the Flyout is shown
        }

        static async Task<string> RunPythonScript(string text, string sourceLanguage, string targetLanguage)
        {
            // Path to the Python interpreter and the script
            string pythonPath = "C:\\Users\\david_pmv0zjd\\Documents\\translation-ebook\\venv\\Scripts\\python.exe";
            string scriptPath = "C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\code\\translation_script.py";

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
                    Debug.WriteLine($"Python errors: {errors}");
                }

                return output.Trim();
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
                        await RestorePositionAsync(); // Restore scroll position
                        //await ExecuteJavaScriptAsync(script1Path); // Re-execute JavaScript after reload
                        //await ExecuteJavaScriptAsync(script2Path); // Execute JavaScript after the WebView has loaded


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

            //await MyWebView.CoreWebView2.ExecuteScriptAsync(script3);

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

                //Debug.WriteLine("GoHomeAction() - Success\n");
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"GoHomeAction() - Fail - {ex.Message}\n");
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
                //Debug.WriteLine("Backward_Click() - Success\n");
            }
            catch
            {
                //Debug.WriteLine("Backward_Click() - Fail\n");
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

                //Debug.WriteLine("Forward_Click() - Success\n");
            }

            catch
            {
                //Debug.WriteLine("Forward_Click() - Fail\n");
            }
        }

        private async void MoveBackward()
        {
            Debug.WriteLine("MoveBackward");
            _chapterNavigated = false;

            await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollBy(0, -window.innerHeight);");
            await SavePosition(); 
            CheckBackward();
        }

        private async void MoveForward()
        {
            Debug.WriteLine("MoveForward");
            _chapterNavigated = false;

            await MyWebView.CoreWebView2.ExecuteScriptAsync("window.scrollBy(0, window.innerHeight);");
            await SavePosition();
            CheckForward();
        }

        private async void MoveToNext()
        {
            // debug message
            /*
            Debug.WriteLine("");
            Debug.WriteLine("***************");
            Debug.WriteLine("MoveToNext");*/


            if (!_chapterNavigated)
            {
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

                        //Debug.WriteLine($"PlayOrder = {navValueTuple.ebookPlayOrder}");



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
                                Debug.WriteLine("MoveToNext - Navigation completed.");

                                documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
                                windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");
                                await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, 0);");
                                _chapterNavigated = true;
                                return;
                            }
                            else
                            {
                                Debug.WriteLine("Navigation failed.");
                            }
                        };
                    }




                    await SavePosition();

                    /*
                    Debug.WriteLine("MoveToNext() - Success");
                    Debug.WriteLine("***************");
                    Debug.WriteLine("");*/
                }
                catch (Exception ex)
                {
                        Debug.WriteLine($"MoveToNext() - Fail - {ex.Message}\n");
                }
            }


        }

        private async void MoveToPrevious()
        {
            // debug message
            /*
            Debug.WriteLine("");
            Debug.WriteLine("***************");
            Debug.WriteLine("MoveToPrevious");*/


            if (!_chapterNavigated)
            {
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

                    //Debug.WriteLine($"PlayOrder = {navValueTuple.ebookPlayOrder}");


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
                            Debug.WriteLine("MoveToPrevious - Navigation completed.");
                            documentHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("document.body.scrollHeight;");
                            windowHeight = await MyWebView.CoreWebView2.ExecuteScriptAsync("window.innerHeight;");
                            wantedScroll = (float.Parse(documentHeight)).ToString();
                            await MyWebView.CoreWebView2.ExecuteScriptAsync($"window.scrollTo(0, {wantedScroll});");
                            _chapterNavigated = true;

                            return;
                        }
                        else
                        {
                            Debug.WriteLine("Navigation failed.");
                        }
                    };

                    await SavePosition();

                    /*
                    Debug.WriteLine("MoveToPrevious() - Success");
                    Debug.WriteLine("***************");
                    Debug.WriteLine("");*/


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MoveToPrevious() - Fail - {ex.Message}\n");
                }
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
                    Debug.WriteLine($"scrollY = {scrollY} - lastScroll = {lastScroll}");
                    MoveToNext();
                }

                //Debug.WriteLine("CheckForward() - Success\n");
            }

            catch
            {
                //Debug.WriteLine("CheckForward() - Fail\n");
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
                    Debug.WriteLine($"scrollY = {scrollY} - lastScroll = {lastScroll}");

                    MoveToPrevious();
                }
                //Debug.WriteLine("CheckBackward() - Success\n");
            }

            catch
            {
                //Debug.WriteLine("CheckBackward() - Fail\n");
            }

            //Debug.WriteLine($"scrollY: {scrollY}, documentHeight: {documentHeight}, windowHeight: {windowHeight}\n");
            lastScroll = scrollY;
        }

        private async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Left)
            {
                Debug.WriteLine("Left key pressed");

                MoveBackward();
            }
            else if (e.Key == Windows.System.VirtualKey.Right)
            {
                Debug.WriteLine("Right key pressed");

                MoveForward();
            }

            else if (e.Key == Windows.System.VirtualKey.Space)
            {
                //await GetTranslation();
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

        public async Task<string> GetTranslation(string _text, string sourceLanguage, string targetLanguage)
        {
            var url = "http://127.0.0.1:5000/translate"; // URL of your Flask server

            // Create the request data
            var requestData = new
            {
                text = _text,
                source_language = sourceLanguage,
                target_language = targetLanguage
            };

            // Convert request data to JSON
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(url, content);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                var responseString = await response.Content.ReadAsStringAsync();

                // Parse the JSON response
                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    if (doc.RootElement.TryGetProperty("translated_text", out JsonElement translatedTextElement))
                    {
                        var translatedText = translatedTextElement.GetString();
                        // Print the translated text
                        Debug.WriteLine($"Translated text: {translatedText}");
                        return translatedText;
                    }
                    else
                    {
                        Debug.WriteLine("Response does not contain 'translated_text' field.");
                        return "ERROR";
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"Request error: {e.Message}");
                return "ERROR";
            }
        }
    }

}




