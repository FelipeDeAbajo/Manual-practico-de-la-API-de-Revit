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

namespace ElementIsElementType
{
    [Transaction(TransactionMode.Manual)]
    public class ElementIsElementType : IExternalCommand
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

            // 

            // Usamos ElementClassFilter para buscar tipos de familia 
            ElementIsElementTypeFilter filterType = new ElementIsElementTypeFilter();

            // Aplicamos el filtro a los elementos del documento activo

            //Seleccionamos solo aquellas con categoría  Wall
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.OfCategory(BuiltInCategory.OST_Windows).WhereElementIsElementType()./*WherePasses(filterType).*/ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son tipos");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Usamos ElementClassFilter para buscar tipos de familia 
            ElementIsElementTypeFilter filterNotType = new ElementIsElementTypeFilter(true);

            // utilizamos el método abreviado  OfCategory()
            // Buscamos elementos de categoría Wall que no son tipos
            collector = new FilteredElementCollector(doc);
            elementsList = collector.OfCategory(BuiltInCategory.OST_Windows).WherePasses(filterNotType).ToElements(); //filtro abreviado

            names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI no son tipos");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
