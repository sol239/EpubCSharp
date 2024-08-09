using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using System.Collections.ObjectModel;
using EpubReader.app_pages;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using System.Text.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AllBooks : Page
    {

        private Frame contentFrame;

        private (string ebookPlayOrder, string ebookFolderPath) naValueTuple;

        private int BookGridWidth;
        private int BookGridHeight;

        private double BookCoverWidth;
        private double BookCoverHeight;
        private double BookCoverMinWidth;

        private double BookStackPanelOpacity;
        private double BookStackPanelHeight;

        private double AllBooksViewWidth;
        private double AllBooksViewHeight;

        private double AllLayout_MaximumRowsOrColumns;
        private double AllLayout_MinRowSpacing;
        private double AllLayout_MinColumnSpacing;

        double actualWidth;
        double actualHeight;

        private string method;


        public AllBooks()
        {
            this.InitializeComponent();

            MyMainWindow.WindowResized += OnSizeChanged; // Subscribe to the event
            this.Unloaded += OnAllBooksUnloaded;

            Ebooks = new ObservableCollection<Ebook>();
            PopulateEbooks("Name", true, false);
            comboBoxesSetup();


        }

        private void OnAllBooksUnloaded(object sender, RoutedEventArgs e)
        {
            MyMainWindow.WindowResized -= OnSizeChanged; // Unsubscribe from the event
        }

        public ObservableCollection<Ebook> Ebooks
        {
            get; private set;
        }

        
        private void OnSizeChanged( object sender, (double width, double height) tp )
        {
            actualWidth = tp.width;
            actualHeight = tp.height;
            AllBooksView.Height = actualHeight - 55;
        }

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

        private void AllBooksView_ItemClick(ItemsView sender, ItemsViewItemInvokedEventArgs args)
        {
            Debug.WriteLine($"Args = {args}");
            Debug.WriteLine($"Invoked Item = {args.InvokedItem}");

            var invokedBook = args.InvokedItem as Ebook;
            naValueTuple = (invokedBook.InBookPosition, invokedBook.EbookFolderPath);
            SelectViewer(naValueTuple);
            
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
                    ebookWindow.Activate();
                    break;
            }

        }

        // Event handler for when the EbookWindow is closed
        private void SecondWindow_WindowClosed(object sender, EventArgs e)
        {
            // clear the Ebooks collection
            Ebooks.Clear();
            PopulateEbooks(method, true, false);

        }

        private void comboBoxesSetup()
        {
            foreach (var method in code.AllBooks.sortingMethods)
            {
                SortComboBox.Items.Add(method);
            }

            SortComboBox.SelectedIndex = code.AllBooks.sortingMethods.IndexOf("Name");

        }

        private void SortComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            method = (string)SortComboBox.SelectedValue;
            PopulateEbooks(method, true, false);
        }

        private List<FrameworkElement> GetParents(FrameworkElement element)
        {
            var parents = new List<FrameworkElement>();
            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;

            while (parent != null)
            {
                parents.Add(parent);
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            return parents;
        }

        private void AllBooksView_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            /*
            // Get the original source of the event
            var originalSource = e.OriginalSource as FrameworkElement;

            if (originalSource != null)
            {

                // Get and print the parents of the element
                var parents = GetParents(originalSource);
                foreach (var parent in parents)
                {

                   // Debug.WriteLine($"Parent: {parent.Ty}");

                    // Retrieve the data context of the tapped item
                    var tappedEbook = parent.DataContext as Ebook;

                    if (tappedEbook != null)
                    {
                        // Now you have the tapped ebook data
                        Debug.WriteLine($"Right-tapped on ebook: {tappedEbook.Title} by {tappedEbook.Author}");
                        // You can now perform any action with the tappedEbook data
                    }
                }
            }
            */
        }

    }



}

