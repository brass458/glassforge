using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        string outPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\assets\glassforge.ico"));
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        var bmp16  = RenderGem(16);
        var bmp32  = RenderGem(32);
        var bmp48  = RenderGem(48);
        var bmp256 = RenderGem(256);

        using var fs = File.Create(outPath);
        WriteIco(fs, bmp16, bmp32, bmp48, bmp256);

        Console.WriteLine($"Icon written to: {outPath}");
    }

    static RenderTargetBitmap RenderGem(int size)
    {
        var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();

        double s = size;

        Point P(double nx, double ny) => new(nx * s, ny * s);

        var A = P(0.28, 0.10);
        var B = P(0.72, 0.10);
        var C = P(0.92, 0.48);
        var D = P(0.50, 0.92);
        var E = P(0.08, 0.48);
        var F = P(0.50, 0.36);

        using (var dc = visual.RenderOpen())
        {
            // Table facet: A-B-F (top highlight)
            dc.DrawGeometry(
                new LinearGradientBrush(
                    Color.FromRgb(0xE8, 0xF4, 0xFF),
                    Color.FromRgb(0xB0, 0xD0, 0xF0),
                    new Point(0.5, 0), new Point(0.5, 1)) { MappingMode = BrushMappingMode.RelativeToBoundingBox },
                null,
                MakeTri(A, B, F));

            // Left crown: A-E-F
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0x3A, 0x6E, 0xB8)), null, MakeTri(A, E, F));

            // Right crown: B-C-F
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0x5A, 0x9E, 0xE8)), null, MakeTri(B, C, F));

            // Left pavilion: E-D-F
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0x1A, 0x2E, 0x78)), null, MakeTri(E, D, F));

            // Right pavilion: C-D-F
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0x24, 0x3B, 0x8A)), null, MakeTri(C, D, F));

            // Outline (only at 32px+)
            if (size >= 32)
            {
                var outline = new PathGeometry(new[]
                {
                    new PathFigure(A, new PathSegment[]
                    {
                        new LineSegment(B, true),
                        new LineSegment(C, true),
                        new LineSegment(D, true),
                        new LineSegment(E, true),
                    }, true)
                });
                double strokeW = size <= 32 ? 0.75 : size <= 48 ? 1.0 : 1.5;
                dc.DrawGeometry(null,
                    new Pen(new SolidColorBrush(Color.FromRgb(0x8A, 0xB8, 0xE8)), strokeW),
                    outline);
            }
        }

        rtb.Render(visual);
        return rtb;
    }

    static Geometry MakeTri(Point p1, Point p2, Point p3)
        => new PathGeometry(new[]
        {
            new PathFigure(p1, new PathSegment[]
            {
                new LineSegment(p2, true),
                new LineSegment(p3, true),
            }, true)
        });

    static void WriteIco(Stream stream, params RenderTargetBitmap[] bitmaps)
    {
        int count = bitmaps.Length;
        var images = new (byte[] data, int w, int h)[count];

        for (int i = 0; i < count; i++)
        {
            int w = bitmaps[i].PixelWidth;
            if (w == 256)
                images[i] = (EncodePng(bitmaps[i]), w, bitmaps[i].PixelHeight);
            else
                images[i] = (EncodeDib(bitmaps[i]), w, bitmaps[i].PixelHeight);
        }

        using var bw = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);

        // ICONDIR
        bw.Write((short)0);     // reserved
        bw.Write((short)1);     // type: ICO
        bw.Write((short)count);

        // ICONDIRENTRY array
        int headerSize = 6 + count * 16;
        int offset = headerSize;
        foreach (var (data, w, h) in images)
        {
            bw.Write((byte)(w == 256 ? 0 : w));
            bw.Write((byte)(h == 256 ? 0 : h));
            bw.Write((byte)0);    // colorCount
            bw.Write((byte)0);    // reserved
            bw.Write((short)1);   // planes
            bw.Write((short)32);  // bitCount
            bw.Write(data.Length);
            bw.Write(offset);
            offset += data.Length;
        }

        foreach (var (data, _, _) in images)
            bw.Write(data);
    }

    static byte[] EncodePng(BitmapSource bmp)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bmp));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    static byte[] EncodeDib(BitmapSource bmp)
    {
        int w = bmp.PixelWidth;
        int h = bmp.PixelHeight;

        var conv = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
        int stride = w * 4;
        var pixels = new byte[stride * h];
        conv.CopyPixels(pixels, stride, 0);

        // Flip vertically (BMP is bottom-up)
        var flipped = new byte[pixels.Length];
        for (int row = 0; row < h; row++)
            Buffer.BlockCopy(pixels, row * stride, flipped, (h - 1 - row) * stride, stride);

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // BITMAPINFOHEADER (40 bytes)
        bw.Write(40);       // biSize
        bw.Write(w);        // biWidth
        bw.Write(h * 2);    // biHeight (doubled for ICO XOR + AND mask)
        bw.Write((short)1); // biPlanes
        bw.Write((short)32);// biBitCount
        bw.Write(0);        // biCompression = BI_RGB
        bw.Write(0);        // biSizeImage
        bw.Write(0);        // biXPelsPerMeter
        bw.Write(0);        // biYPelsPerMeter
        bw.Write(0);        // biClrUsed
        bw.Write(0);        // biClrImportant

        bw.Write(flipped);
        // AND mask (all zero — alpha channel handles transparency for 32bpp)
        int andStride = ((w + 31) / 32) * 4;
        bw.Write(new byte[andStride * h]);

        return ms.ToArray();
    }
}
