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

namespace VistaActual
{
    [Transaction(TransactionMode.Manual)]
    public class VistaActual : IExternalCommand
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

            // Obtenemos la vista actual
            View view = uidoc.ActiveView;

            //Obtenemos la clase
            TaskDialog.Show("API Revit Manual", "Clase de la vista actual: " + view.GetType().Name);

            //Obtenemos el ViewType de la enumeración
            TaskDialog.Show("API Revit Manual", "ViewType de la vista actual: " + view.ViewType);

            //Obtenemos su tipo
            ViewFamilyType viewFamilyType = doc.GetElement(view.GetTypeId()) as ViewFamilyType;
            TaskDialog.Show("API Revit Manual", "Tipo de la vista actual, ViewFamilyType: " + viewFamilyType.Name);

            //Obtenemos su familia
            TaskDialog.Show("API Revit Manual", "Tipo de la vista actual, ViewFamily: " + viewFamilyType.ViewFamily);

            return Result.Succeeded;
        }
    }
}
