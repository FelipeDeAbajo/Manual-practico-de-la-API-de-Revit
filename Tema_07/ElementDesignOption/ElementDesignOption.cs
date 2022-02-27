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

namespace ElementDesignOption
{
    [Transaction(TransactionMode.Manual)]
    public class ElementDesignOption : IExternalCommand
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

            // Creamos un filtro ElementDesignOption para buscar todos los muros en la opci�n de dise�o activa.
            // Tenga en cuenta que si no se est� editando ninguna opci�n de dise�o, el m�todo GetActiveDesignOptionId() devolver� ElementId.InvalidElementId
            // El filtro ElementDesignOptionFilter con un id inv�lido filtrar� los elementos no asociados a una opci�n de dise�o

            ElementId activeOptId = DesignOption.GetActiveDesignOptionId(doc);

            // Creamos el filtro ElementDesignOption
            ElementDesignOptionFilter filter = new ElementDesignOptionFilter(activeOptId);

            // Aplicamos el filtro a los elementos del documento activo
           
            // Utilizamos el m�todo abreviado  OfClass()
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> wallsOfDesignOpt = collector.WherePasses(filter).OfClass(typeof(Wall)).ToElements();

            List<string> names = wallsOfDesignOpt.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI estan en la opci�n de dise�o activa");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));


            // Buscamos todos los muros que no est�n contenidos en la opci�n de dise�o activa:
            ElementDesignOptionFilter notActiveOptFilter = new ElementDesignOptionFilter(activeOptId, true); // inverted filter
            collector = new FilteredElementCollector(doc);
            ICollection<Element> notActiveOptWalls =
                collector.WherePasses(notActiveOptFilter).OfClass(typeof(Wall)).ToElements();

            names = notActiveOptWalls.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO estan en la opci�n de dise�o activa");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
