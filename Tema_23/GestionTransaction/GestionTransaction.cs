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

namespace GestionTransaction
{
    [Transaction(TransactionMode.Manual)]
    public class GestionTransaction : IExternalCommand
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
            //Creamos una Line
            Curve curve = Line.CreateBound(XYZ.Zero, new XYZ(10, 0, 0));

            //Obtenemos Level. Suponemos vista en planta
            Level level = doc.ActiveView.GenLevel;

            if (level == null)
            {
                message = "Solo vistas en planta";
                return Result.Failed;
            }

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Definimos FailureHandlingOptions
                FailureHandlingOptions failureHandlingOptions = tx.GetFailureHandlingOptions();

                //Asignamos a failureHandlingOptions nueva instancia de WallsFailuresManager
                failureHandlingOptions.SetFailuresPreprocessor(new WallsFailuresManager());

                //Iniciamos Transaction
                tx.Start("Transaction Muros superpuestos");
                tx.SetFailureHandlingOptions(failureHandlingOptions);

                //Construimos dos muros identicos
                Wall wallA = Wall.Create(doc, curve, level.Id, false);
                Wall wallB = Wall.Create(doc, curve, level.Id, false);

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }

    //Implementamos IFailuresPreprocessor
    public class WallsFailuresManager : IFailuresPreprocessor
    {

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            //Obtenemos todos lo fallos
            IList<FailureMessageAccessor> failureMessageAccessors = failuresAccessor.GetFailureMessages();

            //Iteramos en todos los fallos
            foreach (FailureMessageAccessor failure in failureMessageAccessors)
            {
                string mensajeText = failure.GetDescriptionText();

                if (failure.GetFailureDefinitionId() == BuiltInFailures.OverlapFailures.WallsOverlap)
                {
                    //Opción 1. Borramos los Element
                  //  failuresAccessor.DeleteElements(failure.GetFailingElementIds() as IList<ElementId>);

                    //Opción 2. Confirmamos la Transaction y salimos
                   // failuresAccessor.CommitPendingTransaction();

                    //Opción 3. Borramos las advertencias
                   // failuresAccessor.DeleteWarning(failure);

                    //Opción 4. Resolvemos el fallo
                    failuresAccessor.ResolveFailure(failure);
                }

            }
            //Si el fallo no es de los controlados
            return FailureProcessingResult.Continue;
        }

    }
}


