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

namespace ElementCategory
{
    [Transaction(TransactionMode.Manual)]
    public class ElementCategory : IExternalCommand
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

            // Use el filtro ElementCategoryFilter para encontrar elementos con una categoría

            // Creamos el filtro ElementCategoryFilter con la categoría muros
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);

            // Aplicamos el filtro a los elementos del documento activo

            // Utilice el método abreviado WhereElementIsNotElementType() para encontrar sólo instancias de muro
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(filter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son muro");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscamos tipos que Si son muros. Forma abreviada
            collector = new FilteredElementCollector(doc);
            elementsList = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements(); //filtro abreviado


            names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son ejemplares de muro. Método abreviado");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscamos tipos que NO son muros
            collector = new FilteredElementCollector(doc);
            // Creamos el filtro ElementCategoryFilter con la categoría muros
            ElementCategoryFilter filterNoWall = new ElementCategoryFilter(new ElementId(-2000011), true);

            elementsList = collector.WherePasses(filterNoWall).ToElements(); //filtro inverso

            names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO son muro");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
