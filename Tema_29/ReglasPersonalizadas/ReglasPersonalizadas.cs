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

namespace ReglasPersonalizadas
{
    [Transaction(TransactionMode.Manual)]
    public class ReglasPersonalizadas : IExternalCommand
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

            //Obtenemos numero de reglas
            int numeroReglas = PerformanceAdviser.GetPerformanceAdviser().GetNumberOfRules();

            //Ejecutamos solo la última
            IList<FailureMessage> failureMessages = PerformanceAdviser.GetPerformanceAdviser().ExecuteRules(doc, new List<int>() { numeroReglas - 1 });

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction ReglasPersonalizadas");

                foreach (FailureMessage failureMessage in failureMessages)
                {
                    //Procesamos las fallas. Debe estar en una Transaction
                    doc.PostFailure(failureMessage);
                }
                tx.Commit();
            }



            return Result.Succeeded;
        }

        public class FlippedWindowCheck : IPerformanceAdviserRule
        {
            private List<ElementId> m_FlippedWindows;

            private string m_name;

            private string m_description;

            public PerformanceAdviserRuleId m_Id = new PerformanceAdviserRuleId(new Guid("BC38854474284491BD03795675AC7386"));

            private FailureDefinitionId m_windowWarningId;

            private FailureDefinition m_windowWarning;

            public FlippedWindowCheck()
            {
                //Constructor. Completamos propiedades
                m_name = "Comprobación. Ventanas volteadas";
                m_description = "Regla para buscar cualquier ventana que esté volteada";
                m_windowWarningId = new FailureDefinitionId(new Guid("25570B8FD4AD42baBD78469ED60FB9A3"));
                m_windowWarning = FailureDefinition.CreateFailureDefinition(m_windowWarningId, FailureSeverity.Warning, "Algunas ventanas de este proyecto están volteadas.");
            }

            public void InitCheck(Document document)
            {
                //Creamos o reiniciamos la colección de ElementId
                if (m_FlippedWindows == null) m_FlippedWindows = new List<ElementId>();
                else m_FlippedWindows.Clear();
                return;
            }

            public void ExecuteElementCheck(Document document, Element element)
            {
                //Si es FamilyInstance
                if ((element is FamilyInstance familyInstance))
                {
                    //Sis es FacingFlipped lo añadimos a la lista
                    if (familyInstance.FacingFlipped) m_FlippedWindows.Add(familyInstance.Id);
                }
            }

            public void FinalizeCheck(Document document)
            {
                //Si hay ElementId en la coleccion 
                if (m_FlippedWindows.Count > 0)
                {
                    //Creamos FailureMessage
                    FailureMessage failureMessage = new FailureMessage(m_windowWarningId);
                    //Asignamos lista de ElementId con falla
                    failureMessage.SetFailingElements(m_FlippedWindows);

                    //Definimos Transaction
                    using (Transaction tx = new Transaction(document))
                    {
                        //Iniciamos Transaction
                        tx.Start("Transaction ReglasPersonalizadas.Informe");

                        PerformanceAdviser.GetPerformanceAdviser().PostWarning(failureMessage);

                        //Confirmamos Transaction
                        tx.Commit();
                    }
                    m_FlippedWindows.Clear();
                }
            }

            public string GetDescription()
            {
                return m_description;
            }

            public ElementFilter GetElementFilter(Document document)
            {
                return new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            }

            public string GetName()
            {
                return m_name;
            }

            public bool WillCheckElements()
            {
                return true;
            }
        }

    }
}
