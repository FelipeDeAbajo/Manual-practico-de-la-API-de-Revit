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

namespace ModosDisplayWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class ModosDisplayWorkset : IExternalCommand
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

            //Obtenemos vista actual
            View activeView = doc.ActiveView;

            //Creamos color rojo
            Color red = new Color(255, 0, 0);

            //Creamos WorksharingDisplayGraphicSettings. 
            WorksharingDisplayGraphicSettings settingsToApply = new WorksharingDisplayGraphicSettings(true, red);

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction ModosDisplayWorkset");

                //Obtenemos WorksharingDisplaySettings del Document
                WorksharingDisplaySettings settings = WorksharingDisplaySettings.GetOrCreateWorksharingDisplaySettings(doc);

                //Configuramos a Estado de permanencia
                activeView.SetWorksharingDisplayMode(WorksharingDisplayMode.CheckoutStatus);
               
                //Configuramos estado de permanencia a otros y asignamos.
                settings.SetGraphicOverrides(CheckoutStatus.OwnedByOtherUser, settingsToApply);

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
