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

namespace SlowAreaTagFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowAreaTagFilter : IExternalCommand
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

            // Use el filtro AreaTagFilter para encontrar las etiquetas de área.

            // Creamos el filtro AreaTagFilter
            AreaTagFilter filter = new AreaTagFilter();

            // Aplicamos el filtro a los elementos del documento activo

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(filter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son etiquetas de área");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
