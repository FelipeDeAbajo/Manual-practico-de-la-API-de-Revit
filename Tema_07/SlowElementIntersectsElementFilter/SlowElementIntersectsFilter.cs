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

namespace SlowElementIntersectsElementFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowElementIntersectsFilter : IExternalCommand
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
            // Selección de un objeto
            Element elementSelect = null;
            try
            {
                Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Seleccionar elemento para intersección.");
                elementSelect = doc.GetElement(reference.ElementId);
            }
            catch (Exception ex)
            {
                //Si el usuario cancela
                message = ex.Message;
                return Result.Cancelled;
            }
            //Construimos el filtro. Similar a 'Comprobar interferencias de Revit'
            ElementIntersectsElementFilter elementIntersectsElementFilter = new ElementIntersectsElementFilter(elementSelect);
            //Construimos el collector
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            //Aplicamos el filtro
            IList<Element> elementsList = collector.WherePasses(elementIntersectsElementFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que Si entersectan con el elemento seleccionado");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            //Construimos el filtro inverso.
            ElementIntersectsElementFilter elementNotIntersectsElementFilter = new ElementIntersectsElementFilter(elementSelect, true);
            //Construimos el collector
            collector = new FilteredElementCollector(doc);
            //Aplicamos el filtro inverso. Restringimos a FamilyInstance
            IList<Element> elementsNotList = collector.OfClass(typeof(FamilyInstance)).WherePasses(elementNotIntersectsElementFilter).ToElements();

             names = elementsNotList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos, que NO entersectan con el elemento seleccionado y son FamilyInstance");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));
            return Result.Succeeded;
        }
    }
}
