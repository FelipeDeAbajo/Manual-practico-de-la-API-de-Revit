#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace GestionApplication
{
    [Transaction(TransactionMode.Manual)]
    public class GestionApplication : IExternalCommand
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

            //Registramos el evento
           app.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(FailureProcessor);

            TaskDialog.Show("Revit API Manual", "Evento registrado.\nRecuerde que debe anular el registro");
            return Result.Succeeded;
        }
        internal void FailureProcessor(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();

            //Obtenemos la lista de FailureMessageAccessor
            List<FailureMessageAccessor> failureMessageAccessors = failuresAccessor.GetFailureMessages().ToList();

            //Iteramos para cada FailureMessageAccessor
            foreach (FailureMessageAccessor failure in failureMessageAccessors)
            {
                //Podemos consultar su contenido
                string mensajeText = failure.GetDescriptionText();

                if (failure.GetFailureDefinitionId() == BuiltInFailures.OverlapFailures.WallsOverlap)
                {
                    //Opción 1. Borramos los Element
                      failuresAccessor.DeleteElements(failure.GetFailingElementIds() as IList<ElementId>);

                    //Opción 2. Confirmamos la Transaction y salimos
                    //  failuresAccessor.CommitPendingTransaction();

                    //Opción 3. Borramos las advertencias
                  //  failuresAccessor.DeleteWarning(failure);

                    //Opción 4. Resolvemos el fallo
                    // failuresAccessor.ResolveFailure(failure);
                }

            }
        }

    }
}
