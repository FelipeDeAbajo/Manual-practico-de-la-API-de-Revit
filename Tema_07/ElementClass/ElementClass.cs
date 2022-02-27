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

namespace ElementClass
{
    [Transaction(TransactionMode.Manual)]
    public class ElementClass : IExternalCommand
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

            // Excepciones https://www.revitapidocs.com/2022/4b7fb6d7-cb9c-d556-56fc-003a0b8a51b7.htm

            // Usamos ElementClassFilter para buscar familyInstances 
           ElementClassFilter filter = new ElementClassFilter(typeof(FamilyInstance));

            // Aplicamos el filtro a los elementos del documento activo

            FilteredElementCollector collector = new FilteredElementCollector(doc);
             IList<Element> elementsList = collector.OfClass(typeof(FamilyInstance))/*.WherePasses(filter)*/.ToElements();

            //Seleccionamos solo aquellas con categoría tipo FamilyInstance

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son FamilyInstance");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Utilizamos el método abreviado  OfClass()
            // Buscamos tipos que Si son muros
            collector = new FilteredElementCollector(doc);
            elementsList = collector.OfClass(typeof(WallType)).ToElements(); //filtro abreviado

            names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son WallType");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            ElementClassFilter elementNotClassFilter = new ElementClassFilter(typeof(Wall), true);//filtro inverso
            // Empleamos .WhereElementIsNotElementType(), solo admitimos ejemplares, no tipos
            // Reiniciamos FilteredElementCollecto
            collector = new FilteredElementCollector(doc);
            ICollection<Element> notMultiCategoryFounds = collector.
                WherePasses(elementNotClassFilter).WhereElementIsNotElementType().ToElements();
            // Mediante LINQ, afinamos, solo con categoria !=null y Category.IsCuttable true
            names = notMultiCategoryFounds.Where(x => x.Category != null && x.Category.IsCuttable).Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO son Wall.\nCon Categoria no null y IsCuttable");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
