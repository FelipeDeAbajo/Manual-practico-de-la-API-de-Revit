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

namespace FamilySymbolFiltro
{
    [Transaction(TransactionMode.Manual)]
    public class FamilySymbolFiltro : IExternalCommand
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

            // Acceso a la selección actual
            Selection sel = uidoc.Selection;

            ICollection<ElementId> elementIdsList = sel.GetElementIds();
            if (elementIdsList.Count != 1)
            {
                message = "Debe selecionar solo un elemento.";
                return Result.Failed;
            }
            else if (doc.GetElement(elementIdsList.FirstOrDefault()) is FamilyInstance familyInstance)
            {
                //Obtenemos el Id de la Familiy
                ElementId familyId = familyInstance.Symbol.Family.Id;

                //Creamos el filtro con el Id de la Familia. Considere utilizar  .GetFamilySymbolIds()
                FamilySymbolFilter filterFamilySymbol = new FamilySymbolFilter(familyId);

                FilteredElementCollector collector = new FilteredElementCollector(doc);

                // Aplicamos el filtro a los elementos del documento activo

                IList<Element> elementsSet = collector.WherePasses(filterFamilySymbol).ToElements();

                List<string> names = elementsSet.Select(x => x.Name).ToList();
                names.Insert(0, "Tipos que SI pertenecen a la Familia. Uso de filtro");
                TaskDialog.Show("Manual Revit API", string.Join("\n", names));

                // Obtenemos lo ids de tipos desde Family
                ISet<ElementId> elementIds = familyInstance.Symbol.Family.GetFamilySymbolIds();
                elementsSet = elementIds.Select(x => doc.GetElement(x)).ToList();

                names = elementsSet.Select(x => x.Name).ToList();
                names.Insert(0, "Tipos que SI pertenecen a la Familia. Metodo GetFamilySymbolIds()");
                TaskDialog.Show("Manual Revit API", string.Join("\n", names));

                return Result.Succeeded;
            }
            else
            {
                message = "Debe selecionar un ejemplar de familia.";
                return Result.Failed;

            }
        }
    }
}
