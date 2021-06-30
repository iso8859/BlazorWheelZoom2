export function capturePointer(el, p) {
    el.setPointerCapture(p);
}

export function releasePointer(el, p) {
    el.releasePointerCapture(p);
}

export function GetBoundingClientRect(el_id) {
    if (document.getElementById(el_id) !== null)
        return JSON.stringify(document.getElementById(el_id).getBoundingClientRect());
    else
        return '{"x":0,"y":0,"width":0,"height":0,"top":0,"right":0,"bottom":0,"left":0}';
}

export function GetElementWidth(elid) {
    if (document.getElementById(elid) !== null)
        return document.getElementById(elid).getBoundingClientRect().width;
    else
        return 0;
}

export function GetElementHeight(elid) {
    if (document.getElementById(elid) !== null)
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