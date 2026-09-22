window.BlazorSimpleSVG = {
    // Called by the inline onload of <image> (see SVGImage.GetSVG).
    // Must never throw and must never call into .NET once the circuit is gone,
    // otherwise Blazor logs "Cannot send data if the connection is not in the
    // 'Connected' State" / "No interop methods are registered for renderer 1".
    imageLoaded: function (rectId) {
        try {
            if (!rectId)
                return;
            const el = document.getElementById(rectId);
            // The element may already have been detached by a Blazor render batch.
            if (!el || !el.isConnected)
                return;
            if (typeof DotNet === 'undefined')
                return;
            const rect = el.getBoundingClientRect();
            if (!rect)
                return;
            DotNet.invokeMethodAsync('BlazorSimpleSVG', 'ImageLoaded', rectId, JSON.stringify(rect))
                .catch(function () { /* circuit disconnected: ignore */ });
        }
        catch (ex) {
            /* interop unavailable or renderer disposed: ignore */
        }
    }
};
