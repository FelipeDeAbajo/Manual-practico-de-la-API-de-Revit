#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace AtenuacionLejana
{
    [Transaction(TransactionMode.Manual)]
    public class AtenuacionLejana : IExternalCommand
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

            // Access current selection
            //Vista actual
            View view = uidoc.ActiveView;

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction atenuación lejana");


                if (!view.CanUseDepthCueing())
                {
                    message = "La vista no soporta atenuación lejana";
                    return Result.Cancelled;
                }
                else
                {
                    // Obtenemos los indicadores de profundidad
                    ViewDisplayDepthCueing depthCueing = view.GetDepthCueing();
                    // Activamos los indicadores de profundidad
                    depthCueing.EnableDepthCueing = true;
                    //Inicio y fin de fundido
                    depthCueing.SetStartEndPercentages(0, 40);
                    //Establecemos el límite de fundido
                    depthCueing.FadeTo = 20;
                    view.SetDepthCueing(depthCueing);
                };

                //Confirmamos Transaction
                tx.Commit();
            }
            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Atenuación lejana actualizada");

            return Result.Succeeded;
        }
    }
}
