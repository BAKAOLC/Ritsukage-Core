using QRCoder;
using System.Drawing;

namespace Ritsukage.Tools
{
    public static class QRCodeTool
    {
        public static Bitmap Generate(string content, int scale = 5)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(scale);
        }
    }
}
