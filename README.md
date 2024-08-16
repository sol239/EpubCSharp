# EpubReader

## User Documentation

### Description

This is a simple **WINUI** .epub reader that reads epub files and displays them in a webview.
The application is written in C# and XAML and uses JavaScript for scrolling and on-tap translation. It also uses Python for Flask server executing translations using argos-translate package. 

The project was created as a Programming II (NPRG031) MFF CUNI semestral project.

### Features

- reading .epub files
- custom fonts, padding and themes
- adding epub ebooks into “shelf” ( = shelf displayes epubs added into the app )
- in-book translations:
    - selected text translations
    - clicked word translations
    - offline translations using argos-translate
    - online translations using My Memory API
- displaying reading statistics:
    - all-time reading time
    - reading time per day

---

## App manual

### Add book:

You ran the app for the first time, you’ll see this:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image.png)

You can add .epub ebook by clicking on the Add Book button. After you click on the button, the file explorer will be opened and you will be able to select .epub file. After the book was added, you should see this:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%201.png)

If you tap on book cover, book viewer will be opened.

### View All books:

When you tap on **All books** ( the section in navigation menu ) you will be redirected to another page where all your ebooks are displayed based on selected sorting option:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%202.png)

You can choose different sortion option from the left upper corner combobox:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%203.png)

If you tap on a book in **All books** you will be able to see more information about the book. By tapping on **Read** you will be able to open ebook viewer. By tapping on **Delete** book will be removed from app storage with its reading statistics. If you want to use **translations,** please select book’s language.

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%204.png)

### Stats page:

When you tap on **Stats** ( the section in navigation menu ) you will be redirected to another page where you will see how much time you spent reading: per each day, all time:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%205.png)

### Settings page:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%206.png)

You can select these options:

- **Font size** = font size of the text in the ebook (recommended: 10 - 30)
- **Padding** = how far will be text shifted from the app window borders (recommended: 0 - 150)

If you want to save changed values, click on the button next to the box you typed in, once you click on the button next to the text box, it will change its color to red if wrong value, green if correct value.

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%207.png)

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%208.png)

- **Ebook font** = font style used in ebook viewer
- **Themes** = custom-made themes, they change background color
- **Ebook Viewer** = you can choose between **Custom** and **epubjs**
    - **Custom** is the main viewer, it saves how much time you spent reading and allow you to use on-tap translations. It’s my own custom made viewer.
    - **epubjs** is epub viewer should be used only if **Custom** viewer fails. I did not create **epubjs**. Link to epubjs github repo [https://github.com/futurepress/epub.js](https://github.com/futurepress/epub.js)
- **Translation Language**  = language into your on-tap translation will be translated.
- **Translation Service** = you can choose between **argos** and **My Memory**
    
       If you want to use translations, please tap on book in **All books** and select the book’s language.
    
    - **My Memory** ([https://mymemory.translated.net/](https://mymemory.translated.net/)) = is online translation service, which is free to use but has limitation usage. You translate up to 5000 chars ( not words ).
    - **argos** ([https://github.com/argosopentech](https://github.com/argosopentech)) = is offline translation service. When you try to use argos for the first time, it downloads the language pair pack used for offline translations. You have to wait until download is finished. In case you want to see installed language packages or you want to delete them, go to: `C:\Users\USER\.local\share\argos-translate\packages` on Windows.
        - If you try to use **argos translate for the first time** (on given language pair) it has to install packages. You can see argos translate status in the right upper corner: not ready = red, ready = green
        - langauges supported by argos:
        
        [Albanian -> English, Arabic -> English, Azerbaijani -> English, Bengali -> English, Bulgarian -> English, Catalan -> English, Chinese (traditional) -> English, Chinese -> English, Czech -> English, Danish -> English, Dutch -> English, English -> Albanian, English -> Arabic, English -> Azerbaijani, English -> Bengali, English -> Bulgarian, English -> Catalan, English -> Chinese, English -> Chinese (traditional), English -> Czech, English -> Danish, English -> Dutch, English -> Esperanto, English -> Estonian, English -> Finnish, English -> French, English -> German, English -> Greek, English -> Hebrew, English -> Hindi, English -> Hungarian, English -> Indonesian, English -> Irish, English -> Italian, English -> Japanese, English -> Korean, English -> Latvian, English -> Lithuanian, English -> Malay, English -> Norwegian, English -> Persian, English -> Polish, English -> Portuguese, English -> Romanian, English -> Russian, English -> Slovak, English -> Slovenian, English -> Spanish, English -> Swedish, English -> Tagalog, English -> Thai, English -> Turkish, English -> Ukranian, English -> Urdu, Esperanto -> English, Estonian -> English, Finnish -> English, French -> English, German -> English, Greek -> English, Hebrew -> English, Hindi -> English, Hungarian -> English, Indonesian -> English, Irish -> English, Italian -> English, Japanese -> English, Korean -> English, Latvian -> English, Lithuanian -> English, Malay -> English, Norwegian -> English, Persian -> English, Polish -> English, Portuguese -> English, Portuguese -> Spanish, Romanian -> English, Russian -> English, Slovak -> English, Slovenian -> English, Spanish -> English, Spanish -> Portuguese, Swedish -> English, Tagalog -> English, Thai -> English, Turkish -> English, Ukranian -> English, Urdu -> English]
        
        **Currently therer is no bridge implemented, so you cannot translate from Deutsch → English → Czech. Only pair above are supported.**
        
        - argos translate is extremely fast and works offline.
        - If you decide to use argos translate you need to provide Python path in the option bellow.  You will need to have this packages install these packages:
    
    ```bash
    pip install git+https://github.com/sol239/argos-translate-epub-reader.git
    pip install numpy==1.26.4   # numpy >=2 will not work with argos-translate
    pip install Flask   # used for python local server executing argos translations offline
    pip install langdetect
    ```
    

### Dictionary

After you save some translations in **Book viewer,** they will be displayed in **Dictionary:**

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%209.png)

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2010.png)

If its the text is too long it will be wrapped, to see full translation please click on it as in the screeshot on the left. The sctrucure of each segment is:

- source text
- translation
- source langauge
- target language

### Book viewer:

After you click on book cover in **Home** or **All books** Read button, new window with the book will be opened. You can move forward by the menu’s:  ←, → buttons or by pressing Arrow left, Arrow right keys. 

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2011.png)

If you tap on a word or select text, its translation will be displayed:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2012.png)

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2013.png)

