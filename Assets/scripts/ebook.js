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
    if (event.key === 'ArrowDown') {
        scrollDown();
        window.chrome.webview.postMessage('scrolledDown');
    } else if (event.key === 'ArrowRight') {
        scrollDown();
        window.chrome.webview.postMessage('scrolledDown');
    } else if (event.key === 'ArrowLeft') {
        scrollUp();
        window.chrome.webview.postMessage('scrolledUp');
    } else if (event.key === 'ArrowUp') {
        scrollUp();
        window.chrome.webview.postMessage('scrolledUp');
    }
}

function setupEventListeners() {
    window.addEventListener('scroll', checkScroll);
    window.addEventListener('keydown', handleKeyDown);
    document.addEventListener('selectionchange', handleSelection);
}

// Re-attach event listeners
setupEventListeners();