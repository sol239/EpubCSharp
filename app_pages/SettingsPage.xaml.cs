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
using CSharpMarkup.WinUI;
using System.Text.RegularExpressions;

using EpubReader.code;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Microsoft.UI.Xaml.Controls.Page
    {
        private List<string> _bookReadingFonts = new List<string>
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

        private List<string> _bookBackgroundColor = new List<string>
        {
            "#efe0cd",
            "#EFE0CD",
            "#E4D8CD", 
            "#E2D3C4", 
            "#D4C2AF",
            "#FFFFFF",
            "Custom +"


        };

        Windows.UI.Color colorSelected;

        public SettingsPage()
        {
            this.InitializeComponent();
            fontsComboBoxSetup();
            string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();
            PageStartup();



        }

        public async void PageStartup()
        {
            string _font = await LoadFontComboBox();
            string _color = await LoadBackgroundColorComboBox();
            fontsComboBox.SelectedIndex = _bookReadingFonts.IndexOf(_font);
            backgroundcolorComboBox.SelectedIndex = _bookBackgroundColor.IndexOf(_color);
        }

        private void ColorPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args)
        {
            colorSelected = args.NewColor;
            Debug.WriteLine($"\nColor selected is ___{colorSelected.ToString()}___");
        }

        private void fontsComboBoxSetup()
        {
            foreach (var font in _bookReadingFonts)
            {
                fontsComboBox.Items.Add(font);
            }

            foreach (var color in _bookBackgroundColor)
            {
                backgroundcolorComboBox.Items.Add(color);
            }
        }

       

        static async Task UpdateBodyFontFamily(string cssFilePath, string newFontFamily)
        {
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

        static async Task UpdateBodyBackgroundColor(string cssFilePath, string color)
        {
            Debug.WriteLine($"\nTryying to change backgournd color to {color}!\n");

            try
            {
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


        private async void fontsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();
            string newFontFamily = _bookReadingFonts[fontsComboBox.SelectedIndex];
            await UpdateBodyFontFamily(cssFilePath, newFontFamily);
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
            string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();
            string color = _bookBackgroundColor[backgroundcolorComboBox.SelectedIndex];
            
            if (color == "Custom +")
            {
                color = Convert8DigitHexTo6Digit(colorSelected.ToString());

            }
            
            await UpdateBodyBackgroundColor(cssFilePath, color);
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


    }
}
