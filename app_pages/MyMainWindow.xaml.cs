using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using EpubReader.app_pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader


{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyMainWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyMainWindow"/> class.
        /// </summary>
        public MyMainWindow()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(HomePage));
        }

        /// <summary>
        /// Occurs when the size of the window changes.
        /// </summary>
        /// <remarks>
        /// The event provides a tuple containing the new width and height of the window.
        /// </remarks>
        public static event EventHandler< (double width, double height)> WindowResized;

        /// <summary>
        /// Handles the SizeChanged event for a FrameworkElement.
        /// Invokes the WindowResized event with the new width and height of the element.
        /// </summary>
        /// <param name="sender">The source of the event, which is the FrameworkElement that has changed size.</param>
        /// <param name="e">The event data containing the new size of the element.</param>
        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double actualWidth = e.NewSize.Width;
            double actualHeight = e.NewSize.Height;
            WindowResized?.Invoke(this, (actualWidth, actualHeight));
        }

        /// <summary>
        /// Handles the selection change event for the NavigationView control.
        /// Navigates to the appropriate page based on the selected item in the NavigationView pane.
        /// </summary>
        /// <param name="sender">The source of the event, which is the NavigationView control.</param>
        /// <param name="args">The event data containing information about the selection change.</param>
        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navigationOptions = new FrameNavigationOptions();
            navigationOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                navigationOptions.IsNavigationStackEnabled = false;
            }

            Type pageType = typeof(AllBooks);
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            if (selectedItem.Name == NavViewAllBooks.Name)
            {
                pageType = typeof(AllBooks);
            }
            else if (selectedItem.Name == NavViewSettings.Name)
            {
                pageType = typeof(SettingsPage);

            }
            else if (selectedItem.Name == NavViewStats.Name)
            {
                pageType = typeof(Stats);
            }
            else if (selectedItem.Name == NavViewHome.Name)
            {
                pageType = typeof(HomePage);
            }

            else if (selectedItem.Name == NavViewDictionary.Name)
            {
                pageType = typeof(Dictionary);
            }

            ContentFrame.Navigate(pageType);
        }
    }
}
