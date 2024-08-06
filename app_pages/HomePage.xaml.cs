using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using System.Text.Json;
using EpubReader.code;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader.app_pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {

        // my objects
        EpubHandler epubHandler = new EpubHandler();
        FileManagment fileManagment = new FileManagment();
        app_logging logger = new app_logging();
        app_controls appControls = new app_controls();
        RecentEbooksHandler REHandler = new RecentEbooksHandler();

        public HomePage()
        {
            this.InitializeComponent();
            LoadImages();
        }


        public async void LoadImages()
        {
            // Clear existing images
            ImageStackPanel.Children.Clear();

            List<string> ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated();

            foreach (var ebookPath in ebookPaths)
            {

                var imagePath = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[0];
                var ebookTitle = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[1];
                var ebookFolderPath = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[2];
                var ebookPlayOrder = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[3];


                (string ebookPlayOrder, string ebookFolderPath) naValueTuple = (ebookPlayOrder, ebookFolderPath);

                // Create a vertical StackPanel to hold the image and title
                StackPanel imagePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                    Width = 200,
                };

                // Create the Image
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(this.BaseUri, imagePath)),
                    Width = 200,
                    Height = 200,


                };

                Button button = new Button
                {
                    Content = image,
                    Width = 200,
                    Height = 200,
                };



                button.Click += (sender, e) =>
                {

                    Debug.WriteLine("");
                    Debug.WriteLine("******************************");
                    Debug.WriteLine($"XHTML: {ebookPlayOrder}");
                    Debug.WriteLine("******************************");
                    Debug.WriteLine("");

                    /*
                    epubjsWindow1 secondWindow = new epubjsWindow1(naValueTuple);
                    secondWindow.WindowClosed += SecondWindow_WindowClosed; // Subscribe to the event
                    secondWindow.Activate();
                    */

                    SelectViewer(naValueTuple);


                };

                // Create the TextBlock for the title
                TextBlock title = new TextBlock
                {
                    Text = ebookTitle, // Use the file name as the title
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0),
                    MaxWidth = 200
                };


                // Add the Image and TextBlock to the StackPanel
                imagePanel.Children.Add(button);
                imagePanel.Children.Add(title);

                // Add the StackPanel to the ImageStackPanel
                ImageStackPanel.Children.Add(imagePanel);
            }
        }

        private void SelectViewer((string ebookPlayOrder, string ebookFolderPath) navTuple)
        {
            globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));

            switch (settings.ebookViewer)
            {
                case "epubjs":
                    epubjsWindow1 secondWindow = new epubjsWindow1(navTuple);
                    secondWindow.WindowClosed += SecondWindow_WindowClosed; // Subscribe to the event
                    secondWindow.Activate();
                    break;

                case "WebView2":
                    EbookWindow ebookWindow = new EbookWindow(navTuple);
                    ebookWindow.WindowClosed += SecondWindow_WindowClosed; // Subscribe to the event
                    ebookWindow.Activate();
                    break;
            }

        }

        // Event handler for when the EbookWindow is closed
        private void SecondWindow_WindowClosed(object sender, EventArgs e)
        {
            // Call the method you want to run after the window is closed
            LoadImages();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            ImageScrollViewer.ChangeView(ImageScrollViewer.HorizontalOffset - 100, null, null);
        }

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            ImageScrollViewer.ChangeView(ImageScrollViewer.HorizontalOffset + 100, null, null);
        }
    }
}
