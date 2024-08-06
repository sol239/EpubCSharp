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
using EpubReader.app_pages;
using System.Security.AccessControl;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader


{
    public class Navigations
    {
        public static void NavigateToEbookViewer()
        {

        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MyMainWindow : Page
    {
        // my objects
        EpubHandler epubHandler = new EpubHandler();
        FileManagment fileManagment = new FileManagment();
        app_logging logger = new app_logging();
        app_controls appControls = new app_controls();
        RecentEbooksHandler REHandler = new RecentEbooksHandler();

        public static MyMainWindow Instance { get; private set; }

        public Frame contentFrame { get; private set; }

        public MyMainWindow()
        {
            this.InitializeComponent();
            Debug.WriteLine("\nMY MAIN WINDOW CONSTRUCTOR CALLED\n");

            ContentFrame.Navigate(typeof(HomePage));

            // Subscribe to visibility change events
            NavigationViewService.NavigationViewVisibilityChanged += OnNavigationViewVisibilityChanged;

            //LoadImages();


        }
        public void NavigateToEbookViewer(Type pageType, (string ebookPlayOrder, string ebookFolderPath) navValueTuple)
        {
            var _pageType = (typeof(EbookViewer), navValueTuple);
            _ = ContentFrame.Navigate(pageType);
        }
        private async void AddBookButtonAction(object sender, RoutedEventArgs e)
        {
            await appControls.AddBookButtonMethod();
            Debug.WriteLine("AddBookButtonAction");

        }

        private void OnNavigationViewVisibilityChanged(Visibility visibility)
        {
            MyNavView.Visibility = visibility;
        }


        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navigationOptions = new FrameNavigationOptions();
            navigationOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                navigationOptions.IsNavigationStackEnabled = false;
            }

            Type pageType = typeof(AllBooks);
            var selectedItem =(NavigationViewItem)args.SelectedItem;
            if (selectedItem.Name == NavView_AllBooks.Name)
            {
                pageType = typeof(AllBooks);
            }
            else if(selectedItem.Name == NavView_Settings.Name) {
                pageType = typeof(SettingsPage);

            }
            else if(selectedItem.Name == NavView_Stats.Name) {
                pageType = typeof(Stats);
            }
            else if (selectedItem.Name == NavView_Home.Name)
            {
                pageType = typeof(HomePage);
            }

            ContentFrame.Navigate(pageType);
        }

        public void NavigateToEbookInfoPage()
        {
            Type pageType = typeof(EbookInfoPage);
            if (ContentFrame != null)
            {
                _ = ContentFrame.Navigate(pageType);
            }
            else
            {
                // Handle the null case, possibly log an error or delay the navigation
                Debug.WriteLine("ContentFrame is null");
            }
        }

        // In the SecondWindow.xaml.cs or any other part where you have a reference to the window


        public static event EventHandler< (double width, double height)> WindowResized;


        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double actualWidth = e.NewSize.Width;
            double actualHeight = e.NewSize.Height;

            WindowResized?.Invoke(this, (actualWidth, actualHeight));

            //Debug.WriteLine($"Width = {actualWidth}");
            //Debug.WriteLine($"Height = {actualHeight}");
        }
    }
}
