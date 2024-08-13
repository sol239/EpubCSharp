/**
 * Handles click events on the document.
 * Determines if a clicked word is within the clicked element and sends information about the clicked word or selected text.
 * 
 * @param {MouseEvent} event - The click event object.
 */
function handleDocumentClick(event) {
    const clickedElement = event.target;

    // Check if the clicked element is an actual HTML element (not a text node or comment)
    if (clickedElement.nodeType === Node.ELEMENT_NODE) {
        const textContent = clickedElement.textContent;
        const range = document.createRange();
        const selection = window.getSelection();

        // Iterate through child nodes of the clicked element
        for (let i = 0; i < clickedElement.childNodes.length; i++) {
            const node = clickedElement.childNodes[i];

            // If the child node is a text node, check if it contains the click position
            if (node.nodeType === Node.TEXT_NODE) {
                range.selectNodeContents(node);
                const rect = range.getBoundingClientRect();

                // Check if the click position is within the text node's bounding rectangle
                if (rect.left <= event.clientX && rect.right >= event.clientX &&
                    rect.top <= event.clientY && rect.bottom >= event.clientY) {
                    const words = node.textContent.split(' ');
                    let clickedWord = '';

                    // Check each word in the text node to see if it contains the click position
                    for (let j = 0; j < words.length; j++) {
                        range.setStart(node, node.textContent.indexOf(words[j]));
                        range.setEnd(node, node.textContent.indexOf(words[j]) + words[j].length);
                        const wordRect = range.getBoundingClientRect();

                        // Check if the click position is within the word's bounding rectangle
                        if (wordRect.left <= event.clientX && wordRect.right >= event.clientX &&
                            wordRect.top <= event.clientY && wordRect.bottom >= event.clientY) {
                            clickedWord = words[j];
                            break;
                        }
                    }

                    // Send information about the clicked word
                    if (clickedWord) {
                        window.chrome.webview.postMessage(`CLICKED WORD = ${clickedWord}`);
                        console.log(`CLICKED WORD = ${clickedWord}`);
                    } else {
                        console.log("CLICKED WORD = EMPTY");
                        window.chrome.webview.postMessage(`CLICKED WORD = *783kd4HJsn`);
                    }

                    break;
                }
            }
        }
    }

    // Display the selected text and send it to the webview
    const selectedText = window.getSelection().toString();
    if (selectedText) {
        window.chrome.webview.postMessage(`SELECTED WORD = ${selectedText}`);
        console.log(`SELECTED WORD = ${selectedText}`);
    } else {
        window.chrome.webview.postMessage('SELECTED WORD = *783kd4HJsn');
        console.log("SELECTED WORD = EMPTY");
    }
}

/**
 * Scrolls the window down by one viewport height.
 */
function scrollDown() {
    window.scrollBy(0, window.innerHeight);
}

/**
 * Scrolls the window up by one viewport height.
 */
function scrollUp() {
    window.scrollBy(0, -window.innerHeight);
}

/**
 * Checks if the user has scrolled to the bottom of the document.
 * If scrolled to the bottom, sends a message to the webview.
 */
function checkScroll() {
    if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight) {
        // Notify C# when scrolled to the bottom
        window.chrome.webview.postMessage('scrolledToBottom');
    }
}

let isScrolling = true;

/**
 * Handles keydown events to scroll the document based on arrow key presses.
 * 
 * @param {KeyboardEvent} event - The keydown event object.
 */
function handleKeyDown(event) {
    if (event.key === 'ArrowDown' || event.key === 'ArrowRight') {
        // Notify that the user scrolled down
        window.chrome.webview.postMessage('scrolledDown');
    } else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
        // Notify that the user scrolled up
        window.chrome.webview.postMessage('scrolledUp');
    }
}

/**
 * Sets up event listeners for scroll and keydown events.
 */
function setupEventListeners() {
    window.addEventListener('scroll', checkScroll);
    window.addEventListener('keydown', handleKeyDown);
}

// Set up event listeners once DOM is loaded
setupEventListeners();

document.addEventListener('DOMContentLoaded', () => {
    document.body.addEventListener('click', handleDocumentClick);
});
