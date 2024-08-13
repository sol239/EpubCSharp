using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;


namespace EpubReader.code
{
    /// <summary>
    /// Class to handle global app controls from different pages.
    /// </summary>
    public class AppControls
    {
        private EpubHandler _epubHandler = new EpubHandler();

        /// <summary>
        /// Asynchronously displays a file picker dialog to allow the user to select an eBook file and adds it to the eBook collection.
        /// </summary>
        /// <param name="debug">
        /// Optional parameter that, when set to <c>true</c>, enables detailed debugging output. The default value is <c>false</c>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result is a <c>boolean</c> indicating whether the file was successfully added to the collection.
        /// </returns>
        /// <remarks>
        /// The method initializes a file picker dialog with a filter for supported eBook formats. It then displays the dialog to the user. If the user selects a file, it attempts to add the file to the eBook collection. The success or failure of the operation is returned. 
        /// If an exception occurs during the process, it is logged if debugging is enabled.
        /// </remarks>
        public async Task<bool> AddBookButtonMethod(bool debug = false)
        {
            try
            {
                bool result;

                if (App.Window == null)
                {
                    Debug.WriteLine("App.Window is null");
                    return false;
                }

                FileOpenPicker fileOpenPicker = new()
                {
                    ViewMode = PickerViewMode.Thumbnail,
                };

                foreach (string ebookFormat in FileManagement.SupportedEbooksFormats)
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
                    result = await Task.Run(() => _epubHandler.AddEpub(file.Path, FileManagement.GetEbooksFolderPath(), file.Name));


                }
                else
                {
                    Debug.WriteLine("No file picked");
                    result = false;
                }

                if (debug) { Debug.WriteLine($"AddBookButtonMethod() - {result}"); }
                return result;
            }

            catch (Exception e)
            {
                if (debug) { Debug.WriteLine($"AddBookButtonMethod() - Fail - {e.Message}"); }
                return false;
            }
        }

        /// <summary>
        /// Asynchronously updates the CSS paths in XHTML files based on the navigation data of an eBook.
        /// </summary>
        /// <param name="ebookFolderPath">
        /// The path to the folder containing the eBook files. This is used to locate the eBook's JSON data file and the CSS file for styling.
        /// </param>
        /// <param name="debug">
        /// Optional parameter that, when set to <c>true</c>, enables detailed debugging output. The default value is <c>false</c>.
        /// </param>
        /// <remarks>
        /// This method reads the eBook's JSON data file to obtain navigation data. For each XHTML file specified in the navigation data, it updates the CSS path to ensure that the XHTML files use the correct styling. 
        /// If an error occurs during the update process, the exception message is logged if debugging is enabled.
        /// </remarks>
        /// <exception cref="Exception">
        /// This method may throw exceptions if there are issues reading the JSON file or updating the CSS paths. Specific exceptions are not caught at this level but are logged if debugging is enabled.
        /// </exception>
        public static async Task UpdateXhtmls(string ebookFolderPath, bool debug = false)
        {

            string jsonDataFilePath = FileManagement.GetEbookDataJsonFile(ebookFolderPath);
            var ebook = JsonHandler.ReadEbookJsonFile(jsonDataFilePath);


            foreach (KeyValuePair<string, List<string>> kvp in ebook.NavData)
            {
                try
                {
                    await UpdateCssPath(kvp.Value[0], FileManagement.GetEbookViewerStyleFilePath());
                    if (debug) { Debug.WriteLine($"UpdateXhtmls() - Success"); }
                }
                catch (Exception e)
                {
                    if (debug) { Debug.WriteLine($"UpdateXhtmls() - Fail - {e.Message}"); }
                }
            }
        }

