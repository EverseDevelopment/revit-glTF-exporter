using Caliburn.Micro;
using System.Windows.Forms;
using System;
using System.Windows.Media.Imaging;
using WixSharp;
using WixSharp.UI.Forms;
using WixSharp.UI.WPF;
using System.Runtime.InteropServices;

namespace GltfInstaller
{
    /// <summary>
    /// The standard MaintenanceTypeDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro View (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MaintenanceTypeDialog : WpfDialog, IWpfDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceTypeDialog" /> class.
        /// </summary>
        public MaintenanceTypeDialog()
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

            var view = parent as IShellView;
            view?.SetSize(510, 571);

            container.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            parent.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

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
            var host = new WelcomeDialogModel { Host = ManagedFormHost };
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
    }

    /// <summary>
    /// ViewModel for standard MaintenanceTypeDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro ViewModel (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="Caliburn.Micro.Screen" />
    internal class MaintenanceTypeDialogModel : Caliburn.Micro.Screen
    {
        public ManagedForm Host;

        ISession session => Host?.Runtime.Session;
        IManagedUIShell shell => Host?.Shell;

        public BitmapImage Banner => session?.GetResourceBitmap("WixUI_Bmp_Banner").ToImageSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceTypeDialog" /> class.
        /// </summary>
        void JumpToProgressDialog()
        {
            int index = shell.Dialogs.IndexOfDialogImplementing<IProgressDialog>();
            if (index != -1)
                shell.GoTo(index);
            else
                shell.GoNext(); // if user did not supply progress dialog then simply go next
        }

        public void Change()
        {
            if (session != null)
            {
                session["MODIFY_ACTION"] = "Change";
                shell.GoNext();
            }
        }

        public void Repair()
        {
            if (session != null)
            {
                session["MODIFY_ACTION"] = "Repair";
                JumpToProgressDialog();
            }
        }

        public void Remove()
        {
            if (session != null)
            {
                session["REMOVE"] = "ALL";
                session["MODIFY_ACTION"] = "Remove";

                JumpToProgressDialog();
            }
        }

        public void GoPrev()
            => shell?.GoPrev();

        public void Cancel()
            => Host?.Shell.Cancel();
    }
}