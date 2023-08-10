mergeInto(LibraryManager.library, {
  CaptureScreenshot: function () {
    // Get the WebGL canvas
    var canvas = document.querySelector('#canvas');

    // Get the image data URL
    var dataUrl = canvas.toDataURL('image/png');

    // Send the data URL to Unity
    SendMessage('CommandManager', 'OnScreenshotTaken', dataUrl);
  }
});
