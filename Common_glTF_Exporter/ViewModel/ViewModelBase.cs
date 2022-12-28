using Autodesk.Revit.DB;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common_glTF_Exporter.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(UnitObject unitobject = null, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (unitobject != null)
            {
            #if REVIT2019 || REVIT2020

                SettingsConfig.Set("units", unitobject.DisplayUnitType.ToString());

            #else

            SettingsConfig.Set("units", unitobject.ForgeTypeId.TypeId.ToString());

            #endif
            }

        }
        public virtual void Dispose() { }
    }
}
