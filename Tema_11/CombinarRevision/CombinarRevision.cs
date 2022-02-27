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

namespace CombinarRevision
{
    [Transaction(TransactionMode.Manual)]
    public class CombinarRevision : IExternalCommand
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

            //Obtenemos todas las Revisions
            List<Revision> revisions = Revision.GetAllRevisionIds(doc).Select(x => doc.GetElement(x)).Cast<Revision>().ToList();

            // Solo se pueden fusion las NO emitidas
            // Seleccionamos solamente NO emitidas
            revisions = revisions.Where(x => x.Issued == false).ToList();

            //Para combinar necesitas >1
            if (revisions.Count < 2)
            {
                message = "Al menos necesitamos 2 Revisiones sin emitir";
                return Result.Cancelled;
            }

            //Obtenemos la ultima
            Revision revision = revisions.LastOrDefault();
            string revisionId = revision.Id.IntegerValue.ToString();
            //Obtenemos revision previa
            Revision previousRevision = revisions.ElementAt(revisions.Count - 2);

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Combinar revisión");

               //Combinamos ultima con previa. obtenemos RevisionCloud
                ISet<ElementId> revisionCloudIds = Revision.CombineWithPrevious(doc, revision.Id);
          
                //Número de RevisionClouds existentes en la última
                int movedClouds = revisionCloudIds.Count;
                //Si la ultima tenia RevisionCloud
                if (movedClouds > 0)
                {
                    //Obtenemios primer RevisionCloud
                    RevisionCloud cloud = doc.GetElement(revisionCloudIds.ElementAt(0)) as RevisionCloud;
                    if (cloud != null)
                    {
                        string msg = string.Format("La Revision {0} se ha borrado y {1} RevisionCloud añadidos a la Revision {2}",
                            revisionId, movedClouds, cloud.RevisionId.ToString());
                        TaskDialog.Show("Manual Revit API", msg);
                    }
                }
                //Confirmamos Transaction
                tx.Commit();
                TaskDialog.Show("Manual Revit API", "Combinacion terminada");

            }

            return Result.Succeeded;
        }
    }
}
