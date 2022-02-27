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

namespace ElementWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class ElementWorkset : IExternalCommand
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

            //Buscamos el Workset actual
            WorksetId worksetId = doc.GetWorksetTable().GetActiveWorksetId();
             
            // Creamos un filtro ElementWorksetFilter para buscar elementos que pertenezcan al Workset,

            ElementWorksetFilter elementWorksetFilter = new ElementWorksetFilter(worksetId);

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo

            ICollection<Element> worksetElemsfounds = collector.WherePasses(elementWorksetFilter).ToElements();

            List<string> names = worksetElemsfounds.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI pertenecen al Workset actual");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Creamos un filtro ElementWorksetFilter para buscar elementos que no pertenezcan al Workset,

            ElementWorksetFilter elementNotWorksetFilter = new ElementWorksetFilter(worksetId,true);

             collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo

            ICollection<Element> worksetElemsNotfounds = collector.WherePasses(elementNotWorksetFilter).ToElements();

             names = worksetElemsNotfounds.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO pertenecen al Workset actual");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
