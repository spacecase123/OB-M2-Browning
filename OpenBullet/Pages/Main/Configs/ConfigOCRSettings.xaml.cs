using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tesseract;

namespace OpenBullet
{
    /// <summary>
    /// Interaction logic for ConfigOCRSettings.xaml
    /// </summary>
    ///
    
    public partial class ConfigOCRSettings : System.Windows.Controls.Page
    {
        private RuriLib.ConfigSettings vm = Globals.mainWindow.ConfigsPage.CurrentConfig.Config.Settings;

        private WebRequest request;
        private Pix OCR;
        private Bitmap captcha;
        private Bitmap appliedCaptcha;

        public ConfigOCRSettings()
        {
            InitializeComponent();
            DataContext = Globals.mainWindow.ConfigsPage.CurrentConfig.Config.Settings;
        }

        //int correct = 0;
        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        
        public void DoOCR()
        {
            var OCRTess = new TesseractEngine(@".\tessdata", "eng", EngineMode.TesseractOnly);

            var result = OCRTess.Process(GetOCRImage()).GetText();
            //if (result.ToLower().Contains("c") && result.ToLower().Contains("h") && result.ToLower().Contains("3") && result.ToLower().Contains("n"))
            //correct++;
            OCRResult.Text = result;
        }

        public Bitmap CreateNonIndexedImage(Bitmap src)
        {
            Bitmap newImage = new Bitmap(src.Width, src.Height);
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.DrawImage(src, 0, 0);
            }
            return newImage;
        }

