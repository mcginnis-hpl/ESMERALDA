function initalizeParent() {
    if (!parent || !parent.resizeIframe) {
        return;
    }
    parent.resizeIframe(document.body.scrollHeight);
}