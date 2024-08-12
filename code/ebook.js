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
}


function setupEventListeners() {
    window.addEventListener('scroll', checkScroll);
    window.addEventListener('keydown', handleKeyDown);
}

// Set up event listeners once DOM is loaded
setupEventListeners();