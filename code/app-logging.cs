using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop; // For initializing with the correct window handle


namespace EpubReader.code;
/*
public class app_logging
{
    // epub-handler.cs:

    // class: JsonHandler
    public string ReadJsonFileSuccess = "ReadEbookJsonFile() - Success\n";
    public string ReadJsonFileFail = "ReadEbookJsonFile() - Fail - ";

    public string StoreJsonEbookFile = "StoreJsonEbookFile() - Success\n";
    public string StoreJsonEbookFileFail = "StoreJsonEbookFile() - Fail";

    // class: ContentHandler
    public string AddMetaDataSuccess = "AddMetaData() - Success\n";
    public string AddMetaDataFail = "AddMetaData() - Fail - ";

    public string GetCoverImagePathSuccess = "GetCoverImagePathSuccess() - Success";
    public string GetCoverImagePathFail = "GetCoverImagePathSuccess() - Fail - ";

    // class: RecentEbooksHandler
    // TO-DO

    public string ExtractEpubSuccess = "ExtractEpub() - Success\n";
    public string ExtractEpubFail = "ExtractEpub() - Fail - ";

    public string GetEpubContentFilePathSuccess = "GetEpubContentFilePath() - Success\n";
    public string GetEpubContentFilePathFail = "GetEpubContentFilePath() - Fail - ";

    public string AddBookMessageSuccess = "AddEpub() - Success\n";
    public string AddBookMessageFail = "AddEpub() - Fail - ";

    // class: Navigation
    public string FindFilesWithExtensionsSuccess = "FindFilesWithExtensions() - Success";
    public string FindFilesWithExtensionsFail = "FindFilesWithExtensions() - Fail - ";

    public string ExtractNavDataSuccess = "ExtractNavData() - Success\n";
    public string ExtractNavDataFail = "ExtractNavData() - Fail - ";


    public void addBookMessage(string name, string type, string path)
    {
        Debug.WriteLine("");
        Debug.WriteLine("**************************************************\n*");
        Debug.WriteLine($"* Name: {name}");
        Debug.WriteLine($"* Path: {path}");
        Debug.WriteLine($"* FileType: {type}");
        Debug.WriteLine("*\n**************************************************");
        Debug.WriteLine("");
    }


    // file-managment.cs
    public string DeleteEbooksMessageSuccess = "DeleteEbooks() - Success\n";
    public string DeleteEbooksMessageFail = "DeleteEbooks() - Fail\n";

    // app_controls.cs
    public string UpdateXhtmlsSuccess = "UpdateXhtmls() - Success\n";
    public string UpdateXhtmlsFail = "UpdateXhtmls() - Fail - ";


}
*/