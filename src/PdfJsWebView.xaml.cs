using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.IO;
using System.Net;
using System.Reflection;
using System;
using System.IO.Compression;
using Microsoft.Maui.Storage;

namespace MauiPdfJSViewer;

public partial class PdfJsWebView : ContentView
{
    public WebView InternalWebView { get => this.pdfviewer; }
    private string pdfFilePath;
    private string pdfJsDir;
    public PdfJsWebView()
    {
        InitializeComponent();
#if NET8_0_OR_GREATER && ANDROID

        var key = "pdfviewer";
        if (Microsoft.Maui.Handlers.WebViewHandler.Mapper.GetProperty(key) is null)
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping(key, (handler, View) =>
            {
                handler.PlatformView.Settings.AllowFileAccess = true;
                handler.PlatformView.Settings.AllowFileAccessFromFileURLs = true;
                handler.PlatformView.Settings.AllowUniversalAccessFromFileURLs = true;
            });
        this.pdfviewer.Navigated += Pdfviewer_Navigated;

#endif
    }

    private void Prepare()
    {
#if NET8_0_OR_GREATER && ANDROID
        this.ExpandPdfJsIfNotExists();
#endif

    }
    private void ExpandPdfJsIfNotExists()
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pdfjs");
        var dir = new DirectoryInfo(filePath);
        if (dir.Exists)
        {
            pdfJsDir = dir.FullName;
            return;
        }
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "MauiPdfJSViewer.EmbeddedResource.pdfjs.zip";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            ZipFile.ExtractToDirectory(stream, dir.Parent.FullName, true);

        pdfJsDir = dir.FullName;
    }

    public async void LoadPdfFromMauiAssets(string assetName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(assetName);
        this.LoadPdfFromSteam(stream);
    }

    public void LoadPdfFromSteam(Stream pdfStream)
    {
        this.Prepare();
        string fileName = $"{Guid.NewGuid():N}.pdf";

        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

        using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            pdfStream.CopyTo(file);


#if ANDROID
        this.pdfFilePath = filePath;
        this.Show();
#else

            this.pdfviewer.Source = filePath;
#endif
    }

    public void LoadPdfFromPath(string path)
    {
        this.Prepare();
#if ANDROID

        this.pdfFilePath = path;
        this.Show();
#else

            this.pdfviewer.Source =path;
#endif
    }

    private void Show()
    {
        //?file=file:///android_asset/{WebUtility.UrlEncode(assetName)}
        this.pdfviewer.Source = Path.Combine(pdfJsDir, $"web/viewer.html");


        //$"file://{pdfJsDir}/web/viewer.html?file=file:///android_asset/{WebUtility.UrlEncode(assetName)}";
    }

    private void Pdfviewer_Navigated(object sender, WebNavigatedEventArgs e)
    {
        if (e.NavigationEvent == WebNavigationEvent.NewPage || e.NavigationEvent == WebNavigationEvent.Refresh)
        {
            this.pdfviewer.Eval($"PDFViewerApplication.open({{url:'{pdfFilePath}'}})");
        }
    }
}