﻿namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;

    public class RevitGridParametersObject
    {
        public List<double> origin { get; set; }

        public List<double> direction { get; set; }

        public double length { get; set; }
    }
}
