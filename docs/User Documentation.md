# User Documentation

## Storing Data

The app uses its own local folder, which can be found at: `{User}\AppData\Local\Packages\EpubCSharp_ttbk31g8t3316\LocalState` 

There will be these folders and files:

- **ebooks/**  = this directory contains another directory per each added ebook, an ebook dir has random name since using ebook’s name causes error because of Windows max path lenghth.
    - ebook directory = this directory constains extracted .epub files plus one extra directory called **DATA** = the directory contains ebook’s metadata used in the app: title, readtime, dateAdded, cover path, …
- **settings/**
    - allBooks.json = the file contains data used for displaying ebooks covers at the home page and the all books page.
    - globalDict.json = the file contains data used for displaying saved translations at the dict page
    - globalSettings.json = the file contains settings you can find at the settings page
- **ebook_viewer-style.css** =  the file where is stored general style used in all ebooks (fonts, background color, etc..)

---

## How to use

### Run the app

If you want to run the app please follow README.md Installation instructions.

### Add book:

You ran the app for the first time, you’ll see this:

![image.png](User%20Documentation%20data/image.png)

You can add .epub ebook by clicking on the Add Book button. After you click on the button, the file explorer will be opened and you will be able to select .epub file. After the book was added, you should see this:

![image.png](User%20Documentation%20data/image%201.png)

If you tap on book cover, book viewer will be opened.

### View All books:

When you tap on **All books** ( the section in navigation menu ) you will be redirected to another page where all your ebooks are displayed based on selected sorting option:

![image.png](User%20Documentation%20data/image%202.png)

You can choose different sortion option from the left upper corner combobox:

![image.png](User%20Documentation%20data/image%203.png)

If you tap on a book in **All books** you will be able to see more information about the book. By tapping on **Read** you will be able to open ebook viewer. By tapping on **Delete** book will be removed from app storage with its reading statistics. If you want to use **translations,** please select book’s language.

![image.png](User%20Documentation%20data/image%204.png)

### Stats page:

When you tap on **Stats** ( the section in navigation menu ) you will be redirected to another page where you will see how much time you spent reading: per each day, all time:

![image.png](User%20Documentation%20data/image%205.png)

### Settings page:

![image.png](User%20Documentation%20data/image%206.png)

You can select these options:

- **Font size** = font size of the text in the ebook (recommended: 10 - 30)
- **Padding** = how far will be text shifted from the app window borders (recommended: 0 - 150)

If you want to save changed values, click on the button next to the box you typed in, once you click on the button next to the text box, it will change its color to red if wrong value, green if correct value.

![image.png](User%20Documentation%20data/image%207.png)

![image.png](User%20Documentation%20data/image%208.png)

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
    

### Dictionary:

After you save some translations in **Book viewer,** they will be displayed in **Dictionary:**

![image.png](User%20Documentation%20data/image%209.png)

![image.png](User%20Documentation%20data/image%2010.png)

If its the text is too long it will be wrapped, to see full translation please click on it as in the screeshot on the left. The sctrucure of each segment is:

- source text
- translation
- source langauge
- target language

### Book viewer:

After you click on book cover in **Home** or **All books** Read button, new window with the book will be opened. You can move forward by the menu’s:  ←, → buttons or by pressing Arrow left, Arrow right keys. 

![image.png](User%20Documentation%20data/image%2011.png)

If you tap on a word or select text, its translation will be displayed:

![image.png](User%20Documentation%20data/image%2012.png)

![image.png](User%20Documentation%20data/image%2013.png)

You can save translations by clicking on **Save**. You can find saved translations in the main menu: **Dictionary**. 

**Sometimes** on-tap translation does not work (known issues), if you double tap/click it or select it, it will work correctly. 

If you tap on **settings icon**, settings menu will be displayed ( it’s more limited than main settings menu):

![image.png](User%20Documentation%20data/image%2014.png)

If you tap on **home icon**, the book viewer will be closed and saved. **DO NOT** close the book window differently, since your stats and page location will **NOT** be saved. 

You can scroll with the menu **`->**` ,**`<-`** buttons or with `ArrowUp/Left` , `ArrowDown/Right` keys.

### Themes:

![image.png](User%20Documentation%20data/image%2015.png)

Pure White

![image.png](User%20Documentation%20data/da69a2cd-fe5e-4d78-b563-960af6420840.png)

Dark Blue

![image.png](User%20Documentation%20data/image%2016.png)

Woodlawn

---