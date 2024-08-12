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
using System.Threading.Tasks;

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

        private Ebook _selectedEbook;
        private bool _state = false;

        private string method;
        private Dictionary<string, string> languageDict = new Dictionary<string, string>()
        {

        };

        public AllBooks()
        {
            this.InitializeComponent();
            LoadLangDict();
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

            if (actualHeight - 55 > 0)
            {
                AllBooksView.Height = actualHeight - 55;
                InfoScrollViewer.Height = actualHeight - 55;
            }

            if (actualWidth - detailsPanel.Width > 0 && detailsPanel.Visibility == Visibility.Visible)
            {
                AllBooksView.Width = actualWidth - detailsPanel.Width;
            }

            else {
                AllBooksView.Width = actualWidth;
            }

        }

        private void PopulateEbooks(string method, bool ascendingOrder, bool print)
        {
            // clear the Ebooks collection
            Ebooks.Clear();
            List<Ebook> ebookPaths = new List<Ebook>();

            try
            {
                ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated(method, ascendingOrder, print);
                Debug.WriteLine("PopulateEbooks() 1.1 - Success");
            }
            catch (Exception e)
            {
                Debug.WriteLine("PopulateEbooks() 1.1 - Fail - " + e.Message);
            }

            try
            {
                foreach (var ebook in ebookPaths)
                {

                    Ebooks.Add(ebook);
                    languageComboBox.SelectedIndex = languageDict.Keys.ToList().IndexOf(ebook.Language);
                }
                Debug.WriteLine("PopulateEbooks() 1.2 - Success");
            }

            catch (Exception e)
            {
                Debug.WriteLine("PopulateEbooks() 1.2 - Fail - " + e.Message);
            }
        }
        private void LoadLangDict()
        {
            try
            {
                string path = "C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\app_pages\\iso639I_reduced.json";
                string json = File.ReadAllText(path);
                languageDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                Debug.WriteLine("LoadLangDict() - Success");
            }
            catch (Exception e)
            {
                Debug.WriteLine("LoadLangDict() - Fail - " + e.Message);
            }
        }
        private void AllBooksView_ItemClick(ItemsView sender, ItemsViewItemInvokedEventArgs args)
        {
            try
            {
                Ebook __selectedEbook = args.InvokedItem as Ebook;

                if (_selectedEbook == null)
                {
                    _selectedEbook = __selectedEbook;
                }

                if ((__selectedEbook.EbookFolderPath != _selectedEbook.EbookFolderPath) || detailsPanel.Visibility == Visibility.Collapsed)
                
                {




                    _selectedEbook = __selectedEbook;
                    naValueTuple = (_selectedEbook.InBookPosition, _selectedEbook.EbookFolderPath);

                    // Populate the detailsPanel with the selected book's information
                    bookTitle.Text = _selectedEbook.Title;
                    bookAuthor.Text = $"Author: {_selectedEbook.Author}";
                    // hh:mm:ss "00:13:44"

                    if (_selectedEbook.BookReadTime != null)
                    {
                        bookReadTime.Text = $"Read Time: {_selectedEbook.BookReadTime.Split(":")[0]}h {_selectedEbook.BookReadTime.Split(":")[1]}m {_selectedEbook.BookReadTime.Split(":")[2]}s";

                    }
                    else {
                        bookReadTime.Text = $"Read Time: 0h 0m 0s";
                    }

                    detailsPanel.Visibility = Visibility.Visible;

                    if (actualWidth - detailsPanel.Width > 0 && detailsPanel.Visibility == Visibility.Visible)
                    {
                        AllBooksView.Width = actualWidth - detailsPanel.Width;
                    }

                    try
                    {
                        languageComboBox.SelectedIndex = languageDict.Keys.ToList().IndexOf(_selectedEbook.Language);
                    }
                    catch
                    {

                    }
                    Debug.WriteLine($"AllBooksView_ItemClick() - Success");
                }

                else
                {
                    detailsPanel.Visibility = Visibility.Collapsed;
                    AllBooksView.Width = actualWidth + detailsPanel.Width;

                    Debug.WriteLine($"AllBooksView_ItemClick() - Success - Details panel collapsed");

                }
            }

            catch (Exception e)
            {
                Debug.WriteLine($"AllBooksView_ItemClick() - Fail - {e.Message}");
            }



        }

        public void SelectViewer((string ebookPlayOrder, string ebookFolderPath) navTuple)
        {
            globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));

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
            try
            {
                foreach (var method in code.AllBooks.SortingMethods)
                {
                    SortComboBox.Items.Add(method);
                }

                SortComboBox.SelectedIndex = code.AllBooks.SortingMethods.IndexOf("Name");

                foreach (var language in languageDict.Keys.ToList())
                {
                    languageComboBox.Items.Add(language);
                }

                Debug.WriteLine("comboBoxesSetup() - Success");
            }
           
            catch (Exception e)
            {
                Debug.WriteLine("comboBoxesSetup() - Fail - " + e.Message);
            }

        }

        private void SortComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                method = (string)SortComboBox.SelectedValue;
                PopulateEbooks(method, true, false);
                Debug.WriteLine("SortComboBox_OnSelectionChanged() - Success");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SortComboBox_OnSelectionChanged() - Fail - " + ex.Message);
            }
        }

        private void languageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!_state)
            {
                try
                {
                    string selectedLang = languageDict.Keys.ToList()[languageComboBox.SelectedIndex];
                    _selectedEbook.Language = selectedLang;
                    File.WriteAllText(FileManagement.GetEbookDataJsonFile(_selectedEbook.EbookFolderPath), JsonSerializer.Serialize(_selectedEbook));
                    Debug.WriteLine("languageComboBoxSelectionChanged() - Success");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("languageComboBoxSelectionChanged() - Fail - " + ex.Message);
                }
            }

            else
            {
                _state = false;
            }

        }
        private void AllBooksView_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Debug.WriteLine(AllBooksView.CurrentItemIndex.ToString());

        }

        private async Task PrepareForDelete()
        {
            string ebookFolderPath = _selectedEbook.EbookFolderPath;
                Debug.WriteLine($"Selected Ebook folder path = {_selectedEbook.EbookFolderPath}");


                try
                {
                    // remove dir
                    Directory.Delete(ebookFolderPath, true);
                    Debug.WriteLine("DeleteButton_OnClick() 1.1 - Success - Directory deleted");
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteButton_OnClick() 1.1 - Fail - " + ex.Message);
                }

                try
                {
                    string allBooksJsonPath = FileManagement.GetEbookAllBooksJsonFile();
                    List<string> Books = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(allBooksJsonPath));
                    Books.Remove(FileManagement.GetEbookDataJsonFile(_selectedEbook.EbookFolderPath));
                    File.WriteAllText(allBooksJsonPath, JsonSerializer.Serialize(Books));
                    Debug.WriteLine("DeleteButton_OnClick() 1.2 - Success - Book removed from allBooks.json");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteButton_OnClick() 1.2 - Fail - " + ex.Message);
                }
        }

        private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TO-DO
            try
            {
                string ebookFolderPath = _selectedEbook.EbookFolderPath;
                Debug.WriteLine($"Selected Ebook folder path = {_selectedEbook.EbookFolderPath}");


                try
                {
                    // remove dir
                    Directory.Delete(ebookFolderPath, true);
                    Debug.WriteLine("DeleteButton_OnClick() 1.1 - Success - Directory deleted");
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteButton_OnClick() 1.1 - Fail - " + ex.Message);
                }

                try
                {
                    string allBooksJsonPath = FileManagement.GetEbookAllBooksJsonFile();
                    List<string> Books = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(allBooksJsonPath));
                    Books.Remove(FileManagement.GetEbookDataJsonFile(_selectedEbook.EbookFolderPath));
                    File.WriteAllText(allBooksJsonPath, JsonSerializer.Serialize(Books));
                    Debug.WriteLine("DeleteButton_OnClick() 1.2 - Success - Book removed from allBooks.json");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteButton_OnClick() 1.2 - Fail - " + ex.Message);
                }

                try
                {
                    PopulateEbooks("Name", true, false);
                    Debug.WriteLine("DeleteButton_OnClick() 1.3 - Success - Ebooks collection updated");
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteButton_OnClick() 1.3 - Fail - " + ex.Message);
                }

                try
                {
                    // BUG = after it is collapsed, page is navigated to HomePage
                    //detailsPanel.Visibility = Visibility.Collapsed;
                    AllBooksView.Width = actualWidth + detailsPanel.Width;
                    Debug.WriteLine("DeleteButton_OnClick() 1.4 - Success - Details panel collapsed");
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteButton_OnClick() 1.4 - Fail - " + ex.Message);
                }

                _state = true;

                Debug.WriteLine("DeleteButton_OnClick() - Success");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeleteButton_OnClick() - Fail - " + ex.Message);
            }
        }


        private void ReadButton_OnClick(object sender, RoutedEventArgs e)
        {
            SelectViewer(naValueTuple);
        }
    }



}

