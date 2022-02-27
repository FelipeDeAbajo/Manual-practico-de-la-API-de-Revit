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

namespace SlowElementLevelFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowElementLevelFilter : IExternalCommand
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

            // Utilice el filtro ElementLevel para encontrar elementos por su nivel asociado en el documento

            // Encuentra el nivel cuyo nombre es "Nivel 1"
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> levels = collector.OfClass(typeof(Level)).ToElements();
            var query = from element in collector where element.Name == "Nivel 1" select element;// Linq query

            // Obtenemos el Id del Level
            List<Element> level1 = query.ToList<Element>();
            ElementId levelId = level1[0].Id;

            // Seleccionamos Wall
            ElementLevelFilter level1Filter = new ElementLevelFilter(levelId);
            collector = new FilteredElementCollector(doc);
            ICollection<Element> allWallsOnLevel1 = collector.OfClass(typeof(Wall)).WherePasses(level1Filter).ToElements();

            List<string> names = allWallsOnLevel1.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos Wall que SI están en el Nivel 1");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscar Wall, que no estan en Nivel 1
            ElementLevelFilter notOnLevel1Filter = new ElementLevelFilter(levelId, true); // Filtro inverso
            collector = new FilteredElementCollector(doc);
            IList<Element> allRoomsNotOnLevel1 = collector.OfClass(typeof(Wall)).WherePasses(notOnLevel1Filter).ToElements();

            names = allRoomsNotOnLevel1.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos Wall que No están en el Nivel 1");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
