using Autodesk.Revit.DB;
using Common_glTF_Exporter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Common_glTF_Exporter.ViewModel
{
    public class UnitsViewModel : ViewModelBase
    {
        private UnitObject _selectedUnit;
        public UnitObject SelectedUnit
        {
            get { return _selectedUnit; }
            set
            {
                _selectedUnit = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<UnitObject> _units;

        public ObservableCollection<UnitObject> Units
        {
            get { return _units; }
            set
            {
                _units = value;
                OnPropertyChanged();
            }
        }

        public UnitsViewModel()
        {
            Units = new ObservableCollection<UnitObject>();
            Units.Add( new UnitObject(UnitTypeId.Meters));
            Units.Add(new UnitObject(UnitTypeId.Inches));
            Units.Add(new UnitObject(UnitTypeId.Millimeters));
            Units.Add(new UnitObject(UnitTypeId.Feet));
            Units.Add(new UnitObject(UnitTypeId.Centimeters));
        }
    }
}
