namespace Common_glTF_Exporter.ViewModel
{
    using System.Collections.ObjectModel;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Model;

    public class UnitsViewModel : ViewModelBase
    {
        private UnitObject selectedUnit;
        private ObservableCollection<UnitObject> units;

        public UnitsViewModel()
        {
            #if REVIT2019 || REVIT2020

            this.Units = new ObservableCollection<UnitObject>
            {
                new UnitObject(DisplayUnitType.DUT_METERS),
                new UnitObject(DisplayUnitType.DUT_DECIMAL_INCHES),
                new UnitObject(DisplayUnitType.DUT_MILLIMETERS),
                new UnitObject(DisplayUnitType.DUT_DECIMAL_FEET),
                new UnitObject(DisplayUnitType.DUT_CENTIMETERS),
            };

            #else

            this.Units = new ObservableCollection<UnitObject>
            {
                new UnitObject(UnitTypeId.Meters),
                new UnitObject(UnitTypeId.Inches),
                new UnitObject(UnitTypeId.Millimeters),
                new UnitObject(UnitTypeId.Feet),
                new UnitObject(UnitTypeId.Centimeters),
            };

            #endif
        }

        public UnitObject SelectedUnit
        {
            get
            {
                return this.selectedUnit;
            }

            set
            {
                this.selectedUnit = value;
                this.OnPropertyChanged(this.SelectedUnit);
            }
        }

        public ObservableCollection<UnitObject> Units
        {
            get
            {
                return this.units;
            }

            set
            {
                this.units = value;
                this.OnPropertyChanged();
            }
        }
    }
}
