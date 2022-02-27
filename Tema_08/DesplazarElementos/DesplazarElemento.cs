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

namespace DesplazarElementos
{
    [Transaction(TransactionMode.Manual)]
    public class DesplazarElemento : IExternalCommand
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

            // Chequeamos que solo tenemos un objeto seleccionado
            if (sel.GetElementIds().Count != 1)
            {
                message = "Se debe seleccionar un solo elemento";
                return Result.Failed;
            }

            // Chequeamos que el objeto seleccionado es FamilyInstance
            if (doc.GetElement(sel.GetElementIds().First()) is FamilyInstance familyInstance)
            {
                // Chequeamos la categoría de la FamilyInstance
                if (familyInstance.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StructuralColumns &&
                    familyInstance.Category.Id.IntegerValue != (int)BuiltInCategory.OST_Columns)
                {
                    message = "Se debe seleccionar Pilar";
                    return Result.Failed;
                }

                // Chequeamos que el pilar esta basado en punto
                if (familyInstance.Location is LocationPoint locationPoint)
                {
                    // Creamos transaction
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Transaction Desplazar");
                        // Creamos el vector de desplazamiento
                        XYZ xYZDesplazado = new XYZ(10, 10, 10);
                        // Desplazamos el elemento
                        ElementTransformUtils.MoveElement(doc, familyInstance.Id, xYZDesplazado);
                        //Confirmamos transaction
                        tx.Commit();
                    }
                }
                else
                {
                    message = "Se debe seleccionar Pilar vertical";
                    return Result.Failed;
                }

            }
            else
            {
                message = "Se debe seleccionar instancia";
                return Result.Failed;
            }

            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Pilar desplazado");

            return Result.Succeeded;
        }

    }
}
