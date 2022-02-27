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

namespace Fases
{
    [Transaction(TransactionMode.Manual)]
    public class Fases : IExternalCommand
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

            //Selección actual
            Selection sel = uidoc.Selection;

            ICollection<ElementId> elementIdsList = sel.GetElementIds();
            if (elementIdsList.Count != 1)
            {
                message = "Debe selecionar solo un elemento Pilar structural.";
                return Result.Failed;
            }
            else if (doc.GetElement(elementIdsList.FirstOrDefault()) is FamilyInstance pilar
                //además de ser Familiinstance debe tener categoría OST_StructuralColumns
                && pilar.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns)
            {
                // Obtenemos el listado de Fases desde el Document obtenido del pilar
                // Podriamos tambien haberlo obtenido del doc
                PhaseArray phaseArray = pilar.Document.Phases;

                // Comprobamos que el pilar puede ser cambiado de fase 
                bool isModificable = pilar.ArePhasesModifiable();

                if(!isModificable)
                {
                    // Si no es modificable salimos con Failed
                    message = "No se pueden modificar las Fases del elemento";
                    elements.Insert(pilar);
                    return Result.Failed;
                }
                // Obtenemos la Fase de creación del pilar
                ElementId idFase = pilar.get_Parameter(BuiltInParameter.PHASE_CREATED).AsElementId();
               
                // Creamos un Fase para almacenar en su caso la nueva Fase
                Phase newFase = null;
   
                // Buscamos el indide de la fase actual en phaseArray
                System.Collections.IEnumerator enumerator = phaseArray.GetEnumerator();
                enumerator.Reset();
                int n = -1; //Contador
                while (enumerator.MoveNext())
                {
                    n++;
                    Phase mPhase = enumerator.Current as Phase;
                    // No puede ser el ultimo de la lista. Dado que queremos el siguiente
                    if (mPhase.Id == idFase && n < phaseArray.Size - 1)
                    {
                        // Obtenemos la Fase siguiente
                        newFase = phaseArray.get_Item(n + 1);
                        break;
                    }
                }
                // Si tenemos newFase
                if (newFase != null)
                {
                    using (Transaction tx = new Transaction(doc, "Cambio de Fase"))
                    {
                        tx.Start();
                        pilar.get_Parameter(BuiltInParameter.PHASE_CREATED).Set(newFase.Id);
                        tx.Commit();
                        
                        TaskDialog.Show("Manual Revit API", "Nueva Fase para el elemento: "+ newFase.Name);

                    }
                }
                // Si no tenemos newFase
                else
                {
                    TaskDialog.Show("Manual Revit API", "Imposible cambiar de Fase" );

                }
            }
            else
            {
                message = "Debe selecionar un ejemplar de Pilar estructural.";
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
