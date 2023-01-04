namespace Common_glTF_Exporter.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Utils;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Dispose()
        {
        }

        protected void OnPropertyChanged(UnitObject unitobject = null, [CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (unitobject != null)
            {
            #if REVIT2019 || REVIT2020

                SettingsConfig.Set("units", unitobject.DisplayUnitType.ToString());

            #else

            SettingsConfig.Set("units", unitobject.ForgeTypeId.TypeId.ToString());

            #endif
            }
        }
    }
}
