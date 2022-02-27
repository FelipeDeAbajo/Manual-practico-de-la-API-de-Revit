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

namespace CrearRevision
{
    [Transaction(TransactionMode.Manual)]
    public class CrearRevision : IExternalCommand
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

            //Creamos 4 puntos 
            XYZ xYZ0 = XYZ.Zero;
            XYZ xYZ1 = new XYZ(0, 20, 0);
            XYZ xYZ2 = new XYZ(20, 20, 0);
            XYZ xYZ3 = new XYZ(20, 0, 0);

            //Creamos lista de curves para nube de revisión
            IList<Curve> curves = new List<Curve>();
            curves.Add(Line.CreateBound(xYZ0, xYZ1));
            curves.Add(Line.CreateBound(xYZ1, xYZ2));
            curves.Add(Line.CreateBound(xYZ2, xYZ3));
            curves.Add(Line.CreateBound(xYZ3, xYZ0));

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction revisiones");

                //Creamos 3 Revision
                AddNewRevision(doc, "Descripción de ejemplo 1", "Supervisor 1", "Arquitecto 1");
                AddNewRevision(doc, "Descripción de ejemplo 2", "Supervisor 1", "Arquitecto 2");
                Revision revision = AddNewRevision(doc, "Descripción de ejemplo 3", "Supervisor 1", "Arquitecto 3");

                if (!(doc.ActiveView is View3D) && revision.Issued == false)
                {
                    //Creamos nube de revisión
                    RevisionCloud.Create(doc, doc.ActiveView, revision.Id, curves);
                }

                //Confirmamos Transaction
                tx.Commit();
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Revisiones creadas");


            return Result.Succeeded;
        }
        /// <summary>
        /// Metodo auxiliar para crear Revision
        /// </summary>
        /// <param name="document">Documento del proyecto</param>
        /// <param name="description">Descripción de la Revision</param>
        /// <param name="issuedBy">Operario que la crea</param>
        /// <param name="issuedTo">Operario al que se le asigna</param>
        /// <returns></returns>
        private Revision AddNewRevision(Document document, string description, string issuedBy, string issuedTo)
        {
            //Creamos la Revision con los parametros indicados
            Revision newRevision = Revision.Create(document);
            newRevision.Description = description;
            newRevision.IssuedBy = issuedBy;
            newRevision.IssuedTo = issuedTo;
            newRevision.NumberType = RevisionNumberType.Alphanumeric;
            newRevision.RevisionDate = DateTime.Now.ToShortDateString();
            return newRevision;
        }
    }
}
