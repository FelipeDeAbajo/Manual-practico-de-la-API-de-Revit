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

namespace ElementStructuralType
{
    [Transaction(TransactionMode.Manual)]
    public class ElementStructuralType : IExternalCommand
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

            // Creamos un filtro ElementStructuralTypeFilter para buscar elementos que pertenezcan a categoría estructural. Pilares

            ElementStructuralTypeFilter elementStructuralTypeFilter =
                 new ElementStructuralTypeFilter(Autodesk.Revit.DB.Structure.StructuralType.Column);
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo

            ICollection<Element> elementsFilterColumn = collector.WherePasses(elementStructuralTypeFilter).ToElements();

            List<string> names = elementsFilterColumn.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos estructurales que SI son pilares estructurales");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Creamos un filtro ElementStructuralTypeFilter para buscar elementos que pertenezcan a categoría estructural. Vigas

            elementStructuralTypeFilter =
                 new ElementStructuralTypeFilter(Autodesk.Revit.DB.Structure.StructuralType.Beam);
             collector = new FilteredElementCollector(doc);

            // Aplicamos el filtro a los elementos del documento activo

            ICollection<Element> elementsFilterBeams = collector.WherePasses(elementStructuralTypeFilter).ToElements();

            names = elementsFilterBeams.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos estructurales que SI son vigas");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            ElementStructuralTypeFilter elementNonStructuralTypeFilter =
                 new ElementStructuralTypeFilter(Autodesk.Revit.DB.Structure.StructuralType.Column,true);
            collector = new FilteredElementCollector(doc);
     
            ICollection<Element> notStructuralTypeFounds = collector.
                WherePasses(elementNonStructuralTypeFilter).ToElements();

            names = notStructuralTypeFounds.Where(x => x.Category != null && x.Category.CategoryType == CategoryType.Model).Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO son pilares structurales");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
