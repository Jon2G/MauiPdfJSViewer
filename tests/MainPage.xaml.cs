
namespace MauiPdfJSViewerTests
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked_FromAssets(object sender, EventArgs e)
        {
            await pdfJsViewer.LoadPdfFromMauiAssets("sample.pdf");
        }

        private async void Button_Clicked_FromStream(object sender, EventArgs e)
        {
            await pdfJsViewer.LoadPdfFromSteam(() => FileSystem.OpenAppPackageFileAsync("sample.pdf"));
        }

        private async void Button_Clicked_FromPath(object sender, EventArgs e)
        {
            string fileName = $"{Guid.NewGuid():N}.pdf";

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            using var pdfStream = await FileSystem.OpenAppPackageFileAsync("sample.pdf");
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                pdfStream.CopyTo(file);

            await pdfJsViewer.LoadPdfFromPath(filePath);
        }
    }

}
