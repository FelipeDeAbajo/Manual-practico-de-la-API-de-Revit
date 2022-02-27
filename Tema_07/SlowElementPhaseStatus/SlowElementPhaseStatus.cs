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

namespace SlowElementPhaseStatus
{
    [Transaction(TransactionMode.Manual)]
    public class SlowElementPhaseStatus : IExternalCommand
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

            // Obtenemos la fase de la vista actual
            Phase phaseVistaActual= doc.GetElement(doc.ActiveView.get_Parameter(BuiltInParameter.VIEW_PHASE).AsElementId()) as Phase;

            //Tambien podemos obtener todas las fases del proyecto
            PhaseArray phaseArray = doc.Phases;
           // Y seleccionamos la de indice 2
            Phase phase = phaseArray.get_Item(2);

            //Construimos el filtro. seleccionamos los element construidos en esta fase
            ElementPhaseStatusFilter elementStatusNewFilter = new ElementPhaseStatusFilter(phaseVistaActual.Id, ElementOnPhaseStatus.New);

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(elementStatusNewFilter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI estan construidos en la fase");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            //Construimos el filtro. seleccionamos los element demolidos en esta fase
            ElementPhaseStatusFilter elementStatusDemoFilter = new ElementPhaseStatusFilter(phaseVistaActual.Id, ElementOnPhaseStatus.Temporary);

             collector = new FilteredElementCollector(doc);
            elementsList = collector.WherePasses(elementStatusDemoFilter).ToElements();

            names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI estan construidos y demolidos en la fase");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));
            return Result.Succeeded;
        }
    }
}
