using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Utils
{
    public static class LogConfiguration
    {
        public static void SaveConfig()
        {
            string format = SettingsConfig.GetValue("format");
            string flipAxis = SettingsConfig.GetValue("flipAxis");
            string normals = SettingsConfig.GetValue("normals");
            string relocateTo0 = SettingsConfig.GetValue("relocateTo0");
            string materials = SettingsConfig.GetValue("materials");
            string units = SettingsConfig.GetValue("units");
            string batchId = SettingsConfig.GetValue("batchId");
            string levels = SettingsConfig.GetValue("levels");
            string properties = SettingsConfig.GetValue("properties");
            string grids = SettingsConfig.GetValue("grids");
            string compression = SettingsConfig.GetValue("compression");
            string release = SettingsConfig.GetValue("release");
            string runs = SettingsConfig.GetValue("runs");

            ExportLog.Write($"format: {format}");
            ExportLog.Write($"flipAxis: {flipAxis}");
            ExportLog.Write($"normals: {normals}");
            ExportLog.Write($"relocateTo0: {relocateTo0}");
            ExportLog.Write($"materials: {materials}");
            ExportLog.Write($"units: {units}");
            ExportLog.Write($"batchId: {batchId}");
            ExportLog.Write($"levels: {levels}");
            ExportLog.Write($"properties: {properties}");
            ExportLog.Write($"grids: {grids}");
            ExportLog.Write($"compression: {compression}");
            ExportLog.Write($"release: {release}");
            ExportLog.Write($"runs: {runs}");
        }

    }
}
