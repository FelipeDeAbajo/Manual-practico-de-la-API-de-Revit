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

namespace EditarElement
{
    [Transaction(TransactionMode.Manual)]
    public class EditarElement : IExternalCommand
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

            Selection sel = uidoc.Selection;

            //Obtenemos colección de ElementId editables
            ICollection<ElementId> checkedOutIds = WorksharingUtils.CheckoutElements(doc, sel.GetElementIds().ToArray());

            TaskDialog.Show("Revit API Manual", "Número de Element seleccionados: " + sel.GetElementIds().Count +
                               "\n\nNúmero de Element editables: " + checkedOutIds.Count);

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction EditarElement");

                //Iteramos para cada ElementId de la selección
                foreach (ElementId elementId in sel.GetElementIds())
                {
                    //Obtenemos el Element
                    Element element = doc.GetElement(elementId);
                 
                    //Confirmamos que el actual es editable
                    bool checkedOutSuccessfully = checkedOutIds.Contains(elementId);

                    if (!checkedOutSuccessfully)
                    {
                        TaskDialog.Show("Revit API Manual", "No se puede editar el Element " + elementId +
                                        ". Es posible que lo este haciendo otro usuario.");
                        continue;
                    }
                    //Si el elemento se actualiza en el Central o se elimina del Central, no es editable
                    ModelUpdatesStatus updatesStatus = WorksharingUtils.GetModelUpdatesStatus(doc, element.Id);
                    if (updatesStatus == ModelUpdatesStatus.DeletedInCentral || updatesStatus == ModelUpdatesStatus.UpdatedInCentral)
                    {
                        TaskDialog.Show("Revit API Manual", "No se puede editar el Element " + elementId +
                            " No está actualizado con el Central, pero está desprotegido.");
                        continue;
                    }

                    //Obtenemos parámetro del Element
                    Parameter parameter = element.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);

                    //Asignamos valor. 
                    parameter.Set(10);

                    //Confirmamos Transaction
                    tx.Commit();
                }

            }

            return Result.Succeeded;
        }
    }

}