You can save translations by clicking on **Save**. You can find saved translations in the main menu: **Dictionary**. 

**Sometimes** on-tap translation does not work (known issues), if you double tap/click it or select it, it will work correctly. 

If you tap on **settings icon**, settings menu will be displayed ( it’s more limited than main settings menu):

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2014.png)

If you tap on **home icon**, the book viewer will be closed and saved. **DO NOT** close the book window differently, since your stats and page location will **NOT** be saved.

### Themes:

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2015.png)

Pure White

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/da69a2cd-fe5e-4d78-b563-960af6420840.png)

Dark Blue

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2016.png)

Woodlawn

---

## Technical documentation

### Description

The app was coded in **`C#`**. It used [**WIN UI](https://learn.microsoft.com/en-us/windows/apps/winui/)** framework for app’s GUI. The app uses WebView2 WIN UI element to display epub’s xhtml files. The app uses **`JavaScript**` to inject custom CSS and control ebook scrolling and translation functions. The app uses **`Python`** to run argos offline translations and flask to run local server running argos.

### Structure

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2017.png)

![image.png](EpubReader%208468f6ccb553496d94602b82fc69a495/image%2018.png)

- **app_pages/** = directory containing pages used for displaying **Home, All book, book viewer, …**
    - **Home = HomePage.xaml**, The app starts with this page.
    - **All books = AllBooks.xaml**
    - **Stats = Stats.xaml**
    - **Settings = SettingsPage.xaml**, the app is on the first start loaded with default settings, which 
    can be found in /code/file-management.cs/CreateGlobalSettingsFile()
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

### Project Status

Epub reader is currently finished, features, known issues and bugs are listed here:

[**https://www.notion.so/eBookReader-5e7f5a7d63a2480594e2ca9edecdd5e7**](https://www.notion.so/eBookReader-5e7f5a7d63a2480594e2ca9edecdd5e7?pvs=21)

### Support

If you need help, contact me: david.valek17@gmail.com

You can find this doc at: [https://www.notion.so/EpubReader-8468f6ccb553496d94602b82fc69a495](https://www.notion.so/EpubReader-8468f6ccb553496d94602b82fc69a495?pvs=21)

---
>>>>>>> master-gitlab
