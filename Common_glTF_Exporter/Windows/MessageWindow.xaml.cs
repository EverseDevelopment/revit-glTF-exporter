namespace Revit_glTF_Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;
    using Common_glTF_Exporter.Windows.MainWindow;
    using View = Autodesk.Revit.DB.View;

    /// <summary>
    /// Interaction logic for Settings.xaml.
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            this.InitializeComponent();
        }

        public static void Show(string title, string message)
        {
            MessageWindow dlg = new MessageWindow();
            dlg.Title = title;
            dlg.labelMessage.Text = message;
            dlg.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Title_Link(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/");
        }
    }
}
