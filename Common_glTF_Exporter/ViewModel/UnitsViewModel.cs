using Autodesk.Revit.DB;
using Common_glTF_Exporter.Model;
using Revit_glTF_Exporter;
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
            #if REVIT2019 || REVIT2020

            Units = new ObservableCollection<UnitObject>();
            Units.Add(new UnitObject(DisplayUnitType.DUT_METERS));
            Units.Add(new UnitObject(DisplayUnitType.DUT_DECIMAL_INCHES));
            Units.Add(new UnitObject(DisplayUnitType.DUT_MILLIMETERS));
            Units.Add(new UnitObject(DisplayUnitType.DUT_DECIMAL_FEET));
            Units.Add(new UnitObject(DisplayUnitType.DUT_CENTIMETERS));

            #else

            Units = new ObservableCollection<UnitObject>();
            Units.Add( new UnitObject(UnitTypeId.Meters));
            Units.Add(new UnitObject(UnitTypeId.Inches));
            Units.Add(new UnitObject(UnitTypeId.Millimeters));
            Units.Add(new UnitObject(UnitTypeId.Feet));
            Units.Add(new UnitObject(UnitTypeId.Centimeters));

            #endif
        }
    }
}
