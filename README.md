# MauiPdfJSViewer

[![NuGet version (PdfJSViewer-MAUI)](https://img.shields.io/nuget/v/PdfJSViewer-MAUI.svg)](https://www.nuget.org/packages/PdfJSViewer-MAUI)

 A implementation of pdfjs for android wrapped into a nuget so you can just install and use it. Have fun :). 
 
 Based on 
 https://github.com/jfversluis/MauiPdfJsViewerSample

 
 Be sure to meet pdf.js Apache license details in your app  https://github.com/mozilla/pdf.js
 
![Recording 2024-05-22 155352](https://github.com/Jon2G/MauiPdfJSViewer/assets/24820069/2d7bc38b-2bc2-42e0-a5f0-e9316917a266)

Usage:
XAML
```
        <pdfjs:PdfJsWebView x:Name="pdfJsViewer" />
```
C#
```
        private void Button_Clicked_FromAssets(object sender, EventArgs e)
        {
            pdfJsViewer.LoadPdfFromMauiAssets("sample.pdf");
        }

        private async void Button_Clicked_FromStream(object sender, EventArgs e)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("sample.pdf");
            pdfJsViewer.LoadPdfFromSteam(stream);

        }

        private async void Button_Clicked_FromPath(object sender, EventArgs e)
        {
            string fileName = $"{Guid.NewGuid():N}.pdf";

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            using var pdfStream = await FileSystem.OpenAppPackageFileAsync("sample.pdf");
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                pdfStream.CopyTo(file);

            pdfJsViewer.LoadPdfFromPath(filePath);
        }
```
