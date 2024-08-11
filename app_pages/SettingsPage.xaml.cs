using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Text.RegularExpressions;
using System.Text.Json;

using EpubReader.code;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml.Shapes;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{

    public class globalSettingsJson
    {
        public string font { get; set; }
        public string backgroundColor { get; set; }
        public string ebookViewer { get; set; }
        public string translationService { get; set; }

        public string pythonPath { get; set; }

        public string language { get; set; }

        public string Theme { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Microsoft.UI.Xaml.Controls.Page
    {
        private int confirmStartup = 3;
        
        public static List<string> _bookReadingFonts = new List<string>
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

        public static List<string> _bookBackgroundColor = new List<string>
        {
            "#efe0cd",
            "#EFE0CD",
            "#E4D8CD", 
            "#E2D3C4", 
            "#D4C2AF",
            "#FFFFFF",
            "Custom +"


        };

        public static List<string> _bookViewer = new List<string>
        {
            "WebView2",
            "epubjs"
        };

        public static List<string> tsServices = new List<string>
        {
            "argos",
            "My Memory",
            "dictionary"
        };

        public static Dictionary<string, Dictionary<string, string>> _themes = new Dictionary<string, Dictionary<string, string>>()
        {

            // Themes
            {
                "Pure White", new Dictionary<string, string>()
                {
                    { "background-color", "#FFFFFF" },
                    { "button-color", "#000000" },
                    { "header-color", "#f5f5f5" },
                    { "text-color", "#000000" }
                }
            },


            {
                "Dark Blue", new Dictionary<string, string>()
                {
                    { "background-color", "#232c40" },
                    { "button-color", "#e6ddc9" },
                    { "header-color", "#192236" },
                    { "text-color", "#e6ddc9" }
                }
            },

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

        private Dictionary<string, string> languageDict = new Dictionary<string, string>()
        {

        };

        private double actualWidth;
        private double actualHeight;

        Windows.UI.Color colorSelected;

        private int _startUp = 0;

        public SettingsPage()
        {
            this.InitializeComponent();
            MyMainWindow.WindowResized += OnSizeChanged; // Subscribe to the event
            this.Unloaded += OnHomePageUnloaded;

            LoadLangDict();
            comboBoxesSetup();
            string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();
            PageStartup();



        }
        private void OnHomePageUnloaded(object sender, RoutedEventArgs e)
        {
            MyMainWindow.WindowResized -= OnSizeChanged; // Unsubscribe from the event
        }

        private void OnSizeChanged(object sender, (double width, double height) tp)
        {
            actualWidth = tp.width;
            actualHeight = tp.height;

        }
        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            // Save theme choice to LocalSettings. 
            // ApplicationTheme enum values: 0 = Light, 1 = Dark
            ApplicationData.Current.LocalSettings.Values["themeSetting"] =
                ((ToggleSwitch)sender).IsOn ? 0 : 1;
        }

        private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            ((ToggleSwitch)sender).IsOn = App.Current.RequestedTheme == ApplicationTheme.Light;
        }


        private void LoadLangDict()
        {
            string path = "C:\\Users\\david_pmv0zjd\\source\\repos\\EpubReader\\app_pages\\iso639I_reduced.json";
            string json = File.ReadAllText(path);
            languageDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        public async void PageStartup()
        {
            //string _font = await LoadFontComboBox();
            //string _color = await LoadBackgroundColorComboBox();

            globalSettingsJson _globalSettings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));

            fontsComboBox.SelectedIndex = _bookReadingFonts.IndexOf(_globalSettings.font);
            backgroundcolorComboBox.SelectedIndex = _bookBackgroundColor.IndexOf(_globalSettings.backgroundColor);
            ebookViewerComboBox.SelectedIndex = _bookViewer.IndexOf(_globalSettings.ebookViewer);
            translationComboBox.SelectedIndex = tsServices.IndexOf(_globalSettings.translationService);
            languageComboBox.SelectedIndex = languageDict.Keys.ToList().IndexOf(_globalSettings.language);
            ThemesComboBox.SelectedIndex = _themes.Keys.ToList().IndexOf(_globalSettings.Theme);

            PythonPathBox.Text = _globalSettings.pythonPath;
            
        }

        private void ColorPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args)
        {
            colorSelected = args.NewColor;
            Debug.WriteLine($"\nColor selected is ___{colorSelected.ToString()}___");
        }

        private void comboBoxesSetup()
        {
            foreach (var font in _bookReadingFonts)
            {
                fontsComboBox.Items.Add(font);
            }

            foreach (var color in _bookBackgroundColor)
            {
                backgroundcolorComboBox.Items.Add(color);
            }

            foreach (var viewer in _bookViewer)
            {
                ebookViewerComboBox.Items.Add(viewer);
            }

            foreach (var service in tsServices)
            {
                translationComboBox.Items.Add(service);
            }

            foreach (var language in languageDict.Keys.ToList())
            {
                languageComboBox.Items.Add(language);
            }

            foreach (var theme in _themes.Keys.ToList())
            {
                ThemesComboBox.Items.Add(theme);
            }

        }

       

        public static async Task UpdateBodyFontFamily(string newFontFamily)
        {

            string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();

            // Read the existing CSS file
            string cssContent = File.ReadAllText(cssFilePath);

            // Regular expression to find the body font-family declaration
            string pattern = @"(?<=body\s*{[^}]*?font-family:\s*).*?(?=;)";
            string replacement = newFontFamily;

            // Replace the existing font-family for body with the new one
            string modifiedCssContent = Regex.Replace(cssContent, pattern, replacement, RegexOptions.Singleline);

            // If no font-family was found, add it
            if (!Regex.IsMatch(cssContent, @"body\s*{[^}]*?font-family:"))
            {
                modifiedCssContent = Regex.Replace(modifiedCssContent, @"body\s*{", $"body {{\n    font-family: {newFontFamily};\n", RegexOptions.Singleline);
            }

            // Write the modified content back to the CSS file
            File.WriteAllText(cssFilePath, modifiedCssContent);


            await Task.Run(() => app_controls.GlobalCssInjector());

            Debug.WriteLine($"\n{newFontFamily} updated successfully!\n");
        }

        public static async Task UpdateBodyBackgroundColor(string color)
        {
            Debug.WriteLine($"\nTryying to change backgournd color to {color}!\n");

            try
            {
                string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();

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
                    Debug.WriteLine($"\nModified content:\n{modifiedCssContent}\n");
                }

                // Write the modified content back to the CSS file
                File.WriteAllText(cssFilePath, modifiedCssContent);

                // Call the global CSS injector
                await app_controls.GlobalCssInjector();

                Debug.WriteLine($"\nBackground color updated to {color} successfully!\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating background color: {ex.Message}");
            }
        }

        public static async Task UpdateBodyTextColor(string color)
        {
            Debug.WriteLine($"\nTryying to change backgournd color to {color}!\n");

            try
            {
                string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();

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
                    Debug.WriteLine($"\nModified content:\n{modifiedCssContent}\n");
                }

                // Write the modified content back to the CSS file
                File.WriteAllText(cssFilePath, modifiedCssContent);

                // Call the global CSS injector
                await app_controls.GlobalCssInjector();

                Debug.WriteLine($"\nBackground color updated to {color} successfully!\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating background color: {ex.Message}");
            }
        }


        private async void fontsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (_startUp >= confirmStartup)
            {
                string newFontFamily = _bookReadingFonts[fontsComboBox.SelectedIndex];

                // store to json
                globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));
                settings.font = newFontFamily;
                File.WriteAllText(FileManagment.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));


                await UpdateBodyFontFamily(newFontFamily);
            }
            else
            {
                _startUp++;
            }
        }

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
        private async void backgroundcolorComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_startUp >= confirmStartup)
            {
                string color = _bookBackgroundColor[backgroundcolorComboBox.SelectedIndex];

                if (color == "Custom +")
                {
                    color = Convert8DigitHexTo6Digit(colorSelected.ToString());

                }

                // store to json
                globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));
                settings.backgroundColor = color;
                File.WriteAllText(FileManagment.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));

                await UpdateBodyBackgroundColor(color);
            }

            else
            {
                _startUp++;
            }
        }

        public async static Task<string> LoadFontComboBox()
        {
            if (File.Exists(FileManagment.GetEbookViewerStyleFilePath()))
            {
                string cssContent = File.ReadAllText(FileManagment.GetEbookViewerStyleFilePath());
                string pattern = @"(?<=body\s*{[^}]*?font-family:\s*).*?(?=;)";
                Match match = Regex.Match(cssContent, pattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }

            return "Verdana";
        }

        public async static Task<string> LoadBackgroundColorComboBox()
        {
            if (File.Exists(FileManagment.GetEbookViewerStyleFilePath()))
            {
                string cssContent = File.ReadAllText(FileManagment.GetEbookViewerStyleFilePath());
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

        private void ebookViewerComboBoxComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedViewer = _bookViewer[ebookViewerComboBox.SelectedIndex];
            globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));
            settings.ebookViewer = selectedViewer;
            File.WriteAllText(FileManagment.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
        }

        private void translationComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string selectedTranslService = tsServices[translationComboBox.SelectedIndex];

            Debug.WriteLine(selectedTranslService);
            globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));
            settings.translationService = selectedTranslService;
            File.WriteAllText(FileManagment.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));

        }

        private void languageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string selectedLang = languageDict.Keys.ToList()[languageComboBox.SelectedIndex];

            globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));
            settings.language = selectedLang;
            File.WriteAllText(FileManagment.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));

        }

        private void PythonPath_Click(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox
            string pythonPath = PythonPathBox.Text;

            // Perform your action with the message here
            // For example, display it in a message box
            if (!string.IsNullOrWhiteSpace(pythonPath))
            {
                globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));
                settings.pythonPath = pythonPath;
                File.WriteAllText(FileManagment.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
            }
        }

        private async void ThemesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_startUp >= confirmStartup)
            {
                string theme = _themes.Keys.ToList()[ThemesComboBox.SelectedIndex];
                globalSettingsJson settings = JsonSerializer.Deserialize<globalSettingsJson>(File.ReadAllText(FileManagment.GetGlobalSettingsFilePath()));
                settings.Theme = theme;
                File.WriteAllText(FileManagment.GetGlobalSettingsFilePath(), JsonSerializer.Serialize(settings));
                await UpdateBodyTextColor(_themes[theme]["text-color"]);
                await UpdateBodyBackgroundColor(_themes[theme]["background-color"]);
            }

            else
            {
                _startUp++; }

        }
    }
}
