using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Ritsukage.Tools
{
    public static class QRCodeTool
    {
        public static Image<Rgba32> Generate(string content, int scale = 5)
        {
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new(qrCodeData);
            return Image.Load<Rgba32>(qrCode.GetGraphic(scale));
        }
    }
}