        /// <summary>
        /// Asynchronously updates the CSS paths in all XHTML files within the specified eBook folder and its subdirectories.
        /// </summary>
        /// <param name="ebookFolderPath">
        /// The path to the folder containing the eBook files. This is used to locate all XHTML files within this directory and its subdirectories.
        /// </param>
        /// <remarks>
        /// This method searches for all `.xhtml` files in the given eBook folder and its subdirectories. For each XHTML file found, it updates the CSS path to point to the appropriate CSS file. 
        /// After processing all XHTML files, it logs a success message. If an exception occurs during the process, the exception message is logged, and the exception is rethrown.
        /// </remarks>
        /// <exception cref="Exception">
        /// This method may throw exceptions if there are issues accessing files or updating CSS paths. The exception is logged and then rethrown for further handling.
        /// </exception>
        public static async Task UpdateXhtmlsRecursive(string ebookFolderPath)
        {
            try
            {
                // Get all .xhtml files in the directory and subdirectories
                string[] xhtmlFiles = Directory.GetFiles(ebookFolderPath, "*.xhtml", SearchOption.AllDirectories);

                // Path to the CSS file
                string cssFilePath = FileManagement.GetEbookViewerStyleFilePath();

                // Update CSS path for each .xhtml file
                foreach (string xhtmlFile in xhtmlFiles)
                {
                    await UpdateCssPath(xhtmlFile, cssFilePath);
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
        /// Asynchronously injects the global CSS into all XHTML files of all eBooks.
        /// </summary>
        /// <param name="debug">
        /// Optional parameter that, when set to <c>true</c>, enables detailed debugging output. The default value is <c>false</c>.
        /// </param>
        /// <remarks>
        /// This method retrieves a list of all eBook directories and updates the CSS paths in the XHTML files for each eBook. 
        /// If debugging is enabled, it logs success or failure messages based on the outcome of the operation. If an exception occurs, it is caught and logged, and the process continues for the remaining eBooks.
        /// </remarks>
        /// <exception cref="Exception">
        /// This method may throw exceptions if errors occur during the CSS injection process. The exception is caught and logged if debugging is enabled, but may be rethrown or handled further up the call stack.
        /// </exception>
        public static async Task GlobalCssInjector(bool debug = false)
        {
            try
            {
                foreach (string ebookFolderPath in GetListOfAllEbooks())
                {
                    await UpdateXhtmls(ebookFolderPath);
                }
                if (debug) { Debug.WriteLine($"GlobalCssInjector() - Success"); }
            }
            catch (Exception e)
            {
                if (debug) { Debug.WriteLine($"GlobalCssInjector() - Fail - {e.Message}"); }
            }
        }

        /// <summary>
        /// Asynchronously updates the CSS path in an XHTML file and injects a script element into the document.
        /// </summary>
        /// <param name="xhtmlPath">
        /// Optional parameter that, when set to <c>true</c>, enables detailed debugging output. The default value is <c>false</c>.
        /// </param>
        /// <param name="newCssPath">
        /// The new CSS file path to be set in the <c>href</c> attributes of <c>&lt;link&gt;</c> elements within the XHTML document.
        /// </param>
        /// <param name="debug">
        /// 
        /// </param>
        /// <remarks>
        /// This method attempts to update the CSS path in all <c>&lt;link&gt;</c> elements that reference a CSS file and injects a script element into the <c>&lt;body&gt;</c> section of the XHTML document. 
        /// The operation is retried up to a specified number of times if an <see cref="IOException"/> occurs, with a delay between retries. 
        /// If the script injection or CSS path update fails, the exception is logged, and the method will retry the operation as needed.
        /// </remarks>
        /// <exception cref="IOException">
        /// This method may throw an <see cref="IOException"/> if there are issues accessing or modifying the XHTML file. If the maximum number of retries is exceeded, the exception is rethrown.
        /// </exception>
        /// <exception cref="Exception">
        /// Other exceptions may be thrown if there are issues parsing or saving the XHTML document. These exceptions are logged and rethrown.
        /// </exception>
        public static async Task UpdateCssPath(string xhtmlPath, string newCssPath, bool debug = false)
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

                        if (debug) { Debug.WriteLine("jsInjection() - Success"); }
                    }

                    catch (Exception ex)
                    {
if (debug) { Debug.WriteLine($"jsInjection() - Fail - {ex.Message}"); }
                    }

                    // Query all <link> elements in the document
                    var linkElements = xhtmlDocument.Descendants()
                        .Where(e => e.Name.LocalName == "link" && e.Attribute("href") != null);

                    // Update the href attribute for those <link> elements that end with .css
                    foreach (var link in linkElements)
                    {
                        string hrefValue = link.Attribute("href")?.Value;
                        if (hrefValue != null && hrefValue.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                        {
                            link.SetAttributeValue("href", newCssPath);
                        }
                    }
                    xhtmlDocument.Save(xhtmlPath);

                    if (debug) { Debug.WriteLine($"UpdateCssPath() - Success"); }
                    return; // Exit the method if successful
                }
                catch (IOException ex) when (ex is IOException)
                {
                    if (debug) { Debug.WriteLine($"UpdateCssPath() {attempt + 1} - Fail - {ex.Message}"); }
                    if (attempt == maxRetries - 1)
                    {
                        throw; // Re-throw the exception if the last attempt fails
                    }
                    await Task.Delay(delayMilliseconds); // Wait before retrying
                }
                catch (Exception ex)
                {
                    if (debug) { Debug.WriteLine($"UpdateCssPath() - Fail - {ex.Message}");};
                    throw;
                }
            }
        }

