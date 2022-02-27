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

namespace SlowPrimaryDesignOptionMember
{
    [Transaction(TransactionMode.Manual)]
    public class SlowPrimaryDesignOptionMember : IExternalCommand
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

            // Creamos un filtro PrimaryDesignOptionMember para encontrar todos los muros contenidos en todas las opciones de diseño primarias
            PrimaryDesignOptionMemberFilter filter = new PrimaryDesignOptionMemberFilter();

            // Aplicamos el filtro a los elementos del documento activo
            //Utilizamos el comando abreviado OfClass() para encontrar sólo las instancias de muro

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elementsList = collector.OfClass(typeof(Wall)).WherePasses(filter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI son muros en Opciones primarias");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Find walls not contained within primary design options: Use inverted filter to match walls
            PrimaryDesignOptionMemberFilter notPrimaryOptFilter = new PrimaryDesignOptionMemberFilter(true); // inverted filter

            collector = new FilteredElementCollector(doc);
            //Obtenemos las ejemplares de muro

            elementsList = collector.OfClass(typeof(Wall)).WherePasses(notPrimaryOptFilter).ToElements();

            names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que NO son muros en Opciones primarias");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));


            return Result.Succeeded;
        }
    }
}
