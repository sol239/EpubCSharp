# Technical Documentation

### Description

The app was coded in **`C#`**. It uses [**WIN UI](https://learn.microsoft.com/en-us/windows/apps/winui/)** framework for app’s GUI. The app uses WebView2 WIN UI element to display epub’s xhtml files. The app uses **`JavaScript**` to inject custom CSS and control ebook scrolling and translation functions. The app uses **`Python`** to run argos offline translations and flask to run local server running argos.

### Structure

![image.png](Technical%20Documentation%20b5af8669873848ee8557f61019959381/image.png)

![image.png](Technical%20Documentation%20b5af8669873848ee8557f61019959381/image%201.png)

- **app_pages/** = directory containing pages used for displaying **Home, All book, book viewer, …**
    - **Home = HomePage.xaml**
    - **All books = AllBooks.xaml**
    - **Stats = Stats.xaml**
    - **Settings = SettingsPage.xaml**
    - **Custom viewer = EbookWindow.xaml**
    - **epubjs viewer = epubjsWindow1.xaml**
- **code/**
    - **app_controls.cs** = there are some methods used by multiple pages: updating css, injecting JS into html files, …
    - **epub-handler.cs**  = handles operations with epub file: extracting epub archive, extracting metadata from its files and than storing the data in the app’s own storage.
    - **file-management.cs** = handles operations with file paths
    - **script.js** = handles WebView2 controls = scrolls, move to neext xhtml file, selected text retreival.
    - **translation_script.py** = starts Flask local server used for argos-offline translations. If using argos causes errors, please  start Flask server manually by running the python script from console. `python translation_script.py`
- **scripts/**
    - **epubjs-reader/** = contains files used by **epubjs** translation service (https://github.com/futurepress/epub.js)
    - **ebook.js** = a back-up script for handling WebView2, is not used.
    - **script.js** = a back-up script for handling WebView2, is not used.
- **App.xaml** = is the initial window of the app which is started as  the first one.
- **doc.xml** = exported xml comment from entire solution
- **README.md** = readme file used in gitlab/github.

For more informations about used classes and methods please view their comments.