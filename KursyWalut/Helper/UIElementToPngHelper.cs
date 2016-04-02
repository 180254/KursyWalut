using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using KursyWalut.Progress;

namespace KursyWalut.Helper
{
    public class UiElementToPngHelper : IDisposable
    {
        private readonly UIElement _uiElement;
        private readonly string _suggestedName;
        private readonly IPProgress _pprogress;

        public UiElementToPngHelper(UIElement uiElement, string suggestedName, EventHandler<int> progressSubscriber)
        {
            _suggestedName = suggestedName;
            _uiElement = uiElement;

            _pprogress = PProgress.NewMaster();
            _pprogress.ProgressChanged += progressSubscriber;
            _pprogress.ReportProgress(0.00);
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task Execute()
        {
            // select file
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeChoices = {{"PNG", new[] {".png"}}},
                SuggestedFileName = _suggestedName
            };
            var file = await savePicker.PickSaveFileAsync();
            _pprogress.ReportProgress(0.30);

            if (file != null)
            {
                // render element to bitmap
                var target = new RenderTargetBitmap();
                await target.RenderAsync(_uiElement);
                _pprogress.ReportProgress(0.40);

                // get bitmap pixels
                var pixelBuffer = await target.GetPixelsAsync();
                _pprogress.ReportProgress(0.50);

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // get png encoder for file
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    _pprogress.ReportProgress(0.60);

                    // set pixels and write
                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        (uint) target.PixelWidth,
                        (uint) target.PixelHeight,
                        96d, 96d,
                        pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                    _pprogress.ReportProgress(0.90);
                }
            }

            _pprogress.ReportProgress(1.00);
        }
    }
}