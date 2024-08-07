using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text.Json;

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Shapes;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml.Controls;


namespace EpubReader.code;

public class NavDataItem
{
    public string Id { get; set; }
    public string Source { get; set; }
    public string PlayOrder { get; set; }
    public string Text { get; set; }
}

public class Ebook
{
    // Ebook attributes
    public string Title { get; set; }
    public string Author { get; set; }
    public string Language { get; set; }
    public string Publisher { get; set; }
    public string Description { get; set; }
    public string DateAdded { get; set; }
    public string DateLastOpened { get; set; }
    public string Format { get; set; }
    public string FileName { get; set; }
    public string InBookPosition { get; set; }
    public string ScrollValue { get; set; }
    public string BookOpenTime { get; set; }
    public string BookCloseTime { get; set; }
    public string BookReadTime { get; set; }

    public string Status { get; set; }   // Finished, Not Started, Reading
    public Dictionary<string, string> StatsRecord1 { get; set; }
    public string StatsRecord2 { get; set; }
    public Dictionary<string, List<string>> NavData { get; set; }





    // Paths
    public string EbookFolderPath { get; set; }
    public string EbookDataFolderPath { get; set; }
    public string ContainerPath { get; set; }
    public string ContentPath { get; set; }
    public string CoverPath { get; set; }
    public string JsonDataPath { get; set; }
    public string NavigationFilePath { get; set; }
    
}

/// <summary>
/// Class for handling all books list
/// </summary>
public class AllBooks
{
    // List containing path to ebook's jsonDataFiles
    public List<string> Books { get; set; }

