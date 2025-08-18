using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ZeiterfassungWeb.Data
{
    public abstract class PdfHelper
    {
        public static byte[] ErzeugePdf()
        {
            using var stream = new MemoryStream();
            var doc = new PdfDocument();
            var seite = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(seite);

            // Text hinzufügen
            gfx.DrawString("Turnierübersicht", new XFont("Arial", 20), XBrushes.Black, new XPoint(50, 50));
            gfx.DrawString("Zusätzliche Infos hier...", new XFont("Arial", 12), XBrushes.Gray, new XPoint(50, 100));

            doc.Save(stream);
            return stream.ToArray();
        }

    }
}
