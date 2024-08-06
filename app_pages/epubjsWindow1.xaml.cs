using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using EpubReader.code;
using HarfBuzzSharp;
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
        private string _cssPath = FileManagment.GetEbookViewerStyleFilePath();


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
            _ebook = JsonHandler.ReadEbookJsonFile(FileManagment.GetEbookDataJsonFile(navValueTuple.ebookFolderPath));
            LoadWebViewAsync();

        }

        public async void LoadWebViewAsync()
        {
            await epubjsWindowLoad();
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
            await epubjsWebView.EnsureCoreWebView2Async(environment);

            //string htmlFilePath1 = "C:\\Users\\david_pmv0zjd\\Desktop\\epubjs-reader\\index.html";
            //string htmlFilePath2 = "C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\scripts\\epubjs-reader\\index.html";
            epubjsWebView.Source = new Uri(htmlCode);
        }
    }
}
