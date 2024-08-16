using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace EpubCSharp.code;

/// <summary>
/// Represents an eBook with various metadata and file paths.
/// This class encapsulates information about an eBook including its title, author, Language, and various paths for managing the eBook's data.
/// </summary>
public class Ebook
{
    /// <summary>
    /// Gets or sets the title of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the eBook's title.
    /// </value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the author of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the name of the eBook's author.
    /// </value>
    public string Author { get; set; }

    /// <summary>
    /// Gets or sets the Language of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the Language in which the eBook is written.
    /// </value>
    public string Language { get; set; }

    /// <summary>
    /// Gets or sets the publisher of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the publisher of the eBook.
    /// </value>
    public string Publisher { get; set; }

    /// <summary>
    /// Gets or sets the description of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> providing a summary or description of the eBook's content.
    /// </value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the date when the eBook was added to the collection.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the date the eBook was added, formatted as a date string.
    /// </value>
    public string DateAdded { get; set; }

    /// <summary>
    /// Gets or sets the date when the eBook was last opened.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the date the eBook was last accessed, formatted as a date string.
    /// </value>
    public string DateLastOpened { get; set; }

    /// <summary>
    /// Gets or sets the format of the eBook (e.g., EPUB, PDF).
    /// </summary>
    /// <value>
    /// A <see cref="string"/> indicating the file format of the eBook.
    /// </value>
    public string Format { get; set; }

    /// <summary>
    /// Gets or sets the file name of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the name of the eBook file.
    /// </value>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the position of the eBook within a collection or library.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> indicating the eBook's position or identifier within a specific context.
    /// </value>
    public string InBookPosition { get; set; }

    /// <summary>
    /// Gets or sets the scroll value indicating the last read position in the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the last scroll position or bookmark in the eBook.
    /// </value>
    public string ScrollValue { get; set; }

    /// <summary>
    /// Gets or sets the time when the eBook was last opened.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the timestamp of when the eBook was last accessed.
    /// </value>
    public string BookOpenTime { get; set; }

    /// <summary>
    /// Gets or sets the time when the eBook was closed.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the timestamp of when the eBook was last closed.
    /// </value>
    public string BookCloseTime { get; set; }

    /// <summary>
    /// Gets or sets the total time spent reading the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the total reading time accumulated for the eBook.
    /// </value>
    public string BookReadTime { get; set; }

    /// <summary>
    /// Gets or sets the current status of the eBook.
    /// Possible values include "Finished", "Not Started", and "Reading".
    /// </summary>
    /// <value>
    /// A <see cref="string"/> indicating the reading status of the eBook.
    /// </value>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets a dictionary representing additional statistical records for the eBook.
    /// </summary>
    /// <value>
    /// A dictionary where the key represents a statistic type and the value contains the statistic details.
    /// </value>
    public Dictionary<string, string> StatsRecord1 { get; set; }

    /// <summary>
    /// Gets or sets a secondary statistical record for the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing additional statistical information related to the eBook.
    /// </value>
    public string StatsRecord2 { get; set; }

    /// <summary>
    /// Gets or sets a dictionary containing navigation data for the eBook.
    /// </summary>
    /// <value>
    /// A dictionary where the key represents a navigation point and the value contains a list of associated data.
    /// </value>
    public Dictionary<string, List<string>> NavData { get; set; }

    /// <summary>
    /// Gets or sets the path to the folder containing the eBook files.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the folder path where the eBook is stored.
    /// </value>
    public string EbookFolderPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the folder containing eBook data files.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the path to the folder where eBook data is stored.
    /// </value>
    public string EbookDataFolderPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the container file of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the path to the eBook's container file.
    /// </value>
    public string ContainerPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the main content file of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the path to the eBook's main content file.
    /// </value>
    public string ContentPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the cover image of the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the path to the eBook's cover image file.
    /// </value>
    public string CoverPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the JSON file containing eBook data.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the path to the eBook's JSON data file.
    /// </value>
    public string JsonDataPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the navigation file for the eBook.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the path to the eBook's navigation file.
    /// </value>
    public string NavigationFilePath { get; set; }
}


/// <summary>
/// Class for handling all books view
/// </summary>
public class AllBooks
{

    /// <summary>
    /// List of all books paths
    /// </summary>
    public List<string> Books { get; set; }

    /// <summary>
    /// Sort methods for sorting books
    /// </summary>
    public static List<string> SortingMethods = new List<string> { "Name", "DateAdded", "DateLastOpened", "Author", "Publisher", "Language" };

