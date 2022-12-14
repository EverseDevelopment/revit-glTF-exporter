using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace Common_glTF_Exporter.ViewModel
{
    public class ProgressBarWindowViewModel : INotifyPropertyChanged
    {
        #region Properties

        private double _ProgressBarMax;
        public double ProgressBarMax
        {
            get { return _ProgressBarMax; }
            set
            {
                _ProgressBarMax = value;
                OnPropertyChanged();
            }
        }
        private double _ProgressBarValue;
        public double ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set
            {
                _ProgressBarValue = value;
                OnPropertyChanged();
            }
        }
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                _Message = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            System.Windows.Forms.Application.DoEvents();
            ProgressBarWindow.MainView.Topmost = true;
            ProgressBarWindow.MainView.Activate();
        }
        #endregion

        #region Commands

        private ICommand _closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get { return _closeWindowCommand; }
            set { _closeWindowCommand = value; }
        }
        #endregion
    }
}
