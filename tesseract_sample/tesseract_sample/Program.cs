using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tesseract;

namespace tesseract_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            string resizedPath = Resize(
                @"C:\\Users\\easyrosie\\Downloads\\number.png",
                @"C:\\Users\\easyrosie\\Downloads\\number_resized.png");

            Bitmap img = new Bitmap(@"C:\\Users\\easyrosie\\Downloads\\number_v.png");
            //img = new Bitmap(ApplyFilter(img, 2));

            var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);
            var tests = ocr.Process(img);
            Console.WriteLine(tests.GetText());
            Console.ReadKey();
        }


        static Bitmap ApplyFilter(Bitmap src, int amp)
        {
            //Bitmap은 회색조(Gray)로 영상을 바꾼 소스를 얻어 내며,
            //amp는 사용자에게 입력받은 값으로 출력 레벨을 결정하는데 사용되는 수이다.

            int i, j, iColorValue;



            // 라프라시안 필터
            int[] iFilter = new int[] { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
            int[,] iArrayValue = new int[src.Width, src.Height];

            System.Drawing.Color[] cArrayColor = new System.Drawing.Color[9]; // 색정보의 배열 중간점을 기준으로

            //라프라시안 필터링 적용할 픽셀들
            System.Drawing.Color color;
            
            // 화상에 대한 필터 처리
            // 각각 너비와 길이에 대하여 -1을 하는 이유는 맨 마지막 pixel을
            // 기준으로 잡을 수 없기 때문
            for (i = 1; i < src.Width - 1; i++)

                for (j = 1; j < src.Height - 1; j++)

                {

                    cArrayColor[0] = src.GetPixel(i - 1, j - 1);

                    cArrayColor[1] = src.GetPixel(i - 1, j);

                    cArrayColor[2] = src.GetPixel(i - 1, j + 1);

                    cArrayColor[3] = src.GetPixel(i, j - 1);

                    cArrayColor[4] = src.GetPixel(i, j);

                    cArrayColor[5] = src.GetPixel(i, j + 1);

                    cArrayColor[6] = src.GetPixel(i + 1, j - 1);

                    cArrayColor[7] = src.GetPixel(i + 1, j);

                    cArrayColor[8] = src.GetPixel(i + 1, j + 1);



                    // 필터 처리

                    iColorValue = iFilter[0] * cArrayColor[0].R + iFilter[1] * cArrayColor[1].R +

                    iFilter[2] * cArrayColor[2].R + iFilter[3] *

                    cArrayColor[3].R + iFilter[4] * cArrayColor[4].R +

                    iFilter[5] * cArrayColor[5].R + iFilter[6] *

                    cArrayColor[6].R + iFilter[7] * cArrayColor[7].R +

                    iFilter[8] * cArrayColor[8].R;



                    //출력 레벨에 따라서 각기 다른 결과물이 나올 수 있다.

                    iColorValue = amp * iColorValue;   // 출력 레벨의 설정

                    // iColorValue가 0보다 작은 경우

                    if (iColorValue < 0)

                        iColorValue = -iColorValue;     // 정의값에 변환

                    // iColorValue가255보다 클 경우

                    if (iColorValue > 255)

                        iColorValue = 255;     // iColorValue를 255으로 설정

                    iArrayValue[i, j] = iColorValue;      // iColorValue의 설정

                }

            // 필터 처리 결과 출력
            for (i = 1; i < src.Width - 1; i++)
                for (j = 1; j < src.Height - 1; j++)
                {
                    color = System.Drawing.Color.FromArgb(
                        iArrayValue[i, j], iArrayValue[i, j],
                        iArrayValue[i, j]);

                    // iArrayValue 값에 의한 색 설정
                    src.SetPixel(i, j, color);   // 픽셀의 색 설정
                }

            //pictureBox1.Image = bBitmap;   // 변경 결과 출력
            return src;
        }

        static string Resize(string sourcePath, string resizedPath)
        {
            string strFileName = sourcePath;
            string strThumbnail = resizedPath;
            byte[] baSource = File.ReadAllBytes(strFileName);
            using (Stream streamPhoto = new MemoryStream(baSource))
            {
                BitmapFrame bfPhoto = ReadBitmapFrame(streamPhoto);

                int nThumbnailSize = 300, nWidth, nHeight;
                if (bfPhoto.Width > bfPhoto.Height)
                {
                    nWidth = nThumbnailSize;
                    nHeight = (int)(bfPhoto.Height * nThumbnailSize / bfPhoto.Width);
                }
                else
                {
                    nHeight = nThumbnailSize;
                    nWidth = (int)(bfPhoto.Width * nThumbnailSize / bfPhoto.Height);
                }
                BitmapFrame bfResize = FastResize(bfPhoto, nWidth, nHeight);
                byte[] baResize = ToByteArray(bfResize);

                BitmapSource sharpen = SharpenFixedByCanvas(bfPhoto, 300, 300);
                var sharpenBytes = GetBytesFromBitmapSource(sharpen);
                var bytes = GetBytesFromBitmapSourceWithEncoder(sharpen);
                //BitmapFrame sharpenBitmap = BitmapFrame.Create(new MemoryStream(sharpenBytes), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                File.WriteAllBytes(@"Thumbnails\" + Path.GetFileNameWithoutExtension(strThumbnail) + ".png", baResize);

                File.WriteAllBytes(@"Thumbnails\" + "sharpen" + ".png", bytes);

                return @"Thumbnails\" + Path.GetFileNameWithoutExtension(strThumbnail) + ".png";
                //return @"Thumbnails\sharpen.png";
            }
        }

        static byte[] GetBytesFromBitmapSourceWithEncoder(BitmapSource bs)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.QualityLevel = 100;
            // byte[] bit = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(bs));
                encoder.Save(stream);
                byte[] bit = stream.ToArray();
                stream.Close();

                return bit;
            }
        }

        static byte[] GetBytesFromBitmapSource(BitmapSource bmp)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * ((bmp.Format.BitsPerPixel + 7) / 8);

            byte[] pixels = new byte[height * stride];

            bmp.CopyPixels(pixels, stride, 0);

            return pixels;
        }

        private static BitmapFrame FastResize(BitmapFrame bfPhoto, int nWidth, int nHeight)
        {
            TransformedBitmap tbBitmap = new TransformedBitmap(bfPhoto, 
                new ScaleTransform(nWidth / bfPhoto.Width, nHeight / bfPhoto.Height, 0, 0));
            return BitmapFrame.Create(tbBitmap);
        }

        private static byte[] ToByteArray(BitmapFrame bfResize)
        {
            using (MemoryStream msStream = new MemoryStream())
            {
                PngBitmapEncoder pbdDecoder = new PngBitmapEncoder();
                pbdDecoder.Frames.Add(bfResize);
                pbdDecoder.Save(msStream);
                return msStream.ToArray();
            }
        }

        private static BitmapFrame ReadBitmapFrame(Stream streamPhoto)
        {
            BitmapDecoder bdDecoder = BitmapDecoder.Create(
                streamPhoto,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.None);
            return bdDecoder.Frames[0];
        }

        private static BitmapSource SharpenFixedByCanvas(BitmapSource input, int width, int height)
        {
            if (input.PixelWidth == width && input.PixelHeight == height)
                return input;

            if (input.Format != PixelFormats.Bgra32 || input.Format != PixelFormats.Pbgra32)
                input = new FormatConvertedBitmap(input, PixelFormats.Bgra32, null, 0);

            //Use the same scale for x and y to keep aspect ratio.
            double scale = Math.Min((double)width / input.PixelWidth, height / (double)input.PixelHeight);

            int x = (int)Math.Round((width - (input.PixelWidth * scale)) / 2);
            int y = (int)Math.Round((height - (input.PixelHeight * scale)) / 2);

            var scaled = new TransformedBitmap(input, new ScaleTransform(scale, scale));
            var stride = scaled.PixelWidth * (scaled.Format.BitsPerPixel / 8);

            #region Make a WhiteCanvas
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.White);

            BitmapPalette palette = new BitmapPalette(colors);
            System.Windows.Media.PixelFormat pf =
                System.Windows.Media.PixelFormats.Indexed1;
            int strideWhite = width / pf.BitsPerPixel;

            byte[] pixels = new byte[height * strideWhite];

            for (int i = 0; i < height * strideWhite; ++i)
            {
                pixels[i] = 0x00;
            }
            BitmapSource whiteimage = BitmapSource.Create(
                    width,
                    height,
                    96,
                    96,
                    pf,
                    palette,
                    pixels,
                    strideWhite);

            #endregion

            #region ResizeImage + WhiteCanvas
            // Create a render bitmap and push the surface to it
            var target = new RenderTargetBitmap(width, height, whiteimage.DpiX, whiteimage.DpiY, PixelFormats.Default);
            var targetVisual = new DrawingVisual();
            var targetContext = targetVisual.RenderOpen();
            //targetContext.DrawImage(scaled, new System.Windows.Rect(0, 0, scaled.Width, scaled.Height));            //ImageDrawing
            targetContext.DrawImage(scaled, new System.Windows.Rect(0, 0, scaled.PixelWidth, scaled.PixelHeight));    //ImageDrawing
            targetContext.Close();
            target.Render(targetVisual);

            // Adding those two render bitmap to the same drawing visual
            var resultBitmap = new RenderTargetBitmap(width, height, whiteimage.DpiX, whiteimage.DpiY, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawImage(whiteimage, new System.Windows.Rect(0, 0, width, height));
            int posTx = (whiteimage.PixelWidth - scaled.PixelWidth) / 2;
            int posTy = (whiteimage.PixelHeight - scaled.PixelHeight) / 2;
            dc.DrawImage(target, new System.Windows.Rect(posTx, posTy, width, height));
            dc.Close();
            resultBitmap.Render(dv);
            #endregion

            #region MediaBitmap to DrawingBitmap - Result Image
            MemoryStream ms = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(resultBitmap));
            encoder.Save(ms);
            ms.Flush();
            System.Drawing.Image gdiImage = System.Drawing.Image.FromStream(ms);
            #endregion

            return ToBitmapImage(ApplySharpen((double)width, (System.Drawing.Bitmap)gdiImage));  //ShapenImage

            //return scaled;
            //return resultBitmap;

        }

        public static BitmapImage ToBitmapImage(System.Drawing.Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        public static Bitmap ApplySharpen(double weight, Bitmap bitmapImage)
        {
            ConvolutionMatrix matrix = new ConvolutionMatrix(3);
            matrix.SetAll(1);
            matrix.Matrix[0, 0] = 0;
            matrix.Matrix[1, 0] = -2;
            matrix.Matrix[2, 0] = 0;
            matrix.Matrix[0, 1] = -2;
            matrix.Matrix[1, 1] = weight;
            matrix.Matrix[2, 1] = -2;
            matrix.Matrix[0, 2] = 0;
            matrix.Matrix[1, 2] = -2;
            matrix.Matrix[2, 2] = 0;
            matrix.Factor = weight - 8;
            return Convolution3x3(bitmapImage, matrix);

        }

        public static Bitmap Convolution3x3(Bitmap b, ConvolutionMatrix m)
        {
            Bitmap newImg = (Bitmap)b.Clone();
            System.Drawing.Color[,] pixelColor = new System.Drawing.Color[3, 3];
            int A, R, G, B;

            for (int y = 0; y < b.Height - 2; y++)
            {
                for (int x = 0; x < b.Width - 2; x++)
                {
                    pixelColor[0, 0] = b.GetPixel(x, y);
                    pixelColor[0, 1] = b.GetPixel(x, y + 1);
                    pixelColor[0, 2] = b.GetPixel(x, y + 2);
                    pixelColor[1, 0] = b.GetPixel(x + 1, y);
                    pixelColor[1, 1] = b.GetPixel(x + 1, y + 1);
                    pixelColor[1, 2] = b.GetPixel(x + 1, y + 2);
                    pixelColor[2, 0] = b.GetPixel(x + 2, y);
                    pixelColor[2, 1] = b.GetPixel(x + 2, y + 1);
                    pixelColor[2, 2] = b.GetPixel(x + 2, y + 2);

                    A = pixelColor[1, 1].A;

                    R = (int)((((pixelColor[0, 0].R * m.Matrix[0, 0]) +
                                 (pixelColor[1, 0].R * m.Matrix[1, 0]) +
                                 (pixelColor[2, 0].R * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].R * m.Matrix[0, 1]) +
                                 (pixelColor[1, 1].R * m.Matrix[1, 1]) +
                                 (pixelColor[2, 1].R * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].R * m.Matrix[0, 2]) +
                                 (pixelColor[1, 2].R * m.Matrix[1, 2]) +
                                 (pixelColor[2, 2].R * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (R < 0)
                    {
                        R = 0;
                    }
                    else if (R > 255)
                    {
                        R = 255;
                    }

                    G = (int)((((pixelColor[0, 0].G * m.Matrix[0, 0]) +
                                 (pixelColor[1, 0].G * m.Matrix[1, 0]) +
                                 (pixelColor[2, 0].G * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].G * m.Matrix[0, 1]) +
                                 (pixelColor[1, 1].G * m.Matrix[1, 1]) +
                                 (pixelColor[2, 1].G * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].G * m.Matrix[0, 2]) +
                                 (pixelColor[1, 2].G * m.Matrix[1, 2]) +
                                 (pixelColor[2, 2].G * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (G < 0)
                    {
                        G = 0;
                    }
                    else if (G > 255)
                    {
                        G = 255;
                    }

                    B = (int)((((pixelColor[0, 0].B * m.Matrix[0, 0]) +
                                 (pixelColor[1, 0].B * m.Matrix[1, 0]) +
                                 (pixelColor[2, 0].B * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].B * m.Matrix[0, 1]) +
                                 (pixelColor[1, 1].B * m.Matrix[1, 1]) +
                                 (pixelColor[2, 1].B * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].B * m.Matrix[0, 2]) +
                                 (pixelColor[1, 2].B * m.Matrix[1, 2]) +
                                 (pixelColor[2, 2].B * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (B < 0)
                    {
                        B = 0;
                    }
                    else if (B > 255)
                    {
                        B = 255;
                    }
                    newImg.SetPixel(x + 1, y + 1, System.Drawing.Color.FromArgb(A, R, G, B));
                }
            }
            return newImg;
        }

    }

    public class ConvolutionMatrix
    {
        public int MatrixSize = 3;

        public double[,] Matrix;
        public double Factor = 1;
        public double Offset = 1;

        public ConvolutionMatrix(int size)
        {
            MatrixSize = 3;
            Matrix = new double[size, size];
        }

        public void SetAll(double value)
        {
            for (int i = 0; i < MatrixSize; i++)
            {
                for (int j = 0; j < MatrixSize; j++)
                {
                    Matrix[i, j] = value;
                }
            }
        }
    }
}
