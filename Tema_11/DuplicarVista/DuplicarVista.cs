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

namespace DuplicarVista
{
    [Transaction(TransactionMode.Manual)]
    public class DuplicarVista : IExternalCommand
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

            //Obtenemos la vista actual
            View view = doc.ActiveView;

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciar Transaction
                tx.Start("Duplicar vista");
                // Creamos ViewDuplicateOption por defecto
                ViewDuplicateOption viewDuplicateOption =  ViewDuplicateOption.Duplicate;
                //Chequeamos que la vista pueda ser duplicada como dependiente
                if (view.CanViewBeDuplicated(ViewDuplicateOption.AsDependent))
                {
                    //Si es poible la duplicaremos como dependiente
                    viewDuplicateOption = ViewDuplicateOption.AsDependent;
                }
                //Duplicamos la vista actual
                ElementId newViewId = view.Duplicate(viewDuplicateOption);
                View viewDuplicada = view.Document.GetElement(newViewId) as View;
                if (null != viewDuplicada && viewDuplicateOption== ViewDuplicateOption.AsDependent)
                {
                    if (viewDuplicada.GetPrimaryViewId() == view.Id)
                    {
                        TaskDialog.Show("API Revit Manual", "Vista dependiente duplicada");
                    }
                }
                else if (null != viewDuplicada)
                {
                    TaskDialog.Show("API Revit Manual", "Vista duplicada");

                }
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