    /// <summary>
    /// Loads allBooks JSON file into Books List
    /// </summary>
    public void LoadAllBooksFromJson()
    {
        try
        {
            string path = FileManagment.GetEbookAllBooksJsonFile();
            Books = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            // if file does not exist, create a new one
            List<string> emptyList = new List<string>();

            // create empty file
            string path = FileManagment.GetEbookAllBooksJsonFile();
            File.WriteAllText(path, JsonSerializer.Serialize(emptyList));
            Books = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path));
        }
    }
    
    /// <summary>
    /// Add a ebook's jsonDataPath to Books List
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
    /// Stores allBooks List to a allBooks JSON file
    /// </summary>
    public void StoreBooksToJson()
    {
        string path = FileManagment.GetEbookAllBooksJsonFile();
        File.WriteAllText(path, JsonSerializer.Serialize(Books));
    }

    /// <summary>
    /// Removes a ebook's jsonDataPath from Books List
    /// </summary>
    /// <param name="ebookDataJsonPath"></param>
    public void RemoveBook(string ebookDataJsonPath)
    {
        Books.Remove(ebookDataJsonPath);
    }

    /// <summary>
    /// Removes a ebook's jsonDataPath from allBooks JSON file
    /// </summary>
    /// <param name="ebookDataJsonPath"></param>
    public void RemoveBookStore(string ebookDataJsonPath)
    {
        LoadAllBooksFromJson();
        Books.Remove(ebookDataJsonPath);
        StoreBooksToJson();
    }

    /// <summary>
    /// Prints Titles of all books on next line
    /// </summary>
    public void PrintAllBooks()
    {
        LoadAllBooksFromJson();
        //Debug.WriteLine("");

        foreach (string ebookDataJsonPath in Books)
        {
            var book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            //Debug.WriteLine($"Title: {book.Title}");
        }
        //Debug.WriteLine("");
    }

    /// <summary>
    /// Returns a list of books ebub folder sorted by Name alphabetically
    /// </summary>
    /// <param name="ascendingOrder"></param>
    /// <param name="print"></param>
    /// <returns></returns>
    public List<string> GetBooksEpubFoldersByName(bool ascendingOrder = false, bool print = false)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        foreach (string ebookDataJsonPath in Books)
        {
            var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            books.Add(_book.JsonDataPath, _book.Title);
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
                var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                //Debug.WriteLine($"Title: {_book.Title}");
            }
        }
        return Books;
    }

    // Returns a list of books ebub folder sorted by DateAdded
    public List<string> GetBooksEpubFoldersByDateAdded(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, DateTime> books = new Dictionary<string, DateTime>();

        //Debug.WriteLine("*****************************");

        foreach (string ebookDataJsonPath in Books)
        {
            var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);


            DateTime dateLastOpened = DateTime.ParseExact(_book.DateAdded, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            books.Add(_book.JsonDataPath, dateLastOpened);
            //Debug.WriteLine($"*   {_book.Title}   {_book.DateAdded}");
        }

        //Debug.WriteLine("*****************************");

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
                var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                //Debug.WriteLine($"Title: {_book.Title}");
            }
        }

        return Books;
    }

    // Returns a list of books ebub folder sorted by DateLastOpened
    public List<string> GetBooksEpubFoldersByDateLastOpened(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, DateTime> books = new Dictionary<string, DateTime>();


        foreach (string ebookDataJsonPath in Books)
        {
            var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);


            DateTime dateLastOpened = DateTime.ParseExact(_book.DateLastOpened, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            books.Add(_book.JsonDataPath, dateLastOpened);
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
                var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                //Debug.WriteLine($"Title: {_book.Title}");
            }
        }

        return Books;
    }
    // Returns a list of books ebub folder sorted by lenght TO-DO
    public List<string> GetBooksEpubFoldersByLenght(bool ascendingOrder, bool print)
    {
        return null;
    }

    // Returns a list of books ebub folder sorted by Author alphabetically
    public List<string> GetBooksEpubFoldersByAuthor(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        foreach (string ebookDataJsonPath in Books)
        {
            var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            books.Add(_book.JsonDataPath, _book.Author);
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
                var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                //Debug.WriteLine($"Title: {_book.Title}");
            }
        }

        return Books;
    }

    // Returns a list of books ebub folder sorted by Publisher alphabetically
    public List<string> GetBooksEpubFoldersByPublisher(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        foreach (string ebookDataJsonPath in Books)
        {
            var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            books.Add(_book.JsonDataPath, _book.Publisher);
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
                var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                //Debug.WriteLine($"Title: {_book.Title}");
            }
        }

        return Books;
    }

    // Returns a list of books ebub folder sorted by Language
    public List<string> GetBooksEpubFoldersByLanguage(bool ascendingOrder, bool print)
    {
        LoadAllBooksFromJson();
        Dictionary<string, string> books = new Dictionary<string, string>();

        foreach (string ebookDataJsonPath in Books)
        {
            var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
            books.Add(_book.JsonDataPath, _book.Language);
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
                var _book = JsonHandler.ReadEbookJsonFile(ebookDataJsonPath);
                //Debug.WriteLine($"Title: {_book.Title}");
            }
        }

        return Books;
    }

    public List<string> SelectSortMethod(string method, bool ascendingOrder, bool print)
    {
        switch (method)
        {
            case "Name":
                return GetBooksEpubFoldersByName(ascendingOrder, print);
                break;
            case "DateAdded":
                return GetBooksEpubFoldersByDateAdded(ascendingOrder, print);
                break;
            case "DateLastOpened":
                return GetBooksEpubFoldersByDateLastOpened(ascendingOrder, print);
                break;
            case "Author":
                return GetBooksEpubFoldersByAuthor(ascendingOrder, print);
                break;
            case "Publisher":
                return GetBooksEpubFoldersByPublisher(ascendingOrder, print);
                break;
            case "Language":
                return GetBooksEpubFoldersByLanguage(ascendingOrder, print);
                break;
        }

        return GetBooksEpubFoldersByName(ascendingOrder, print);
    }

    public static List<string> sortingMethods = new List<string> { "Name", "DateAdded", "DateLastOpened", "Author", "Publisher", "Language" };
}

