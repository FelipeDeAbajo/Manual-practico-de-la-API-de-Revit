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

namespace CrearLeyenda
{
    [Transaction(TransactionMode.Manual)]
    public class CrearLeyenda : IExternalCommand
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

            //Obtenemos View actual
            View view = uidoc.ActiveView;

            ElementId roomCatId = new ElementId(BuiltInCategory.OST_Rooms);

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {

                //Iniciamos Transaction
                tx.Start("Transaction Leyenda");
                //Creamos la leyenda en (0,0,0)
                ColorFillLegend.Create(view.Document, view.Id, roomCatId, XYZ.Zero);
                //Confirmamos Transaction             
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
