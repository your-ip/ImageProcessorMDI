using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageProcessorMDI
{
    public partial class ImageChildForm : Form
    {
        private Bitmap originalImage;
        private Bitmap currentImage;
        private string imagePath;
        private ProcessingMode processingMode;

        private int currentBrightness = 0;
        private float currentContrast = 1.0f;
        private bool isGrayscale = false;

        public ImageChildForm(string imagePath, ProcessingMode mode)
        {
            InitializeComponent();
            this.imagePath = imagePath;
            this.processingMode = mode;
            LoadImage();
            this.Text = $"{System.IO.Path.GetFileName(imagePath)} ({mode} processing)";

            brightnessValueLabel.Text = "0";
            contrastValueLabel.Text = "0%";
        }

        private void LoadImage()
        {
            try
            {
                originalImage = new Bitmap(imagePath);
                currentImage = new Bitmap(originalImage);
                pictureBox.Image = currentImage;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

                currentBrightness = 0;
                currentContrast = 1.0f;
                isGrayscale = false;
                brightnessTrackBar.Value = 0;
                contrastTrackBar.Value = 0;
                brightnessValueLabel.Text = "0";
                contrastValueLabel.Text = "0%";
                grayscaleToolStripMenuItem.Checked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void ApplyAllFilters()
        {
            if (originalImage == null) return;

            currentImage = new Bitmap(originalImage);

            if (isGrayscale) ApplyGrayscale();
            if (currentContrast != 1.0f) ApplyContrast(currentContrast);
            if (currentBrightness != 0) ApplyBrightness(currentBrightness);

            pictureBox.Image = currentImage;
        }

        private void ApplyGrayscale()
        {
            if (processingMode == ProcessingMode.Fast)
                ApplyFastGrayscale();
            else
                ApplySlowGrayscale();
        }

        private void ApplyFastGrayscale()
        {
            Rectangle rect = new Rectangle(0, 0, currentImage.Width, currentImage.Height);
            BitmapData bmpData = currentImage.LockBits(rect, ImageLockMode.ReadWrite, currentImage.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * currentImage.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int pixelSize = Image.GetPixelFormatSize(currentImage.PixelFormat) / 8;

            for (int i = 0; i < rgbValues.Length; i += pixelSize)
            {
                byte gray = (byte)(rgbValues[i] * 0.11 + rgbValues[i + 1] * 0.59 + rgbValues[i + 2] * 0.3);
                rgbValues[i] = rgbValues[i + 1] = rgbValues[i + 2] = gray;
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            currentImage.UnlockBits(bmpData);
        }

        private void ApplySlowGrayscale()
        {
            for (int y = 0; y < currentImage.Height; y++)
            {
                for (int x = 0; x < currentImage.Width; x++)
                {
                    Color pixel = currentImage.GetPixel(x, y);
                    byte gray = (byte)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    currentImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
        }

        private void BrightnessTrackBar_Scroll(object sender, EventArgs e)
        {
            currentBrightness = brightnessTrackBar.Value;
            brightnessValueLabel.Text = currentBrightness.ToString();
            ApplyAllFilters();
        }

        private void ApplyBrightness(int brightness)
        {
            if (processingMode == ProcessingMode.Fast)
                ApplyFastBrightness(brightness);
            else
                ApplySlowBrightness(brightness);
        }

        private void ApplyFastBrightness(int brightness)
        {
            Rectangle rect = new Rectangle(0, 0, currentImage.Width, currentImage.Height);
            BitmapData bmpData = currentImage.LockBits(rect, ImageLockMode.ReadWrite, currentImage.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * currentImage.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int pixelSize = Image.GetPixelFormatSize(currentImage.PixelFormat) / 8;

            for (int i = 0; i < rgbValues.Length; i += pixelSize)
            {
                for (int j = 0; j < 3; j++)
                {
                    int value = rgbValues[i + j] + brightness;
                    rgbValues[i + j] = (byte)Math.Max(0, Math.Min(255, value));
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            currentImage.UnlockBits(bmpData);
        }

        private void ApplySlowBrightness(int brightness)
        {
            for (int y = 0; y < currentImage.Height; y++)
            {
                for (int x = 0; x < currentImage.Width; x++)
                {
                    Color pixel = currentImage.GetPixel(x, y);
                    int r = Math.Max(0, Math.Min(255, pixel.R + brightness));
                    int g = Math.Max(0, Math.Min(255, pixel.G + brightness));
                    int b = Math.Max(0, Math.Min(255, pixel.B + brightness));
                    currentImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
        }

        private void ContrastTrackBar_Scroll(object sender, EventArgs e)
        {
            currentContrast = 1.0f + (contrastTrackBar.Value / 100f);
            contrastValueLabel.Text = $"{contrastTrackBar.Value}%";
            ApplyAllFilters();
        }

        private void ApplyContrast(float contrast)
        {
            if (processingMode == ProcessingMode.Fast)
                ApplyFastContrast(contrast);
            else
                ApplySlowContrast(contrast);
        }

        private void ApplyFastContrast(float contrast)
        {
            contrast = contrast * contrast;

            Rectangle rect = new Rectangle(0, 0, currentImage.Width, currentImage.Height);
            BitmapData bmpData = currentImage.LockBits(rect, ImageLockMode.ReadWrite, currentImage.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * currentImage.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int pixelSize = Image.GetPixelFormatSize(currentImage.PixelFormat) / 8;

            for (int i = 0; i < rgbValues.Length; i += pixelSize)
            {
                for (int j = 0; j < 3; j++)
                {
                    float pixel = rgbValues[i + j] / 255f;
                    pixel -= 0.5f;
                    pixel *= contrast;
                    pixel += 0.5f;
                    pixel *= 255;

                    rgbValues[i + j] = (byte)Math.Max(0, Math.Min(255, pixel));
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            currentImage.UnlockBits(bmpData);
        }

        private void ApplySlowContrast(float contrast)
        {
            contrast = contrast * contrast;

            for (int y = 0; y < currentImage.Height; y++)
            {
                for (int x = 0; x < currentImage.Width; x++)
                {
                    Color pixel = currentImage.GetPixel(x, y);

                    float r = pixel.R / 255f;
                    r -= 0.5f;
                    r *= contrast;
                    r += 0.5f;
                    r *= 255;
                    r = Math.Max(0, Math.Min(255, r));

                    float g = pixel.G / 255f;
                    g -= 0.5f;
                    g *= contrast;
                    g += 0.5f;
                    g *= 255;
                    g = Math.Max(0, Math.Min(255, g));

                    float b = pixel.B / 255f;
                    b -= 0.5f;
                    b *= contrast;
                    b += 0.5f;
                    b *= 255;
                    b = Math.Max(0, Math.Min(255, b));

                    currentImage.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }
        }

        private void GrayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isGrayscale = !isGrayscale;
            grayscaleToolStripMenuItem.Checked = isGrayscale;
            ApplyAllFilters();
        }

        private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                currentBrightness = 0;
                currentContrast = 1.0f;
                isGrayscale = false;
                brightnessTrackBar.Value = 0;
                contrastTrackBar.Value = 0;
                grayscaleToolStripMenuItem.Checked = false;

                brightnessValueLabel.Text = "0";
                contrastValueLabel.Text = "0%";

                currentImage = new Bitmap(originalImage);
                pictureBox.Image = currentImage;
            }
        }

        public void SaveImage()
        {
            if (currentImage == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp",
                Title = "Сохранить изображение"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ImageFormat format = ImageFormat.Jpeg;
                string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();

                switch (ext)
                {
                    case ".png": format = ImageFormat.Png; break;
                    case ".bmp": format = ImageFormat.Bmp; break;
                }

                try
                {
                    currentImage.Save(saveFileDialog.FileName, format);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}