#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace SlowAreaFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowAreaFilter : IExternalCommand
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

            // Use el filtro AreaFilter para encontrar las áreas. Similar a ElementClassFilter

            // Creamos el filtro AreaFilter
            AreaFilter filter = new AreaFilter();

            // Aplicamos el filtro a los elementos del documento activo

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(filter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son áreas");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
