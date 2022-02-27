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

namespace VisibleInView
{
    [Transaction(TransactionMode.Manual)]
    public class VisibleInView : IExternalCommand
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
           
            // Buscamos el nombre de la vista a procesar. La vista debe existir
            string viewToTest = "Nivel 1";

            //Obtenemos la vista, y finalmente su Id
            ElementId viewToTestId = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().
                Where(q => !q.IsTemplate && q.Name == viewToTest).FirstOrDefault().Id;

            //Construimos el filtro
            VisibleInViewFilter vivFilter = new VisibleInViewFilter(doc, viewToTestId);

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo
          
            IList<Element> elementsSet = collector.WherePasses(vivFilter).ToElements();

            List<string> names = elementsSet.Select(x => x.Name).ToList();
            names.Insert(0, "Tipos que SI son visibles en la vista");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
