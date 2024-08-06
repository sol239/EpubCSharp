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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    public class Photo
    {
        public BitmapImage PhotoBitmapImage { get; set; }
        public string Title { get; set; } //public int Likes { get; set; }

        public string EbookPath{ get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AllBooks : Page
    {

        private Frame contentFrame;

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



        public AllBooks()
        {
            this.InitializeComponent();
            Photos = new ObservableCollection<Photo>();
            PopulatePhotos();
            SetDimensions();
            ObtainDimensions();

            MyMainWindow.WindowResized += OnSizeChanged; // Subscribe to the event

        }

        public ObservableCollection<Photo> Photos
        {
            get; private set;
        }

        private void ObtainDimensions()
        {
            /*
            BookCoverWidth = BookCover.Width;
            BookCoverHeight = BookCover.Height;
            BookCoverMinWidth = BookCover.MinWidth; */

            /*
            BookStackPanelOpacity = BookStackPanel.Opacity;
            BookStackPanelHeight = BookStackPanel.Height;*/

            AllBooksViewWidth = AllBooksView.Width;
            AllBooksViewHeight = AllBooksView.Height;

            /*
            AllLayout_MaximumRowsOrColumns = AllLayout.MaximumRowsOrColumns;
            AllLayout_MinColumnSpacing = AllLayout.MinColumnSpacing;
            AllLayout_MinRowSpacing = AllLayout.MinRowSpacing;
            */

            AllLayout_MaximumRowsOrColumns = AllLayout.MaximumRowsOrColumns;
            AllLayout_MinColumnSpacing = AllLayout.MinColumnSpacing;
            AllLayout_MinRowSpacing = AllLayout.MinRowSpacing;

        }

        private void ChangeDimensions()
        {
            /*
            double width = actualWidth;
            int grid_size = 210;
            double x = 0;
            x = width - (grid_size * ((int)width / grid_size));
            double newPadding = x / ((int)width / grid_size);
            AllLayout.MinColumnSpacing = newPadding;*/
        }


        private void OnSizeChanged( object sender, (double width, double height) tp )
        {
            actualWidth = tp.width;
            actualHeight = tp.height;
            AllBooksView.Height = actualHeight - 55;

            Debug.WriteLine($"Width = {actualWidth}");
            Debug.WriteLine($"Height = {actualHeight}");
            //ChangeDimensions();

            // Handle the new dimensions
        }






        private void PopulatePhotos()
        {
            // add photos into Photos collection

            List<string> ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated();

            foreach (var ebookPath in ebookPaths)
            {

                var imagePath = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[0];
                var ebookTitle = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[1];
                var ebookFolderPath = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[2];
                var ebookPlayOrder = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[3];
                (string ebookPlayOrder, string ebookFolderPath) naValueTuple = (ebookPlayOrder, ebookFolderPath);

                Photos.Add(new Photo { PhotoBitmapImage = new BitmapImage(new Uri(imagePath)), Title = ebookTitle, EbookPath = ebookPath });


            }
        }

        public void SetDimensions()
        {
            float bookCount = RecentEbooksHandler.GetRecentEbooksPathsUpdated().Count;
            int multiply = 210;
            // Assuming 'this' is your Window
            double width = this.ActualWidth;
            double height = this.ActualHeight;
            //AllBooksView_GridLayout.MaximumRowsOrColumns = 
            //AllBooksView.Width = 5 * (multiply);
            //AllBooksView.Height = Math.Ceiling(bookCount / 5) * (multiply);
        }

        private void AllBooksView_ItemClick(ItemsView sender, ItemsViewItemInvokedEventArgs args)
        {
            var invokedBook = args.InvokedItem as Photo;

            var ebookPath = invokedBook.EbookPath;

            var imagePath = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[0];
            var ebookTitle = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[1];
            var ebookFolderPath = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[2];
            var ebookPlayOrder = ebookPath.Split(RecentEbooksHandler.MetaSplitter)[3];


            (string ebookPlayOrder, string ebookFolderPath) naValueTuple = (ebookPlayOrder, ebookFolderPath);

            /*
            EbookWindow secondWindow = new EbookWindow(naValueTuple);
            secondWindow.WindowClosed += SecondWindow_WindowClosed; // Subscribe to the event
            secondWindow.Activate();
            */



        }

        // Event handler for when the EbookWindow is closed
        private void SecondWindow_WindowClosed(object sender, EventArgs e)
        {
            // clear the Photos collection
            Photos.Clear();

            // Call the method you want to run after the window is closed
            PopulatePhotos();
        }

    }



}

