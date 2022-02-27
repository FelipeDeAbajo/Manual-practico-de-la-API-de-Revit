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

namespace SlowCurveElementFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowCurveElementFilter : IExternalCommand
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

            // Use el filtro CurveElementFilter para encontrar las Curves. Por ejemplo separadoras de area.

            // Creamos el filtro CurveElementFilter
            CurveElementFilter filter = new CurveElementFilter(CurveElementType.AreaSeparation);

            // Aplicamos el filtro a los elementos del documento activo

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(filter).ToElements();

            //Obtenemos una lista de CurveElement
            List<CurveElement> curveElements = elementsList.Select(x => x as CurveElement).ToList();

            List<string> names = curveElements.Select(x => x.CurveElementType.ToString()).ToList();

            names.Insert(0, "Elementos Curve que SI son separadores de area");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscamos Curves que no sean separadoras de area.
            CurveElementFilter notRoomSeparationFilter = new CurveElementFilter(CurveElementType.AreaSeparation, true); // filtro inverso
            collector = new FilteredElementCollector(doc);
            ICollection<Element> notRoomSeparationFounds = collector.
                WherePasses(notRoomSeparationFilter).ToElements();

            //Obtenemos los Ids de los bocetos de la Nube de revisión. Estan formadas por 4
            curveElements = notRoomSeparationFounds.Select(x => x as CurveElement).ToList();

            names = curveElements.Select(x => x.CurveElementType.ToString()).ToList();
            names.Insert(0, "Elementos Curve que NO son separadores de area");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
