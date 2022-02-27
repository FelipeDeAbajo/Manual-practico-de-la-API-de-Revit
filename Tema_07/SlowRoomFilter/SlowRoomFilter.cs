#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace SlowRoomFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowRoomFilter : IExternalCommand
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

            // Use el filtro RoomFilter para encontrar las habitaciones. Similar a ElementClassFilter

            // Creamos el filtro RoomFilter
            RoomFilter filter = new RoomFilter();

            // Aplicamos el filtro a los elementos del documento activo

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(filter).ToElements();

           // List<string> names = elementsList.Select(x => x.Name).ToList();
            List<string> names = elementsList.Where(x => (x as Room).Area >0).Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI son habitaciones");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
