using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;


namespace EpubReader.code;
/// <summary>
/// Class used for file management in the application
/// </summary>
public class FileManagement
{

    // file names used in the app
    private static readonly string EbookFolderName = "ebooks"; 
    private static readonly string SettingsFolderName = "settings";
    private static readonly string EbookViewerStyleFileName = "ebook_viewer-style.css";
    private static readonly string GlobalSettingsFileName = "globalSettings.json";
    private static readonly string DictFileName = "globalDict.json";
    private static readonly string EbookDataFolderName = "DATA";
    private static readonly string EbookAllBooksFileName = "allBooks.json";

    /// <summary>
    /// List of supported ebook formats
    /// </summary>
    public static readonly List<String> SupportedEbooksFormats = new List<string>() { ".epub" };

    /// <summary>
    /// Name of the file used for storing the recent ebooks
    /// </summary>
    public static readonly string EbookDataFileName = "ebookData.json";


    /// <summary>
    ///  Writes string content to file in the application storage
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task WriteTextToFileAsync(string fileName, string content)
    {
        // Get the local folder
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        // Create or open the file
        var file = await localFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);

        // Write text to the file
        await Windows.Storage.FileIO.WriteTextAsync(file, content);
    }

    /// <summary>
    /// Creates a folder in the application storage
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public static async Task CreateFolderAsync(string folderName)
    {
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        // Create or get the subfolder
        await localFolder.CreateFolderAsync(folderName, Windows.Storage.CreationCollisionOption.OpenIfExists);
    }

    /// <summary>
    /// Returns true if the folder exists in the application storage
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public static async Task<bool> DoesFolderExistAsync(string folderName)
    {
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        try
        {
            // Try to get the folder
             await localFolder.GetFolderAsync(folderName);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /* Deprecated
    public static string GetEbookFolderName()
    {
        return EbookFolderName;
    }

    public static string GetSettingsFolderName()
    {
        return SettingsFolderName;
    }
    
    public static List<string> GetSupportedEbooksFormats()
    {
        return SupportedEbooksFormats;
    }

    public static string GetRecentEbooksFileName()
    {
        return RecentEbooksFileName;
    }

    public static string GetEbookViewerStyleFileName()
    {
        return EbookViewerStyleFileName;
    }

    public static string GetEbookDataFolderName()
    {
        return EbookDataFolderName;
    }

    public static string GetEbookDataFileName()
    {
        return EbookDataFileName;
    }*/



    /// <summary>
    /// Returns the application storage address
    /// </summary>
    /// <returns></returns>
    public static string GetAppAddress()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
    }

    /// <summary>
    /// Returns the ebooks folder path ( = LocalState/ebooks )
    /// </summary>
    /// <returns></returns>
    public static string GetEbooksFolderPath()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + EbookFolderName;
    }

    /// <summary>
    /// Returns the settings folder path ( = LocalState/settings )
    /// </summary>
    /// <returns></returns>
    public static string GetSettingsFolderPath()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + SettingsFolderName;
    }
    
    /// <summary>
    /// Returns the path of ebooks css file used for the ebook viewer
    /// </summary>
    /// <returns></returns>
    public static string GetEbookViewerStyleFilePath()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + EbookViewerStyleFileName;
    }

    /// <summary>
    /// Returns the path of the ebook data json file
    /// </summary>
    /// <param name="ebookFolderPath"></param>
    /// <returns></returns>
    public static string GetEbookDataJsonFile(string ebookFolderPath)
    {
        return ebookFolderPath + "\\" + EbookDataFolderName + "\\" + EbookDataFileName;
    }

    /// <summary>
    /// Returns the path of the all books json file used for displaying all books in the library
    /// </summary>
    /// <returns></returns>
    public static string GetEbookAllBooksJsonFile()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + SettingsFolderName + "\\" + EbookAllBooksFileName;
    }

    /// <summary>
    /// Returns the path of the xhtml file from ebook data json file
    /// </summary>
    /// <param name="ebookFolderPath"></param>
    /// <param name="playOrder"></param>
    /// <returns></returns>
    public static string GetBookContentFilePath(string ebookFolderPath, string playOrder)
    {
        var ebook = JsonHandler.ReadEbookJsonFile(GetEbookDataJsonFile(ebookFolderPath));
        string xhtmlPath;
        try
        {
            xhtmlPath = ebook.NavData[playOrder][0];
        }
        catch (KeyNotFoundException)
        {
            xhtmlPath = ebook.NavData["1"][0];
        }
        return xhtmlPath;
    }
    
    /// <summary>
    /// Created the css file used for the ebook viewer
    /// </summary>
    public static async Task CreateCssSettingsFile()
    {
        string cssContent = @"/* Hide scrollbars for WebKit-based browsers */
                            body {
                                Font-family: 'Merriweather', serif;
                                Font-size: 150%;
                                color: #000000;
                                /*
                                background colors I like:display: EFE0CD, E4D8CD, E2D3C4, D4C2AF
                                */
                                background-color: #e6ddc9;
                                
                                text-align: justify;
                                overflow: hidden; /*  Hide scrollbars for the body element */
                            
                            }
                            
                            /* Hide scrollbars for other elements */
                            * {
                                scrollbar-width: none; /* Firefox */
                                -ms-overflow-style: none; /* Internet Explorer and Edge */
                            }
                            
                            ::-webkit-scrollbar {
                                display: none; /* WebKit browsers */
                            }
                            
                            /* Center images and ensure they fit within the viewport */
                            img {
                                display: block;
                                margin: 0 auto;
                                max-width: 100%;
                                height: auto;
                            }";
        
        string cssFilePath = GetAppAddress() + "\\" + EbookViewerStyleFileName;

        if (!File.Exists(cssFilePath))
        {
            await WriteTextToFileAsync(EbookViewerStyleFileName, cssContent);
        }

    }

    /// <summary>
    /// Creates the global settings file with default values
    /// </summary>
    public static void CreateGlobalSettingsFile()
    {
        string filePath = GetAppAddress() + "\\" + SettingsFolderName + "\\" + GlobalSettingsFileName;
        GlobalSettingsJson globalSettings = new GlobalSettingsJson();
        globalSettings.EbookViewer = "Custom";
        globalSettings.Font = "Merriweather";
        globalSettings.BackgroundColor = "#efe0cd";
        globalSettings.TranslationService = "My Memory";
        globalSettings.Language = "English";
        globalSettings.Theme = "Woodlawn";
        globalSettings.FontSize = "1.5rem";
        globalSettings.Padding = "60";

        File.WriteAllText(filePath, JsonSerializer.Serialize(globalSettings));
    }

    /// <summary>
    /// Created the global dictionary file used for storing the words and their translations
    /// </summary>
    public static void CreateGlobalDictFile()
    {
        string filePath = GetGlobalDictPath();
        GlobalDictJson globalDict = new GlobalDictJson();
        globalDict.TranslationsDict = new Dictionary<string, List<string>>();
        File.WriteAllText(filePath, JsonSerializer.Serialize(globalDict));
    }
    
    /// <summary>
    /// Returns the path of the global settings file
    /// </summary>
    /// <returns></returns>
    public static string GetGlobalSettingsFilePath()
    {
        return GetAppAddress() + "\\" + SettingsFolderName + "\\" + GlobalSettingsFileName;
    }

    /// <summary>
    /// Deletes all the ebooks in the ebooks folder
    /// </summary>
    public void DeleteEbooks(bool debug = false)
    {
        try
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var folder = localFolder.GetFolderAsync(EbookFolderName).GetAwaiter().GetResult();

            // for each folder in the ebooks folder

            foreach (var item in folder.GetFoldersAsync().GetAwaiter().GetResult())
            {
                item.DeleteAsync().GetAwaiter().GetResult();
            }

            if (debug)
            {
                Debug.WriteLine($"DeleteEbooks() - Success");
            }
        }
        catch (Exception ex)
        {
            if (debug)
            {
                Debug.WriteLine($"DeleteEbooks() - Fail\n{ex}");
            }
        }
    }

    /// <summary>
    /// Returns the path of the global dictionary file
    /// </summary>
    /// <returns></returns>
    public static string GetGlobalDictPath()
    {
        return GetAppAddress() + "\\" + SettingsFolderName + "\\" + DictFileName;
    }
    
    /// <summary>
    /// Method which runs at the start of the application
    /// </summary>
    public static async Task StartUp(bool debug = false)
    {
        try
        {
            // Create the ebooks storage folder if it doesn't exist
            if (!await DoesFolderExistAsync(EbookFolderName))
            {
                await CreateFolderAsync(EbookFolderName);
            }

            // Creates settings folder, css file, global settings file and global dictionary file if they don't exist
            if (!await DoesFolderExistAsync(SettingsFolderName))
            {
                await CreateFolderAsync(SettingsFolderName);
                 CreateGlobalSettingsFile();
                CreateGlobalDictFile();
                await CreateCssSettingsFile();
            }
            if (debug) { Debug.WriteLine($"StartUp() - Success\n"); }
        }

        catch
        {
            if (debug) { Debug.WriteLine($"StartUp() - Fail\n"); }
        }

    }
    
    /// <summary>
    /// Dictionary data class used for storing the words and their translations
    /// </summary>
    public class GlobalDictJson
    {
        /// <summary>
        /// Dictionary used for storing the words and their translations
        /// </summary>
        public Dictionary<string, List<string>> TranslationsDict { get; set; } 
    }
}
