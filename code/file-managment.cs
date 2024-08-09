using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
using static EpubReader.code.FileManagment;

namespace EpubReader.code;
public class FileManagment
{

    app_logging logger = new app_logging();

    private static string _ebookFolderName = "ebooks"; 
    private static string _settingsFolderName = "settings";
    public static List<String> SupportedEbooksFormats = new List<string>() {".epub"};
    public static string recentEbooksFileName = "recentEbooks.json";
    public static string ebookViewerStyleFileName = "ebook_viewer-style.css";
    public static string globalSettingsFileName = "globalSettings.json";
    public static string dictFileName = "globalDict.json";


    // /DATA
    public static string ebookDataFolderName = "DATA";
    public static string eboookDataFileName = "ebookData.json";
    public static string ebookAllBooksFileName = "allBooks.json";


    

    // Write text to a file
    public static async Task WriteTextToFileAsync(string fileName, string content)
    {
        // Get the local folder
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        // Create or open the file
        var file = await localFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);

        // Write text to the file
        await Windows.Storage.FileIO.WriteTextAsync(file, content);
    }

    // Read text from a file
    public static async Task<string> ReadTextFromFileAsync(string fileName)
    {
        // Get the local folder
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        // Get the file
        var file = await localFolder.GetFileAsync(fileName);

        // Read the text from the file
        return await Windows.Storage.FileIO.ReadTextAsync(file);
    }

    // Create folder
    public static async Task CreateFolderAsync(string folderName)
    {
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        // Create or get the subfolder
        var folder = await localFolder.CreateFolderAsync(folderName, Windows.Storage.CreationCollisionOption.OpenIfExists);
    }

    // Does folder exist
    public static async Task<bool> DoesFolderExistAsync(string folderName)
    {
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        try
        {
            // Try to get the folder
            var folder = await localFolder.GetFolderAsync(folderName);
            Debug.WriteLine("Folder exists");
            // print the folder path
            Debug.WriteLine(folder.Path);
            return true;
        }
        catch
        {
            Debug.WriteLine("Folder does not exist");
            return false;
        }
    }

    // Does file exist
    public static async Task<bool> DoesFileExistAsync(string fileName)
    {
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        try
        {
            // Try to get the file
            var file = await localFolder.GetFileAsync(fileName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string GetEbookFolderName()
    {
        return _ebookFolderName;
    }

    public static string GetSettingsFolderName()
    {
        return _settingsFolderName;
    }
    
    public static List<string> GetSupportedEbooksFormats()
    {
        return SupportedEbooksFormats;
    }

    public static string GetRecentEbooksFileName()
    {
        return recentEbooksFileName;
    }

    public static string GetEbookViewerStyleFileName()
    {
        return ebookViewerStyleFileName;
    }

    public static string GetEbookDataFolderName()
    {
        return ebookDataFolderName;
    }

    public static string GetEbookDataFileName()
    {
        return eboookDataFileName;
    }



    // Get the app address = ..LocalState
    public static string GetAppAddress()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
    }

    // Get the ebook folder path = LocalState/ebooks
    public static string GetEbooksFolderPath()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + _ebookFolderName;

    }

    // Get the settings folder path = LocalState/settings
    public static string GetSettingsFolderPath()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + _settingsFolderName;
    }

    // Get the recent ebooks file path = LocalState/settings/recentEbooks.json
    public static string GetRecentEbooksFilePath()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + _settingsFolderName + "\\" + recentEbooksFileName;
    }

    // Get the ebook viewer style file path = LocalState/settings/ebook_viewer-style.css
    public static string GetEbookViewerStyleFilePath()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + ebookViewerStyleFileName;
    }

    // Get the ebook data json file path = LocalState/ebooks/ebookName/DATA/ebookData.json
    public static string GetEbookDataJsonFile(string ebookFolderPath)
    {
        return ebookFolderPath + "\\" + ebookDataFolderName + "\\" + eboookDataFileName;
    }

    public static string GetEbookAllBooksJsonFile()
    {
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + _settingsFolderName + "\\" + ebookAllBooksFileName;
    }

    public static string GetBookContentFilePath(string ebookFolderPath, string playOrder)
    {
        var ebook = JsonHandler.ReadEbookJsonFile(GetEbookDataJsonFile(ebookFolderPath));
        string xhtmlPath;

        try
        {
            xhtmlPath = ebook.NavData[playOrder][0].ToString();
        }

        catch (KeyNotFoundException)
        {
            xhtmlPath = ebook.NavData["1"][0].ToString();

        }

        //return $"{ebookFolderPath}\\{xhtmlPath}";
        return xhtmlPath;
    }
    
    // Create CSS settings file
    public static async Task CreateCssSettingsFile()
    {
        string cssContent = @"/* Hide scrollbars for WebKit-based browsers */
                            body {
                                font-family: 'Merriweather', serif;
                                font-size: 150%;
                                color: #000000;
                                /*
                                background colors I like:display: EFE0CD, E4D8CD, E2D3C4, D4C2AF
                                */
                                background-color: #efe0cd;
                                
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

        string settingsFolderPath = GetSettingsFolderPath();

        // Check if the file already exists

        string css_file_path = GetAppAddress() + "\\" + ebookViewerStyleFileName;

        if (!File.Exists(css_file_path))
        {
            await WriteTextToFileAsync(ebookViewerStyleFileName, cssContent);
            Debug.WriteLine($"CreateCssSettingsFile() - Success - css file created\n");
        }

    }

    public static async Task CreateGlobalSettingsFile()
    {
        string filePath = GetAppAddress() + "\\" + _settingsFolderName + "\\" + globalSettingsFileName;
        globalSettingsJson globalSettings = new globalSettingsJson();
        globalSettings.ebookViewer = "WebView2";
        globalSettings.font = "Merriweather";
        globalSettings.backgroundColor = "#efe0cd";
        File.WriteAllText(filePath, JsonSerializer.Serialize(globalSettings));
    }

    public static async Task CreateGlobalDictFile()
    {
        string filePath = GetGlobalDictPath();
        globalDictJson globalDict = new globalDictJson();
        globalDict.dict = new Dictionary<string, List<string>>();
        File.WriteAllText(filePath, JsonSerializer.Serialize(globalDict));


    }

    public static string GetGlobalSettingsFilePath()
    {
        return GetAppAddress() + "\\" + _settingsFolderName + "\\" + globalSettingsFileName;
    }

    // Delete all ebooks folders
    public void DeleteEbooks()
    {
        try
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var folder = localFolder.GetFolderAsync(_ebookFolderName).GetAwaiter().GetResult();

            // for each folder in the ebooks folder

            foreach (var item in folder.GetFoldersAsync().GetAwaiter().GetResult())
            {
                item.DeleteAsync().GetAwaiter().GetResult();
            }

            Debug.WriteLine($"{logger.DeleteEbooksMessageSuccess}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{logger.DeleteEbooksMessageFail} : {ex.Message}");
        }
    }

    public static string GetGlobalDictPath()
    {
        return GetAppAddress() + "\\" + _settingsFolderName + "\\" + dictFileName;
    }
    // StartUp operations run when the app starts
    public static async Task StartUp()
    {
        try
        {
            // Create the ebooks storage folder if it doesn't exist
            if (!await DoesFolderExistAsync(_ebookFolderName))
            {
                await CreateFolderAsync(_ebookFolderName);
            }

            // Create settings file if it doesn't exist
            if (!await DoesFolderExistAsync(_settingsFolderName))
            {
                await CreateFolderAsync(_settingsFolderName);
                await CreateGlobalSettingsFile();
                await CreateGlobalDictFile();
            }

            await CreateCssSettingsFile();

            Debug.WriteLine($"StartUp() - Settings\n");

        }

        catch
        {
            Debug.WriteLine($"StartUp() - Fail\n");
        }

    }

    public class globalDictJson
    {

        public Dictionary<string, List<string>> dict { get; set; } 
    }


}
