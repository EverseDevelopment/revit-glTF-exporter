// <copyright file="IObjects.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Revit_glTF_Exporter.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Autodesk.Revit.DB;

    public interface IObjects<T>
    {
        List<T> ObjectsList { get; set; }

        int Count { get; set; }

        Category Category { get; set; }
    }
}
