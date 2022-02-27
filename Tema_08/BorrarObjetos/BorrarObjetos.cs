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

namespace BorrarObjetos
{
    [Transaction(TransactionMode.Manual)]
    public class BorrarObjetos : IExternalCommand
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

            //Accedemos con algún objetos seleccionado
            Selection sel = uidoc.Selection;
            //creamos una Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Borrar elementos");
                //Borramos los objetos
                doc.Delete(sel.GetElementIds());
                //Confirmamos la Transaction
                tx.Commit();
            }
            TaskDialog.Show("Revit API Manual", "Elementos borrados");

            return Result.Succeeded;
        }
    }
}
