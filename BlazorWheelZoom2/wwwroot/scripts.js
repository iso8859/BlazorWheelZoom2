export function capturePointer(el, p) {
    el.setPointerCapture(p);
}

export function releasePointer(el, p) {
    el.releasePointerCapture(p);
}

export function GetBoundingClientRect(el_id) {
    return JSON.stringify(document.getElementById(el_id).getBoundingClientRect());
}

export function GetElementWidth(elid) {
    return document.getElementById(elid).getBoundingClientRect().width;
}

export function GetElementHeight(elid) {
    return document.getElementById(elid).getBoundingClientRect().height;
}