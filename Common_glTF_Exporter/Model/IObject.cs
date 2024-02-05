namespace Common_glTF_Exporter.Model
{
    using Autodesk.Revit.DB;

    public interface IObject
    {
        Category Category { get; set; }

        string FamilySymbol { get; set; }

        string ElementName { get; set; }

        ElementId EId { get; set; }

        Location Location { get; set; }
    }
}
