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

namespace BoundingBoxContainsPoint
{
    [Transaction(TransactionMode.Manual)]
    public class BoundingBoxContainsPoint : IExternalCommand
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

            // Utilice el filtro BoundingBoxContainsPoint para encontrar elementos con una BoundingBox
            // que contenga el punto dado en el documento..

            // Creamos un filtro BoundingBoxContainsPoint para el punto (0, 0, 0)
            XYZ basePnt = new XYZ(0, 0, 0);
            BoundingBoxContainsPointFilter filter = new BoundingBoxContainsPointFilter(basePnt);

            // Aplicamos el filtro a los elementos del documento activo

            // Utilice el comando abreviado OfClass() para localizar sólo muros

            FilteredElementCollector col = new FilteredElementCollector(doc);
            IList<Element> elementsList = col.OfClass(typeof(Wall)).WherePasses(filter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI engloban (0,0,0)");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscamos muros que no contengan el punto dado: usamos el parametro inverso
            BoundingBoxContainsPointFilter notContainFilter =
                new BoundingBoxContainsPointFilter(basePnt, true); // filtro inverso
            col = new FilteredElementCollector(doc);

            IList<Element> notContainFounds = 
                col.OfClass(typeof(Wall)).WherePasses(notContainFilter).ToElements();

            names = notContainFounds.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO engloban (0,0,0)");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));


            return Result.Succeeded;
        }
    }
}
