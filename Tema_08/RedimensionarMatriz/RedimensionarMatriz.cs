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

namespace RedimensionarMatriz
{
    [Transaction(TransactionMode.Manual)]
    public class RedimensionarMatriz : IExternalCommand
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
            if (sel.GetElementIds().Count != 1)
            {
                message = "Se debe seleccionar un solo elemento";
                return Result.Failed;
            }

            // Chequeamos que el objeto seleccionado es Group
            if (doc.GetElement(sel.GetElementIds().First()) is Group group)
            {

                // Creamos transaction
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Transaction redimensionar");
                  
                    //Creamos filtro
                    ElementClassFilter elementClassFilter = new ElementClassFilter(typeof(LinearArray));
                    //Obtenemos la primera LinearArray
                    LinearArray linearArray = group.Document.GetElement(group.GetDependentElements(elementClassFilter).First()) as LinearArray;
                   
                    //Redimensionamos
                    linearArray.NumMembers = 5;

                    //Confirmamos transaction
                    tx.Commit();
                }
            }
            else
            {
                message = "Se debe seleccionar grupo";
                return Result.Failed;
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Matriz redimensionada");
            return Result.Succeeded;
        }
    }
}