    /// <summary>
    /// Loads allBooks.json into Books List
    /// </summary>
    public void LoadAllBooksFromJson(bool debug = false)
    {
        string path = FileManagement.GetEbookAllBooksJsonFile();

        try
        {
            Books = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path));
            if (debug) {Debug.WriteLine($"LoadAllBooksFromJson() - Success"); }
        }
        catch (Exception ex)
        {
            // if file does not exist, create a new one
            List<string> emptyList = new List<string>();
            File.WriteAllText(path, JsonSerializer.Serialize(emptyList));
            Books = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path));
            if (debug) { Debug.WriteLine($"LoadAllBooksFromJson() - Fail - {ex.Message}"); }
        }
    }
    
    /// <summary>
    /// Add an ebook's jsonDataPath to Books List
    /// </summary>
    /// <param name="ebookDataJsonPath"></param>
    public void AddBook(string ebookDataJsonPath)
    {
        Books.Add(ebookDataJsonPath);
    }

    /// <summary>
    /// Stores a ebook's jsonDataPath to allBooks JSON file
    /// </summary>
    /// <param name="ebookDataJsonPath"></param>
    public void AddBookStore(string ebookDataJsonPath)
    {
        LoadAllBooksFromJson();
        Books.Add(ebookDataJsonPath);
        StoreBooksToJson();
    }

    /// <summary>
    /// Stores allBooks List to a allBooks.json file
    /// </summary>
    public void StoreBooksToJson()
    {
        string path = FileManagement.GetEbookAllBooksJsonFile();
        File.WriteAllText(path, JsonSerializer.Serialize(Books));
    }

    /// <summary>
    /// Removes an ebook's jsonDataPath from Books List
    /// </summary>
    /// <param name="ebookDataJsonPath"></param>
    public void RemoveBook(string ebookDataJsonPath)
    {
        Books.Remove(ebookDataJsonPath);
    }

    /// <summary>
    /// Removes an ebook's jsonDataPath from allBooks JSON file
    /// </summary>
    /// <param name="ebookDataJsonPath"></param>
    public void RemoveBookStore(string ebookDataJsonPath)
    {
        LoadAllBooksFromJson();
        Books.Remove(ebookDataJsonPath);
        StoreBooksToJson();
    }

    /// <summary>
    /// Prints titles of all books on next line
    /// </summary>
    public void PrintAllBooks()
    {
        LoadAllBooksFromJson();

        foreach (string ebookDataJsonPath in Books)
        {
            var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            Debug.WriteLine($"Title: {book.Title}");
        }
        Debug.WriteLine("");
    }

    /// <summary>
    /// Returns a list of ebooks ebub folder sorted by Name alphabetically
    /// </summary>
    /// <param name="ascendingOrder"></param>
    /// <param name="print"></param>
    /// <returns></returns>
    public List<string> GetBooksEpubFoldersByName(bool ascendingOrder = false, bool print = false)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        try
        {
            foreach (string ebookDataJsonPath in Books)
            {
                var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                books.Add(book.JsonDataPath, book.Title);
            }

            if (ascendingOrder)
            {
                books = books.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                books = books.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            Books = books.Keys.ToList();

            // print the ebooks
            if (print)
            {
                foreach (string ebookDataJsonPath in Books)
                {
                    var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                    Debug.WriteLine($"Title: {book.Title}");
                }
            }

            Debug.WriteLine("GetBooksEpubFoldersByName() - Success");
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"GetBooksEpubFoldersByName() - Fail - {ex.Message}");
        }
        return Books;
    }

    /// <summary>
    /// Returns a list of books ebub folder sorted by DateAdded
    /// </summary>
    /// <param name="ascendingOrder"></param>
    /// <param name="print"></param>
    /// <returns></returns>
    public List<string> GetBooksEpubFoldersByDateAdded(bool ascendingOrder, bool print)
    {
        ascendingOrder = !ascendingOrder;
        LoadAllBooksFromJson();
        Dictionary<string, DateTime> books = new Dictionary<string, DateTime>();

        try
        {
            foreach (string ebookDataJsonPath in Books)
            {
                var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);


                DateTime dateLastOpened = DateTime.ParseExact(book.DateAdded, "dd/MM/yyyy HH:mm:ss",FileManagement.Culture);
                books.Add(book.JsonDataPath, dateLastOpened);
            }

            if (ascendingOrder)
            {
                books = books.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                books = books.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            Books = books.Keys.ToList();

            // print he books
            if (print)
            {
                foreach (string ebookDataJsonPath in Books)
                {
                    var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                    Debug.WriteLine($"Title: {book.Title}");
                }
            }
            Debug.WriteLine("GetBooksEpubFoldersByDateAdded() - Success");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetBooksEpubFoldersByDateAdded() - Fail - {ex.Message}");
        }

        return Books;
    }

    /// <summary>
    /// Returns a list of books ebub folder sorted by DateLastOpened
    /// </summary>
    /// <param name="ascendingOrder"></param>
    /// <param name="print"></param>
    /// <returns></returns>
    public List<string> GetBooksEpubFoldersByDateLastOpened(bool ascendingOrder, bool print)
    {
        ascendingOrder = !ascendingOrder;

        LoadAllBooksFromJson();
        Dictionary<string, DateTime> books = new Dictionary<string, DateTime>();


        try
        {
            foreach (string ebookDataJsonPath in Books)
            {
                var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);


                DateTime dateLastOpened = DateTime.ParseExact(book.DateLastOpened, "dd/MM/yyyy HH:mm:ss", FileManagement.Culture);
                books.Add(book.JsonDataPath, dateLastOpened);
            }


            if (ascendingOrder)
            {
                books = books.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                books = books.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            Books = books.Keys.ToList();

            // print he books
            if (print)
            {
                foreach (string ebookDataJsonPath in Books)
                {
                    var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                    Debug.WriteLine($"Title: {book.Title}");
                }
            }

            Debug.WriteLine("GetBooksEpubFoldersByDateLastOpened() - Success");
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"GetBooksEpubFoldersByDateLastOpened() - Fail - {ex.Message}");
        }

        return Books;
    }
    
    /// <summary>
    /// Returns a list of books ebub folder sorted by Lenght
    /// </summary>
    /// <param name="ascendingOrder"></param>
    /// <param name="print"></param>
    /// <returns></returns>
    public static List<string> GetBooksEpubFoldersByLenght(bool ascendingOrder, bool print)
    { 
        // TO-DO
        return null;
    }

    /// <summary>
    /// Returns a list of books ebub folder sorted by Author
    /// </summary>
    /// <param name="ascendingOrder"></param>
    /// <param name="print"></param>
    /// <returns></returns>
    public List<string> GetBooksEpubFoldersByAuthor(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        foreach (string ebookDataJsonPath in Books)
        {
            var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            books.Add(book.JsonDataPath, book.Author);
        }

        if (ascendingOrder)
        {
            books = books.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }
        else
        {
            books = books.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        Books = books.Keys.ToList();

        // print he books
        if (print)
        {
            foreach (string ebookDataJsonPath in Books)
            {
                var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                Debug.WriteLine($"Title: {book.Title}");
            }
        }

        return Books;
    }

    /// <summary>
    /// Returns a list of books ebub folder sorted by Publisher
    /// </summary>
    /// <param name="ascendingOrder"></param>
    /// <param name="print"></param>
    /// <returns></returns>
    public List<string> GetBooksEpubFoldersByPublisher(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        foreach (string ebookDataJsonPath in Books)
        {
            var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            books.Add(book.JsonDataPath, book.Publisher);
        }

        if (ascendingOrder)
        {
            books = books.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }
        else
        {
            books = books.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        Books = books.Keys.ToList();

        // print he books
        if (print)
        {
            foreach (string ebookDataJsonPath in Books)
            {
                var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                Debug.WriteLine($"Title: {book.Title}");
            }
        }

        return Books;
    }

    /// <summary>
    /// Retrieves a list of EPUB book folders sorted by Language.
    /// </summary>
    /// <param name="ascendingOrder">Determines the sort order of the books by Language. 
    /// Set to <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <param name="print">If set to <c>true</c>, prints the details of each book to the debug output.</param>
    /// <returns>A list of strings representing the paths to the EPUB book folders, sorted by Language.</returns>
    /// <remarks>
    /// This method first loads all the book data from a JSON file, then sorts the books 
    /// based on their Language, and optionally prints their details. The sort order 
    /// can be either ascending or descending depending on the <paramref name="ascendingOrder"/> parameter.
    /// </remarks>
    public List<string> GetBooksEpubFoldersByLanguage(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        foreach (string ebookDataJsonPath in Books)
        {
            var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            books.Add(book.JsonDataPath, book.Language);
        }

        if (ascendingOrder)
        {
            books = books.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }
        else
        {
            books = books.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        Books = books.Keys.ToList();

        // print he books
        if (print)
        {
            foreach (string ebookDataJsonPath in Books)
            {
                var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                Debug.WriteLine($"Title: {book.Title}");
            }
        }

        return Books;
    }

    /// <summary>
    /// Selects and applies a sorting method to the list of EPUB book folders.
    /// </summary>
    /// <param name="method">The sorting method to apply. Valid options are: "Name", "DateAdded", "DateLastOpened", "Author", "Publisher", "Language".</param>
    /// <param name="ascendingOrder">Determines the sort order. Set to <c>true</c> for ascending order, or <c>false</c> for descending order.</param>
    /// <param name="print">If set to <c>true</c>, prints the details of each book to the debug output.</param>
    /// <returns>A list of strings representing the paths to the EPUB book folders, sorted according to the specified method.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid sorting method is provided.</exception>
    public List<string> SelectSortMethod(string method, bool ascendingOrder, bool print)
    {
        switch (method)
        {
            case "Name":
                return GetBooksEpubFoldersByName(ascendingOrder, print);

            case "DateAdded":
                return GetBooksEpubFoldersByDateAdded(ascendingOrder, print);

            case "DateLastOpened":
                return GetBooksEpubFoldersByDateLastOpened(ascendingOrder, print);

            case "Author":
                return GetBooksEpubFoldersByAuthor(ascendingOrder, print);

            case "Publisher":
                return GetBooksEpubFoldersByPublisher(ascendingOrder, print);

            case "Language":
                return GetBooksEpubFoldersByLanguage(ascendingOrder, print);

            default:
                throw new ArgumentException($"Invalid sort method: {method}. Valid options are: Name, DateAdded, DateLastOpened, Author, Publisher, Language.");
        }
    }

}


/// <summary>
/// Class for handling json files
/// </summary>
public class JsonHandler
{
    /// <summary>
    /// Reads and deserializes an eBook JSON file into an <see cref="Ebook"/> object.
    /// </summary>
    /// <param name="jsonPath">The file path of the JSON file to read.</param>
    /// <param name="debug">If <c>true</c>, writes debug information to the output.</param>
    /// <returns>The deserialized <see cref="Ebook"/> object, or a new <see cref="Ebook"/> if the operation fails.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified JSON file is not found.</exception>
    /// <exception cref="JsonException">Thrown when deserialization of the JSON content fails.</exception>
    public static Ebook ReadEbookJsonFile(string jsonPath, bool debug = false)
    {
        try
        {
            string jsonString = File.ReadAllText(jsonPath);
            if (debug) { Debug.WriteLine("ReadEbookJsonFile() - Success"); }
            return JsonSerializer.Deserialize<Ebook>(jsonString);
        }
        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"ReadEbookJsonFile() - Fail - {ex.Message}");}
        }

        return new Ebook();
    }

    /// <summary>
    /// Serializes an <see cref="Ebook"/> object and stores it as a JSON file.
    /// </summary>
    /// <param name="ebook">The <see cref="Ebook"/> object to serialize and store.</param>
    /// <param name="jsonPath">The directory path where the JSON file will be saved.</param>
    /// <param name="debug">If <c>true</c>, writes debug information to the output.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks necessary permissions to write to the specified directory.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during file writing.</exception>
    public static void StoreJsonEbookFile(Ebook ebook,string jsonPath, bool debug = false)
    {
        try
        {
            ebook.JsonDataPath = jsonPath + "\\" + FileManagement.EbookDataFileName;
            string jsonString = JsonSerializer.Serialize(ebook);
            File.WriteAllText(jsonPath + "\\" + FileManagement.EbookDataFileName, jsonString);
            if (debug) { Debug.WriteLine("StoreJsonEbookFile() - Success"); }
        }

        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"StoreJsonEbookFile() - Fail - {ex.Message}"); }
        }
    }
}

