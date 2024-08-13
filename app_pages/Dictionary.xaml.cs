using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using EpubReader.code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using static EpubReader.code.FileManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Dictionary : Page
    {
        private double _actualWidth;
        private double _actualHeight;
        private double _bottomOffset = 40;

        private readonly ObservableCollection<Translation> Translations = new ObservableCollection<Translation>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary"/> class.
        /// </summary>
        public Dictionary()
        {
            this.InitializeComponent();
            MyMainWindow.WindowResized += OnSizeChanged; // Subscribe to the event

            LoadItems();
        }

        /// <summary>
        /// Handles the size change event for the window or container. This method updates the width and height of
        /// the <see cref="TransaltionsListView"/> control based on the new dimensions of the window or container.
        /// </summary>
        /// <param name="sender">The source of the event, typically the window or container whose size has changed.</param>
        /// <param name="tp">A tuple containing the new width and height dimensions.</param>
        private void OnSizeChanged(object sender, (double width, double height) tp)
        {
            _actualWidth = tp.width;
            _actualHeight = tp.height;

            TransaltionsListView.Width = _actualWidth;
            TransaltionsListView.Height = _actualHeight - _bottomOffset;
        }

        /// <summary>
        /// Loads translation items into the <see cref="Translations"/> collection from a global dictionary JSON file.
        /// This method performs the following actions:
        /// <list type="number">
        /// <item>Retrieves the path to the global dictionary JSON file using the <see cref="FileManagement.GetGlobalDictPath"/> method.</item>
        /// <item>Reads the content of the JSON file and deserializes it into a <see cref="GlobalDictJson"/> object.</item>
        /// <item>Iterates through the dictionary entries in the <see cref="GlobalDictJson.TranslationsDict"/>.</item>
        /// <item>For each dictionary entry:
        /// <list type="bullet">
        /// <item>Creates a new <see cref="Translation"/> object.</item>
        /// <item>Sets the <see cref="Translation.OriginalText"/> property to the dictionary key.</item>
        /// <item>Sets the <see cref="Translation.TranslatedText"/> property to the third element of the dictionary value array (index 2).</item>
        /// <item>Sets the <see cref="Translation.SourceLanguage"/> property to the first element of the dictionary value array (index 0).</item>
        /// <item>Sets the <see cref="Translation.TargetLanguage"/> property to the second element of the dictionary value array (index 1).</item>
        /// </list>
        /// </item>
        /// <item>Adds each newly created <see cref="Translation"/> object to the <see cref="Translations"/> collection.</item>
        /// </list>
        /// This method populates the <see cref="Translations"/> collection with translation data from a JSON file.
        /// </summary>
        private void LoadItems()
        {
            string dictPath = FileManagement.GetGlobalDictPath();
            GlobalDictJson globalDict = JsonSerializer.Deserialize<GlobalDictJson>(File.ReadAllText(dictPath));
            foreach (var kvp in globalDict.TranslationsDict)
            {
                Translations.Add(new Translation
                {
                    OriginalText = kvp.Key,
                    TranslatedText = kvp.Value[2],
                    SourceLanguage = kvp.Value[0],
                    TargetLanguage = kvp.Value[1]
                });
            }

        }

        /// <summary>
        /// Handles the click event of an item in the <see cref="TransaltionsListView"/>.
        /// This method performs the following actions:
        /// <list type="number">
        /// <item>Cast the clicked item to the expected <see cref="Translation"/> type.</item>
        /// <item>If the cast is successful, find the corresponding <see cref="ListViewItem"/> container for the clicked item.</item>
        /// <item>Access the <see cref="TextBlock"/> controls within the container that display the original and translated text.</item>
        /// <item>Toggle the <see cref="TextWrapping"/> and <see cref="TextTrimming"/> properties of the <see cref="TextBlock"/> controls:
        /// <list type="bullet">
        /// <item>If the <see cref="TextWrapping"/> is set to <see cref="TextWrapping.Wrap"/>, change it to <see cref="TextWrapping.NoWrap"/> and set <see cref="TextTrimming"/> to <see cref="TextTrimming.CharacterEllipsis"/>.</item>
        /// <item>If the <see cref="TextWrapping"/> is set to <see cref="TextWrapping.NoWrap"/>, change it to <see cref="TextWrapping.Wrap"/> and set <see cref="TextTrimming"/> to <see cref="TextTrimming.None"/>.</item>
        /// </list>
        /// </item>
        /// </list>
        /// This method is used to toggle the text wrapping and trimming of the original and translated text blocks when an item is clicked.
        /// </summary>
        private void TransaltionsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Cast the clicked item to the expected type
            var clickedItem = e.ClickedItem as Translation;

            if (clickedItem != null)
            {
                // Find the clicked ListViewItem container
                var container = (sender as ListView).ContainerFromItem(clickedItem) as ListViewItem;

                // Access the TextBlocks within the container
                var originalTextBlock = FindChild<TextBlock>(container, "OriginalText");
                var translatedTextBlock = FindChild<TextBlock>(container, "TranslatedText");

                // Change TextWrapping to Clip
                if (originalTextBlock != null)
                {
                    if (originalTextBlock.TextWrapping == TextWrapping.Wrap)
                    {
                        originalTextBlock.TextWrapping = TextWrapping.NoWrap;
                        originalTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                    }
                    else if (originalTextBlock.TextWrapping == TextWrapping.NoWrap)
                    {
                        originalTextBlock.TextWrapping = TextWrapping.Wrap;
                        originalTextBlock.TextTrimming = TextTrimming.None;

                    }
                }

                if (translatedTextBlock != null)
                {
                    if (translatedTextBlock.TextWrapping == TextWrapping.Wrap)
                    {
                        translatedTextBlock.TextWrapping = TextWrapping.NoWrap;
                        translatedTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                    }
                    else if (translatedTextBlock.TextWrapping == TextWrapping.NoWrap)
                    {
                        translatedTextBlock.TextWrapping = TextWrapping.Wrap;
                        translatedTextBlock.TextTrimming = TextTrimming.None;

                    }
                }
            }
        }

        /// <summary>
        /// Recursively searches for a child element of a specified type and name within a visual tree.
        /// This method traverses the visual tree starting from the given parent element, looking for a child element
        /// that matches the specified name. If such an element is found, it is returned as the specified type.
        /// If no matching child is found, the method returns <c>null</c>.
        /// </summary>
        /// <param name="parent">The parent element from which to start the search. This should be a visual container
        /// that may contain child elements.</param>
        /// <param name="childName">The name of the child element to search for. This must match the <see cref="FrameworkElement.Name"/>
        /// property of the desired child element.</param>
        /// <typeparam name="T">The type of the child element to be returned. This should be a type that derives from
        /// <see cref="DependencyObject"/>.</typeparam>
        /// <returns>A child element of type <typeparamref name="T"/> if found, otherwise <c>null</c>.</returns>
        /// <remarks>
        /// This method uses the <see cref="VisualTreeHelper"/> class to traverse the visual tree. It checks each child
        /// of the given parent to see if it matches the specified name. If a match is found, it returns the child cast
        /// to the specified type. If the child is not of the expected type, or if the name does not match, the method
        /// continues searching recursively within each child element's subtree. The search terminates when all children
        /// have been checked or when a matching child is found.
        /// </remarks>
        private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    Debug.WriteLine("Found child");
                    return child as T;
                }

                var foundChild = FindChild<T>(child, childName);
                if (foundChild != null)
                {
                    Debug.WriteLine("Found child");
                    return foundChild;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Represents a translation entry containing details about a text translation.
    /// This class encapsulates the original text, its translation, and the languages involved.
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// Gets or sets the original text that needs to be translated.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the text in its original Language.
        /// </value>
        public string OriginalText { get; set; }

        /// <summary>
        /// Gets or sets the translated text.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the translated version of the original text.
        /// </value>
        public string TranslatedText { get; set; }

        /// <summary>
        /// Gets or sets the source Language code of the original text.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the Language code of the original text's Language (e.g., "en" for English).
        /// </value>
        public string SourceLanguage { get; set; }

        /// <summary>
        /// Gets or sets the target Language code of the translated text.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the Language code of the target Language (e.g., "fr" for French).
        /// </value>
        public string TargetLanguage { get; set; }
    }

}
