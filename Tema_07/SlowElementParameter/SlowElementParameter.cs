#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace SlowElementParameter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowElementParameter : IExternalCommand
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

            //Creamos un filtro ElementParameter para buscar habitaciones cuya área es mayor que el valor especificado.
            //Necesitamos un provider y un evaluator 

            // Provider
            ParameterValueProvider pvp = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_AREA));
            // Evaluator
            FilterNumericRuleEvaluator fnrv = new FilterNumericGreater();
            // rule value    
            double ruleValue = 700; // habitaciones cuya área es mayor que 200
            
            // rule
            FilterRule fRule = new  FilterDoubleRule(pvp, fnrv, ruleValue, 1E-6);

            // Creaamos  ElementParameterFilter
            ElementParameterFilter filterMayorQue = new ElementParameterFilter(fRule);

            // Aplicar el filtro a los elementos del documento activo.
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> roomsMayorQue = collector.WherePasses(filterMayorQue).ToElements();

            List<string> names = roomsMayorQue.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI tienen el área mayor");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Usamos filtro inverso
            ElementParameterFilter filterMenorQue = new ElementParameterFilter(fRule, true);
            collector = new FilteredElementCollector(doc);

            // Creamos el filtro RoomFilter
            RoomFilter roomFilter = new RoomFilter();

            IList<Element> roomsMenorQue = collector.WherePasses(roomFilter).WherePasses(filterMenorQue).ToElements();

             names = roomsMenorQue.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO tienen el área mayor");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
