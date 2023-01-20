namespace Common_glTF_Exporter.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Revit_glTF_Exporter;

    public class ProgressBarWindowViewModel : INotifyPropertyChanged
    {
        private double progressBarMax;
        private double progressBarPercentage;
        private double progressBarValue;
        private double progressBarGraphicValue;
        private string message;
        private string action;
        private ICommand closeWindowCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public double ProgressBarMax
        {
            get
            {
                return this.progressBarMax;
            }

            set
            {
                this.progressBarMax = value;
                this.OnPropertyChanged();
            }
        }

        public double ProgressBarPercentage
        {
            get
            {
                return this.progressBarPercentage;
            }

            set
            {
                this.progressBarPercentage = value;
                this.OnPropertyChanged();
            }
        }

        public double ProgressBarValue
        {
            get
            {
                return this.progressBarValue;
            }

            set
            {
                if (((value / ProgressBarMax) * 100) > 7)
                {
                    ProgressBarGraphicValue = value;
                }

                this.progressBarValue = value;
                ProgressBarPercentage = (value / ProgressBarMax) * 100;
                this.OnPropertyChanged();
            }
        }

        public double ProgressBarGraphicValue
        {
            get
            {
                return this.progressBarGraphicValue;
            }

            set
            {
                this.progressBarGraphicValue = value;
                this.OnPropertyChanged();
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                this.OnPropertyChanged();
            }
        }

        public string Action
        {
            get
            {
                return this.action;
            }

            set
            {
                this.action = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand CloseWindowCommand
        {
            get { return this.closeWindowCommand; }
            set { this.closeWindowCommand = value; }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            System.Windows.Forms.Application.DoEvents();
            ProgressBarWindow.MainView.Topmost = true;
            ProgressBarWindow.MainView.Activate();
        }
    }
}
