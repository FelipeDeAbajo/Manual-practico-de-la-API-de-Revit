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

namespace BloqueoAlternar
{
    [Transaction(TransactionMode.Manual)]
    public class BloqueoAlternar : IExternalCommand
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

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;

            // Chequeamos que solo tenemos un objeto seleccionado
            if (sel.GetElementIds().Count == 0)
            {
                message = "Se debe seleccionar al menos un elemento";
                return Result.Failed;
            }

            // Creamos transaction
            using (Transaction tx = new Transaction(doc))
            {
                // Iniciamos transaction

                tx.Start("Transaction bloqueo");
                //Iniciamos bucle para cada elemento seleccionado
                foreach(Element element in sel.GetElementIds().Select(x => doc.GetElement(x)))
                {
                    //Alternamos el bloqueo
                    if (element.Pinned) element.Pinned = false;
                    else element.Pinned = true;
                }

                //Confirmamos transaction
                tx.Commit();
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Bloqueo alternado");
            return Result.Succeeded;
        }
    }
}
