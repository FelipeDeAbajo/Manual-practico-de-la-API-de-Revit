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

namespace PanelAcoplable
{
    [Transaction(TransactionMode.Manual)]
    public class PanelAcoplable : IExternalCommand
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

            try
            {
                //Construimos el DockablePaneId con el mismo GUID. Considere establecer una const
                DockablePaneId dpid = new DockablePaneId(new Guid("{77C963CE-B7CA-426A-8D51-6E8254D21199}"));

                //Recuperamos el Panel desde la UIApplication
                DockablePane dp = uiapp.GetDockablePane(dpid);

                //Conmutamos su estado
                if (dp.IsShown() == false)
                    dp.Show();

                else

                    dp.Hide();

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
