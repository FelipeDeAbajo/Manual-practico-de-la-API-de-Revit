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

namespace ElementOwnerView
{
    [Transaction(TransactionMode.Manual)]
    public class ElementOwnerView : IExternalCommand
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

            // Usamos el filtro ElementOwnerView para buscar Etiquetas en la vista activa

            // Creamos un filtro ElementOwnerView con el Id de la vista actual
            ElementOwnerViewFilter elementOwnerViewFilter = new ElementOwnerViewFilter(doc.ActiveView.Id);

            // Aplicamos el filtro a los elementos del documento activo
            //Anidamos el método abreviado OfClass()

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> ownedByViewFounds =
                collector.WherePasses(elementOwnerViewFilter).OfClass(typeof(IndependentTag)).ToElements();

            List<string> names = ownedByViewFounds.Select(x => x.Name).ToList();
            names.Insert(0, "Etiquetas que SI dependen de la vista actual");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // buscar Etiquetas que no dependen de la vista actual 
            ElementOwnerViewFilter notOwnedFilter = new ElementOwnerViewFilter(doc.ActiveView.Id, true); // filtro inverso
            collector = new FilteredElementCollector(doc);
            ICollection<Element> notOwnedByViewFounds =
                collector.WherePasses(notOwnedFilter).OfClass(typeof(IndependentTag)).ToElements();

            names = notOwnedByViewFounds.Select(x => x.Name).ToList();
            names.Insert(0, "Etiquetas que No dependen de la vista actual");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
