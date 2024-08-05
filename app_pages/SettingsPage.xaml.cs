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
            "Libre Baskerville"
        };
        public SettingsPage()
        {
            this.InitializeComponent();
            fontsComboBoxSetup();
            string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();
        }

        private void ColorPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args)
        {
            // Update the Rectangle's fill color based on the selected color
            //colorDisplay.Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(args.NewColor);
        }

        private void fontsComboBoxSetup()
        {
            foreach (var font in _bookReadingFonts)
            {
                fontsComboBox.Items.Add(font);
            }
            
        }

        static async void UpdateBodyFontFamily(string cssFilePath, string newFontFamily)
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

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            string cssFilePath = FileManagment.GetEbookViewerStyleFilePath();
            string newFontFamily = _bookReadingFonts[fontsComboBox.SelectedIndex];
            UpdateBodyFontFamily(cssFilePath, newFontFamily);
        }
    }
}
