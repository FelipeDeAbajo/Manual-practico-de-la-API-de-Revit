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

namespace LogicalOR
{
    [Transaction(TransactionMode.Manual)]
    public class LogicalOR : IExternalCommand
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

            // Creamos un filtro de categoría. Windows
            ElementCategoryFilter windowsCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);

            // Creamos un filtro de categoría. Doors
            ElementCategoryFilter doorsCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);

            // Creamos el filtro lógico OR para Windows y Doors
            LogicalOrFilter doorOrWindowFilter = new LogicalOrFilter(windowsCategoryfilter, doorsCategoryfilter);


            // Aplicamos el filtro a los elementos del documento activo
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            //Aplicamos el método  WhereElementIsNotElementType, solo aceptamos instancias
            IList<Element> elementsList = collector.WhereElementIsNotElementType().WherePasses(doorOrWindowFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son Ventanas o Puertas");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
