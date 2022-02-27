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

namespace FormularioWinForms
{
    [Transaction(TransactionMode.Manual)]
    public class FormlarioWinForms : IExternalCommand
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

            //Creamos una nueva instancia
            FormularioInicio formularioInicio = new FormularioInicio();

            //Mostramos, esperamos respuesta y obtenemos DialogResult
            System.Windows.Forms.DialogResult dialogResult= formularioInicio.ShowDialog();

            //Segun sea el DialogResult
            if (dialogResult== System.Windows.Forms.DialogResult.OK)
                TaskDialog.Show("Revit API Manual", "SI se ha pulsado el botón Cerrar");
            else
                TaskDialog.Show("Revit API Manual", "NO se ha pulsado el botón Cerrar");
            return Result.Succeeded;
        }
    }
}
