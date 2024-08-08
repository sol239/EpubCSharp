function handleDocumentClick(event) {
    const clickedElement = event.target;

    if (clickedElement.nodeType === Node.ELEMENT_NODE) {
        const textContent = clickedElement.textContent;

        const range = document.createRange();
        const selection = window.getSelection();

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
                        window.chrome.webview.postMessage(`CLICKED WORD = ${clickedWord}`);
                        console.log(`CLICKED WORD = ${clickedWord}`);
                    }

                    else {
                        console.log("CLICKED WORD = EMPTY");
                        window.chrome.webview.postMessage(`CLICKED WORD = *783kd4HJsn`);

                    }

                    break;
                }
            }
        }
    }

    // Display the selected text
    const selectedText = window.getSelection().toString();
    if (selectedText) {
        window.chrome.webview.postMessage(`SELECTED WORD = ${selectedText}`);
        console.log(`SELECTED WORD = ${selectedText}`);

    } else {
        window.chrome.webview.postMessage('SELECTED WORD = *783kd4HJsn');
        console.log("SELECTED WORD = EMPTY");
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.body.addEventListener('click', handleDocumentClick);
});