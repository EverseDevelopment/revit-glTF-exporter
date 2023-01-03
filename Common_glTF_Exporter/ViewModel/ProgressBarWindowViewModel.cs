namespace Common_glTF_Exporter.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows.Input;
    using Revit_glTF_Exporter;

    public class ProgressBarWindowViewModel : INotifyPropertyChanged
    {
        private double progressBarMax;
        private double progressBarValue;
        private string message;
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

        public double ProgressBarValue
        {
            get
            {
                return this.progressBarValue;
            }

            set
            {
                this.progressBarValue = value;
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
