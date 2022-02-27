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

namespace TestCommand.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CommandTestMensaje : IExternalCommand
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

            // Access current selection

            Selection sel = uidoc.Selection;

            // Retrieve elements from database

            FilteredElementCollector col
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Wall));

            // Filtered element collector is iterable
            int contador = 0;
            foreach (Element e in col)
            {
                contador++;
                elements.Insert(e);
                Debug.Print(e.Name);
            }
           
            // Modify document within a transaction

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Change ProjectInfo");
                doc.ProjectInformation.Author = "Felipe de Abajo";
                tx.Commit();
            }

            if (contador == 0)
            {
                message = "Error. No hay muros";
                return Result.Failed;
            }
            else if (contador == 1)
            {
                message = "Cancelado. Solo hay un muro";
                return Result.Cancelled;

            }
            TaskDialog.Show("Revit API Manual", "Perfecto, hay :" + contador);
            return Result.Succeeded;
        }
    }
}
