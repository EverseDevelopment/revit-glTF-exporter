using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Transform
{
    internal class ModelRotation
    {
        public static List<double> Get(bool flipAxis)
        {
            if (flipAxis)
            {
                return new List<double> { 0.7071, 0, 0, -0.7071 };
            }
            else
            {
                return new List<double> { 0, 0, 0, 0};
            }
        }
    }
}
