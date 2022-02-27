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

namespace CrearMatriz
{
    [Transaction(TransactionMode.Manual)]
    public class CrearMatriz : IExternalCommand
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

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;

            // Chequeamos que tenemos algún objeto seleccionado
            if (sel.GetElementIds().Count == 0)
            {
                message = "Se debe seleccionar algún elemento";
                return Result.Failed;
            }


            // Creamos transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Matriz");
                #region lineal sin asociar
                //Creamos matriz sin asociar 5 miembros
                ICollection<ElementId> elementIdsSinAsociar = LinearArray.ArrayElementWithoutAssociation(doc, uidoc.ActiveView, sel.GetElementIds().First(), 5, new XYZ(0, 3, 0), ArrayAnchorMember.Second);
                #endregion

                #region lineal con asociación
                //Creamos matriz lineal con asociación 5 miembros
                LinearArray linearArray = LinearArray.Create(doc, uidoc.ActiveView, sel.GetElementIds(), 5, new XYZ(30, 0, 0), ArrayAnchorMember.Last);
                //Podemos obtener los ElementId de los grupos de la matriz
                ICollection<ElementId> elementIds = linearArray.GetOriginalMemberIds();
                //Redimensionamo la matriz a 10
                linearArray.NumMembers = 10;
                #endregion

                #region radial con asociación
                //Creamos eje vertical por el (0,0,0)
                Line eje = Line.CreateBound(XYZ.Zero, XYZ.Zero + XYZ.BasisZ);
                //Creamos matriz radial
                RadialArray radialArray = RadialArray.Create(doc, uidoc.ActiveView, elementIds.First(), 5, eje, Math.PI, ArrayAnchorMember.Last);
                #endregion
                //Confirmamos transaction
                tx.Commit();
            }

            TaskDialog.Show("Manual Revit API", "Matrices creadas");

            return Result.Succeeded;
        }
    }
}
