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

namespace ElementMultiClass
{
    [Transaction(TransactionMode.Manual)]
    public class ElementMultiClass : IExternalCommand
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

            // Usamos ElementMulticlassFilter para buscar familyInstances y wall
            //Creamos un lista con las los tipos Wall y FamilyInstance
            List<Type> elementType = new List<Type>() { typeof(Wall), typeof(FamilyInstance) };

            ElementMulticlassFilter filter = new ElementMulticlassFilter(elementType);

            // Aplicamos el filtro a los elementos del documento activo

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(filter).ToElements();


            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son FamilyInstance o Wall");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            ElementMulticlassFilter elementNotClassFilter = new ElementMulticlassFilter(elementType, true);//filtro inverso
            // Reiniciamos FilteredElementCollecto
            collector = new FilteredElementCollector(doc);
            ICollection<Element> notMultiClassFounds = collector.
                WherePasses(elementNotClassFilter).ToElements();
            // Mediante LINQ, afinamos, solo con categoria !=null y Category.IsCuttable true
            names = notMultiClassFounds.Where(x => x.Category != null && x.Category.IsCuttable).Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO son Wall ni FamilyInstance.\nCon Categoria no null y IsCuttable");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