/// <summary>
/// Class for handling content of an epub file
/// </summary>
public class ContentHandler
{

    /// <summary>
    /// Extracts the inner XML content of all elements with the specified tag from an XML file.
    /// </summary>
    /// <param name="contentPath">The file path to the XML content file to be parsed.</param>
    /// <param name="xmlTag">The XML tag whose inner content is to be extracted and concatenated.</param>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console.</param>
    /// <returns>
    /// A concatenated string of the inner XML content of all elements with the specified tag.
    /// Returns <c>null</c> if an error occurs during the operation.
    /// </returns>
    /// <exception cref="XmlException">Thrown if the XML content is not well-formed.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the specified XML file does not exist.</exception>
    /// <remarks>
    /// This method attempts to load an XML document from the provided file path, searches for all elements 
    /// matching the specified tag, and concatenates their inner XML content into a single string. 
    /// If the process succeeds, the concatenated string is returned. If any errors occur, the method 
    /// returns <c>null</c> and logs the error if debugging is enabled.
    /// </remarks>
    public string AddMetaData(string contentPath, string xmlTag, bool debug = false )
    {
        try
        {
            string xmlSegment = "";

            XmlDocument doc = new XmlDocument();
            doc.Load(contentPath);

            XmlNodeList elemList = doc.GetElementsByTagName(xmlTag);


            for (int i = 0; i < elemList.Count; i++)
            {
                try
                {
                    xmlSegment += elemList[i].InnerXml;
                }

                catch 
                {
                    continue;
                }
            }

if (debug) { Debug.WriteLine($"AddMetaData() - Success - {xmlTag} = {xmlSegment}"); }
            return xmlSegment;
        }

        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"AddMetaData() - Fail - {ex.Message}"); }
            return null;
        }
    }

    /// <summary>
    /// Retrieves the full file path of the cover image from an EPUB's content file (OPF).
    /// </summary>
    /// <param name="contentOpfPath">The file path to the OPF (Open Packaging Format) file.</param>
    /// <param name="extractedEpubDir">The directory where the EPUB contents are extracted.</param>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console.</param>
    /// <returns>
    /// The full file path to the cover image. If the cover image is not found, returns a path combining 
    /// the <paramref name="extractedEpubDir"/> and the cover ID.
    /// </returns>
    /// <exception cref="XmlException">Thrown if the OPF file content is not well-formed.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the specified OPF file does not exist.</exception>
    /// <remarks>
    /// This method loads the OPF file, searches for the meta tag with the name attribute set to "cover," 
    /// and retrieves the corresponding cover image's file path. If the cover image is found, 
    /// its full path is returned. If any errors occur, the method returns a default path 
    /// using the cover ID and logs the error if debugging is enabled.
    /// </remarks>
    public string GetCoverImagePath(string contentOpfPath, string extractedEpubDir, bool debug = false)
    {

        string coverId = "";

        try
        {
            XDocument contentOpf = XDocument.Load(contentOpfPath);
            XNamespace ns = "http://www.idpf.org/2007/opf";

            // Find the meta tag with name="cover"
            var coverMeta = contentOpf.Descendants(ns + "meta")
                .FirstOrDefault(meta => (string)meta.Attribute("name") == "cover");

            if (coverMeta != null)
            {
                coverId = (string)coverMeta.Attribute("content");

                // Find the item tag with the corresponding id
                var coverItem = contentOpf.Descendants(ns + "item")
                    .FirstOrDefault(item => (string)item.Attribute("id") == coverId);

                if (coverItem != null)
                {
                    string coverImagePath = (string)coverItem.Attribute("href");

                    // convert / to \\
                    coverImagePath = coverImagePath.Replace("/", "\\");

                    string fullCoverImagePath = Path.Combine(extractedEpubDir, coverImagePath);


                    if (debug) { Debug.WriteLine($"GetCoverImagePath() - Success - {fullCoverImagePath}"); }
                    return fullCoverImagePath;
                }
            }

        }

        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"GetCoverImagePath() - Fail - {ex.Message}"); }
        }

        return Path.Combine(extractedEpubDir, coverId);
    }

}

