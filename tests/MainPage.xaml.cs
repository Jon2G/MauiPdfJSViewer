
namespace MauiPdfJSViewerTests
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

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
    }

}
