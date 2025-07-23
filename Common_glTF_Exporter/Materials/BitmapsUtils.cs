using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
            ImageFormat mimeType,
            Autodesk.Revit.DB.Color tintColor)
        {
            bool noFlatBlend = fade >= 1.0 || flatColor == null;
            if (noFlatBlend && tintColor == null)
                return imageBytes; 


            float fFade = noFlatBlend ? 1f              
                                      : Clamp((float)fade, 0f, 1f);
            float fInv = 1f - fFade;                    // 0 if no blend

            // face colour in linear space (0 if unused)
            float lrFlat = 0f, lgFlat = 0f, lbFlat = 0f;
            if (!noFlatBlend)
            {
                lrFlat = SrgbToLinear(flatColor.Red / 255f);
                lgFlat = SrgbToLinear(flatColor.Green / 255f);
                lbFlat = SrgbToLinear(flatColor.Blue / 255f);
            }

            // tint factors (1,1,1 ⇒ no change)
            float lrTint = 1f, lgTint = 1f, lbTint = 1f;
            if (tintColor != null)
            {
                lrTint = SrgbToLinear(tintColor.Red / 255f);
                lgTint = SrgbToLinear(tintColor.Green / 255f);
                lbTint = SrgbToLinear(tintColor.Blue / 255f);

                lrTint = Math.Min(lrTint + 0.10f, 1.0f);
                lgTint = Math.Min(lgTint + 0.10f, 1.0f);
                lbTint = Math.Min(lbTint + 0.10f, 1.0f);
            }

            byte[] resultBytes;
            using (var inputMs = new MemoryStream(imageBytes))
            using (var bitmap = new Bitmap(inputMs))
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite,
                                                  PixelFormat.Format32bppArgb);

                int byteCount = Math.Abs(data.Stride) * bitmap.Height;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(data.Scan0, pixels, 0, byteCount);

                for (int i = 0; i < byteCount; i += 4)
                {
                    //BGRA → linear
                    float sb = pixels[i + 0] / 255f;
                    float sg = pixels[i + 1] / 255f;
                    float sr = pixels[i + 2] / 255f;
                    float lb = SrgbToLinear(sb);
                    float lg = SrgbToLinear(sg);
                    float lr = SrgbToLinear(sr);

                    // BLEND WITH FLAT COLOUR
                    lb = lb * fFade + lbFlat * fInv;
                    lg = lg * fFade + lgFlat * fInv;
                    lr = lr * fFade + lrFlat * fInv;

                    //APPLY TINT
                    lb *= lbTint;
                    lg *= lgTint;
                    lr *= lrTint;

                    //* linear → sRGB, write back
                    pixels[i + 0] = (byte)(Clamp(LinearToSrgb(lb), 0f, 1f) * 255f + 0.5f);
                    pixels[i + 1] = (byte)(Clamp(LinearToSrgb(lg), 0f, 1f) * 255f + 0.5f);
                    pixels[i + 2] = (byte)(Clamp(LinearToSrgb(lr), 0f, 1f) * 255f + 0.5f);
                    // alpha (pixels[i+3]) untouched
                }

                Marshal.Copy(pixels, 0, data.Scan0, byteCount);
                bitmap.UnlockBits(data);

                using (var outputMs = new MemoryStream())
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
