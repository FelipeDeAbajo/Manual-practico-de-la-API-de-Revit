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

namespace CrearNubeRevision
{
    [Transaction(TransactionMode.Manual)]
    public class CrearNubeRevision : IExternalCommand
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

           


            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Nube revisión");

                RevisionCloud.Create(document, document.ActiveView, revision.Id, curves);
                newRevisionCloud.Commit();
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
