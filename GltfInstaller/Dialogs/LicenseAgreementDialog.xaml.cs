using Caliburn.Micro;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using WixSharp;
using WixSharp.UI.Forms;
using WixSharp.UI.WPF;
using IO = System.IO;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using WixSharp;
using WixSharp.UI.Forms;
using WixSharp.UI.WPF;

namespace GltfInstaller
{
    /// <summary>
    /// The standard LicenceDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro View (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    public partial class LicenceDialog : WpfDialog, IWpfDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LicenceDialog"/> class.
        /// </summary>
        public LicenceDialog()
        {
            InitializeComponent();
        }

        private System.Windows.Point _dragStartPoint;
        private bool _isDragging;

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {

            var container = ManagedFormHost;
            var parent = container.Parent as Form;
            parent.FormBorderStyle = FormBorderStyle.None;
            PreviewMouseLeftButtonDown += (s, e) =>
            {
                _dragStartPoint = e.GetPosition(this);
                _isDragging = true;
            };

            PreviewMouseLeftButtonUp += (s, e) => _isDragging = false;
            PreviewMouseMove += (s, e) =>
            {
                if (!_isDragging)
                    return;

                System.Windows.Point currentPoint = e.GetPosition(this);
                double deltaX = currentPoint.X - _dragStartPoint.X;
                double deltaY = currentPoint.Y - _dragStartPoint.Y;

                parent.Left += Convert.ToInt32(deltaX);
                parent.Top += Convert.ToInt32(deltaY);
            };

            container.BackColor = System.Drawing.ColorTranslator.FromHtml("#e8e3df");

            parent.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, parent.Width, parent.Height, 20, 20));
            var host = new LicenseDialogModel { Host = ManagedFormHost };
            ViewModelBinder.Bind(host, this, null);
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,      // x-coordinate of upper-left corner
            int nTopRect,       // y-coordinate of upper-left corner
            int nRightRect,     // x-coordinate of lower-right corner
            int nBottomRect,    // y-coordinate of lower-right corner
            int nWidthEllipse,  // height of ellipse
            int nHeightEllipse  // width of ellipse
        );

        private void LicenseAcceptedChecked_Checked(object sender, RoutedEventArgs e)
        {
            //Enable or disable next button
        }

        private void Title_Link(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/");
        }

        private void Contact_Link(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/contact/");
        }

        private void Terms_Link(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/terms-and-conditions-aec-industry/");
        }
    }

    /// <summary>
    /// ViewModel for standard LicenceDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro ViewModel (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="Caliburn.Micro.Screen" />
    internal class LicenseDialogModel : Caliburn.Micro.Screen
    {
        ManagedForm host;
        ISession session => Host?.Runtime.Session;
        IManagedUIShell shell => Host?.Shell;

        public Action<string> ShowRtfContent;

        public ManagedForm Host
        {
            get => host;
            set
            {
                host = value;

                ShowRtfContent?.Invoke(LicenceText);
                NotifyOfPropertyChange(() => Banner);
                NotifyOfPropertyChange(() => LicenseAcceptedChecked);
                NotifyOfPropertyChange(() => CanGoNext);
            }
        }

        public string LicenceText => session?.GetResourceString("WixSharp_LicenceFile");

        public BitmapImage Banner => session?.GetResourceBitmap("WixUI_Bmp_Banner").ToImageSource();

        public bool LicenseAcceptedChecked
        {
            get => session?["LastLicenceAcceptedChecked"] == "True";
            set
            {
                if (Host != null)
                    session["LastLicenceAcceptedChecked"] = value.ToString();

                NotifyOfPropertyChange(() => LicenseAcceptedChecked);
                NotifyOfPropertyChange(() => CanGoNext);
            }
        }

        public bool CanGoNext
            => LicenseAcceptedChecked;

        public void GoPrev()
            => shell?.GoPrev();

        public void GoNext()
            => shell?.GoNext();

        public void Cancel()
            => shell?.Cancel();

        public void Print()
        {
            try
            {
                var file = IO.Path.GetTempPath().PathCombine(Host?.Runtime.Session.Property("ProductName") + ".licence.rtf");
                IO.File.WriteAllText(file, LicenceText);
                Process.Start(file);
            }
            catch
            {
                // Catch all, we don't want the installer to crash in an
                // attempt to write to a file.
            }
        }
    }
}