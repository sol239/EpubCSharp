using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using EpubReader.code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Web.WebView2.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader.app_pages
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class epubjsWindow1 : Window
    {
        //  the current ebook being read.
        private Ebook _ebook;

        // path to the XHTML file of the ebook
        private string _xhtmlPath = "";

        // path to the CSS file for styling the ebook.
        private string _cssPath = FileManagement.GetEbookViewerStyleFilePath();


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
        public epubjsWindow1((string ebookPlayOrder, string ebookFolderPath) data)
        {
            this.InitializeComponent();
            navValueTuple = data;
            _ebook = JsonHandler.ReadEbookJsonFile(FileManagement.GetEbookDataJsonFile(navValueTuple.ebookFolderPath));
            LoadWebViewAsync();

        }

        public async void LoadWebViewAsync()
        {
            await epubjsWindowLoad();
        }

        private int startUp = 0;

        private async void fontsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine($"fontsComboBoxSelectionChanged() - Success - {startUp}");

            if (startUp >= 2)
            {
                string newFontFamily = SettingsPage.BookReadingFonts[fontsComboBox.SelectedIndex];

                // store to json
                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                settings.Font = newFontFamily;
                File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));


                SettingsPage.UpdateBodyFontFamily(newFontFamily);
                await UpdateCSSAction();

            }

            else
            {
                startUp++;
            }

        }

        private async void ThemesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine($"ThemesComboBox_OnSelectionChanged() - Success - {startUp}");

            if (startUp >= 2)
            {
                string theme = SettingsPage.Themes.Keys.ToList()[ThemesComboBox.SelectedIndex];
                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                settings.Theme = theme;
                File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                SettingsPage.UpdateBodyTextColor(SettingsPage.Themes[theme]["text-color"]);
                SettingsPage.UpdateBodyBackgroundColor(SettingsPage.Themes[theme]["background-color"]);
                await UpdateCSSAction();
                ChangeCommandBarColors();

            }

            else
            {
                startUp++;
            }

        }

        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            PopupControl.IsOpen = !PopupControl.IsOpen;
        }

        private void PaddingButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox
            string padding = PaddingBox.Text;

            // Try to convert the string to a double
            if (double.TryParse(padding, out double paddingValue))
            {
                // Perform your action with the message here
                // For example, display it in a message box
                if (!string.IsNullOrWhiteSpace(padding) && paddingValue > 0)
                {
                    GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                    settings.Padding = padding;
                    File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                    PaddingBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#c9ffad"));
                    Debug.WriteLine("PaddingButton_OnClick() - Success");
                    ChangeCommandBarColors();
                }
            }
            else
            {
                PaddingBox.Text = "Type a number bigger than 0...";
                PaddingBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#f2aeb4"));

            }



        }

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


        private async void ChangeCommandBarColors()
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

        private async Task UpdateCSSAction()
        {
            try
            {
                //await Task.Run(() => app_controls.GlobalCssInjector());
                MyWebView.Reload(); // Reload the WebView to apply CSS changes

                // Wait for the WebView to finish loading
                MyWebView.NavigationCompleted += async (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        //await RestorePositionAsync(); // Restore scroll position
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


        private async void FontSizeButton_OnClick(object sender, RoutedEventArgs e)
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
                    Debug.WriteLine("FontSizeButton_OnClick() - Success");
                    await UpdateCSSAction();
                }
            }
            else
            {
                FontSizeBox.Text = "Type a number bigger than 0...";
                FontSizeBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#f2aeb4"));

            }
        }

        public string jsPathConverter(string path)
        {
            string newPath = path.Replace("\\", "/");
            return newPath;
        }

        public static async Task WriteJavaScriptFile(string filePath, string epubFilePath)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Debug.WriteLine($"Saved To: {filePath}");

            // Define the JavaScript content with a placeholder for the EPUB path
            // Define the JavaScript content with a placeholder for the EPUB path
            string jsContent = $@"
    ""use strict"";

    document.onreadystatechange = function () {{
        if (document.readyState === ""complete"") {{
            window.reader = ePubReader(""{epubFilePath}"", {{
                restore: true
            }});
        }}
    }};
";

            // Write the content to the specified file
            await File.WriteAllTextAsync(filePath, jsContent);

            Debug.WriteLine($"WriteJavaScriptFile() - Success");
        }

        private async Task epubjsWindowLoad()
        {


            // get path of the app
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);



            string htmlCode = appPath + "\\" + "scripts" + "\\" + "epubjs-reader" + "\\" + "index.html";
            string jsCodePath = appPath + "\\" + "scripts" + "\\" + "epubjs-reader" + "\\" + "ebookLocator.js";
            await WriteJavaScriptFile(jsCodePath, jsPathConverter(_ebook.ContentPath) );

            // print content of the js file:
            string jsCode = await File.ReadAllTextAsync(jsCodePath);
            Debug.WriteLine($"\n{jsCode}\n");


            Debug.WriteLine($"HTML Code Path: {htmlCode}");


            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--disable-web-security");

            var environmentOptions = new CoreWebView2EnvironmentOptions
            {
                AdditionalBrowserArguments = "--disable-web-security"
            };

            var environment = await CoreWebView2Environment.CreateWithOptionsAsync("", "", environmentOptions);
            await MyWebView.EnsureCoreWebView2Async(environment);

            //string htmlFilePath1 = "C:\\Users\\david_pmv0zjd\\Desktop\\epubjs-reader\\index.html";
            //string htmlFilePath2 = "C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\scripts\\epubjs-reader\\index.html";
            MyWebView.Source = new Uri(htmlCode);
        }
    }
}
