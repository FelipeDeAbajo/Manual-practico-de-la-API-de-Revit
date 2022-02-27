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

namespace ConvertirLineas
{
    [Transaction(TransactionMode.Manual)]
    public class ConvertirLineas : IExternalCommand
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

            //Accedemos a la selección
            Selection sel = uidoc.Selection;

            //Obtenemos los Element seleccionados
            List<Element> curves = sel.GetElementIds().Select(x => doc.GetElement(x)).ToList();

            //Creamos nuevo DetailCurveArray y ModelCurveArray
            DetailCurveArray detailCurveArray = new DetailCurveArray();
            ModelCurveArray modelCurveArray = new ModelCurveArray();

            //Iteramos en toda la selección
            foreach (Element curve in curves)
            {
                if (curve is DetailCurve) //Si es DetailCurve
                    detailCurveArray.Append(curve as DetailCurve);
                else if (curve is ModelCurve) //Si es ModelCurve
                    modelCurveArray.Append(curve as ModelCurve);
                //Pdemos haber seleccionado muros...
            }

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //Convertimos
                doc.ConvertDetailToModelCurves(doc.ActiveView, detailCurveArray);
                doc.ConvertModelToDetailCurves(doc.ActiveView, modelCurveArray);

                //Confirmamos Transaction
                tx.Commit();
            }

            TaskDialog.Show("Manual Revit API", detailCurveArray.Size+ " Lineas de detalle convertidas.\n"+
                 modelCurveArray.Size + " Lineas de modelo convertidas.");

            return Result.Succeeded;
        }
    }
}
