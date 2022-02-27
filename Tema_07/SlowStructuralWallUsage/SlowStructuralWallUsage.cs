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

namespace SlowStructuralWallUsage
{
    [Transaction(TransactionMode.Manual)]
    public class SlowStructuralWallUsage : IExternalCommand
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

            //Construimos el filtro para muros de Carga
            StructuralWallUsageFilter structuralWallUsageFilter = new StructuralWallUsageFilter(StructuralWallUsage.Bearing);
            //Construimos el colector
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            //Obtenemos las ejemplares de muro
            IList<Element> elementsList = collector.WherePasses(structuralWallUsageFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI son muros de carga");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
