using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using EpubReader.code;
using static EpubReader.code.FileManagment;
using System.Text.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Dictionary : Page
    {
        double actualWidth;
        double actualHeight;
        private ObservableCollection<Translation> Translations = new ObservableCollection<Translation>();

        public Dictionary()
        {
            this.InitializeComponent();
            MyMainWindow.WindowResized += OnSizeChanged; // Subscribe to the event

            LoadItems();
        }

        private void OnSizeChanged(object sender, (double width, double height) tp)
        {
            actualWidth = tp.width;
            actualHeight = tp.height;

            TransaltionsListView.Width = actualWidth;
            TransaltionsListView.Height = actualHeight - 40;
        }

        private void SetDimensions()
        {

        }

        private void LoadItems()
        {
            string dictPath = FileManagment.GetGlobalDictPath();
            globalDictJson globalDict = JsonSerializer.Deserialize<globalDictJson>(File.ReadAllText(dictPath));
            foreach (var kvp in globalDict.dict)
            {
                Translations.Add(new Translation
                {
                    OriginalText = kvp.Key,
                    TranslatedText = kvp.Value[2],
                    SourceLanguage = kvp.Value[0],
                    TargetLanguage = kvp.Value[1]
                }); 

                Debug.WriteLine(kvp.Key + " " + kvp.Value[0] + " " + kvp.Value[1] + " " + kvp.Value[2]);
            }

        }

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

        // Helper method to find a child element by name
        private T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
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


    public class Translation
    {
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
    }
}
