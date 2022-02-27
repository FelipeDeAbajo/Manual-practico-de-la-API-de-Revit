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

namespace RegionRecorteDividida
{
    [Transaction(TransactionMode.Manual)]
    public class RegionRecorteDividida : IExternalCommand
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
                //Creamos Transaction
                using (Transaction tx = new Transaction(doc))
                {
                    //Iniciamos Transaction
                    tx.Start("Transaction Region recorte");

                    //Accedemos a la region
                    ViewCropRegionShapeManager vcrShapeMgr = view.GetCropRegionShapeManager();

                    //Eliminamos regiones poligonales
                    vcrShapeMgr.RemoveCropRegionShape();

                    //Dividimos la region
                    vcrShapeMgr.SplitRegionHorizontally(0, 0.4, 0.6);

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
            TaskDialog.Show("Manual Revit API", "Region de recorte dividida");

            return Result.Succeeded;
        }

    }
}
