export function checkValidElement(el_id) {
    return document.getElementById(el_id) !== null;
}
export function capturePointer(el, p) {
    if (checkValidElement(el.id)) {
        el.setPointerCapture(p);
    }
}

export function releasePointer(el, p) {
    if (checkValidElement(el.id)) {
        el.releasePointerCapture(p);
    }
}

export function GetBoundingClientRect(el_id) {
    if (checkValidElement(el_id))
        return JSON.stringify(document.getElementById(el_id).getBoundingClientRect());
    else
        return '{"x":0,"y":0,"width":0,"height":0,"top":0,"right":0,"bottom":0,"left":0}';
}

export function GetElementWidth(elid) {
    if (checkValidElement(elid))
        return document.getElementById(elid).getBoundingClientRect().width;
    else
        return 0;
}

export function GetElementHeight(elid) {
    if (checkValidElement(elid))
        return document.getElementById(elid).getBoundingClientRect().height;
    else
        return 0;
}

//export function GetImageSize(uri) {
//    var img = document.createElement('img');
//    img.src = uri;
//    var poll = setInterval(function () {
//        if (img.naturalWidth) {
//            clearInterval(poll);
//            console.log(img.naturalWidth, img.naturalHeight);
//        }
//    }, 10);

//    img.onload = function () { console.log('Fully loaded'); }
//}