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

namespace ElementMulticategory
{
    [Transaction(TransactionMode.Manual)]
    public class ElementMulticategory : IExternalCommand
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

            // Creamos un filtro ElementMulticategory para buscar elementos que pertenezcan a alguna de las categorias pasadas,
            //Creamos un lista con las categorias Puertas y Ventanas
            ICollection<BuiltInCategory> elementBic = new List<BuiltInCategory>() { BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows };

            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(elementBic);
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo
            // Empleamos .WhereElementIsNotElementType(), solo admitimos ejemplares, no tipos
            ICollection<Element> elementsFilter = collector.WherePasses(elementMulticategoryFilter).WhereElementIsNotElementType().ToElements();

            List<string> names = elementsFilter.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son ejemplares de puertas o ventanas");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            ElementMulticategoryFilter elementNotMulticategoryFilter = new ElementMulticategoryFilter(elementBic, true);//filtro inverso
            // Empleamos .WhereElementIsNotElementType(), solo admitimos ejemplares, no tipos
            // Reiniciamos FilteredElementCollecto
            collector = new FilteredElementCollector(doc);
            ICollection<Element> notMultiCategoryFounds = collector.
                WherePasses(elementNotMulticategoryFilter).WhereElementIsNotElementType().ToElements();
            // Mediante LINQ, afinamos, solo con categoria !=null y Category.IsCuttable true
            names = notMultiCategoryFounds.Where(x => x.Category!=null && x.Category.IsCuttable).Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO son ejemplares de puertas ni ventanas.\nCon Categoria no null y IsCuttable");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
