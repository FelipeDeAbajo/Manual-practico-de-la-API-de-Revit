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

namespace DesplazarCreandoLocation
{
    [Transaction(TransactionMode.Manual)]
    public class DesplazarCreandoLocation : IExternalCommand
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

            // Chequeamos que el objeto seleccionado es Muro
            if (doc.GetElement(sel.GetElementIds().First()) is Wall wall)
            {

                // Chequeamos que el muro esta basado en linea. Puede ser un muro basado en masa, o "In situ"
                if (wall.Location is LocationCurve locationCurve)
                {
                    // Creamos transaction
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Transaction Desplazar");
                      
                        //Creamos 2 puntos
                        //Obedeceran a alguna lógica y por norma general serán calculados
                        XYZ xYZ0 = new XYZ(-10,-10,-10);
                        XYZ xYZ1 = new XYZ(10, 10, 10);
                        //Creamos una nueva Line
                        Line line = Line.CreateBound(xYZ0, xYZ1);
                        //Asignamos la bueva Line a Location
                        locationCurve.Curve = line;
                        TaskDialog.Show("Manual Revit API", "Elemento desplazado, desplazando su LocationCurve");
                        //Confirmamos transaction
                        tx.Commit();
                    }
                }
                else
                {
                    message = "Se debe seleccionar muro basado en linea";
                    return Result.Failed;
                }

            }
            else
            {
                message = "Se debe seleccionar muro";
                return Result.Failed;
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Muro desplazado");

            return Result.Succeeded;
        }
    }
}
