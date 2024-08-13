using System;
using EpubReader.code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EpubReader
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Static property to access the main window of the application.
        /// </summary>
        public static Window Window { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            object value = ApplicationData.Current.LocalSettings.Values["themeSetting"];

            if (value != null)
            {
                // Apply theme choice.
                App.Current.RequestedTheme = (ApplicationTheme)(int)value;
            }
        }

        /// <summary>
        /// Handles the application launch and initializes the main window. This method overrides the 
        /// <see cref="Microsoft.UI.Xaml.Application.OnLaunched"/> method to set up and configure the 
        /// application's main window after performing necessary startup operations, such as file management.
        /// </summary>
        /// <param name="args">Contains the event data for the launch event. This includes any arguments 
        /// that were passed to the application at startup.</param>
        /// <exception cref="Exception">Thrown if an exception occurs during the execution of 
        /// <see cref="FileManagement.StartUp"/>. The exception is logged and then re-thrown to halt the 
        /// application if necessary.</exception>
        /// <remarks>
        /// The method ensures that the file management startup procedure is completed before proceeding 
        /// to create and display the main window. If the file management fails, the exception is logged 
        /// for debugging purposes, and the application is halted by re-throwing the exception.
        /// 
        /// After successful file management, a new <see cref="Window"/> is created and configured with a 
        /// specified size and position. The <see cref="Frame"/> is then initialized and navigated to the 
        /// application's main page (<see cref="MyMainWindow"/>). Finally, the window is activated to display 
        /// the content to the user.
        /// </remarks>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                // Ensure FileManagement.StartUp() runs first
                await FileManagement.StartUp();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in FileManagement.StartUp: {ex.Message}");
                throw; // Re-throw the exception if you want to halt the application
            }

            Window = new Window();
            Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(100, 100, 1200, 1000));
            Frame rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;
            rootFrame.Navigate(typeof(MyMainWindow), args.Arguments);
            Window.Content = rootFrame;
            Window.Title = "EpubReader";
            Window.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}