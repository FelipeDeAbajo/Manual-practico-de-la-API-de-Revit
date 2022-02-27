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

namespace SeleccionPrevia
{
    [Transaction(TransactionMode.Manual)]
    public class SeleccionPrevia : IExternalCommand
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
          
            TaskDialog.Show("Manual Revit API", "Seleccionado/s " + elementIdsList.Count +
                " objeto/s.");

            //Iteramos para cada objeto
            Debug.Print("Salida nombres en varías líneas:");
            foreach (ElementId id in elementIdsList)
            {
                Debug.Print(doc.GetElement(id).Name);
            }

            List<string>  names = sel.GetElementIds().
                Select(x => doc.GetElement(x).Name).ToList();

            Debug.Print("Salida nombres en una sola línea:");
            Debug.Print(string.Join(",", names));

            return Result.Succeeded;
        }
    }
}
