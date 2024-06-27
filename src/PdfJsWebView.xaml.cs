using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.IO;
using System.Net;
using System.Reflection;
using System;
using System.IO.Compression;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace MauiPdfJSViewer;

public partial class PdfJsWebView : ContentView
{
    public WebView InternalWebView { get => this.pdfviewer; }
    private string? pdfFilePath;
    private string? pdfJsDir;
    private bool IsReady = false;
    private Task? ExpandPdfJsIfNotExistsTask;
    public PdfJsWebView()
    {
        InitializeComponent();
#if ANDROID
        this.IsReady = false;
        AddPlattformHandlers();
        this.ExpandPdfJsIfNotExists();
#else
this.IsReady=true;
#endif
    }
#if ANDROID
    internal void AddPlattformHandlers()
    {
        const string key = "pdfviewer";
        if (Microsoft.Maui.Handlers.WebViewHandler.Mapper.GetProperty(key) is null)
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping(key, (handler, View) =>
            {
                handler.PlatformView.Settings.AllowFileAccess = true;
                handler.PlatformView.Settings.AllowFileAccessFromFileURLs = true;
                handler.PlatformView.Settings.AllowUniversalAccessFromFileURLs = true;
            });
        this.pdfviewer.Navigated += Pdfviewer_Navigated;
    }

    internal void ExpandPdfJsIfNotExists()
    {
        try
        {
            this.ExpandPdfJsIfNotExistsTask = Task.Run(() =>
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pdfjs");
                var dir = new DirectoryInfo(filePath);
                var htmlFile = new FileInfo(Path.Combine(dir.FullName, "web/viewer.html"));
                if (dir.Exists && htmlFile.Exists)
                {
                    pdfJsDir = dir.FullName;
                    return;
                }
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "MauiPdfJSViewer.EmbeddedResource.pdfjs.zip";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    ZipFile.ExtractToDirectory(stream, dir.Parent.FullName, true);

                pdfJsDir = dir.FullName;
                this.IsReady = true;
            });
            if (ExpandPdfJsIfNotExistsTask.Status == TaskStatus.WaitingForActivation)
            {
                this.ExpandPdfJsIfNotExistsTask.Start();
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine(ex?.Message);
#endif
        }
    }
#endif

    public async void LoadPdfFromMauiAssets(string assetName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(assetName);
        this.LoadPdfFromSteam(stream);
    }

    public async void LoadPdfFromSteam(Stream pdfStream)
    {
        await Task.Yield();
        string fileName = $"{Guid.NewGuid():N}.pdf";

        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

        using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            pdfStream.CopyTo(file);


#if ANDROID
        if (!IsReady)
            await ExpandPdfJsIfNotExistsTask;
        this.pdfFilePath = filePath;
        this.Show();
#else

            this.pdfviewer.Source = filePath;
#endif
    }

    public async void LoadPdfFromPath(string path)
    {
        await Task.Yield();
#if ANDROID
        if (!IsReady)
            await ExpandPdfJsIfNotExistsTask;
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
    private bool NewPageActivated = false;
    private void Pdfviewer_Navigated(object sender, WebNavigatedEventArgs e)
    {
        if ((NewPageActivated==false && e.NavigationEvent == WebNavigationEvent.NewPage) || e.NavigationEvent == WebNavigationEvent.Refresh)
        {
            NewPageActivated = true;
            this.pdfviewer.Eval($"PDFViewerApplication.open({{url:'{pdfFilePath}'}})");
        }
    }
}