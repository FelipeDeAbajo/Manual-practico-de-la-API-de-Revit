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

namespace SlowSelectableInView
{
    [Transaction(TransactionMode.Manual)]
    public class SlowSelectableInView : IExternalCommand
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

            // Construimos el filtro con la vista actual
            SelectableInViewFilter selectableInViewFilter = new SelectableInViewFilter(doc, doc.ActiveView.Id);

            //Construimos el colector
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            //Obtenemos los ejemplares. Restringimos a muros
            IList<Element> elementsList = collector.OfClass(typeof(Wall)).WherePasses(selectableInViewFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI son seleccionables en la vista activa");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
