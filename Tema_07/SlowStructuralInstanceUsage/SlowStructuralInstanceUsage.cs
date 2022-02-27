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

namespace SlowStructuralInstanceUsage
{
    [Transaction(TransactionMode.Manual)]
    public class SlowStructuralInstanceUsage : IExternalCommand
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

            //Creamos el filtro, restringimos a Pilares estructurales
            StructuralInstanceUsageFilter structuralInstanceUsageFilter = new StructuralInstanceUsageFilter(StructuralInstanceUsage.Column);

            //Construimos el colector
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            //Obtenemos las FamilyInstance
            IList<Element> elementsList = collector.WherePasses(structuralInstanceUsageFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI son pilares estructurales");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            //Construimos un nuevo filtro. Restringimos a Muros
            structuralInstanceUsageFilter = new StructuralInstanceUsageFilter(StructuralInstanceUsage.Wall);

            //Construimos el colector
            collector = new FilteredElementCollector(doc);

            //Ontenemos las FamilyInstance 
            elementsList = collector.WherePasses(structuralInstanceUsageFilter).ToElements();

            names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI son muros");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
