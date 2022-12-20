﻿using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.ViewModel;
using Common_glTF_Exporter.Utils;
using System.Windows.Input;

namespace Revit_glTF_Exporter
{
    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindowViewModel ViewModel { get; set; } = new ProgressBarWindowViewModel();
        public static ProgressBarWindow MainView { get; set; }

        public ProgressBarWindow()
        {
            InitializeComponent();
            MainView = this;
            this.DataContext = this;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (ProgressBarWindow.MainView != null && ProgressBarWindow.MainView.IsActive)
                ProgressBarWindow.MainView.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}