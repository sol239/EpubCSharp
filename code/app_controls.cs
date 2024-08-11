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

        private static string _scriptSelect = @"
function handleDocumentClick(event) {
    const clickedElement = event.target; {}

    if (clickedElement.nodeType === Node.ELEMENT_NODE) {
        const textContent = clickedElement.textContent;
        const range = document.createRange();

        for (let i = 0; i < clickedElement.childNodes.length; i++) {
            const node = clickedElement.childNodes[i];
            if (node.nodeType === Node.TEXT_NODE) {
                range.selectNodeContents(node);
                const rect = range.getBoundingClientRect();

                if (rect.left <= event.clientX && rect.right >= event.clientX &&
                    rect.top <= event.clientY && rect.bottom >= event.clientY) {
                    const words = node.textContent.split(' ');
                    let clickedWord = '';

                    for (let j = 0; j < words.length; j++) {
                        range.setStart(node, node.textContent.indexOf(words[j]));
                        range.setEnd(node, node.textContent.indexOf(words[j]) + words[j].length);
                        const wordRect = range.getBoundingClientRect();

                        if (wordRect.left <= event.clientX && wordRect.right >= event.clientX &&
                            wordRect.top <= event.clientY && wordRect.bottom >= event.clientY) {
                            clickedWord = words[j];
                            break;
                        }
                    }

                    if (clickedWord) {
                        window.chrome.webview.postMessage(`${clickedWord}`);
                        console.log(clickedWord);
                    }
                    else {
                        window.chrome.webview.postMessage(`*783kd4HJsn`);
                    }

                    break;
                }
            }
        }
    }

    // Display the selected text
    const selectedText = window.getSelection().toString();
    if (selectedText) {
        window.chrome.webview.postMessage(`${selectedText}`);
        console.log(selectedText);

    } else {
        window.chrome.webview.postMessage('*783kd4HJsn');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.body.addEventListener('click', handleDocumentClick);
});
";

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

                    try
                    {
                        // Path to the external script
                        string scriptPath = "C:/Users/david_pmv0zjd/source/repos/EpubReader/code/script.js";

                        // Create a script element with the src attribute
                        XElement scriptElement = new XElement(XName.Get("script", "http://www.w3.org/1999/xhtml"),
                            new XAttribute("src", scriptPath),
                            new XText(""));  // Ensure it is not self-closing

                        // Find the body element
                        XElement body = xhtmlDocument.Root.Element(XName.Get("body", "http://www.w3.org/1999/xhtml"));

                        // Append the script element to the body
                        body.Add(scriptElement);
                        xhtmlDocument.Save(xhtmlPath);

                        Debug.WriteLine("Script injected successfully");
                    }

                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Script injection failed - {ex.Message}");
                    }

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
