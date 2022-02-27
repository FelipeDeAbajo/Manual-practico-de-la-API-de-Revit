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

namespace ElementIsCurveDriven
{
    [Transaction(TransactionMode.Manual)]
    public class ElementIsCurveDriven : IExternalCommand
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

            // Creamos un filtro ElementIsCurveDriven para buscar pilares inclinadas (generados curvas) en el documento,
            // Un pilar normal, su Location es LocationPoint no LocationCurve
            ElementIsCurveDrivenFilter filter = new ElementIsCurveDrivenFilter();

            // Aplicamos el filtro a los elementos del documento activo
            //Anidamos el método abreviado OfCategory()

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> slantColumns = collector
                .WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

            List<string> names = slantColumns.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI estan basados en linea y son pilar estructural");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscamos pilares estructurales que no están basados en linea
            ElementIsCurveDrivenFilter notCurveDrivenFilter = new ElementIsCurveDrivenFilter(true); // filtro inverso
            collector = new FilteredElementCollector(doc);
            ICollection<Element> notCurveDrivenFounds = collector.
                WherePasses(notCurveDrivenFilter).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsNotElementType()
                .ToElements();

            names = notCurveDrivenFounds.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO estan basados en linea y son pilar estructural");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));


            return Result.Succeeded;
        }
    }
}