        /// <summary>
        /// Retrieves a list of all eBook directories within the eBooks folder.
        /// </summary>
        /// <returns>
        /// A <see cref="List{T}"/> of <see cref="string"/> representing the paths of all immediate subdirectories (each directory corresponding to an eBook) in the eBooks folder.
        /// </returns>
        /// <remarks>
        /// This method calls <see cref="FileManagement.GetEbooksFolderPath"/> to obtain the path of the eBooks folder, then retrieves the immediate subdirectories of that path.
        /// Each subdirectory represents an eBook, and the method returns a list of these directories.
        /// </remarks>
        public static List<string> GetListOfAllEbooks()
        {
            return GetImmediateSubdirectories(FileManagement.GetEbooksFolderPath());
        }

        /// <summary>
        /// Retrieves a list of immediate subdirectories for the specified root path.
        /// </summary>
        /// <param name="rootPath">
        /// The root directory path from which to retrieve subdirectories.
        /// </param>
        /// <param name="debug">
        /// Optional parameter that, when set to <c>true</c>, enables detailed debugging output. The default value is <c>false</c>.
        /// </param>
        /// <returns>
        /// A <see cref="List{T}"/> of <see cref="string"/> representing the paths of the immediate subdirectories of the specified root directory.
        /// </returns>
        /// <remarks>
        /// This method attempts to get all immediate subdirectories from the provided <paramref name="rootPath"/>. If the operation is successful, it returns a list of these subdirectories. 
        /// If an exception occurs, such as unauthorized access, the exception message is logged if debugging is enabled.
        /// </remarks>
        private static List<string> GetImmediateSubdirectories(string rootPath, bool debug = false)
        {
            List<string> subdirectories = new List<string>();

            try
            {
                string[] subdirs = Directory.GetDirectories(rootPath);
                subdirectories.AddRange(subdirs);
                if (debug) { Debug.WriteLine($"GetImmediateSubdirectories() - Success"); }
            }
            catch (UnauthorizedAccessException e)
            {
                if (debug) { Debug.WriteLine($"GetImmediateSubdirectories() - Fail - UnauthorizedAccessException - {e.Message}"); }
            }
            catch (Exception e)
            {
                if (debug) { Debug.WriteLine($"GetImmediateSubdirectories() - Fail - {e.Message}"); }
            }

            return subdirectories;
        }

    }
}
