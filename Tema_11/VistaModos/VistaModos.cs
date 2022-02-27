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

namespace VistaModos
{
    [Transaction(TransactionMode.Manual)]
    public class VistaModos : IExternalCommand
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

            //Vista actual
            View view = uidoc.ActiveView;

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction modos temporales");

                //Obtenemos los modos temporales
                TemporaryViewModes viewModes = view.TemporaryViewModes;

                if (viewModes == null)
                {
                    message = "La vista no soporta modos temporales";
                    return Result.Cancelled;
                }
                else if(doc.IsFamilyDocument) //Si es FamilyDocument podemos acceder a PreviewFamilyVisibilityMode
                {
                    // Los modos debe ser viables y estar habilitado
                    if (viewModes.IsModeAvailable(TemporaryViewMode.PreviewFamilyVisibility) && viewModes.IsModeEnabled(TemporaryViewMode.PreviewFamilyVisibility))
                    {
                        //El estado debe ser viable
                        if (viewModes.IsValidState(PreviewFamilyVisibilityMode.On))
                        {
                            viewModes.PreviewFamilyVisibility = PreviewFamilyVisibilityMode.On;
                        }
                    }
                }
                else
                {
                    // Los modos debe ser viables y estar habilitado
                    if (viewModes.IsModeEnabled(TemporaryViewMode.RevealHiddenElements) && viewModes.IsModeAvailable(TemporaryViewMode.RevealHiddenElements))
                    {
                        viewModes.RevealHiddenElements = true;
                    }
                };

                //Confirmamos Transaction
                tx.Commit();
            }
            //Mensaje final
            TaskDialog.Show("Manual Revit API", "Modos cambiados");

            return Result.Succeeded;
        }
    }
}
