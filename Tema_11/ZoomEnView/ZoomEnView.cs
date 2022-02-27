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

namespace ZoomEnView
{
    [Transaction(TransactionMode.Manual)]
    public class ZoomEnView : IExternalCommand
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
            
            //Obtenemos la UIView que coincide con la vista actual
            UIView uiView = uidoc?.GetOpenUIViews()?.FirstOrDefault(item => item.ViewId == uidoc.ActiveView.Id);
            //Ajustamos en pamtalla.
            uiView.ZoomToFit();

            return Result.Succeeded;
        }
    }
}
