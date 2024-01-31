window.BlazorSimpleSVG = {
    imageLoaded: function (rectId) {
        DotNet.invokeMethodAsync('BlazorSimpleSVG', 'ImageLoaded', rectId, JSON.stringify(document.getElementById(rectId).getBoundingClientRect()));
    }
}