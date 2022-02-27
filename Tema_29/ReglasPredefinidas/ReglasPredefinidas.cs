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

namespace ReglasPredefinidas
{
    [Transaction(TransactionMode.Manual)]
    public class ReglasPredefinidas : IExternalCommand
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

            //Creamos string para salida
            string rules = string.Empty;

            //Iteramos por todas las reglas
            foreach (PerformanceAdviserRuleId id in PerformanceAdviser.GetPerformanceAdviser().GetAllRuleIds())
            {
                //Obtenenos nombre de cada regla
                string ruleName = PerformanceAdviser.GetPerformanceAdviser().GetRuleName(id);
                rules = rules + ruleName + "\r";

            }
            
            //Obtenemos numero de reglas
            int numberRules = PerformanceAdviser.GetPerformanceAdviser().GetNumberOfRules();

            rules = rules + "\n\nNúmero total de reglas: " + numberRules;
            TaskDialog.Show("Revit API Manual", rules);

            //Ejecutamos todas las reglas
            IList<FailureMessage> failureMessages = PerformanceAdviser.GetPerformanceAdviser().ExecuteAllRules(doc);

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction ReglasPredefinidas");

                foreach (FailureMessage failureMessage in failureMessages)
                {
                    //Obtenemos descripción
                    string temp = failureMessage.GetDescriptionText();

                    //Obtenemos ElementId involucrados
                    ICollection<ElementId> elementIds= failureMessage.GetFailingElements();

                    //Procesamos las fallas. Debe estar en una Transaction
                    doc.PostFailure(failureMessage);
                }

              //Confirmamos Transaction
              tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
