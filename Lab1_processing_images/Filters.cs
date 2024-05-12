using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Lab1_processing_images
{
    abstract class Filters
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
    }

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                                                255 - sourceColor.G,
                                                255 - sourceColor.B);
            return resultColor;
        }
    }

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)((float)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B));
            Color resultColor = Color.FromArgb(intensity, intensity, intensity);
            return resultColor;
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)((float)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B));
            double resultR = intensity + 2 * 20;
            double resultG = intensity + (0.5 * 20);
            double resultB = intensity - 1 * 20;
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255));
        }
    }

    class BrightFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double resultR = sourceColor.R + 80;
            double resultG = sourceColor.G + 80;
            double resultB = sourceColor.B + 80;
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255));
        }
    }

    class TransferFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            int idI = Clamp(i, 0, sourceImage.Width - 1);
            int idJ = Clamp(j + 50, 0, sourceImage.Height - 1);
            if (idJ == sourceImage.Height - 1) { return Color.FromArgb(0, 0, 0); }
            Color resultColor = sourceImage.GetPixel(idI, idJ);
            return resultColor;
        }
    }

    class TurnFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2;
            double corner = 1;
            int idI = Clamp((int)((i - x0) * Math.Cos(corner) - (j - y0) * Math.Sin(corner)) + x0, 0, sourceImage.Width - 1);
            int idJ = Clamp((int)((i - x0) * Math.Sin(corner) + (j - y0) * Math.Cos(corner)) + y0, 0, sourceImage.Height - 1);
            if (idJ == sourceImage.Height - 1) { return Color.FromArgb(0, 0, 0); }
            Color resultColor = sourceImage.GetPixel(idI, idJ);
            return resultColor;
        }
    }

    class WaveFilterVertical : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            int idI = Clamp(i + (int)(20 * Math.Sin(Math.PI * j / 30)), 0, sourceImage.Width - 1);
            int idJ = Clamp(j, 0, sourceImage.Height - 1);
            if (idJ == sourceImage.Height - 1) { return Color.FromArgb(0, 0, 0); }
            Color resultColor = sourceImage.GetPixel(idI, idJ);
            return resultColor;
        }
    }

    class WaveFilterHorizontal : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            int idI = Clamp(i + (int)(20 * Math.Sin(Math.PI * i / 15)), 0, sourceImage.Width - 1);
            int idJ = Clamp(j, 0, sourceImage.Height - 1);
            if (idJ == sourceImage.Height - 1) { return Color.FromArgb(0, 0, 0); }
            Color resultColor = sourceImage.GetPixel(idI, idJ);
            return resultColor;
        }
    }

    class StripesAndInversion : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {

            int idJ = Clamp(j, 0, sourceImage.Height - 1);
            if (idJ == sourceImage.Height - 1) { return Color.FromArgb(0, 0, 0); }
            Color resultColor = sourceImage.GetPixel(sourceImage.Width, idJ);
            return resultColor;
        }

    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0, resultG = 0, resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255));
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= norm;
                }
            }
        }
        public GaussianFilter() { createGaussianKernel(3, 2); }
    }

    class SobelFilter : MatrixFilter
    {
        private float[,] kernelX = {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
    };

        private float[,] kernelY = {
        { -1, -2, -1 },
        { 0, 0, 0 },
        { 1, 2, 1 }
    };
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float gradientX = CalculateGradient(sourceImage, x, y, kernelX);
            float gradientY = CalculateGradient(sourceImage, x, y, kernelY);

            // Calculate gradient magnitude
            float magnitude = (float)Math.Sqrt(gradientX * gradientX + gradientY * gradientY);

            // Clamp and return the color based on magnitude
            int intensity = Clamp((int)magnitude, 0, 255);
            return Color.FromArgb(intensity, intensity, intensity);
        }

        private float CalculateGradient(Bitmap sourceImage, int x, int y, float[,] kernel)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float result = 0;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    float grayValue = (float)(0.299 * neighborColor.R + 0.587 * neighborColor.G + 0.114 * neighborColor.B);
                    result += grayValue * kernel[k + radiusX, l + radiusY];
                }
            }

            return result;
        }
    }

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            kernel = new float[,]
            {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0 }
            };
        }
    }

    class EmbossingFilter : MatrixFilter
    {
        protected float[,] kernel;
        public EmbossingFilter()
        {
            kernel = new float[,] {
                { 0, 1, 0},
                { 1, 0, -1},
                { 0, -1, 0}
            };
        }


        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            int radiusJ = 3 / 2;
            int radiusI = 3 / 2;
            for (int l = -radiusJ; l <= radiusJ; l++)
            {
                for (int k = -radiusI; k <= radiusI; k++)
                {
                    int idI = Clamp(i + k, 0, sourceImage.Width - 1);
                    int idJ = Clamp(j + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idI, idJ);
                    resultR += neighborColor.R * kernel[k + radiusI, l + radiusJ];
                    resultG += neighborColor.G * kernel[k + radiusI, l + radiusJ];
                    resultB += neighborColor.B * kernel[k + radiusI, l + radiusJ];
                }
            }
            resultR += 128;
            resultG += 128;
            resultB += 128;
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255));
            throw new NotImplementedException();
        }
    }

    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            int size = 5;
            kernel = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j) kernel[i, j] = (float)(1.0 / size);
                    else kernel[i, j] = 0;
                }
            }
        }
    }

    class SharraFilter : MatrixFilter
    {
        protected float[,] kernelX;
        protected float[,] kernelY;
        public SharraFilter()
        {
            kernelX = new float[,]
            {
                { 3, 0, -3 },
                { 10, 0, -10 },
                { 3, 0, -3 }
            };
            kernelY = new float[,]
            {
                { 3, 10, 3 },
                { 0, 0, 0 },
                { -3, -10, -3 }
            };
        }


        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            float resultRX = 0;
            float resultGX = 0;
            float resultBX = 0;
            float resultRY = 0;
            float resultGY = 0;
            float resultBY = 0;
            int radiusJ = 3 / 2;
            int radiusI = 3 / 2;
            for (int l = -radiusJ; l <= radiusJ; l++)
            {
                for (int k = -radiusI; k <= radiusI; k++)
                {
                    int idI = Clamp(i + k, 0, sourceImage.Width - 1);
                    int idJ = Clamp(j + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idI, idJ);
                    resultRX += neighborColor.R * kernelX[k + radiusI, l + radiusJ];
                    resultGX += neighborColor.G * kernelX[k + radiusI, l + radiusJ];
                    resultBX += neighborColor.B * kernelX[k + radiusI, l + radiusJ];
                    resultRY += neighborColor.R * kernelY[k + radiusI, l + radiusJ];
                    resultGY += neighborColor.G * kernelY[k + radiusI, l + radiusJ];
                    resultBY += neighborColor.B * kernelY[k + radiusI, l + radiusJ];
                }
            }
            float resultR = (float)Math.Sqrt(Math.Pow(resultRX, 2) + Math.Pow(resultRY, 2));
            float resultG = (float)Math.Sqrt(Math.Pow(resultGX, 2) + Math.Pow(resultGY, 2));
            float resultB = (float)Math.Sqrt(Math.Pow(resultBX, 2) + Math.Pow(resultBY, 2));
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255));
            throw new NotImplementedException();
        }
    }

    class PruitFilter : MatrixFilter
    {
        protected float[,] kernelX;
        protected float[,] kernelY;
        public PruitFilter()
        {
            kernelX = new float[,]
            {
                { -1, 0, 1 },
                { -1, 0, 1 },
                { -1, 0, 1 }
            };
            kernelY = new float[,]
            {
                { -1, -1, -1 },
                { 0, 0, 0 },
                { 1, 1, 1 }
            };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            float resultRX = 0;
            float resultGX = 0;
            float resultBX = 0;
            float resultRY = 0;
            float resultGY = 0;
            float resultBY = 0;
            int radiusJ = 3 / 2;
            int radiusI = 3 / 2;
            for (int l = -radiusJ; l <= radiusJ; l++)
            {
                for (int k = -radiusI; k <= radiusI; k++)
                {
                    int idI = Clamp(i + k, 0, sourceImage.Width - 1);
                    int idJ = Clamp(j + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idI, idJ);
                    resultRX += neighborColor.R * kernelX[k + radiusI, l + radiusJ];
                    resultGX += neighborColor.G * kernelX[k + radiusI, l + radiusJ];
                    resultBX += neighborColor.B * kernelX[k + radiusI, l + radiusJ];
                    resultRY += neighborColor.R * kernelY[k + radiusI, l + radiusJ];
                    resultGY += neighborColor.G * kernelY[k + radiusI, l + radiusJ];
                    resultBY += neighborColor.B * kernelY[k + radiusI, l + radiusJ];
                }
            }
            float resultR = (float)Math.Sqrt(Math.Pow(resultRX, 2) + Math.Pow(resultRY, 2));
            float resultG = (float)Math.Sqrt(Math.Pow(resultGX, 2) + Math.Pow(resultGY, 2));
            float resultB = (float)Math.Sqrt(Math.Pow(resultBX, 2) + Math.Pow(resultBY, 2));
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255));
            throw new NotImplementedException();
        }
    }
}