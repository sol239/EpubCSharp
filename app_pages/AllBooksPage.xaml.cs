using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using EpubCSharp.code;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubCSharp.app_pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AllBooksPage : Page
    {
        private (string ebookPlayOrder, string ebookFolderPath) _navValueTuple;

        private double _actualWidth;
        private double _actualHeight;

        private Ebook _selectedEbook;
        private bool _state;
        private string _method;

        private readonly double _bottomOffset = 55;

        private Dictionary<string, string> _languageDict;

        /// <summary>
        /// Collection of Ebooks viewable in the AllBooks page.
        /// </summary>
        public ObservableCollection<Ebook> Ebooks
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllBooks"/> class and sets up its components.
        /// </summary>
        public AllBooksPage()
        {
            this.InitializeComponent();
            LoadLangDict();

            MyMainWindow.WindowResized += OnSizeChanged;
            this.Unloaded += OnAllBooksUnloaded;

            Ebooks = new ObservableCollection<Ebook>();
            PopulateEbooks("Name", true, false);
            ComboBoxesSetup();

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
        /// Handles the event when the AllBooks control is unloaded.
        /// </summary>
        private void OnAllBooksUnloaded(object sender, RoutedEventArgs e)
        {
            MyMainWindow.WindowResized -= OnSizeChanged; // Unsubscribe from the event
        }

        /// <summary>
        /// Handles the event when the size of the main window changes.
        /// </summary>
        /// <param name="sender">
        /// The source of the event
        /// </param>
        /// <param name="tp">
        /// A tuple containing the new width and height of the window.
        /// </param>
        private void OnSizeChanged(object sender, (double width, double height) tp)
        {
            _actualWidth = tp.width;
            _actualHeight = tp.height;

            if (_actualHeight - _bottomOffset > 0)
            {
                AllBooksView.Height = _actualHeight - _bottomOffset;
                InfoScrollViewer.Height = _actualHeight - _bottomOffset;
            }

            if (_actualWidth - detailsPanel.Width > 0 && detailsPanel.Visibility == Visibility.Visible)
            {
                AllBooksView.Width = _actualWidth - detailsPanel.Width;
            }

            else
            {
                AllBooksView.Width = _actualWidth;
            }

        }

        /// <summary>
        /// Populates the Ebooks collection based on the specified sorting method and order, and optionally prints and debugs the process.
        /// </summary>
        /// <param name="method">
        /// The method used for sorting or filtering the ebooks. Could represent criteria such as "Name", "Date", etc.
        /// </param>
        /// <param name="ascendingOrder">
        /// A boolean indicating whether the sorting should be in ascending order. True for ascending, false for descending.
        /// </param>
        /// <param name="print">
        /// A boolean indicating whether to print information about the ebooks. This might be used for debugging or logging purposes.
        /// </param>
        /// <param name="debug">
        /// A boolean indicating whether to output debug information. If true, debug messages will be logged.
        /// </param>
        private void PopulateEbooks(string method, bool ascendingOrder, bool print, bool debug = false)
        {
            try
            {
                Ebooks.Clear();
                List<Ebook> ebookPaths = new List<Ebook>() { };
                ebookPaths = RecentEbooksHandler.GetRecentEbooksPathsUpdated(method, ascendingOrder, print);

                foreach (var ebook in ebookPaths)
                {

                    Ebooks.Add(ebook);
                    languageComboBox.SelectedIndex = _languageDict.Keys.ToList().IndexOf(ebook.Language);
                }
                if (debug) { Debug.WriteLine("PopulateEbooks() - Success"); }
            }
            catch (Exception e)
            {
                if (debug) { Debug.WriteLine("PopulateEbooks() - Fail - " + e.Message); }
            }
        }

        /// <summary>
        /// Loads a dictionary of Language codes and their corresponding names from a JSON file and deserializes it into a dictionary.
        /// </summary>
        /// <param name="debug">
        /// A boolean indicating whether to output debug information. If true, debug messages will be logged.
        /// </param>
        private void LoadLangDict(bool debug = false)
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Assets\\iso639I_reduced.json");
                string json = File.ReadAllText(path);
                _languageDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (debug) { Debug.WriteLine("LoadLangDict() - Success"); }
            }
            catch (Exception e)
            {
                if (debug) { Debug.WriteLine("LoadLangDict() - Fail - " + e.Message); }
            }
        }

        /// <summary>
        /// Handles the click event on an item in the AllBooksView control, updating the details panel with information about the selected book.
        /// </summary>
        /// <param name="sender">
        /// The ItemsView control that triggered the event.
        /// </param>
        /// <param name="args">
        /// The event arguments containing the invoked item information.
        /// </param>
        private void AllBooksView_ItemClick(ItemsView sender, ItemsViewItemInvokedEventArgs args)
        {
            bool debug = false;

            try
            {
                Ebook selectedEbook = args.InvokedItem as Ebook;

                if (_selectedEbook == null)
                {
                    _selectedEbook = selectedEbook;
                }

                if ((selectedEbook.EbookFolderPath != _selectedEbook.EbookFolderPath) || detailsPanel.Visibility == Visibility.Collapsed)
                {
                    _selectedEbook = selectedEbook;
                    _navValueTuple = (_selectedEbook.InBookPosition, _selectedEbook.EbookFolderPath);

                    // Populate the detailsPanel with the selected book's information
                    bookTitle.Text = _selectedEbook.Title;
                    bookAuthor.Text = $"Author: {_selectedEbook.Author}";
                    // hh:mm:ss "00:13:44"

                    if (_selectedEbook.BookReadTime != null)
                    {
                        bookReadTime.Text = $"Read Time: {_selectedEbook.BookReadTime.Split(":")[0]}h {_selectedEbook.BookReadTime.Split(":")[1]}m {_selectedEbook.BookReadTime.Split(":")[2]}s";

                    }
                    else
                    {
                        bookReadTime.Text = $"Read Time: 0h 0m 0s";
                    }

                    detailsPanel.Visibility = Visibility.Visible;

                    if (_actualWidth - detailsPanel.Width > 0 && detailsPanel.Visibility == Visibility.Visible)
                    {
                        AllBooksView.Width = _actualWidth - detailsPanel.Width;
                    }

                    try
                    {
                        languageComboBox.SelectedIndex = _languageDict.Keys.ToList().IndexOf(_selectedEbook.Language);
                    }
                    catch
                    {
                        languageComboBox.SelectedIndex = 1;
                    }

                    if (debug) { Debug.WriteLine("AllBooksView_ItemClick() - Success - Details panel visible"); }
                }

                else
                {
                    detailsPanel.Visibility = Visibility.Collapsed;
                    AllBooksView.Width = _actualWidth + detailsPanel.Width;

                    if (debug) { Debug.WriteLine("AllBooksView_ItemClick() - Success - Details panel collapsed"); }

                }
            }

            catch (Exception e)
            {
                if (debug) { Debug.WriteLine("AllBooksView_ItemClick() - Fail - " + e.Message); }
            }

        }

        /// <summary>
        /// When the second window is closed, the Ebooks collection is cleared and repopulated to reflect any changes made to the ebooks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SecondWindow_WindowClosed(object sender, EventArgs e)
        {
            // clear the Ebooks collection
            Ebooks.Clear();
            PopulateEbooks(_method, true, false);

        }

        /// <summary>
        /// Initializes and populates the combo boxes for sorting methods and languages.
        /// </summary>
        /// <param name="debug">
        /// A boolean flag indicating whether to output debug information. If true, success or failure messages are logged.
        /// </param>
        private void ComboBoxesSetup(bool debug = false)
        {
            try
            {
                foreach (var method in code.AllBooks.SortingMethods)
                {
                    SortComboBox.Items.Add(method);
                }

                SortComboBox.SelectedIndex = code.AllBooks.SortingMethods.IndexOf("Name");

                foreach (var language in _languageDict.Keys.ToList())
                {
                    languageComboBox.Items.Add(language);
                }

                if (debug)
                {
                    Debug.WriteLine("ComboBoxesSetup() - Success");
                }
            }
            catch (Exception e)
            {
                if (debug)
                {
                    Debug.WriteLine("ComboBoxesSetup() - Fail - " + e.Message);
                }
            }

        }

        /// <summary>
        /// Handles the event when the selection in the SortComboBox changes. 
        /// Updates the list of ebooks based on the selected sorting method.
        /// </summary>
        /// <param name="sender">
        /// The source of the event, which is the SortComboBox control.
        /// </param>
        /// <param name="e">
        /// Provides data for the event, including the old and new selected items.
        /// </param>

        private void SortComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool debug = false;
            try
            {
                _method = (string)SortComboBox.SelectedValue;
                PopulateEbooks(_method, true, false);
                if (debug) { Debug.WriteLine("SortComboBox_OnSelectionChanged() - Success"); }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine("SortComboBox_OnSelectionChanged() - Fail - " + ex.Message); }
            }
        }

        /// <summary>
        /// Handles the event when the selection in the languageComboBox changes. 
        /// Updates the Language of the selected ebook and saves the updated information to a JSON file.
        /// </summary>
        /// <param name="sender">
        /// The source of the event, which is the languageComboBox control.
        /// </param>
        /// <param name="e">
        /// Provides data for the event, including the old and new selected items.
        /// </param>
        private void LanguageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool debug = false;

            if (!_state)
            {
                try
                {
                    string selectedLang = _languageDict.Keys.ToList()[languageComboBox.SelectedIndex];
                    _selectedEbook.Language = selectedLang;
                    File.WriteAllText(FileManagement.GetEbookDataJsonFile(_selectedEbook.EbookFolderPath), JsonSerializer.Serialize(_selectedEbook));
                    if (debug) { Debug.WriteLine("LanguageComboBoxSelectionChanged() - Success"); }
                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine("LanguageComboBoxSelectionChanged() - Fail - " + ex.Message); }
                }
            }

            else
            {
                _state = false;
            }
        }

        /// <summary>
        /// Handles the click event of the delete button. The method performs the following actions:
        /// <list type="number">
        /// <item>Deletes the directory of the selected eBook and its contents using the <see cref="Directory.Delete"/> method.</item>
        /// <item>Updates the JSON file that tracks all books by removing the entry for the deleted eBook using methods from <see cref="FileManagement"/> and <see cref="JsonSerializer"/>.</item>
        /// <item>Repopulates the eBooks display list to reflect the removal of the eBook using the <see cref="PopulateEbooks"/> method.</item>
        /// <item>Adjusts the width of the <see cref="AllBooksView"/> UI element to account for layout changes after the deletion.</item>
        /// </list>
        /// The method includes debug logging to trace execution and capture errors. If debug logging is enabled, detailed messages about the success or failure of each action are output.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that provide data about the event.</param>
        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            bool debug = false;
            try
            {
                string ebookFolderPath = _selectedEbook.EbookFolderPath;

                try
                {
                    // remove dir
                    Directory.Delete(ebookFolderPath, true);
                    if (debug) { Debug.WriteLine("DeleteButton_OnClick() 1.0 - Success - Directory removed"); }
                }

                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine("DeleteButton_OnClick() 1.1 - Fail - " + ex.Message); }
                }

                try
                {
                    string allBooksJsonPath = FileManagement.GetEbookAllBooksJsonFile();
                    List<string> books = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(allBooksJsonPath));
                    books.Remove(FileManagement.GetEbookDataJsonFile(_selectedEbook.EbookFolderPath));
                    File.WriteAllText(allBooksJsonPath, JsonSerializer.Serialize(books));
                    if (debug) { Debug.WriteLine("DeleteButton_OnClick() 1.2 - Success - AllBooks.json updated"); }
                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine("DeleteButton_OnClick() 1.2 - Fail - " + ex.Message); }
                }

                try
                {
                    PopulateEbooks("Name", true, false);
                    if (debug) { Debug.WriteLine("DeleteButton_OnClick() 1.3 - Success - Ebooks repopulated"); }
                }

                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine("DeleteButton_OnClick() 1.3 - Fail - " + ex.Message); }
                }

                try
                {
                    // BUG = after it is collapsed, page is navigated to HomePage
                    //detailsPanel.Visibility = Visibility.Collapsed;
                    AllBooksView.Width = _actualWidth + detailsPanel.Width;
                    if (debug)
                    {
                        Debug.WriteLine("DeleteButton_OnClick() 1.4 - Success - Details panel collapsed");
                    }
                }

                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine("DeleteButton_OnClick() 1.4 - Fail - " + ex.Message); }
                }

                _state = true;

                if (debug) { Debug.WriteLine("DeleteButton_OnClick() - Success"); }
            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine("DeleteButton_OnClick() - Fail - " + ex.Message); }
            }
        }

        /// <summary>
        /// Opens EbookWindow after the Read button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadButton_OnClick(object sender, RoutedEventArgs e)
        {
            SelectViewer(_navValueTuple);
        }
    }

}
