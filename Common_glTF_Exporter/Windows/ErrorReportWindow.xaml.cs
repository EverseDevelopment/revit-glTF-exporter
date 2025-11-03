namespace Revit_glTF_Exporter
{
    using Common_glTF_Exporter;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;
    using System.Net.Mail;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Feedback Window.
    /// </summary>
    public partial class ErrorReportWindow : Window
    {
        private static string exError;
        public ErrorReportWindow(string excepcionError)
        {
            this.InitializeComponent();
            this.DataContext = this;
            Theme.ApplyDarkLightMode(this.Resources.MergedDictionaries[0]);
            exError = excepcionError;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            string email = email_textBox.Text;

            if (!IsValidEmail(email))
            {
                MessageWindow.Show("Email", "Please provide a valid email");
                return;
            }

            string userDescription = description_textBox.Text;

            if (!IsDescriptionValid(userDescription))
            {
                MessageWindow.Show("Description", "Please provide a detailed description");
                return;
            }

            ClickUpFormSender.CreateClickUpTask(email, userDescription, exError);

            this.Close();
        }

        private void CancelProcess_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Title_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run(Links.contactLink);
        }
        private void Leia_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run("https://e-verse.com/leia/");
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDescriptionValid(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return false;

            if (description.Length < 5)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