public class JsonHandler
{
    /// <summary>
    /// Opens a JSON file and returns the Ebook object
    /// </summary>
    /// <param name="jsonPath">a path of the ebook's json data file</param>
    /// <returns>Ebook object (.Title, .Author, ...)</returns>
    public static Ebook ReadEbookJsonFile(string jsonPath)
    {
        try
        {
            string jsonString = File.ReadAllText(jsonPath);
            return System.Text.Json.JsonSerializer.Deserialize<Ebook>(jsonString);
            //Debug.WriteLine( logger.ReadJsonFileSuccess );
        }
        catch (Exception ex)
        {
            //Debug.WriteLine( $"{logger.ReadJsonFileFail}: {ex.Message}" );
        }

        // does this work?
        return new Ebook();
    }

    public static async Task StoreJsonEbookFile(Ebook ebook,string jsonPath)
    {
        try
        {
            ebook.JsonDataPath = jsonPath + "\\" + FileManagment.eboookDataFileName;
            string jsonString = System.Text.Json.JsonSerializer.Serialize(ebook);
            File.WriteAllText(jsonPath + "\\" + FileManagment.eboookDataFileName, jsonString);
        }

        catch (Exception ex)
        {
            Debug.WriteLine( $"StoreJsonEbookFile() - Fail - {ex.Message}" );
        }
    }
}
public class ContentHandler
{
    app_logging logger = new app_logging();
    public string AddMetaData(string contentPath, string xmlTag)
    {
        try
        {
            string xmlSegment = "";

            XmlDocument doc = new XmlDocument();
            doc.Load(contentPath);

            XmlNodeList elemList = doc.GetElementsByTagName(xmlTag);


            for (int i = 0; i < elemList.Count; i++)
            {
                xmlSegment += elemList[i].InnerXml;
            }

            Debug.WriteLine($"{logger.AddMetaDataSuccess} - {xmlTag} = {xmlSegment}");
            return xmlSegment;
        }

        catch (Exception ex)
        {
            Debug.WriteLine( $"{logger.AddMetaDataFail} - {xmlTag}: {ex.Message}" );
            return null;
        }
    }

    public string GetCoverImagePath(string contentOpfPath, string extractedEpubDir)
    {

        string coverId = "";

        try
        {
            // Load the content.opf file
            XDocument contentOpf = XDocument.Load(contentOpfPath);

            XNamespace ns = "http://www.idpf.org/2007/opf";

            // Find the meta tag with name="cover"
            var coverMeta = contentOpf.Descendants(ns + "meta")
                .FirstOrDefault(meta => (string)meta.Attribute("name") == "cover");

            if (coverMeta != null)
            {

                Debug.WriteLine($"CoverMeta = {coverMeta}");
                coverId = (string)coverMeta.Attribute("content");

                // Find the item tag with the corresponding id
                var coverItem = contentOpf.Descendants(ns + "item")
                    .FirstOrDefault(item => (string)item.Attribute("id") == coverId);

                Debug.WriteLine($"CoverID = {coverId}");
                Debug.WriteLine($"CoverItem = {coverItem}");
                if (coverItem != null)
                {
                    string coverImagePath = (string)coverItem.Attribute("href");

                    // convert / to \\
                    coverImagePath = coverImagePath.Replace("/", "\\");

                    Debug.WriteLine( $"extractedEpubDir = {extractedEpubDir}");
                    Debug.WriteLine( $"coverImagePath = {coverImagePath}");

                    string fullCoverImagePath = System.IO.Path.Combine(extractedEpubDir, coverImagePath);


                    Debug.WriteLine( $"{logger.GetCoverImagePathSuccess}: {fullCoverImagePath}" );
                    return fullCoverImagePath;
                }
            }

        }

        catch (Exception ex)
        {
            Debug.WriteLine( $"{logger.GetCoverImagePathFail}: {ex.Message}" );
        }

        return System.IO.Path.Combine(extractedEpubDir, coverId);
    }

}
public class RecentEbooksHandler
{
    public static Dictionary<string, string> recentEbooks = new Dictionary<string, string>();
    public static string MetaSplitter = "*cxlpfdsl?82349---";

