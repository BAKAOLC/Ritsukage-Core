using QRCoder;
using System.Drawing;
using System.IO;

namespace Ritsukage.Tools
{
    public static class QRCodeTool
    {
        public static Bitmap Generate(string content, int scale = 5)
        {
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new(qrCodeData);
            return (Bitmap)Image.FromStream(new MemoryStream(qrCode.GetGraphic(scale)));
        }
    }
}