        public Pix GetOCRImage()
        {
            if (vm.Base64 == "")
            {
                request = WebRequest.Create(@OCRUrl.Text);

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    captcha = (Bitmap)Bitmap.FromStream(stream);
                    appliedCaptcha = CreateNonIndexedImage(captcha);
                }
                //System.Windows.Forms.MessageBox.Show(appliedCaptcha.Width + " < W | H > " + appliedCaptcha.Height);
                OcrImage.Source = ImageSourceFromBitmap(captcha);

                //appliedCaptcha = CreateNonIndexedImage(appliedCaptcha);

                if (vm.ContrastGamma)
                {
                    vm.Contrast = float.Parse(ContrastAmt.Text);
                    vm.Gamma = float.Parse(GammaAmt.Text);
                    vm.Brightness = float.Parse(BrightnessAmt.Text);
                    appliedCaptcha = SetContrastGamma(appliedCaptcha);
                }

                if (vm.Saturate)
                    appliedCaptcha = SetSaturation(appliedCaptcha);

                if (vm.RemoveNoise)
                {
                    vm.Threshold = float.Parse(ThresholdAmt.Text);
                    appliedCaptcha = AdjustThreshold(appliedCaptcha, vm.Threshold); //appliedCaptcha = RemoveNoise(appliedCaptcha);
                }

                if (vm.Transparent)
                    appliedCaptcha = SetTransparent(appliedCaptcha);

                if (vm.Grayscale)
                    appliedCaptcha = ToGrayScale(appliedCaptcha);

                if (vm.OnlyShow)
                    appliedCaptcha = SetOnlyShow(appliedCaptcha);

                if (vm.RemoveLines)
                    appliedCaptcha = RemoveImageLines(appliedCaptcha);

                //if (vm.Contour)
                //appliedCaptcha = ContourImage(appliedCaptcha);

                if (vm.Dilate)
                    appliedCaptcha = Dilate(appliedCaptcha);

                AppliedImage.Source = ImageSourceFromBitmap(appliedCaptcha);
                OCR = PixConverter.ToPix(appliedCaptcha);
                return OCR;
            }
            else
            {
                byte[] imageBytes = Convert.FromBase64String(vm.Base64);

                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    captcha = (Bitmap)Bitmap.FromStream(ms);
                    appliedCaptcha = CreateNonIndexedImage(captcha);

                    OcrImage.Source = ImageSourceFromBitmap(captcha);

                    if (vm.ContrastGamma)
                    {
                        vm.Contrast = float.Parse(ContrastAmt.Text);
                        vm.Gamma = float.Parse(GammaAmt.Text);
                        vm.Brightness = float.Parse(BrightnessAmt.Text);
                        appliedCaptcha = SetContrastGamma(appliedCaptcha);
                    }

                    if (vm.Saturate)
                        appliedCaptcha = SetSaturation(appliedCaptcha);

                    if (vm.RemoveNoise)
                    {
                        vm.Threshold = float.Parse(ThresholdAmt.Text);
                        appliedCaptcha = AdjustThreshold(appliedCaptcha, vm.Threshold); //appliedCaptcha = RemoveNoise(appliedCaptcha);
                    }

                    if (vm.Transparent)
                        appliedCaptcha = SetTransparent(appliedCaptcha);

                    if (vm.Grayscale)
                        appliedCaptcha = ToGrayScale(appliedCaptcha);

                    if (vm.OnlyShow)
                        appliedCaptcha = SetOnlyShow(appliedCaptcha);

                    if (vm.RemoveLines)
                    {
                        vm.RemoveLinesMin = Int32.Parse(PixelAmtMin.Text);
                        vm.RemoveLinesMax = Int32.Parse(PixelAmtMax.Text);
                        appliedCaptcha = RemoveImageLines(appliedCaptcha);
                    }

                    //if (vm.Contour)
                    appliedCaptcha = ContourImage(appliedCaptcha);

                    if (vm.Dilate)
                        appliedCaptcha = Dilate(appliedCaptcha);

                    //Rectangle cropping = new Rectangle();
                    //cropping.Width = appliedCaptcha.Width - 20;
                    //cropping.Height = appliedCaptcha.Height - 15;
                    //appliedCaptcha = cropImage(appliedCaptcha, cropping);
                }
                AppliedImage.Source = ImageSourceFromBitmap(appliedCaptcha);
                OCR = PixConverter.ToPix(appliedCaptcha);
                return OCR;
            }
        }

        public Bitmap ContourImage(Bitmap original)
        {
            System.Drawing.Color current;
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    current = original.GetPixel(x, y);

                    if (original.GetPixel(x - 1, y) == current &&
                        original.GetPixel(x, y - 1) == current &&
                        original.GetPixel(x + 1, y) == current &&
                        original.GetPixel(x, y + 1) == current &&
                        original.GetPixel(x - 1, y - 1) == current &&
                        original.GetPixel(x + 1, y + 1) == current &&
                        original.GetPixel(x - 1, y + 1) == current &&
                        original.GetPixel(x + 1, y - 1) == current)
                        System.Windows.Forms.MessageBox.Show(original.GetPixel(x, y).ToString());
                }
            }
            return original;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                //for (int i = 0; i < 1000; i++)
                //{
                DoOCR();
                //}
                //System.Windows.Forms.MessageBox.Show("Correct amount: " + correct + "\nOut of: 1000");
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.ToString()); }
        }

        private static Bitmap cropImage(Bitmap img, Rectangle cropArea)
        {
            Bitmap bmpCrop = img.Clone(cropArea, img.PixelFormat);
            return bmpCrop;
        }

        
        public Bitmap SetSaturation(Bitmap original)
        {
            int saturationAmt = int.Parse(SaturationAmt.Text);
            int saturation = saturationAmt;

            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    int red = (original.GetPixel(x, y).R + saturation > 255 ? 255 : (original.GetPixel(x, y).R + saturation)); //Int32.Parse(original.GetPixel(x, y).R*0.5);
                    int green = (original.GetPixel(x, y).G + saturation > 255 ? 255 : (original.GetPixel(x, y).G + saturation)); ;
                    int blue = (original.GetPixel(x, y).B + saturation > 255 ? 255 : (original.GetPixel(x, y).B + saturation)); ;
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(original.GetPixel(x, y).A, red, green, blue);
                    original.SetPixel(x, y, newColor);
                }
            }
            return original;
        }

        
        public Bitmap SetContrastGamma(Bitmap original)
        {
            float contrastAmt = float.Parse(ContrastAmt.Text);
            float gammaAmt = float.Parse(GammaAmt.Text);
            float brightnessAmt = float.Parse(BrightnessAmt.Text);

            Bitmap adjustedImage = original;
            float brightness = brightnessAmt; // no change in brightness
            float contrast = contrastAmt; // twice the contrast
            float gamma = gammaAmt; // no change in gamma

            float adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray ={
        new float[] {contrast, 0, 0, 0, 0}, // scale red
        new float[] {0, contrast, 0, 0, 0}, // scale green
        new float[] {0, 0, contrast, 0, 0}, // scale blue
        new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
        new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            Graphics g = Graphics.FromImage(adjustedImage);
            g.DrawImage(original, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height)
                , 0, 0, original.Width, original.Height,
                GraphicsUnit.Pixel, imageAttributes);
            return adjustedImage;
        }

        
        public Bitmap ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            System.Drawing.Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    Bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(rgb, rgb, rgb));
                }
            return Bmp;
        }

        
        private Bitmap AdjustThreshold(Bitmap image, float threshold)
        {
            // Make the result bitmap.
            Bitmap bm = new Bitmap(image.Width, image.Height);

            // Make the ImageAttributes object and set the threshold.
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetThreshold(threshold);

            // Draw the image onto the new bitmap
            // while applying the new ColorMatrix.
            System.Drawing.Point[] points =
            {
                new System.Drawing.Point(0, 0),
                new System.Drawing.Point(image.Width, 0),
                new System.Drawing.Point(0, image.Height),
            };
            Rectangle rect =
                new Rectangle(0, 0, image.Width, image.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }

            // Return the result.
            return bm;
        }

        
        public Bitmap RemoveImageLines(Bitmap Bmp)
        {
            int amtMin = Int32.Parse(PixelAmtMin.Text);
            int amtMax = Int32.Parse(PixelAmtMax.Text);
            System.Drawing.Color c;
            System.Drawing.Color compare1;
            System.Drawing.Color compare2;

            //    for (int i = 0; i < Bmp.Width; i++)
            //    {
            //        for (int j = 0; j < Bmp.Height; j++)
            //        {
            //            c = Bmp.GetPixel(i, j);
            //            if (Bmp.GetPixel(i, j) == c)
            //            {
            //                if (i > 0 && Bmp.GetPixel(i - 1, j) != c) Bmp.SetPixel(i - 1, j, System.Drawing.Color.Transparent);
            //                if (j > 0 && Bmp.GetPixel(i, j - 1) != c) Bmp.SetPixel(i, j - 1, System.Drawing.Color.Transparent);
            //                if (i + 1 < Bmp.Width && Bmp.GetPixel(i + 1, j) != c) Bmp.SetPixel(i + 1,j, System.Drawing.Color.Transparent);
            //                if (j + 1 < Bmp.Height && Bmp.GetPixel(i, j + 1) != c) Bmp.SetPixel(i, j + 1, System.Drawing.Color.Transparent);
            //}
            //        }
            //    }
            //    for (int i = 0; i < Bmp.Width; i++)
            //    {
            //        for (int j = 0; j < Bmp.Height; j++)
            //        {
            //            c = Bmp.GetPixel(i, j);
            //            if (Bmp.GetPixel(i, j) != c)
            //            {
            //                Bmp.SetPixel(i, j, System.Drawing.Color.Transparent);
            //            }
            //        }
            //    }
            //    return Bmp;

            //for (int x = 0; x < Bmp.Width; x++)
            //    for (int y = 0; y < Bmp.Height; y++)
            //    {
            //        c = Bmp.GetPixel(x, y);
            //        if (x - (amtMax + 1) > 0 && y - (amtMax + 1) > 0)
            //        {
            //            compare1 = Bmp.GetPixel(x - amtMin, y - amtMin);
            //            compare11 = Bmp.GetPixel(x - (amtMin - 1), y - (amtMin - 1));
            //            compare2 = Bmp.GetPixel(x - amtMax, y - amtMax);
            //            compare22 = Bmp.GetPixel(x - (amtMax - 1), y - (amtMax - 1));

            //            if (compare1 != compare2)
            //            {
            //                if (compare1 != c && compare11 != c)
            //                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
            //                if (compare2 != c && compare22 != c)
            //                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
            //            }
            //        }
            //        if (x + (amtMax + 1) < Bmp.Width && y + (amtMax + 1) < Bmp.Height)
            //        {
            //            compare1 = Bmp.GetPixel(x + amtMin, y + amtMin);
            //            compare11 = Bmp.GetPixel(x + (amtMin - 1), y + (amtMin - 1));
            //            compare2 = Bmp.GetPixel(x + amtMax, y + amtMax);
            //            compare22 = Bmp.GetPixel(x + (amtMax - 1), y + (amtMax - 1));

            //            if (compare1 != compare2)
            //            {
            //                if (compare1 != c && compare11 != c)
            //                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
            //                if (compare2 != c && compare22 != c)
            //                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
            //            }
            //        }
            //    }

            for (int x = 0; x < Bmp.Width; x++)
                for (int y = 0; y < Bmp.Height; y++)
                {
                    c = Bmp.GetPixel(x, y);

                    if (x - (amtMax + 1) > 0 && y - (amtMax + 1) > 0)
                    {
                        compare1 = Bmp.GetPixel(x - amtMin, y - amtMin);
                        compare2 = Bmp.GetPixel(x - amtMax, y - amtMax);
                        if (compare1 == compare2)
                            if (c != compare1)
                            {
                                if (compare1 != Bmp.GetPixel(x - (amtMin - 1), y - (amtMin - 1)))
                                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
                                if (compare2 != Bmp.GetPixel(x - (amtMax + 1), y - (amtMax + 1)))
                                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
                            }
                    }
                    if (x + (amtMax + 1) < Bmp.Width && y + (amtMax + 1) < Bmp.Height)
                    {
                        compare1 = Bmp.GetPixel(x + amtMin, y + amtMin);
                        compare2 = Bmp.GetPixel(x + amtMax, y + amtMax);
                        if (compare1 == compare2)
                            if (c != compare1)
                            {
                                if (compare1 != Bmp.GetPixel(x + (amtMin - 1), y + (amtMin - 1)))
                                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
                                if (compare2 != Bmp.GetPixel(x + (amtMax + 1), y + (amtMax + 1)))
                                    Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
                            }
                    }

                    //if (x - amtMax > 0 && y - amtMax > 0)
                    //{
                    //    compare1 = Bmp.GetPixel(x - amtMin, y - amtMin);
                    //    compare2 = Bmp.GetPixel(x - amtMax, y - amtMax);
                    //    if (compare1 == compare2)
                    //        if (c != compare1) //Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
                    //            if (x + amtMax < Bmp.Width && y + amtMax < Bmp.Height)
                    //            {
                    //                compare1 = Bmp.GetPixel(x + amtMin, y + amtMin);
                    //                compare2 = Bmp.GetPixel(x + amtMax, y + amtMax);
                    //                if (compare1 == compare2)
                    //                    if (c != compare1)
                    //                        Bmp.SetPixel(x, y, System.Drawing.Color.Transparent);
                    //            }
                    //}
                }

            return Bmp;
        }

        //
        //public Bitmap RemoveNoise(Bitmap Bmp)
        //{
        //    return Bmp;
        //}

        
        public Bitmap SetTransparent(Bitmap Bmp)
        {
            foreach (System.Drawing.Color i in ColorsList.Items)
                Bmp.MakeTransparent(i);
            return Bmp;
        }

        
        public Bitmap SetOnlyShow(Bitmap Bmp)
        {
            for (int n = 0; n < (Bmp.Width - 1); n++)
                for (int m = 0; m < (Bmp.Height - 1); m++)
                    foreach (System.Drawing.Color i in ColorsListShow.Items)
                        if (Bmp.GetPixel(n, m) != i)//if (!Bmp.GetPixel(n, m).ToString().Contains("B=0") && !Bmp.GetPixel(n, m).ToString().Contains("R=0") && !Bmp.GetPixel(n, m).ToString().Contains("G=0"))
                            if (Math.Abs(Bmp.GetPixel(n, m).R - i.R) > vm.DiffKeep || Math.Abs(Bmp.GetPixel(n, m).G - i.G) > vm.DiffKeep || Math.Abs(Bmp.GetPixel(n, m).B - i.B) > vm.DiffKeep)
                            {
                                //if (n-3 > 1 && m-3 > 1 && n-3 < Bmp.Width && m-3 < Bmp.Height && Bmp.GetPixel(n-3, m-3) == Bmp.GetPixel(n+3, m+3))
                                //{
                                //    Bmp.SetPixel(n, m, Bmp.GetPixel(n - 3, m - 3));
                                //}
                                //else
                                Bmp.SetPixel(n, m, System.Drawing.Color.Transparent);
                            }

            return Bmp;
        }

        
        public Bitmap Dilate(Bitmap SrcImage)
        {
            // Create Destination bitmap.
            Bitmap tempbmp = new Bitmap(SrcImage.Width, SrcImage.Height);

            // Take source bitmap data.
            BitmapData SrcData = SrcImage.LockBits(new Rectangle(0, 0,
                SrcImage.Width, SrcImage.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Take destination bitmap data.
            BitmapData DestData = tempbmp.LockBits(new Rectangle(0, 0, tempbmp.Width,
                tempbmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Element array to used to dilate.
            byte[,] sElement = new byte[5, 5] {
        {0,0,1,0,0},
        {0,1,1,1,0},
        {1,1,1,1,1},
        {0,1,1,1,0},
        {0,0,1,0,0}
    };

            // Element array size.
            int size = 5;
            byte max, clrValue;
            int radius = size / 2;
            int ir, jr;

            unsafe
            {
                // Loop for Columns.
                for (int colm = radius; colm < DestData.Height - radius; colm++)
                {
                    // Initialise pointers to at row start.
                    byte* ptr = (byte*)SrcData.Scan0 + (colm * SrcData.Stride);
                    byte* dstPtr = (byte*)DestData.Scan0 + (colm * SrcData.Stride);

                    // Loop for Row item.
                    for (int row = radius; row < DestData.Width - radius; row++)
                    {
                        max = 0;
                        clrValue = 0;

                        // Loops for element array.
                        for (int eleColm = 0; eleColm < 5; eleColm++)
                        {
                            ir = eleColm - radius;
                            byte* tempPtr = (byte*)SrcData.Scan0 +
                                ((colm + ir) * SrcData.Stride);

                            for (int eleRow = 0; eleRow < 5; eleRow++)
                            {
                                jr = eleRow - radius;

                                // Get neightbour element color value.
                                clrValue = (byte)((tempPtr[row * 3 + jr] +
                                    tempPtr[row * 3 + jr + 1] + tempPtr[row * 3 + jr + 2]) / 3);

                                if (max < clrValue)
                                {
                                    if (sElement[eleColm, eleRow] != 0)
                                        max = clrValue;
                                }
                            }
                        }

                        dstPtr[0] = dstPtr[1] = dstPtr[2] = max;

                        ptr += 3;
                        dstPtr += 3;
                    }
                }
            }

            // Dispose all Bitmap data.
            SrcImage.UnlockBits(SrcData);
            tempbmp.UnlockBits(DestData);

            // return dilated bitmap.
            return tempbmp;
        }

        private System.Drawing.Color textColor;
        private int posX;
        private int posY;

        private void AppliedImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(AppliedImage);
            try
            {
                posX = (int)(position.X * appliedCaptcha.Width / AppliedImage.Width);
                posY = (int)(position.Y * appliedCaptcha.Height / AppliedImage.Height);
                textColor = appliedCaptcha.GetPixel(posX, posY);
                ColorText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(textColor.A, textColor.R, textColor.G, textColor.B));
                ColorText.Text = appliedCaptcha.GetPixel((int)(posX * appliedCaptcha.Width / AppliedImage.Width), (int)(posY * appliedCaptcha.Height / AppliedImage.Height)).ToString();
            }
            catch { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ColorsList.Items.Count == 1)
                ColorsList.Items.Clear();
            else
                ColorsList.Items.Remove(ColorsList.SelectedValue);
            ColorsList.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ColorsListShow.Items.Count == 1)
                ColorsListShow.Items.Clear();
            else
                ColorsListShow.Items.Remove(ColorsListShow.SelectedValue);
            ColorsListShow.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
        }

        private void AppliedImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                ColorsList.Items.Add(textColor);
                ColorsList.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(textColor.A, textColor.R, textColor.G, textColor.B));
                //ColorsList.Items.Add(appliedCaptcha.GetPixel((int)(posX * appliedCaptcha.Width / AppliedImage.Width), (int)(posY * appliedCaptcha.Height / AppliedImage.Height)).ToString());
            }
            catch { }
        }

        private void AppliedImage_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                ColorsListShow.Items.Add(textColor);
                ColorsListShow.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(textColor.A, textColor.R, textColor.G, textColor.B));
                //ColorsList.Items.Add(appliedCaptcha.GetPixel((int)(posX * appliedCaptcha.Width / AppliedImage.Width), (int)(posY * appliedCaptcha.Height / AppliedImage.Height)).ToString());
            }
            catch { }
        }
    }
}