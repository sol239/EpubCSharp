function scrollDown() {
    window.scrollBy(0, window.innerHeight);
}

function scrollUp() {
    window.scrollBy(0, -window.innerHeight);
}

function checkScroll() {
    if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight) {
        // Notify C# when scrolled to the bottom
        window.chrome.webview.postMessage('scrolledToBottom');
    }
}

function handleKeyDown(event) {
    if (event.key === 'ArrowDown' || event.key === 'ArrowRight') {
        scrollDown();
        window.chrome.webview.postMessage('scrolledDown');
    } else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
        scrollUp();
        window.chrome.webview.postMessage('scrolledUp');
    }

    console.log(`Hello World`);
}

function handleDocumentClick(event) {
    const clickedElement = event.target;

    if (clickedElement.nodeType === Node.ELEMENT_NODE) {
        const textContent = clickedElement.textContent;
        const range = document.createRange();

        for (let i = 0; i < clickedElement.childNodes.length; i++) {
            const node = clickedElement.childNodes[i];
            if (node.nodeType === Node.TEXT_NODE) {
                range.selectNodeContents(node);
                const rect = range.getBoundingClientRect();

                if (rect.left <= event.clientX && rect.right >= event.clientX &&
                    rect.top <= event.clientY && rect.bottom >= event.clientY) {
                    const words = node.textContent.split(' ');
                    let clickedWord = '';

                    for (let j = 0; j < words.length; j++) {
                        range.setStart(node, node.textContent.indexOf(words[j]));
                        range.setEnd(node, node.textContent.indexOf(words[j]) + words[j].length);
                        const wordRect = range.getBoundingClientRect();

                        if (wordRect.left <= event.clientX && wordRect.right >= event.clientX &&
                            wordRect.top <= event.clientY && wordRect.bottom >= event.clientY) {
                            clickedWord = words[j];
                            break;
                        }
                    }

                    if (clickedWord) {
                        console.log(`You clicked: ${clickedWord}`);
                    }

                    break;
                }
            }
        }
    }

    // Display the selected text
    const selectedText = window.getSelection().toString();
    if (selectedText) {
        console.log(`You selected: ${selectedText}`);
    } else {
        console.log('You selected:');
    }
}

function setupEventListeners() {
    window.addEventListener('scroll', checkScroll);
    window.addEventListener('keydown', handleKeyDown);
    
    document.addEventListener('DOMContentLoaded', () => {
        document.body.addEventListener('click', handleDocumentClick);
    });
    

}

// Set up event listeners once DOM is loaded
setupEventListeners();