#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace SlowFamilyStructuralMaterialType
{
    [Transaction(TransactionMode.Manual)]
    public class SlowFamilyStructuralMaterialType : IExternalCommand
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

            // Usamos FamilyStructuralMaterialType para buscar Familias con material estructural Wood
            //Atención se seleccionan Familias
            FamilyStructuralMaterialTypeFilter filter = new FamilyStructuralMaterialTypeFilter(StructuralMaterialType.Wood);

            // Aplicamos el filtro a los elementos del documento activo
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> familyList = collector.WherePasses(filter).ToElements();

            List<string> names  = familyList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI tiemen por material estructural madera");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscamos Familias sin material estructural Wood: Usamos el inverso
            FamilyStructuralMaterialTypeFilter notWoodFilter =
                new FamilyStructuralMaterialTypeFilter(StructuralMaterialType.Wood, true); // filtro inverso
            collector = new FilteredElementCollector(doc);

            familyList = collector.WherePasses(notWoodFilter).ToElements();

            //Construimos una lista de Family.
            // Las Family no implementan Category, Necesitamos FamilyCategory. Hay que pasar de Element a Family
            List<Family> families = familyList.Cast<Family>().Where(x => x.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns).ToList();

            names = families.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que NO tiemen por material estructural madera");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