    public static List<string> GetRecentEbooksPathsUpdated(string method, bool ascendingOrder=true, bool print=false)
    {
        
        AllBooks allBooks = new AllBooks();


        List<string> coverPaths = new List<string>();
        

        foreach (var jsonFile in allBooks.SelectSortMethod(method,ascendingOrder, print))
        {
            if (File.Exists(jsonFile))
            {
                Ebook ebook = JsonHandler.ReadEbookJsonFile(jsonFile);
                if (!string.IsNullOrEmpty(ebook.CoverPath))
                {
                    coverPaths.Add($"{ebook.CoverPath}{MetaSplitter}{ebook.Title}{MetaSplitter}{ebook.EbookFolderPath}{MetaSplitter}{ebook.InBookPosition}{MetaSplitter}{ebook.ScrollValue}");
                }
            }
            else
            {
                Debug.WriteLine($"File not found: {jsonFile}");
            }
        }
        return coverPaths;
    }
}

public class EpubHandler
{
    app_logging logger = new app_logging();
    ContentHandler contentHandler = new ContentHandler();
    Navigation nvg = new Navigation();

    public event Action BookAdded;

    private string _ebookFolderPath = "";
    private string _containerFileName = "container.xml";
    private string _contentFilePath = "";

    private Ebook _ebook;

    private List<string> _metadataTags = new List<string> { "dc:title", "dc:creator", "dc:language", "dc:publisher", "dc:description" };

    // Extracts epub file