/// <summary>
/// Class for handling file management
/// </summary>
public class RecentEbooksHandler
{

    /// <summary>
    /// Retrieves a list of recent eBooks that have a cover path, sorted according to the specified method.
    /// </summary>
    /// <param name="method">The sorting method to use. This could be "Name", "DateAdded", "DateLastOpened", "Author", "Publisher", "Language", etc.</param>
    /// <param name="ascendingOrder">If <c>true</c>, sorts the eBooks in ascending order; otherwise, sorts them in descending order. Defaults to <c>true</c>.</param>
    /// <param name="print">If <c>true</c>, prints the details of each eBook to the debug output. Defaults to <c>false</c>.</param>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console. Defaults to <c>false</c>.</param>
    /// <returns>
    /// A list of <see cref="Ebook"/> objects that have a cover path. If an error occurs during the process, returns <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method initializes an instance of the <see cref="AllBooks"/> class to get the list of JSON file paths for eBooks, 
    /// then reads each eBook file and filters out those that have a non-empty cover path. The filtered eBooks are returned in 
    /// a list sorted according to the specified method and order. If any errors occur, the method logs the exception message 
    /// if debugging is enabled and returns <c>null</c>.
    /// </remarks>
    public static List<Ebook> GetRecentEbooksPathsUpdated(string method, bool ascendingOrder = true, bool print = false, bool debug = false)
    {

        try
        {
            AllBooks allBooks = new AllBooks();


            List<Ebook> coverPaths = new List<Ebook>();


            foreach (var jsonFile in allBooks.SelectSortMethod(method, ascendingOrder, print))
            {
                if (File.Exists(jsonFile))
                {
                    Ebook ebook = JsonHandler.ReadEbookJsonFile(jsonFile);
                    if (!string.IsNullOrEmpty(ebook.CoverPath))
                    {
                        coverPaths.Add(ebook);
                    }
                }
                else
                {
                    Debug.WriteLine($"File not found: {jsonFile}");
                }
            }

            if (debug) { Debug.WriteLine("GetRecentEbooksPathsUpdated() - Success"); }
            return coverPaths;
        }
        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"GetRecentEbooksPathsUpdated() - Fail - {ex.Message}"); }
            return null;
        }
    }
}

