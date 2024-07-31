using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop; // For initializing with the correct window handle


// my usings
using EpubReader.code;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;
using EpubReader.code;
using EpubReader;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Page
    {
        // my objects
        EpubHandler epubHandler = new EpubHandler();
        FileManagment fileManagment = new FileManagment();
        app_logging logger = new app_logging();
        app_controls appControls = new app_controls();
        RecentEbooksHandler REHandler = new RecentEbooksHandler();


        public MainWindow()
        {
            this.InitializeComponent();
            fileManagment.StartUp();


            Debug.WriteLine("Subscribed to BookAdded event");

            LoadImages();


        }

        private async void MoveToViewerAction(object sender, RoutedEventArgs e)
        {
            // navigate to EbookViewer.xaml
            Frame.Navigate(typeof(EbookViewer));




        }


        private async void AddBookButtonAction(object sender, RoutedEventArgs e)
        {
            await appControls.AddBookButtonMethod();
            Debug.WriteLine("AddBookButtonAction");
            LoadImages();

        }

        public async void LoadImages()
        {
            // Clear existing images
            ImageStackPanel.Children.Clear();

            List<string> ebookPaths = REHandler.GetRecentEbooksPaths();

            foreach (var ebookPath in ebookPaths)
            {

                var imagePath = ebookPath.Split(REHandler.MetaSplitter)[0];
                var ebookTitle = ebookPath.Split(REHandler.MetaSplitter)[1];
                var ebookFolderPath = ebookPath.Split(REHandler.MetaSplitter)[2];
                var ebookPlayOrder = ebookPath.Split(REHandler.MetaSplitter)[3];
                var ebookScroll = ebookPath.Split(REHandler.MetaSplitter)[4];

                var ebookPosition = FileManagment.GetBookContentFilePath(ebookFolderPath, ebookPlayOrder);

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
                    Frame.Navigate(typeof(EbookViewer), naValueTuple);


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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            ImageScrollViewer.ChangeView(ImageScrollViewer.HorizontalOffset - 100, null, null);
        }

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            ImageScrollViewer.ChangeView(ImageScrollViewer.HorizontalOffset + 100, null, null);
        }
        private void NavigateToStats(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(MainWindow));
        }
        private void NavigateToHome(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(MainWindow));
        }
        private void NavigateToDictionary(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Dictionary));
        }
        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(SettingsPage));
        }





    }
}
