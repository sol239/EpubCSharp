using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using EpubReader.app_pages;
using EpubReader.code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{

    /// <summary>
    /// Represents global settings configuration loaded from a JSON file.
    /// </summary>
    public class GlobalSettingsJson
    {
        /// <summary>
        /// Gets or sets the Font used in the application.
        /// </summary>
        /// <value>
        /// A string representing the Font family or typeface.
        /// </value>
        public string Font { get; set; }

        /// <summary>
        /// Gets or sets the background color of the application.
        /// </summary>
        /// <value>
        /// A string representing the background color, typically in a hexadecimal color code format (e.g., "#FFFFFF").
        /// </value>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the name or path of the ebook viewer application or component.
        /// </summary>
        /// <value>
        /// A string representing the ebook viewer configuration or executable path.
        /// </value>
        public string EbookViewer { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the translation service used in the application.
        /// </summary>
        /// <value>
        /// A string representing the translation service configuration or API key.
        /// </value>
        public string TranslationService { get; set; }

        /// <summary>
        /// Gets or sets the path to the Python executable used for scripting or other purposes.
        /// </summary>
        /// <value>
        /// A string representing the file system path to the Python executable.
        /// </value>
        public string PythonPath { get; set; }

        /// <summary>
        /// Gets or sets the Language preference for the application.
        /// </summary>
        /// <value>
        /// A string representing the Language code or name (e.g., "en-US" for English or "fr-FR" for French).
        /// </value>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the theme configuration for the application.
        /// </summary>
        /// <value>
        /// A string representing the theme name or identifier (e.g., "dark", "light").
        /// </value>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the padding configuration for the application interface.
        /// </summary>
        /// <value>
        /// A string representing the padding values, which may be in a specific format or unit (e.g., "10px", "1em").
        /// </value>
        public string Padding { get; set; }

        /// <summary>
        /// Gets or sets the Font size configuration for the application.
        /// </summary>
        /// <value>
        /// A string representing the Font size (e.g., "12pt", "14px").
        /// </value>
        public string FontSize { get; set; }
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Microsoft.UI.Xaml.Controls.Page
    {
        private readonly int _confirmStartup = 2;

        private Dictionary<string, string> _languageDict = new Dictionary<string, string>()
        {

        };

        private double _actualWidth;
        private double _actualHeight;

        private Windows.UI.Color _colorSelected;

        private int _startUp = 0;

        /// <summary>
        /// A list of font families suitable for book reading.
        /// </summary>
        /// <value>
        /// A <see cref="List{String}"/> containing font names commonly used for reading, including both serif and sans-serif options.
        /// </value>
        public static List<string> BookReadingFonts = new List<string>
        {
            "Georgia",
            "Times New Roman",
            "Garamond",
            "Arial",
            "Merriweather",
            "Palatino Linotype",
            "Book Antiqua",
            "Cambria",
            "Serif",
            "Baskerville",
            "Caslon",
            "Adobe Caslon Pro",
            "Charter",
            "Minion Pro",
            "Sabon",
            "Janson Text",
            "Hoefler Text",
            "Tisa",
            "Verdana",
            "Century Schoolbook",
            "Bookman Old Style",
            "Utopia",
            "Libre Baskerville",
            "Custom +"
        };

        /// <summary>
        /// A list of background color options for book reading.
        /// </summary>
        /// <value>
        /// A <see cref="List{String}"/> containing color codes in hexadecimal format and a custom option for background colors.
        /// </value>
        public static List<string> BookBackgroundColor = new List<string>
        {
            "#efe0cd",
            "#EFE0CD",
            "#E4D8CD",
            "#E2D3C4",
            "#D4C2AF",
            "#FFFFFF",
            "Custom +"


        };

        /// <summary>
        /// A list of ebook viewer options.
        /// </summary>
        /// <value>
        /// A <see cref="List{String}"/> containing names of ebook viewer technologies or components.
        /// </value>
        public static List<string> BookViewer = new List<string>
    {
        "WebView2",
        "epubjs"
    };

        /// <summary>
        /// A list of available translation services.
        /// </summary>
        /// <value>
        /// A <see cref="List{String}"/> containing names of translation services or tools.
        /// </value>
        public static List<string> TsServices = new List<string>
    {
        "argos",
        "My Memory",
        "dictionary"
    };

        /// <summary>
        /// A dictionary of theme configurations.
        /// </summary>
        /// <value>
        /// A dictionary where each key is a theme name and each value is a dictionary
        /// of CSS-like properties and their corresponding color values.
        /// </value>
        public static Dictionary<string, Dictionary<string, string>> Themes = new Dictionary<string, Dictionary<string, string>>()
    {
        // Theme configuration for "Pure White"
        {
            "Pure White", new Dictionary<string, string>()
            {
                { "background-color", "#FFFFFF" },
                { "button-color", "#000000" },
                { "header-color", "#f5f5f5" },
                { "text-color", "#000000" }
            }
        },

        // Theme configuration for "Dark Blue"
        {
            "Dark Blue", new Dictionary<string, string>()
            {
                { "background-color", "#232c40" },
                { "button-color", "#e6ddc9" },
                { "header-color", "#192236" },
                { "text-color", "#e6ddc9" }
            }
        },

        // Theme configuration for "Woodlawn"
        {
            "Woodlawn", new Dictionary<string, string>()
            {
                { "background-color", "#e6ddc9" },
                { "button-color", "#000000" },
                { "header-color", "#bcb4a4" },
                { "text-color", "#000000" }
            }
        },
    };

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor performs the following initialization tasks:
        /// <list type="bullet">
        ///   <item>
        ///     Calls <see cref="InitializeComponent"/> to initialize the user interface components defined in XAML.
        ///   </item>
        ///   <item>
        ///     Subscribes to the <see cref="MyMainWindow.WindowResized"/> event of <see cref="MyMainWindow"/> to handle window size changes via the <see cref="OnSizeChanged"/> method.
        ///   </item>
        ///   <item>
        ///     Subscribes to the <see cref="SettingsPage.Unloaded"/> event of the page to handle cleanup via the <see cref="OnHomePageUnloaded"/> method.
        ///   </item>
        ///   <item>
        ///     Calls <see cref="LoadLangDict"/> to load the language dictionary necessary for localization.
        ///   </item>
        ///   <item>
        ///     Calls <see cref="ComboBoxesSetup"/> to set up and configure any combo boxes present on the page.
        ///   </item>
        ///   <item>
        ///     Calls <see cref="PageStartup"/> to perform any additional startup operations required for the page.
        ///   </item>
        /// </list>
        /// </remarks>
        public SettingsPage()
        {
            this.InitializeComponent();
            MyMainWindow.WindowResized += OnSizeChanged; 
            this.Unloaded += OnHomePageUnloaded;

            LoadLangDict();
            ComboBoxesSetup();
            PageStartup();
        }

        /// <summary>
        /// Initializes the page with settings loaded from a JSON configuration file.
        /// </summary>
        /// <remarks>
        /// This method performs the following:
        /// <list type="bullet">
        ///   <item>Loads global settings from a JSON file using <see cref="FileManagement.GetGlobalSettingsFilePath"/>.</item>
        ///   <item>Sets the selected indices of various combo boxes based on the loaded settings.</item>
        ///   <item>Updates text fields for Python path, padding, and font size.</item>
        /// </list>
        /// </remarks>
        public void PageStartup()
        {
            GlobalSettingsJson globalSettings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
            FontsComboBox.SelectedIndex = BookReadingFonts.IndexOf(globalSettings.Font);
            EbookViewerComboBox.SelectedIndex = BookViewer.IndexOf(globalSettings.EbookViewer);
            TranslationComboBox.SelectedIndex = TsServices.IndexOf(globalSettings.TranslationService);
            LanguageComboBox.SelectedIndex = _languageDict.Keys.ToList().IndexOf(globalSettings.Language);
            ThemesComboBox.SelectedIndex = Themes.Keys.ToList().IndexOf(globalSettings.Theme);
            PythonPathBox.Text = globalSettings.PythonPath;
            PaddingBox.Text = globalSettings.Padding;

            if (double.TryParse(globalSettings.FontSize.Split("rem")[0], out double fontSizeValue))
            {
                int fontSizeValueInt = (int)(fontSizeValue * 10);
                if (fontSizeValueInt > 0)
                {
                    FontSizeBox.Text = fontSizeValueInt.ToString();
                }
            }

            else
            {
                FontSizeBox.Text = "Provide valid fontsize = whole numbers > 0";
            }



        }

        /// <summary>
        /// Populates the combo boxes on the settings page with predefined options.
        /// </summary>
        /// <remarks>
        /// This method adds items to the following combo boxes:
        /// <list type="bullet">
        ///   <item><see cref="FontsComboBox"/> with font names from <see cref="BookReadingFonts"/>.</item>
        ///   <item><see cref="EbookViewerComboBox"/> with ebook viewer options from <see cref="BookViewer"/>.</item>
        ///   <item><see cref="TranslationComboBox"/> with translation services from <see cref="TsServices"/>.</item>
        ///   <item><see cref="LanguageComboBox"/> with language codes from <see cref="_languageDict"/>.</item>
        ///   <item><see cref="ThemesComboBox"/> with theme names from <see cref="Themes"/>.</item>
        /// </list>
        /// <para>
        /// The <see cref="BookBackgroundColor"/> is currently not being populated.
        /// </para>
        /// </remarks>
        private void ComboBoxesSetup()
        {
            foreach (var font in BookReadingFonts)
            {
                FontsComboBox.Items.Add(font);
            }

            /*foreach (var color in BookBackgroundColor)
            {
                backgroundcolorComboBox.Items.Add(color);
            }*/

            foreach (var viewer in BookViewer)
            {
                EbookViewerComboBox.Items.Add(viewer);
            }

            foreach (var service in TsServices)
            {
                TranslationComboBox.Items.Add(service);
            }

            foreach (var language in _languageDict.Keys.ToList())
            {
                LanguageComboBox.Items.Add(language);
            }

            foreach (var theme in Themes.Keys.ToList())
            {
                ThemesComboBox.Items.Add(theme);
            }

        }

        /// <summary>
        /// Updates the font family for the body element in the CSS file.
        /// </summary>
        /// <param name="newFontFamily">The new font family to apply to the body element.</param>
        /// <param name="debug"> A boolean indicating whether to output debug information. If true, debug messages will be logged. </param>
        /// <remarks>
        /// This method modifies the CSS file by replacing or adding the font-family declaration within the body tag.
        /// If no Font-family is found, it is added to the body declaration.
        /// </remarks>
        public static void UpdateBodyFontFamily(string newFontFamily, bool debug = false)
        {

            try
            {
                string cssFilePath = FileManagement.GetEbookViewerStyleFilePath();

                // Read the existing CSS file
                string cssContent = File.ReadAllText(cssFilePath);

                // Regular expression to find the body Font-family declaration
                string pattern = @"(?<=body\s*{[^}]*?Font-family:\s*).*?(?=;)";
                string replacement = newFontFamily;

                // Replace the existing Font-family for body with the new one
                string modifiedCssContent = Regex.Replace(cssContent, pattern, replacement, RegexOptions.Singleline);

                // If no Font-family was found, add it
                if (!Regex.IsMatch(cssContent, @"body\s*{[^}]*?Font-family:"))
                {
                    modifiedCssContent = Regex.Replace(modifiedCssContent, @"body\s*{", $"body {{\n    Font-family: {newFontFamily};\n", RegexOptions.Singleline);
                }

                // Write the modified content back to the CSS file
                File.WriteAllText(cssFilePath, modifiedCssContent);

                if (debug) {Debug.WriteLine($"UpdateBodyFontFamily() - Success - {newFontFamily}");}
            }
            catch (Exception ex)
            {
if (debug) {Debug.WriteLine($"UpdateBodyFontFamily() - Fail - {ex.Message}");}
            }
        }

        /// <summary>
        /// Updates the background color for the body element in the CSS file.
        /// </summary>
        /// <param name="color">The new background color to be applied.</param>
        /// <param name="debug"> A boolean indicating whether to output debug information. If true, debug messages will be logged. </param>

        public static void UpdateBodyBackgroundColor(string color, bool debug = false)
        {
            try
            {
                string cssFilePath = FileManagement.GetEbookViewerStyleFilePath();

                // Read the existing CSS file
                string cssContent = File.ReadAllText(cssFilePath);

                // Regular expression to find the body background-color declaration
                string backgroundColorPattern = @"(?<=body\s*{[^}]*?background-color:\s*)([^;]*)(?=;)";

                // Replace the existing background color for body with the new one
                string modifiedCssContent = Regex.Replace(cssContent, backgroundColorPattern, color, RegexOptions.Singleline);

                // If no background-color was found, add it
                if (!Regex.IsMatch(cssContent, @"body\s*{[^}]*?background-color:"))
                {
                    modifiedCssContent = Regex.Replace(modifiedCssContent, @"body\s*{", $"body {{\n    background-color: {color};\n", RegexOptions.Singleline);
                }

                // Write the modified content back to the CSS file
                File.WriteAllText(cssFilePath, modifiedCssContent);
                if (debug) { Debug.WriteLine($"UpdateBodyBackgroundColor() - Success - {color}"); }


            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"UpdateBodyBackgroundColor() - Fail - {ex.Message}"); }
            }
        }

        /// <summary>
        /// Updates the text color for the body element in the CSS file.
        /// </summary>
        /// <param name="color">The new text color to be applied.</param>
        /// <param name="debug"> A boolean indicating whether to output debug information. If true, debug messages will be logged. </param>
        
        public static void UpdateBodyTextColor(string color, bool debug = false)
        {

            try
            {
                string cssFilePath = FileManagement.GetEbookViewerStyleFilePath();

                // Read the existing CSS file
                string cssContent = File.ReadAllText(cssFilePath);

                // Regular expression to find the body background-color declaration
                string backgroundColorPattern = @"(?<=body\s*{[^}]*?color:\s*)([^;]*)(?=;)";

                // Replace the existing background color for body with the new one
                string modifiedCssContent = Regex.Replace(cssContent, backgroundColorPattern, color, RegexOptions.Singleline);

                // If no background-color was found, add it
                if (!Regex.IsMatch(cssContent, @"body\s*{[^}]*?color:"))
                {
                    modifiedCssContent = Regex.Replace(modifiedCssContent, @"body\s*{", $"body {{\n    color: {color};\n", RegexOptions.Singleline);
                }

                // Write the modified content back to the CSS file
                File.WriteAllText(cssFilePath, modifiedCssContent);

                if (debug) { Debug.WriteLine($"UpdateBodyTextColor() - Success - {color}"); }


            }
            catch (Exception ex)
            {
                if (debug) { Debug.WriteLine($"UpdateBodyTextColor() - Fail - {ex.Message}"); }
            }
        }

        /// <summary>
        /// Updates the font size for the body element in the CSS file.
        /// </summary>
        /// <param name="fontSize">The new font size to be applied, including units (e.g., "16px").</param>
/// <param name="debug"> A boolean indicating whether to output debug information. If true, debug messages will be logged. </param>

        public static void UpdateBodyFontSize(string fontSize, bool debug = false)
        {

            try
            {
                string cssFilePath = FileManagement.GetEbookViewerStyleFilePath();

                // Read the existing CSS file
                string cssContent = File.ReadAllText(cssFilePath);

                // Regular expression to find the body background-color declaration
                string backgroundColorPattern = @"(?<=body\s*{[^}]*?Font-size:\s*)([^;]*)(?=;)";

                // Replace the existing background color for body with the new one
                string modifiedCssContent = Regex.Replace(cssContent, backgroundColorPattern, fontSize, RegexOptions.Singleline);

                // If no background-color was found, add it
                if (!Regex.IsMatch(cssContent, @"body\s*{[^}]*?Font-size:"))
                {
                    modifiedCssContent = Regex.Replace(modifiedCssContent, @"body\s*{", $"body {{\n    Font-size: {fontSize};\n", RegexOptions.Singleline);
                }

                // Write the modified content back to the CSS file
                File.WriteAllText(cssFilePath, modifiedCssContent);

if (debug) {Debug.WriteLine($"UpdateBodyFontSize() - Success - {fontSize}");}
            }
            catch (Exception ex)
            {
                if (debug) {Debug.WriteLine($"UpdateBodyFontSize() - Fail - {ex.Message}");}
            }
            
        }

        /// <summary>
        /// Handles the event when the selected item in the FontsComboBox changes.
        /// </summary>
        /// <param name="sender">The source of the event, typically the ComboBox.</param>
        /// <param name="e">Event data that contains information about the selection change.</param>
        private void FontsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (_startUp >= _confirmStartup)
            {
                string newFontFamily = BookReadingFonts[FontsComboBox.SelectedIndex];

                // store to json
                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                settings.Font = newFontFamily;
                File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));


                UpdateBodyFontFamily(newFontFamily);
            }
            else
            {
                _startUp++;
            }
        }

        /// <summary>
        /// Converts an 8-digit hexadecimal color code (with alpha) to a 6-digit hexadecimal color code (without alpha).
        /// </summary>
        /// <param name="hexColor">The 8-digit hex color string to convert. Can be in the format "#AARRGGBB" or "AARRGGBB".</param>
        /// <returns>A 6-digit hex color string in the format "#RRGGBB".</returns>
        /// <exception cref="ArgumentException">Thrown if the input is not in a valid 8-digit hex color format.</exception>
        private static string Convert8DigitHexTo6Digit(string hexColor)
        {
            // Ensure the input is in the correct 8-digit hex format
            if (hexColor.Length == 9 && hexColor.StartsWith("#"))
            {
                // Extract the RRGGBB part
                string sixDigitHex = hexColor.Substring(3);
                return "#" + sixDigitHex;
            }
            else if (hexColor.Length == 8 && !hexColor.StartsWith("#"))
            {
                // For cases without the leading '#'
                string sixDigitHex = hexColor.Substring(2);
                return "#" + sixDigitHex;
            }
            else
            {
                throw new ArgumentException("Invalid 8-digit hex color format");
            }
        }

        /// <summary>
        /// Handles the event triggered when the selected item in the BackgroundcolorComboBox changes.
        /// Updates the background color in the global settings and applies the change to the CSS file.
        /// </summary>
        /// <param name="sender">The source of the event, typically the ComboBox.</param>
        /// <param name="e">Event data that contains information about the selection change.</param>
        private void BackgroundcolorComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_startUp >= _confirmStartup)
            {
                string color = "#000000";

                if (color == "Custom +")
                {
                    color = Convert8DigitHexTo6Digit(_colorSelected.ToString());
                }

                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                settings.BackgroundColor = color;
                File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                UpdateBodyBackgroundColor(color);
            }

            else
            {
                _startUp++;
            }
        }

        /// <summary>
        /// Loads the current font family from the CSS file used in the ebook viewer.
        /// If a font family is found within the CSS file, it is returned; otherwise, a default value ("Merriweather") is returned.
        /// </summary>
        /// <returns>
        /// The font family string found in the CSS file, or "Merriweather" if no match is found or the file does not exist.
        /// </returns>
        public static string LoadFontComboBox()
        {
            if (File.Exists(FileManagement.GetEbookViewerStyleFilePath()))
            {
                string cssContent = File.ReadAllText(FileManagement.GetEbookViewerStyleFilePath());
                string pattern = @"(?<=body\s*{[^}]*?Font-family:\s*).*?(?=;)";
                Match match = Regex.Match(cssContent, pattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }

            return "Merriweather";
        }

        /// <summary>
        /// Loads the current background color from the CSS file used in the ebook viewer.
        /// If a background color is found in the CSS file, it is returned; otherwise, a default value ("#efe0cd") is returned.
        /// </summary>
        /// <returns>
        /// The background color string found in the CSS file, or "#efe0cd" if no background color is found or the file does not exist.
        /// </returns>
        public static string LoadBackgroundColorComboBox()
        {
            if (File.Exists(FileManagement.GetEbookViewerStyleFilePath()))
            {
                string cssContent = File.ReadAllText(FileManagement.GetEbookViewerStyleFilePath());
                string pattern = @"(?<=body\s*{[^}]*?background-color:\s*).*?(?=;)";
                Match match = Regex.Match(cssContent, pattern);
                if (match.Success)
                {
                    Debug.WriteLine($"\nColor changed to {match.Value}");
                    return match.Value.Trim();
                }
            }
            Debug.WriteLine($"Color not changed!");
            return "#efe0cd";
        }

        /// <summary>
        /// Handles the selection change event for the ebook viewer combo box.
        /// Updates the global settings with the selected ebook viewer and saves the changes to the settings file.
        /// </summary>
        /// <param name="sender">The source of the event, typically the combo box control.</param>
        /// <param name="e">The event data associated with the selection change event.</param>
        private void EbookViewerComboBoxComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedViewer = BookViewer[EbookViewerComboBox.SelectedIndex];
            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
            settings.EbookViewer = selectedViewer;
            File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
        }

        /// <summary>
        /// Handles the selection change event for the translation service combo box.
        /// Updates the global settings with the selected translation service and saves the changes to the settings file.
        /// </summary>
        /// <param name="sender">The source of the event, typically the combo box control.</param>
        /// <param name="e">The event data associated with the selection change event.</param>
        private void TranslationComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string selectedTranslService = TsServices[TranslationComboBox.SelectedIndex];

            Debug.WriteLine(selectedTranslService);
            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
            settings.TranslationService = selectedTranslService;
            File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));

        }

        /// <summary>
        /// Handles the selection change event for the language combo box.
        /// Updates the global settings with the selected language and saves the changes to the settings file.
        /// </summary>
        /// <param name="sender">The source of the event, typically the combo box control.</param>
        /// <param name="e">The event data associated with the selection change event.</param>
        private void LanguageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string selectedLang = _languageDict.Keys.ToList()[LanguageComboBox.SelectedIndex];

            GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
            settings.Language = selectedLang;
            File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));

        }

        /// <summary>
        /// Handles the click event for the Python path button.
        /// Validates the Python path provided in the TextBox, updates the global settings if the path is valid,
        /// and provides visual feedback based on the validity of the path.
        /// </summary>
        /// <param name="sender">The source of the event, typically the button control.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void PythonPath_Click(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox
            string pythonPath = PythonPathBox.Text;

            // Perform your action with the message here
            // For example, display it in a message box
            if (!string.IsNullOrWhiteSpace(pythonPath))
            {

                // if path is valid 
                if (File.Exists(pythonPath))
                {
                    GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                    settings.PythonPath = pythonPath;
                    File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                    PythonPathBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#c9ffad"));
                }
                else
                {
                    PythonPathBox.Text = "Invalid path...";
                    PythonPathBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#f2aeb4"));
                }
            }
        }

        /// <summary>
        /// Handles the selection change event for the themes combo box.
        /// Updates the global settings with the selected theme and applies the corresponding text and background colors to the body.
        /// </summary>
        /// <param name="sender">The source of the event, typically the themes combo box control.</param>
        /// <param name="e">The event data associated with the selection change event.</param>
        private void ThemesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_startUp >= _confirmStartup)
            {
                string theme = Themes.Keys.ToList()[ThemesComboBox.SelectedIndex];
                GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                settings.Theme = theme;
                File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                UpdateBodyTextColor(Themes[theme]["text-color"]);
UpdateBodyBackgroundColor(Themes[theme]["background-color"]);
            }

            else
            {
                _startUp++; }

        }

        /// <summary>
        /// Handles Unloaded event of the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHomePageUnloaded(object sender, RoutedEventArgs e)
        {
            MyMainWindow.WindowResized -= OnSizeChanged; // Unsubscribe from the event
        }

        /// <summary>
        /// Handles the WindowResized event of the MyMainWindow class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tp"></param>
        private void OnSizeChanged(object sender, (double width, double height) tp)
        {
            _actualWidth = tp.width;
            _actualHeight = tp.height;
        }

        /// <summary>
        /// Loads the language dictionary from a JSON file.
        /// </summary>
        private void LoadLangDict()
        {
            // Get the path to the application's installed location
            StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;

            // Define the relative path to the script file
            string relativePath = "app_pages\\iso639I_reduced.json";

            // Combine the installed location path with the relative path
            string path = Path.Combine(installedLocation.Path, relativePath);

            string json = File.ReadAllText(path);
            _languageDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        private void PaddingButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox
            string padding = PaddingBox.Text;

            // Try to convert the string to a double
            if (double.TryParse(padding, out double paddingValue))
            {
                // Perform your action with the message here
                // For example, display it in a message box
                if (!string.IsNullOrWhiteSpace(padding) && paddingValue > 0)
                {
                    GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                    settings.Padding = padding;
                    File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                    PaddingBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#c9ffad"));

                }
            }
            else
            {
                PaddingBox.Text = "Type a number bigger than 0...";
                PaddingBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#f2aeb4"));

            }



        }

        private void FontSizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox
            string fontSize = FontSizeBox.Text;

            // Try to convert the string to a double
            if (double.TryParse(fontSize, out double paddingValue))
            {
                // Perform your action with the message here
                // For example, display it in a message box
                if (!string.IsNullOrWhiteSpace(fontSize) && paddingValue > 0)
                {
                    GlobalSettingsJson settings = JsonSerializer.Deserialize<GlobalSettingsJson>(File.ReadAllText(FileManagement.GetGlobalSettingsFilePath()));
                    settings.FontSize = $"{(paddingValue / 10)}rem";
                    File.WriteAllText(FileManagement.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                    FontSizeBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#c9ffad"));
                    UpdateBodyFontSize(settings.FontSize);

                }
            }
            else
            {
                FontSizeBox.Text = "Type a number bigger than 0...";
                FontSizeBox.Background = new SolidColorBrush(EbookWindow.ParseHexColor("#f2aeb4"));

            }
        }
    }
}
