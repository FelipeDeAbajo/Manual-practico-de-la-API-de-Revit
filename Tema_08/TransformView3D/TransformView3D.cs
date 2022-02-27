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

namespace TransformView3D
{
    [Transaction(TransactionMode.Manual)]
    public class TransformView3D : IExternalCommand
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
                    Line line = locationCurve.Curve as Line;
                    double angle = XYZ.BasisX.AngleTo(line.Direction);
                    // Creamos transaction
                    using (Transaction tx = new Transaction(doc))
                    {
                        //Iniciamos transaction
                        tx.Start("Transaction Desplazar");

                        //Obtenemos la vista actual
                        View view = uidoc.ActiveView;
                        // Si no es 3D finalizamos
                        if (view.ViewType != ViewType.ThreeD)
                        {
                            message = "Situate en una vista 3D";
                            return Result.Failed;
                        }

                        //Obtenemos la Caja de sección
                        BoundingBoxXYZ box = (view as View3D).GetSectionBox();

                        //Giramos según el cuadrante
                        if (angle > Math.PI / 2 && angle < 3 * Math.PI / 2) angle = -angle;

                         //Creamos una Transform con el giro del wall
                         Transform transform = Transform.CreateRotation(XYZ.BasisZ, -angle);
                        //Aplicamos la Transform
                        box.Transform = box.Transform.Multiply(transform);

                        //Asignamos la transformada a la vista 3D
                        (view as View3D).SetSectionBox(box);
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
            TaskDialog.Show("Manual Revit API", "Caja de sección girada");    

            return Result.Succeeded;
        }
    }
}
