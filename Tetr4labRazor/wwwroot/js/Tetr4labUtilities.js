
function scrollToTop() {
    document.documentElement.scrollTop = 0;
}

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

function getElementRect(selector) {
    const element = document.querySelector(selector);
    if (element) {
        const rect = element.getBoundingClientRect();
        return {
            x: rect.x,
            y: rect.y,
            width: rect.width,
            height: rect.height,
            top: rect.top,
            right: rect.right,
            bottom: rect.bottom,
            left: rect.left
        };
    }
    return null;
}

function getElementRectPair(selector, selector2) {
    const element = document.querySelector(selector);
    const element2 = document.querySelector(selector2);
    if (element && element2) {
        const rect = element.getBoundingClientRect();
        const rect2 = element2.getBoundingClientRect();
        return {
            x: rect.x,
            y: rect.y,
            width: rect.width,
            height: rect.height,
            top: rect.top,
            right: rect.right,
            bottom: rect.bottom,
            left: rect.left,
            x2: rect2.x,
            y2: rect2.y,
            width2: rect2.width,
            height2: rect2.height,
            top2: rect2.top,
            right2: rect2.right,
            bottom2: rect2.bottom,
            left2: rect2.left
        };
    }
    return null;
}

function noOperation() { }
