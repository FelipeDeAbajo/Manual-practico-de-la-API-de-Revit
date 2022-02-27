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

namespace LogicalAnd
{
    [Transaction(TransactionMode.Manual)]
    public class LogicalAnd : IExternalCommand
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

           // Buscamos todas las instancias de la puerta en el proyecto encontrando todos los elementos
           // que pertenecen a la categoría de la puerta  y son instancias de la familia.
         
            //Contruimos un filtro de clase.
           ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

            // Creamos un filtro de categoría. Doors
            ElementCategoryFilter doorsCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);

            // Creamos el filtro lógico. Door y FamilyInstances
            LogicalAndFilter doorInstancesFilter = new LogicalAndFilter(familyInstanceFilter, doorsCategoryfilter);

            // Aplicamos el filtro a los elementos del documento activo
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.WherePasses(doorInstancesFilter).ToElements();
        
            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI son FamiliInstance y puertas");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
