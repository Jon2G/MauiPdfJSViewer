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

public partial class PdfJsWebView : ContentView, IDisposable
{
    public WebView InternalWebView { get => this.pdfviewer; }
    private string? PdfFilesCacheFolder;
    private Task? DeleteTmpFolderIfExistsTask;
#if ANDROID
    private string? pdfJsDir;
    private string? pdfFilePath;

    private bool IsReady = false;
    private Task? ExpandPdfJsIfNotExistsTask;
#endif
    public PdfJsWebView()
    {
        InitializeComponent();
#if ANDROID
        this.IsReady = false;
        AddPlattformHandlers();
        this.ExpandPdfJsIfNotExists();
#endif
        this.DeleteTmpFolderIfExists();
    }

    private void TryDeleteFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
               catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine(ex?.Message);
#endif
        }
    }

    private void DeleteTmpFolderIfExists()
    {
        try
        {
            this.DeleteTmpFolderIfExistsTask = Task.Run(() =>
            {
                this.PdfFilesCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");
                TryDeleteFolder(this.PdfFilesCacheFolder);
                Directory.CreateDirectory(this.PdfFilesCacheFolder);
                //delete temporal files if found
            });
            if (DeleteTmpFolderIfExistsTask.Status == TaskStatus.WaitingForActivation)
            {
                this.DeleteTmpFolderIfExistsTask.Start();
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine(ex?.Message);
#endif
        }
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
                    this.IsReady = true;
                    pdfJsDir = dir.FullName;
                    return;
                }
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "MauiPdfJSViewer.EmbeddedResource.pdfjs.zip";

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream is null) throw new Exception("Unable to get pdfjs.zip from embedded resources");
                    ZipFile.ExtractToDirectory(stream, dir.Parent!.FullName, true);
                }

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

    public Task LoadPdfFromMauiAssets(string assetName) => this.LoadPdfFromSteam(() => FileSystem.OpenAppPackageFileAsync(assetName));

    public async Task LoadPdfFromSteam(Func<Task<Stream>> pdfStreamFunction)
    {
        await Task.Yield();
        string fileName = $"{Guid.NewGuid():N}.pdf";

        string filePath = Path.Combine(this.PdfFilesCacheFolder!, fileName);

        using (FileStream file = new(filePath, FileMode.Create, FileAccess.Write))
        {
            using Stream pdfStream = await pdfStreamFunction.Invoke();
            await pdfStream.CopyToAsync(file);
        }
        await LoadPdfFromPath(filePath);
    }

    public async Task LoadPdfFromPath(string path)
    {
        await Task.Yield();
#if ANDROID
        if (!IsReady)
            await ExpandPdfJsIfNotExistsTask!;
        this.pdfFilePath = path;
        this.Show();
#else

            this.pdfviewer.Source =path;
#endif
    }
#if ANDROID
    private void Show()
    {
        //?file=file:///android_asset/{WebUtility.UrlEncode(assetName)}
        this.pdfviewer.Source = Path.Combine(pdfJsDir!, $"web/viewer.html");


        //$"file://{pdfJsDir}/web/viewer.html?file=file:///android_asset/{WebUtility.UrlEncode(assetName)}";
    }

    private bool NewPageActivated = false;
    private void Pdfviewer_Navigated(object? sender, WebNavigatedEventArgs e)
    {
        if ((NewPageActivated == false && e.NavigationEvent == WebNavigationEvent.NewPage) || e.NavigationEvent == WebNavigationEvent.Refresh)
        {
            NewPageActivated = true;
            this.pdfviewer.Eval($"setTimeout(()=>PDFViewerApplication.open({{url:'{pdfFilePath}'}}),50)");
        }
    }
#endif
    public void Dispose()
    {
        TryDeleteFolder(this.PdfFilesCacheFolder);
    }
}