/// <summary>
/// Class for handling file management
/// </summary>
public class EpubHandler
{
    private ContentHandler _contentHandler = new ContentHandler();
    private Navigation _nvg = new Navigation();

    private string _ebookFolderPath = "";
    private string _containerFileName = "container.xml";
    private string _contentFilePath = "";

    private Ebook _ebook;

    private List<string> _metadataTags = new List<string> { 
        "dc:title", 
        "dc:creator", 
        "dc:Language", 
        "dc:publisher", 
        "dc:description"
    };

    /// <summary>
    /// Event that is triggered when a book is added to the system.
    /// </summary>
    public event EventHandler<string> BookAddedEvent;

    /// <summary>
    /// Extracts an EPUB file to a specified destination directory and initializes an <see cref="Ebook"/> object with relevant paths and metadata.
    /// </summary>
    /// <param name="epubFilePath">The file path to the EPUB file to be extracted.</param>
    /// <param name="destination">The destination directory where the EPUB content will be extracted.</param>
    /// <param name="fileName">The name of the EPUB file being processed. This will be used to set the <see cref="Ebook.FileName"/> property.</param>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console. Defaults to <c>false</c>.</param>
    /// <remarks>
    /// This method opens the specified EPUB file as a ZIP archive, extracts its contents to a unique subdirectory within the 
    /// specified destination, and initializes an <see cref="Ebook"/> object with paths and metadata derived from the extracted content.
    /// A random tag is appended to the destination folder to avoid issues with file path length limitations.
    /// The method sets properties on the <see cref="Ebook"/> object such as folder paths, navigation file paths, format, file name, and timestamps for 
    /// when the book was added and last opened.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="epubFilePath"/> or <paramref name="destination"/> is invalid.</exception>
    /// <exception cref="IOException">Thrown if an I/O error occurs during file extraction or directory creation.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the application does not have permission to access the file or directory.</exception>
    public void ExtractEpub(string epubFilePath, string destination, string fileName, bool debug = false)
    {
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(epubFilePath))
            {
                _ebook = new Ebook();

                // Old: paths in windows are limited to 260 characters
                // Some cover paths couldnt be accessed because of this
                //destination = destination + "\\" + fileName;

                // New: generate random tag for the folder name
                // Generate a random tag for the folder name to handle path length limitations
                destination = Path.Combine(destination, GenerateRandomTag(5));

                // Extract the EPUB archive to the destination directory
                archive.ExtractToDirectory(destination);

                // Create a subdirectory for additional data
                Directory.CreateDirectory(Path.Combine(destination, "DATA"));

                // Initialize the Ebook object with paths and metadata
                _ebookFolderPath = destination;
                _ebook.EbookFolderPath = destination;
                _ebook.EbookDataFolderPath = Path.Combine(destination, "DATA");
                _ebook.NavigationFilePath = _nvg.FindFilesWithExtensions(_ebookFolderPath, new List<string> { ".ncx", ".nav" });
                _ebook.Format = "epub";
                _ebook.FileName = Path.GetFileNameWithoutExtension(fileName);
                _ebook.DateAdded = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", FileManagement.Culture);  // Format: 27/07/2024 08:21:56
                _ebook.DateLastOpened = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", FileManagement.Culture);  // Format: 27/07/2024 08:21:56

                if (debug) { Debug.WriteLine($"ExtractEpub() - Success - Extracted to {destination}"); }
            }
        }
        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"ExtractEpub() - Fail - {ex.Message}"); }
            throw;
        }
    }

    /*
    public void ExtractEpub(string epubFilePath, string destination, string fileName, bool debug = false)
    {
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(epubFilePath))
            {

                _ebook = new Ebook();


                
                destination = destination + "\\" + GenerateRandomTag(5);


                archive.ExtractToDirectory( destination );
                Directory.CreateDirectory( destination + "\\" + "DATA");

                _ebookFolderPath = destination;
                _ebook.EbookFolderPath = destination;
                _ebook.EbookDataFolderPath = destination + "\\" + "DATA";
                _ebook.NavigationFilePath = _nvg.FindFilesWithExtensions(_ebookFolderPath, new List<string> { ".ncx", ".nav" });
                _ebook.Format = "epub";
                _ebook.FileName = fileName.Split(".epub")[0];
                _ebook.DateAdded = DateTime.Now.ToString();   // format: 27/07/2024 08:21:56
                _ebook.DateLastOpened = DateTime.Now.ToString();   // format: 27/07/2024 08:21:56

                if (debug) { Debug.WriteLine($"ExtractEpub() - Success");}
            }
        }
        catch (Exception ex)
        {

            if (debug) { Debug.WriteLine($"ExtractEpub() - Fail - {ex.Message}"); }

            throw;
        }

    }
    */

    /// <summary>
    /// Retrieves the file path to the main content file of an EPUB by parsing the container XML file.
    /// </summary>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console. Defaults to <c>false</c>.</param>
    /// <remarks>
    /// This method loads the container XML file from the EPUB, which is located in the META-INF directory. 
    /// It then parses the XML to find and extract the path of the primary content file specified within the container XML. 
    /// The extracted content file path is used to update the <see cref="_contentFilePath"/> and <see cref="Ebook.ContentPath"/> properties.
    /// The method assumes that the container XML and content paths follow a specific format and that the necessary XML nodes and attributes are present.
    /// </remarks>
    /// <exception cref="IOException">Thrown if there is an I/O error accessing the container XML file. The exception is only caught if the message contains "already exists".</exception>
    /// <exception cref="XmlException">Thrown if there is an issue parsing the XML document.</exception>
    /// <exception cref="ArgumentException">Thrown if the XML content does not contain the expected format or required elements.</exception>
    public void GetEpubContentFilePath(bool debug = false)
    {
        try
        {
            string contentFilePath;
            string xmlFilePath = _ebookFolderPath + "\\" + "META-INF" + "\\" + _containerFileName;
            _ebook.ContainerPath = xmlFilePath;
            string xmlSegment = "";

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);
            XmlNodeList elemList = doc.GetElementsByTagName("rootfiles");

            for (int i = 0; i < elemList.Count; i++)
            {
                xmlSegment += elemList[i].InnerXml;
            }

            contentFilePath = xmlSegment.Split(" ")[1].Split("full-path=\"")[1].Split("\"")[0];
            _contentFilePath = _ebookFolderPath + "\\" + contentFilePath;
            _ebook.ContentPath = _contentFilePath;

            if (debug) { Debug.WriteLine($"GetEpubContentFilePath() - Success - {_contentFilePath}"); }

        }
        catch (IOException ex) when (ex.Message.Contains("already exists"))
        {
            if (debug) { Debug.WriteLine($"GetEpubContentFilePath() - Fail - {ex.Message}"); }
            throw; // Rethrow the exception if you want it to propagate further
        }
        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"GetEpubContentFilePath() - Fail - {ex.Message}"); }
            throw;
        }
    }

    /// <summary>
    /// Adds an EPUB file to the system by extracting its contents, processing metadata, and updating relevant records.
    /// </summary>
    /// <param name="epubFilePath">The file path of the EPUB file to be added.</param>
    /// <param name="destination">The directory where the EPUB file will be extracted.</param>
    /// <param name="fileName">The name of the EPUB file being processed. This name will be used to determine the file's entry in the system.</param>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console. Defaults to <c>false</c>.</param>
    /// <returns>
    /// <c>true</c> if the EPUB file was successfully added and processed; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    /// <item>Extracts the EPUB file to the specified destination directory using the <see cref="ExtractEpub"/> method.</item>
    /// <item>Retrieves the path to the main content file of the EPUB using the <see cref="GetEpubContentFilePath"/> method.</item>
    /// <item>Processes metadata tags from the EPUB content and updates corresponding properties of the <see cref="_ebook"/> object, including title, author, Language, publisher, and description.</item>
    /// <item>Determines and sets the cover image path for the EPUB using the <see cref="ContentHandler.GetCoverImagePath"/> method.</item>
    /// <item>Clears any existing navigation data and extracts new navigation data from the EPUB's OPF file using the <see cref="Navigation.ExtractNavDataFromOpf"/> method.</item>
    /// <item>Updates additional properties of the <see cref="_ebook"/> object, including navigation data, statistics, book position, scroll value, and status.</item>
    /// <item>Saves the updated <see cref="_ebook"/> object to a JSON file using the <see cref="JsonHandler.StoreJsonEbookFile"/> method.</item>
    /// <item>Updates XHTML files related to the EPUB using the <see cref="AppControls.UpdateXhtmls"/> method.</item>
    /// <item>Adds the EPUB book's information to the collection of books using the <see cref="AllBooks.AddBookStore"/> method.</item>
    /// </list>
    /// </remarks>
    /// <exception cref="Exception">Thrown if any errors occur during the process of adding and processing the EPUB file.</exception>
    /// <event cref="BookAddedEvent">Triggered when the EPUB file is successfully added or if an error occurs during the process.</event>
    public async Task<bool> AddEpub(string epubFilePath, string destination, string fileName, bool debug = false)
    {
        try
        {
            // Extract the EPUB file to the specified destination
            ExtractEpub(epubFilePath, destination, fileName);

            // Retrieve the path to the content file within the EPUB
            GetEpubContentFilePath();

            // Iterate over metadata tags and extract corresponding values
            foreach (string tag in _metadataTags)
            {
                string metadata = _contentHandler.AddMetaData(_contentFilePath, tag);

                switch (tag)
                {
                    case "dc:title":
                        _ebook.Title = metadata;
                        break;
                    case "dc:creator":
                        _ebook.Author = metadata;
                        break;
                    case "dc:Language":
                        _ebook.Language = metadata;
                        break;
                    case "dc:publisher":
                        _ebook.Publisher = metadata;
                        break;
                    case "dc:description":
                        _ebook.Description = metadata;
                        break;
                }
            }

            // Set the path to the cover image
            _ebook.CoverPath = _contentHandler.GetCoverImagePath(_contentFilePath, Path.GetDirectoryName(_ebook.ContentPath));

            // Clear existing navigation data and extract new data from the EPUB
            _nvg.NavData.Clear();
            _nvg.ExtractNavDataFromOpf(_ebook.ContentPath);
            _ebook.NavData = _nvg.NavData;

            // Initialize other properties of the _ebook object
            _ebook.StatsRecord1 = new Dictionary<string, string>();
            _ebook.InBookPosition = "1";
            _ebook.ScrollValue = "0";
            _ebook.Status = "Not Started";

            // Store the updated eBook information
            JsonHandler.StoreJsonEbookFile(_ebook, _ebook.EbookDataFolderPath);

            // Update XHTML files
            await AppControls.UpdateXhtmls(_ebook.EbookFolderPath);

            // Add the book to the collection
            AllBooks allBooks = new AllBooks();
            allBooks.AddBookStore(_ebook.JsonDataPath);

            if (debug) { Debug.WriteLine("AddEpub() - Success"); }

            // Trigger event indicating success
            BookAddedEvent?.Invoke(this, "The book was added successfully.");
            return true;
        }
        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"AddEpub() - Fail - {ex.Message}"); }

            // Trigger event indicating failure
            BookAddedEvent?.Invoke(this, $"Failed to add the book: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Generates a random alphanumeric tag of the specified length.
    /// </summary>
    /// <param name="length">The length of the tag to generate. Must be greater than 0.</param>
    /// <returns>A random alphanumeric string of the specified length.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified length is less than or equal to 0.</exception>
    /// <remarks>
    /// This method uses a combination of uppercase letters, lowercase letters, and digits to create a random tag.
    /// It ensures that the length parameter is valid before generating the tag. If the length is not valid, an
    /// <see cref="ArgumentOutOfRangeException"/> is thrown.
    /// </remarks>
    private string GenerateRandomTag(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");
        }

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var tag = new char[length];

        for (int i = 0; i < length; i++)
        {
            tag[i] = chars[random.Next(chars.Length)];
        }

        return new string(tag);
    }

    /* TO-DO in future
    /// <summary>
    /// Shows a dialog with the specified title and content.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task ShowDialog(string title, string content)
    {
        ContentDialog dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "Ok"
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// Returns the Language of the ebook
    /// </summary>
    private void CheckEbookLanguage()
    {
        // TO-DO
    }
    */

}

