#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace SlowStructuralMaterialType
{
    [Transaction(TransactionMode.Manual)]
    public class SlowStructuralMaterialType : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            StructuralMaterialTypeFilter structuralMaterialTypeFilter = new StructuralMaterialTypeFilter(StructuralMaterialType.Concrete);
            //Construimos el colector
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            //Obtenemos las FamilyInstance
            IList<Element> elementsList = collector.WherePasses(structuralMaterialTypeFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI son de Hormig�n");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