    private string GenerateRandomTag(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());

    }

    public void ExtractEpub(string epubFilePath, string destination, string fileName)
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
                destination = destination + "\\" + GenerateRandomTag(5);


                archive.ExtractToDirectory( destination );
                Directory.CreateDirectory( destination + "\\" + "DATA");

                _ebookFolderPath = destination;
                _ebook.EbookFolderPath = destination;
                _ebook.EbookDataFolderPath = destination + "\\" + "DATA";
                _ebook.NavigationFilePath = nvg.FindFilesWithExtensions(_ebookFolderPath, new List<string> { ".ncx", ".nav" });
                _ebook.Format = "epub";
                _ebook.FileName = fileName.Split(".epub")[0];
                _ebook.DateAdded = DateTime.Now.ToString();   // format: 27/07/2024 08:21:56
                _ebook.DateLastOpened = DateTime.Now.ToString();   // format: 27/07/2024 08:21:56

                //_ebook.DateLastOpened = _ebook.DateAdded;

                Debug.WriteLine( logger.ExtractEpubSuccess);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine( $"{logger.ExtractEpubFail}: {ex.Message}" );

            if (ex.Message.Contains("already exists"))
            {
                Debug.WriteLine("");
                Debug.WriteLine("**************************************************\n*");
                Debug.WriteLine($"* Book Has Already Been Added!");
                Debug.WriteLine("*\n**************************************************");
                Debug.WriteLine("");
            }
            throw;
        }

    }

    // Returns .opf file path
    public void GetEpubContentFilePath()
    {
        try
        {
            string contentFilePath = "";
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

            Debug.WriteLine( logger.GetEpubContentFilePathSuccess );


        }

        catch (IOException ex) when (ex.Message.Contains("already exists"))
        {
            Debug.WriteLine($"{logger.ExtractEpubFail}: The file already exists.");
            throw; // Rethrow the exception if you want it to propagate further
        }

        catch (Exception ex)
        {
            Debug.WriteLine( $"{logger.GetEpubContentFilePathFail}: {ex.Message}" );
            throw;
        }
    }

    // Adds book to the library
    public async Task<bool> AddEpub(string epubFilePath, string destination, string fileName)
    {
        try
        {
            ExtractEpub(epubFilePath, destination, fileName);
            GetEpubContentFilePath();

            foreach (string tag in _metadataTags)
            {
                string metadata = contentHandler.AddMetaData(_contentFilePath, tag);

                switch (tag)
                {
                    case "dc:title":
                        _ebook.Title = metadata;
                        break;
                    case "dc:creator":
                        _ebook.Author = metadata;
                        break;
                    case "dc:language":
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

            _ebook.CoverPath = contentHandler.GetCoverImagePath(_contentFilePath, System.IO.Path.GetDirectoryName(_ebook.ContentPath));
            //Debug.WriteLine($"NavFilePath = {_ebook.NavigationFilePath}");
            nvg.navData.Clear();
            await nvg.ExtractNavDataFromOPF(_ebook.ContentPath);
            _ebook.NavData = nvg.navData;
            _ebook.StatsRecord1 = new Dictionary<string, string>();

            // select value with key 1
            _ebook.InBookPosition = "1";
            _ebook.ScrollValue = "0";
            _ebook.Status = "Not Started";

            

            JsonHandler.StoreJsonEbookFile(_ebook,_ebook.EbookDataFolderPath);

            //RecentEbooksHandler.LoadJsonToList();
            //REHandler.AddEbookToList(_ebook.JsonDataPath, REHandler.GetNewerDate(_ebook.DateAdded, _ebook.DateLastOpened));
            //REHandler.SortRecentsEbooks();
            //REHandler.WriteListToJson();

            app_controls.UpdateXhtmls(_ebook.EbookFolderPath);

            // store ebook to all books
            AllBooks allBooks = new AllBooks();
            allBooks.AddBookStore(_ebook.JsonDataPath);

            Debug.WriteLine( logger.AddBookMessageSuccess );

            BookAddedEvent?.Invoke(this ,"The book was added successfully.");
            return true;



        }
        catch (Exception ex)
        {
            Debug.WriteLine( $"{logger.AddBookMessageFail}: {ex.Message}" );
            BookAddedEvent?.Invoke(this, $"Failed to add the book: {ex.Message}");
            return false;

        }
    }

    public event EventHandler<string> BookAddedEvent;


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
}

/// <summary>
/// Class for handling epub navigation data
/// </summary>
public class Navigation
{
    // PlayOrder : { Source, Text }
    public Dictionary<string, List<string>> navData;
    private app_logging logger = new app_logging();

    public Navigation()
    {
        navData = new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Returns List of files with the specified extensions, here used to find the .ncx file
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public string FindFilesWithExtensions(string directory, List<string> extensions)
    {
        List<string> matches = new List<string>();   // List of files that match the extensions - not used

        try
        {
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                if (extensions.Contains(System.IO.Path.GetExtension(file)))
                {
                    matches.Add(file);
                    Debug.WriteLine($"{logger.FindFilesWithExtensionsSuccess} - {file}");
                    return file;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{logger.FindFilesWithExtensionsFail}{e.Message}");
        }
        return null;
    }
    
    /// <summary>
    /// Method to extract the navigation data from the .opf file
    /// </summary>
    /// <param name="opfFilePath"></param>
    /// <returns></returns>
    public async Task ExtractNavDataFromOPF(string opfFilePath)
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

            /*
            Debug.WriteLine("");
            Debug.WriteLine($"ID: {id}");
            Debug.WriteLine($"HREF: {href}");   
            Debug.WriteLine($"Media Type: {mediaType}");
            Debug.WriteLine("");
            */
        }

        Debug.WriteLine("\nManifest Finished\n");
        
        var spine = opfDoc.Root.Element(ns + "spine");

        foreach (var element in spine.Elements())
        {
            string idref = (element.Attribute("idref")?.Value);
            spineData.Add(idref);

            /*
            Debug.WriteLine("");
            Debug.WriteLine($"IDREF: {idref}");
            Debug.WriteLine("");
            */

        }

        Debug.WriteLine("\nSpine Finished\n");

        int playOrder = 0;
        foreach (var idref in spineData)
        {
            playOrder++;
            string source = manifestData[idref][0];
            string text = manifestData[idref][1];

            navData.Add(playOrder.ToString(),
                new List<string> { $"{System.IO.Path.GetDirectoryName(opfFilePath)}\\{source}", text });
            /*
            Debug.WriteLine("");
            Debug.WriteLine($"Play Order: {playOrder}");
            Debug.WriteLine($"Source: {source}");
            Debug.WriteLine($"Text: {text}");
            Debug.WriteLine("");
            */
        }

        Debug.WriteLine("\nSync Finished\n");

    }

    /// <summary>
    /// Sets the navData dictionary with the extracted data from the .ncx file, does not work correctly with every .epub file
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

                    navData.Add(playOrder,
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


}