/// <summary>
/// Class for handling epub navigation data
/// </summary>
public class Navigation
{

    /// <summary>
    /// Dictionary to store navigation data extracted from an EPUB file.
    /// </summary>
    public Dictionary<string, List<string>> NavData;

    /// <summary>
    /// Initializes a new instance of the <see cref="Navigation"/> class.
    /// </summary>
    public Navigation()
    {
        NavData = new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Finds a file with one of the specified extensions in a given directory and its subdirectories.
    /// </summary>
    /// <param name="directory">The directory to search within.</param>
    /// <param name="extensions">A list of file extensions to search for, including the dot (e.g., ".txt").</param>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console. Defaults to <c>false</c>.</param>
    /// <returns>
    /// The full path of the first file found with one of the specified extensions, or <c>null</c> if no matching files are found.
    /// </returns>
    /// <remarks>
    /// This method searches through all files in the specified directory and its subdirectories. It returns the path of the first file that matches one of the provided extensions.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="directory"/> or <paramref name="extensions"/> is <c>null</c>.</exception>
    public string FindFilesWithExtensions(string directory, List<string> extensions, bool debug = false)
    {
        List<string> matches = new List<string>();   // List of files that match the extensions - not used

        try
        {
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                if (extensions.Contains(Path.GetExtension(file)))
                {
                    matches.Add(file);
                    return file;
                }
            }

            if (debug) { Debug.WriteLine($"FindFilesWithExtensions() - Success - {string.Join(", ", matches)}" ); }
        }
        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"FindFilesWithExtensions() - Fail - {ex.Message}"); }
        }
        return null;
    }

    /// <summary>
    /// Extracts navigation data from an OPF (Open Packaging Format) file.
    /// </summary>
    /// <param name="opfFilePath">The file path of the OPF file to extract navigation data from.</param>
    /// <param name="debug">If <c>true</c>, outputs debug information to the debug console. Defaults to <c>false</c>.</param>
    /// <remarks>
    /// This method reads the OPF file to extract information about the manifest and spine elements. It populates the <see cref="NavData"/> dictionary with the play order and source details of each item specified in the spine.
    /// </remarks>
    /// <exception cref="FileNotFoundException">Thrown when the specified OPF file does not exist.</exception>
    /// <exception cref="XmlException">Thrown when there is an error parsing the OPF file.</exception>
    public void ExtractNavDataFromOpf(string opfFilePath, bool debug = false)
    {

        try
        {
            Dictionary<string, List<string>> manifestData = new Dictionary<string, List<string>>();
            List<string> spineData = new List<string>();

            XDocument opfDoc = XDocument.Load(opfFilePath);
            XNamespace ns = "http://www.idpf.org/2007/opf";
            var manifest = opfDoc.Root.Element(ns + "manifest");

            foreach (var item in manifest.Elements(ns + "item"))
            {
                string id = item.Attribute("id")?.Value;
                string href = item.Attribute("href")?.Value;
                string mediaType = item.Attribute("media-type")?.Value;

                manifestData.Add(id, new List<string> { href, mediaType });

                if (debug) {
                    Debug.WriteLine("");
                    Debug.WriteLine($"ID: {id}");
                    Debug.WriteLine($"HREF: {href}");
                    Debug.WriteLine($"Media Type: {mediaType}");
                    Debug.WriteLine("");
                }
            }

            var spine = opfDoc.Root.Element(ns + "spine");

            foreach (var element in spine.Elements())
            {
                string idref = (element.Attribute("idref")?.Value);
                spineData.Add(idref);
            }


            int playOrder = 0;
            foreach (var idref in spineData)
            {
                playOrder++;
                string source = manifestData[idref][0];
                string text = manifestData[idref][1];

                NavData.Add(playOrder.ToString(),
                    new List<string> { $"{Path.GetDirectoryName(opfFilePath)}\\{source}", text });

                if (debug) {
                    Debug.WriteLine("");
                    Debug.WriteLine($"Play Order: {playOrder}");
                    Debug.WriteLine($"Source: {source}");
                    Debug.WriteLine($"Text: {text}");
                    Debug.WriteLine("");
                }
            }

            if (debug) { Debug.WriteLine("ExtractNavDataFromOpf() - Success"); } 
            
        }

        catch (Exception ex)
        {
            if (debug) { Debug.WriteLine($"ExtractNavDataFromOpf() - Fail - {ex.Message}"); }
        }

    }

    /* Deprecated
    /// <summary>
    /// Sets the NavData dictionary with the extracted data from the .ncx file, does not work correctly with every .epub file
    /// </summary>
    /// <param name="navFilePath"></param>
    public void ExtractNavDataFromNcx(string navFilePath)
    {
        try
        {
            XDocument navDoc = XDocument.Load(navFilePath);

            XNamespace ns = "http://www.daisy.org/z3986/2005/ncx/";
            var navMaps = navDoc.Root.Elements(ns + "navMap");

            foreach (var navMap in navMaps)
            {
                foreach (var navPoint in navMap.Descendants(ns + "navPoint"))
                {

                    Debug.WriteLine("");
                    Debug.WriteLine(navPoint);
                    Debug.WriteLine("");

                    string playOrder = navPoint.Attribute("playOrder")?.Value;
                    XElement contentElement = navPoint.Descendants(ns + "content").FirstOrDefault();
                    XElement textElement = navPoint.Descendants(ns + "text").FirstOrDefault();

                    string source = contentElement?.Attribute("src")?.Value;
                    string text = textElement?.Value;

                    Debug.WriteLine("");
                    Debug.WriteLine($"Play Order: {playOrder}");
                    Debug.WriteLine($"Source: {source}");
                    Debug.WriteLine($"Text: {text}");
                    Debug.WriteLine("");

                    NavData.Add(playOrder,
                        new List<string> { $"{System.IO.Path.GetDirectoryName(navFilePath)}\\{source}", text });
                }
            }

            Debug.WriteLine( logger.ExtractNavDataSuccess );
        }
        catch (Exception e)
        {
            Debug.WriteLine( $"{logger.ExtractNavDataFail}{e.Message}" );
        }
    }
    */


}


