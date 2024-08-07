using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using System.Xml.Linq;
using System.IO;
using EpubReader.app_pages;
using System.Text.Json;

namespace EpubReader.code
{
    public class app_controls
    {
        // my objects
        EpubHandler epubHandler = new EpubHandler();
        app_logging logger = new app_logging();

        /// <summary>
        /// Method to add a book to the library - ebook folder
        /// </summary>
        public async Task<bool> AddBookButtonMethod()
        {
            bool result = true;
            Debug.WriteLine("AddBookButtonMethod started");

            if (App.Window == null)
            {
                Debug.WriteLine("App.Window is null");
                return false;
            }

            FileOpenPicker fileOpenPicker = new()
            {
                ViewMode = PickerViewMode.Thumbnail,
            };

            foreach (string ebookFormat in FileManagment.SupportedEbooksFormats)
            {
                fileOpenPicker.FileTypeFilter.Add(ebookFormat);
            }

            nint windowHandle = WindowNative.GetWindowHandle(App.Window);
            if (windowHandle == IntPtr.Zero)
            {
                Debug.WriteLine("windowHandle is zero");
                return false;
            }

            InitializeWithWindow.Initialize(fileOpenPicker, windowHandle);

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                logger.addBookMessage(file.Name, file.FileType, file.Path);
                result = await Task.Run(() => epubHandler.AddEpub(file.Path, FileManagment.GetEbooksFolderPath(), file.Name)); 

                
            }
            else
            {
                Debug.WriteLine("No file picked");
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Returns a list of all ebooks in the ebook folder.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetListOfAllEbooks()
        {
            return GetImmediateSubdirectories(FileManagment.GetEbooksFolderPath());
        }

        static List<string> GetImmediateSubdirectories(string rootPath)
        {
            List<string> subdirectories = new List<string>();

            try
            {
                string[] subdirs = Directory.GetDirectories(rootPath);
                subdirectories.AddRange(subdirs);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Access denied to directory: {rootPath}. Exception: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }

            return subdirectories;
        }

        /// <summary>
        /// Injects the global CSS file path into all xhtml files in the ebook folder.
        /// </summary>
        /// <param name="ebookFolderPath"></param>
        /// <returns></returns>
        public static async Task UpdateXhtmls(string ebookFolderPath)
        {
            try
            {
                string jsonDataFilePath = FileManagment.GetEbookDataJsonFile(ebookFolderPath);
                var ebook = JsonHandler.ReadEbookJsonFile(jsonDataFilePath);
                
                foreach (KeyValuePair<string, List<string>> kvp in ebook.NavData)
                {
                    await UpdateCssPath(kvp.Value[0], FileManagment.GetEbookViewerStyleFilePath());
                }

                Debug.WriteLine($"UpdateXhtmls() - Success");
            }

            catch (Exception e)
            {
                Debug.WriteLine($"UpdateXhtmls() - Fail - {e.Message}");
                throw;
            }

        }

        /// <summary>
        /// Injects the global CSS file path into all xhtml files in all ebook folders.
        /// </summary>
        /// <returns></returns>
        public static async Task GlobalCssInjector()
        {
            foreach (string ebookFolderPath in GetListOfAllEbooks())
            {
                await UpdateXhtmls(ebookFolderPath);
            }
            Debug.WriteLine("");
            Debug.WriteLine("******************");
            Debug.WriteLine("Injection Ended");
            Debug.WriteLine("******************");
            Debug.WriteLine("");

        }

        /// <summary>
        /// Updates the CSS path in the xhtml file.git 
        /// </summary>
        /// <param name="xhtmlPath"></param>
        /// <param name="newCssPath"></param>
        /// <returns></returns>
        public static async Task UpdateCssPath(string xhtmlPath, string newCssPath)
        {
            const int maxRetries = 10;
            const int delayMilliseconds = 100;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    XDocument xhtmlDocument = XDocument.Load(xhtmlPath);
                    var linkElement = xhtmlDocument.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "link" && e.Attribute("rel")?.Value == "stylesheet");

                    if (linkElement != null)
                    {
                        linkElement.SetAttributeValue("href", newCssPath);
                        xhtmlDocument.Save(xhtmlPath);
                    }
                    else
                    {
                        //Debug.WriteLine("No <link> element with rel=\"stylesheet\" found.");
                    }
                    //Debug.WriteLine($"UpdateCssPath() - Success");
                    return; // Exit the method if successful
                }
                catch (IOException ex) when (ex is IOException)
                {
                    Debug.WriteLine($"UpdateCssPath() - Attempt {attempt + 1} failed - {ex.Message}");
                    if (attempt == maxRetries - 1)
                    {
                        throw; // Re-throw the exception if the last attempt fails
                    }
                    await Task.Delay(delayMilliseconds); // Wait before retrying
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UpdateCssPath() - Fail - {ex.Message}");
                    throw;
                }
            }
        }


    }
}
