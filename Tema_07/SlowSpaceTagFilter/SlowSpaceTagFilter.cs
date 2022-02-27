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

namespace SlowSpaceTagFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowSpaceTagFilter : IExternalCommand
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

            // Use el filtro SpaceFilter para encontrar las espacios. Similar a ElementClassFilter

            // Creamos el filtro SpaceFilter

            Autodesk.Revit.DB.Mechanical.SpaceTagFilter spaceTagFilter = new Autodesk.Revit.DB.Mechanical.SpaceTagFilter();

            // Aplicamos el filtro a los elementos del documento activo

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.WherePasses(spaceTagFilter);

            IList<Element> elementsList = collector.WherePasses(spaceTagFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son Etiquetas de espacios");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
