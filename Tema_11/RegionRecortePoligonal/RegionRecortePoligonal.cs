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

namespace RegionRecortePoligonal
{
    [Transaction(TransactionMode.Manual)]
    public class RegionRecortePoligonal : IExternalCommand
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

            //Solo admitimos vista en planta
            if (uidoc.ActiveView is ViewPlan view)
            {
                //Creamos nuevo CurveLoop
                CurveLoop loop = new CurveLoop();
                //Creamos 6 XYZ
                XYZ xYZ0 = new XYZ(30, 20, 0);
                XYZ xYZ1 = new XYZ(30, -20, 0);
                XYZ xYZ2 = new XYZ(-10, -20, 0);
                XYZ xYZ3 = new XYZ(-10, -10, 0);
                XYZ xYZ4 = new XYZ(-30, -10, 0);
                XYZ xYZ5 = new XYZ(-30, 20, 0);

                //Añadimos 6 Line ordenadasu orientadas al CurveLoop
                loop.Append(Line.CreateBound(xYZ0, xYZ1));
                loop.Append(Line.CreateBound(xYZ1, xYZ2));
                loop.Append(Line.CreateBound(xYZ2, xYZ3));
                loop.Append(Line.CreateBound(xYZ3, xYZ4));
                loop.Append(Line.CreateBound(xYZ4, xYZ5));
                loop.Append(Line.CreateBound(xYZ5, xYZ0));

                //Creamos Transaction
                using (Transaction tx = new Transaction(doc))
                {
                    //Iniciamos Transaction
                    tx.Start("Transaction Region recorte");

                    //Accedemos a la region
                    ViewCropRegionShapeManager vcrShapeMgr = view.GetCropRegionShapeManager();
                    //Asignamos la region
                    vcrShapeMgr.SetCropShape(loop);

                    //Activamos recorte y visibilidad
                    view.CropBoxActive = true;
                    view.CropBoxVisible = true;

                    //Confirmamos Transaction
                    tx.Commit();
                }
            }
            else
            {
                message = "Solo vista en planta";
                return Result.Cancelled;
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Region de recorte creada");

            return Result.Succeeded;
        }      
    }
}
