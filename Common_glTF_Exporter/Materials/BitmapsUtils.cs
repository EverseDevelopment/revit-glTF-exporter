using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Common_glTF_Exporter.Materials
{
    public static class BitmapsUtils
    {
        public static (string, ImageFormat) GetMimeType(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return ("image/jpeg", ImageFormat.Jpeg);
                case ".png":
                    return ("image/png", ImageFormat.Png);
                case ".bmp":
                    return ("image/bmp", ImageFormat.Bmp);
                case ".gif":
                    return ("image/gif", ImageFormat.Gif);
                default:
                    return ("image/png", ImageFormat.Png);
            }
        }

        public static byte[] BlendImageWithColor(
    byte[] imageBytes,
    double fade,
    Autodesk.Revit.DB.Color flatColor,
    ImageFormat mimeType)
        {
            if (fade >= 1.0)
                return imageBytes;

            float fFade = Clamp((float)fade, 0f, 1f);
            float fInv = 1f - fFade;

            float lr = SrgbToLinear(flatColor.Red / 255f);
            float lg = SrgbToLinear(flatColor.Green / 255f);
            float lb = SrgbToLinear(flatColor.Blue / 255f);

            byte[] resultBytes;

            using (MemoryStream inputMs = new MemoryStream(imageBytes))
            using (Bitmap bitmap = new Bitmap(inputMs))
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData data = bitmap.LockBits(
                    rect,
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);

                int byteCount = Math.Abs(data.Stride) * bitmap.Height;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(data.Scan0, pixels, 0, byteCount);

                for (int i = 0; i < byteCount; i += 4)
                {
                    // bytes BGRA → float 0-1
                    float sb = pixels[i + 0] / 255f;
                    float sg = pixels[i + 1] / 255f;
                    float sr = pixels[i + 2] / 255f;

                    // sRGB → lineal
                    float lbSrc = SrgbToLinear(sb);
                    float lgSrc = SrgbToLinear(sg);
                    float lrSrc = SrgbToLinear(sr);

                    // lineal interpolation
                    float lbMix = lbSrc * fFade + lb * fInv;
                    float lgMix = lgSrc * fFade + lg * fInv;
                    float lrMix = lrSrc * fFade + lr * fInv;

                    pixels[i + 0] = (byte)(Clamp(LinearToSrgb(lbMix), 0f, 1f) * 255f + 0.5f);
                    pixels[i + 1] = (byte)(Clamp(LinearToSrgb(lgMix), 0f, 1f) * 255f + 0.5f);
                    pixels[i + 2] = (byte)(Clamp(LinearToSrgb(lrMix), 0f, 1f) * 255f + 0.5f);
                }

                Marshal.Copy(pixels, 0, data.Scan0, byteCount);
                bitmap.UnlockBits(data);

                using (MemoryStream outputMs = new MemoryStream())
                {
                    bitmap.Save(outputMs, mimeType);
                    resultBytes = outputMs.ToArray();
                }
            }

            return resultBytes;
        }

        private static float Clamp(float v, float min, float max)
    => (v < min) ? min : (v > max) ? max : v;

        private static float Pow(float b, double exp)
            => (float)Math.Pow(b, exp);


        private static float SrgbToLinear(float c)
        {
            return (c <= 0.04045f)
                ? c / 12.92f
                : Pow((c + 0.055f) / 1.055f, 2.4);
        }

        private static float LinearToSrgb(float l)
        {
            return (l <= 0.0031308f)
                ? l * 12.92f
                : 1.055f * Pow(l, 1.0 / 2.4) - 0.055f;
        }
    }
}
