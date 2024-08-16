using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using EpubCSharp.code;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubCSharp.app_pages
{
    /// <summary>
    /// Class for the HomePage
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private EpubHandler _epubHandler = new EpubHandler();
        private AppControls _appControls = new AppControls();

        private double _actualWidth;
        private double _actualHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomePage"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the event handlers for various events related to the <see cref="HomePage"/> instance. 
        /// It initializes the components of the page, subscribes to events such as when a book is added or when the main window is resized, 
        /// and handles the page unloading event. Additionally, it invokes the <see cref="LoadImages2"/> method to perform further initialization tasks related to images.
        /// </remarks>
        public HomePage()
        {
            this.InitializeComponent();

            _epubHandler.BookAddedEvent += OnBookAdded;
            MyMainWindow.WindowResized += OnSizeChanged;
            this.Unloaded += OnHomePageUnloaded;

            LoadImages2();
        }

        /// <summary>
        /// Loads images representing recent ebooks into the <see cref="ImageStackPanel"/> with specified width for each item.
        /// </summary>
        /// <param name="stackPanelWidth">
        /// The width to be applied to each image grid and stack panel. Default is 200.
        /// </param>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="number">
        /// <item>Clears any existing children from the <see cref="ImageStackPanel"/> to prepare for new content.</item>
        /// <item>Retrieves a list of recent ebooks, ordered by their last opened date, using <see cref="RecentEbooksHandler.GetRecentEbooksPathsUpdated"/>.</item>
        /// <item>Iterates over each ebook in the list to create a visual representation.</item>
        /// <item>For each ebook:
        ///   <list type="bullet">
        ///     <item>Creates a <see cref="Grid"/> to hold the ebook's image and details.</item>
        ///     <item>Sets up event handlers for mouse interactions to change the grid's background color and to handle clicks.</item>
        ///     <item>Creates a <see cref="StackPanel"/> for layout, containing an <see cref="Image"/> of the ebook cover and a <see cref="Button"/> to handle click events.</item>
        ///     <item>Creates another <see cref="StackPanel"/> to display the ebook's title and author.</item>
        ///     <item>Adds the image and details to the grid.</item>
        ///     <item>Adds the grid to the <see cref="ImageStackPanel"/>.</item>
        ///   </list>
        /// </item>
        /// </list>
        /// </remarks>
        public void LoadImages2(double stackPanelWidth = 200)
        {
            ImageStackPanel.Children.Clear();
            //            List<Ebook> ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated("DateLastOpened");
            List<string> ebooksPaths = JsonSerializer.Deserialize<List<string>>(
                File.ReadAllText(FileManagement.GetEbookAllBooksJsonFile()));

            List<Ebook> ebookPaths = new List<Ebook>();

            foreach (var ebookPath in ebooksPaths)
            {
                var ebook = JsonHandler.ReadEbookJsonFile(ebookPath);
                ebookPaths.Add(ebook);

            }
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
                    Width = stackPanelWidth,
                    Height = 200,
                };

                button.Click += (sender, e) =>
                {
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

        /// <summary>
        /// Opens the appropriate ebook viewer window based on the user's settings.
        /// </summary>
        /// <param name="navTuple">
        /// A tuple containing the ebook's play order and folder path, used to initialize the viewer.
        /// </param>
        public void SelectViewer((string ebookPlayOrder, string ebookFolderPath) navTuple)
        {
            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
            string title = JsonHandler.ReadEbookJsonFile(FileManagement.GetEbookDataJsonFile(navTuple.ebookFolderPath)).Title;
            Debug.WriteLine(title);
            switch (settings.EbookViewer)
            {
                case "epubjs":
                    epubjsWindow secondWindow = new epubjsWindow(navTuple);
                    secondWindow.WindowClosed += SecondWindow_WindowClosed;
                    secondWindow.Title = title;
                    secondWindow.Activate();
                    break;

                case "Custom":
                    EbookWindow ebookWindow = new EbookWindow(navTuple);
                    ebookWindow.WindowClosed += SecondWindow_WindowClosed;
                    ebookWindow.Title = title;
                    ebookWindow.Activate();
                    break;

            }


        }

        /// <summary>
        /// Handles the Unloaded event for the <see cref="HomePage"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the Unloaded/> event.</param>
        private void OnHomePageUnloaded(object sender, RoutedEventArgs e)
        {
            MyMainWindow.WindowResized -= OnSizeChanged; // Unsubscribe from the event
        }

        /// <summary>
        /// Handles the <see cref="EpubHandler.BookAddedEvent"/> event to display a dialog when a book is added.
        /// </summary>
        /// <param name="sender">The source of the event, typically the object that raised the <see cref="EpubHandler.BookAddedEvent"/>.</param>
        /// <param name="message">The message to be displayed in the dialog, indicating the status of the book addition.</param>
        private async void OnBookAdded(object sender, string message)
        {

            ContentDialog dialog = new ContentDialog
            {
                Title = "Book Addition Status",
                Content = message,
                CloseButtonText = "Ok"
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Handles the click event for the "Add Book" button. This method attempts to add a book using the <see cref="AppControls.AddBookButtonMethod"/> 
        /// and displays a dialog to inform the user of the result. It also reloads images or performs related tasks after the book addition attempt.
        /// </summary>
        /// <param name="sender">
        /// The source of the event, which is typically the "Add Book" button that was clicked. This parameter is used to identify the control that triggered the event.
        /// </param>
        /// <param name="e">
        /// The event data associated with the click event, providing additional information about the event. In this case, it contains data related to the button click action.
        /// </param>
        /// <remarks>
        /// This method asynchronously invokes the <see cref="AppControls.AddBookButtonMethod"/> to perform the book addition process. 
        /// Depending on the result of the book addition attempt, it displays a dialog to inform the user whether the operation was successful or failed. 
        /// After showing the result dialog, it calls the <see cref="LoadImages2"/> method to refresh or load images or perform other related tasks.
        /// </remarks>
        /// <exception cref="Exception">
        /// This method does not explicitly handle exceptions, but exceptions thrown during the execution of <see cref="AppControls.AddBookButtonMethod"/> 
        /// or while showing the dialog may impact the method. Ensure proper error handling is in place in the <see cref="AppControls.AddBookButtonMethod"/> 
        /// and other related methods to manage potential issues.
        /// </exception>
        private async void AddBookButtonAction(object sender, RoutedEventArgs e)
        {
            bool result = await _appControls.AddBookButtonMethod();

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

        /// <summary>
        /// Asynchronously displays a dialog to indicate the status of a book addition operation.
        /// </summary>
        /// <param name="message">
        /// The message to be shown in the dialog, which describes the result of the book addition.
        /// </param>
        /// <param name="debug">
        /// A boolean value indicating whether to log debug messages. Default is <c>false</c>.
        /// </param>
        /// <remarks>
        /// This method creates a <see cref="ContentDialog"/> with the specified message and displays it to the user.
        /// The dialog's <c>XamlRoot</c> is set to the page's content root to ensure proper display within the current context.
        /// If the dialog is shown successfully, a success message is logged. If an exception occurs, it is logged for troubleshooting.
        /// </remarks>
        /// <exception cref="Exception">
        /// Any exceptions thrown while creating or showing the dialog are caught and logged.
        /// </exception>
        private async void OpenBookAddedDialogue(string message, bool debug = false)
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
                if (debug) { Debug.WriteLine($"OpenBookAddedDialogue() - Success"); }
            }
            catch (Exception e)
            {
                if (debug) { Debug.WriteLine($"OpenBookAddedDialogue() - Fail - {e.Message}"); }
            }
        }

        /// <summary>
        /// Handles the event when the size of the main window changes.
        /// </summary>
        /// <param name="sender">
        /// The source of the event, typically the window or control whose size changed.
        /// </param>
        /// <param name="tp">
        /// A tuple containing the new width and height of the window or control.
        /// </param>
        /// <remarks>
        /// This method updates the internal width and height properties based on the new size. It then adjusts the width of the <see cref="ImageScrollViewer"/> 
        /// and calls the <see cref="LoadImages2"/> method to reload images based on the new width. Note that setting the width of <see cref="ImageScrollViewer"/> 
        /// directly can cause performance issues such as lag, which may need to be addressed.
        /// </remarks>
        private void OnSizeChanged(object sender, (double width, double height) tp)
        {
            _actualWidth = tp.width;
            _actualHeight = tp.height;

            // Causes lags  
            ImageScrollViewer.Width = _actualWidth;
            LoadImages2(_actualWidth / 5);
        }

        /// <summary>
        /// Loads images representing recent ebooks into the <see cref="ImageStackPanel"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SecondWindow_WindowClosed(object sender, EventArgs e)
        {
            // Call the method you want to run after the window is closed
            LoadImages2();
        }

        /// <summary>
        /// Handles the click event for scrolling the image viewer left.
        /// </summary>
        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            ImageScrollViewer.ChangeView(ImageScrollViewer.HorizontalOffset - 100, null, null);
        }

        /// <summary>
        /// Handles the click event for scrolling the image viewer right.
        /// </summary>
        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            ImageScrollViewer.ChangeView(ImageScrollViewer.HorizontalOffset + 100, null, null);
        }
    }
}
