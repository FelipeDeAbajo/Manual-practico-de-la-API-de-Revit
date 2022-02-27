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

namespace ElementIdSet
{
    [Transaction(TransactionMode.Manual)]
    public class ElementIdSet : IExternalCommand
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

            // Este filtro permite restringir la aplicaci�n de otros filtros, solamente a la selecci�n inicial
            IList<Element> elementSelect = null;
            try
            {
                //Hacemos una selecci�n rectangular
                elementSelect = uidoc.Selection.PickElementsByRectangle("Selecciona objetos por rect�ngulo");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }

            // Convertimos la selecci�n a una ICollection de ElementId
            ICollection<ElementId> elementIds = elementSelect.Select(x => x.Id).ToList();

            //Creamos el filtro solamente con la selecci�n rectangular
            ElementIdSetFilter filterElementIdSet = new ElementIdSetFilter(elementIds);


            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo

            collector.WherePasses(filterElementIdSet).ToElements();

            // Utilizamos el m�todo abreviado  OfClass()
            // Aplicamos un segundo filtro para seleccionar solamente muros
            IList<Element> elementsSetWall = collector.OfClass(typeof(Wall)).ToElements();

            List<string> names = elementsSetWall.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI estan en la seleccion rectangular y son muros");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));


            return Result.Succeeded;
        }
    }
}
