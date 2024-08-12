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
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using ABI.Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Text;
using BitmapImage = Microsoft.UI.Xaml.Media.Imaging.BitmapImage;
using Button = Microsoft.UI.Xaml.Controls.Button;
using ContentDialog = Microsoft.UI.Xaml.Controls.ContentDialog;
using Grid = Microsoft.UI.Xaml.Controls.Grid;
using Image = Microsoft.UI.Xaml.Controls.Image;
using Page = Microsoft.UI.Xaml.Controls.Page;
using StackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using TextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using System.Collections.ObjectModel;

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

        private double actualWidth;
        private double actualHeight;

        //public ObservableCollection<Ebook> Ebooks { get; set; }
        public HomePage()
        {
            this.InitializeComponent();
            epubHandler.BookAddedEvent += OnBookAdded; // Subscribe to the event
            // = new ObservableCollection<Ebook>();
            //PopulateEbooks("LastOpened", true, false);

            LoadImages2();
            MyMainWindow.WindowResized += OnSizeChanged; // Subscribe to the event

            this.Unloaded += OnHomePageUnloaded;

        }

        private void OnHomePageUnloaded(object sender, RoutedEventArgs e)
        {
            MyMainWindow.WindowResized -= OnSizeChanged; // Unsubscribe from the event
        }

        private async void OnBookAdded(object sender, string message)
        {
            Debug.WriteLine($"OnBookAdded Event!");

            ContentDialog dialog = new ContentDialog
            {
                Title = "Book Addition Status",
                Content = message,
                CloseButtonText = "Ok"
            };


            await dialog.ShowAsync();
        }

        private async void AddBookButtonAction(object sender, RoutedEventArgs e)
        {
            bool result = await appControls.AddBookButtonMethod();
            Debug.WriteLine("AddBookButtonAction");

            if (result)
            {
                OpenBookAddedDialogue("Book Added Successfully!");
            }

            else
            {
                OpenBookAddedDialogue("Book Addition Failed!");
            }

            LoadImages2();


        }

        private async void OpenBookAddedDialogue(string message)
        {

            try
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Book Addition Status",
                    Content = message,
                    CloseButtonText = "Ok",
                    XamlRoot = this.Content.XamlRoot // Set the XamlRoot property

                };

                await dialog.ShowAsync();
                Debug.WriteLine($"OpenBookAddedDialogue() - Success");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"OpenBookAddedDialogue() - Fail - {e.Message}");
            }
        }

        /// <summary>
        /// Method to handle the window resize event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tp"></param>
        private void OnSizeChanged(object sender, (double width, double height) tp)
        {
            actualWidth = tp.width;
            actualHeight = tp.height;

            // Causes lags  
            ImageScrollViewer.Width = actualWidth;
            LoadImages2(actualWidth / 5);
        }

        /*
        private void PopulateEbooks(string method, bool ascendingOrder, bool print)
        {
            // clear the Ebooks collection
            Ebooks.Clear();

            List<Ebook> ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated(method, ascendingOrder, print);

            foreach (var ebook in ebookPaths)
            {

                Ebooks.Add(ebook);
            }
        }
        */

        public async void LoadImages2(double stackPanelWidth = 200)
        {
            // Clear existing images
            ImageStackPanel.Children.Clear();
            List<Ebook> ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated("DateLastOpened", true, false);
            foreach (var ebook in ebookPaths)
            {
                (string ebookPlayOrder, string ebookFolderPath) naValueTuple = (ebook.InBookPosition, ebook.EbookFolderPath);


                Grid grid = new Grid
                {
                    Width = stackPanelWidth,
                    Height = 200,
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    
                };

                grid.PointerEntered += (s, e) => ((Grid)s).Background = new SolidColorBrush(Colors.LightGray);
                grid.PointerExited += (s, e) => ((Grid)s).Background = new SolidColorBrush(Colors.Transparent);

                grid.PointerReleased += (sender, e) =>
                {
                    SelectViewer(naValueTuple);


                };


                StackPanel imagePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                    Width = stackPanelWidth,
                };

                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(this.BaseUri, ebook.CoverPath)),
                    Width = 200,
                    MinWidth = 70,
                    Height = 200,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Button button = new Button
                {
                    //Content = image,
                    Width = stackPanelWidth,
                    Height = 200,
                    //Background = new SolidColorBrush(Colors.Transparent),
                    //BorderBrush = new SolidColorBrush(Colors.Transparent)

                };

                button.Click += (sender, e) =>
                {
                    /*
                    Debug.WriteLine("");
                    Debug.WriteLine("******************************");
                    Debug.WriteLine($"XHTML: {ebook.InBookPosition}");
                    Debug.WriteLine("******************************");
                    Debug.WriteLine("");
                    */

                    /*
                    epubjsWindow1 secondWindow = new epubjsWindow1(naValueTuple);
                    secondWindow.WindowClosed += SecondWindow_WindowClosed; // Subscribe to the event
                    secondWindow.Activate();
                    */

                    SelectViewer(naValueTuple);


                };

                StackPanel stackPanel1 = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Height = 40,
                    Opacity = 0.99,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Padding = new Thickness(0, 0, 0, 0),
                    Background = (Brush)Application.Current.Resources["SystemControlBackgroundBaseMediumBrush"]

                };

                TextBlock title = new TextBlock
                {
                    Text = ebook.Title,
                    Foreground = (Brush)Application.Current.Resources["SystemControlForegroundAltHighBrush"],
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(3, 2, 0, 0),
                    TextTrimming = TextTrimming.WordEllipsis
                };

                StackPanel stackPanel2 = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                };

                TextBlock author = new TextBlock
                {
                    Text = ebook.Author,
                    Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                    Foreground = (Brush)Application.Current.Resources["SystemControlForegroundAltHighBrush"],
                    Margin = new Thickness(3, 0, 0, 2)

                };

                stackPanel1.Children.Add(title);
                stackPanel2.Children.Add(author);
                stackPanel1.Children.Add(stackPanel2);
                grid.Children.Add(image);
                grid.Children.Add(stackPanel1);

                ImageStackPanel.Children.Add(grid);
            }
        }


        public async void LoadImages(double stackPanelWidth=200)
        {
            // Clear existing images
            ImageStackPanel.Children.Clear();

            List<Ebook> ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated("DateLastOpened", true, false);

            foreach (var ebook in ebookPaths)
            {



                (string ebookPlayOrder, string ebookFolderPath) naValueTuple = (ebook.InBookPosition, ebook.EbookFolderPath);

                // Create a vertical StackPanel to hold the image and title
                StackPanel imagePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                    Width = stackPanelWidth,
                };

                // Create the Image
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(this.BaseUri, ebook.CoverPath)),
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
                    Debug.WriteLine($"XHTML: {ebook.InBookPosition}");
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
                    Text = ebook.Title, // Use the file name as the title
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0),
                    MaxWidth = 200,
                    TextTrimming = TextTrimming.WordEllipsis
                };


                // Add the Image and TextBlock to the StackPanel
                imagePanel.Children.Add(button);
                imagePanel.Children.Add(title);

                // Add the StackPanel to the ImageStackPanel
                ImageStackPanel.Children.Add(imagePanel);
            }
        }

        public void SelectViewer((string ebookPlayOrder, string ebookFolderPath) navTuple)
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
                    Debug.WriteLine($"Current Size = {ebookWindow.AppWindow.Size}");
                    ebookWindow.Activate();
                    break;
            }

        }

        // Event handler for when the EbookWindow is closed
        private void SecondWindow_WindowClosed(object sender, EventArgs e)
        {
            // Call the method you want to run after the window is closed
            LoadImages2();
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
