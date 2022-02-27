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

namespace SlowFamilyInstance
{
    [Transaction(TransactionMode.Manual)]
    public class SlowFamilyInstance : IExternalCommand
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

            // Crea un filtro FamilyInstance para los elementos que son instancias de familia del símbolo de familia dado

            // Encuentra todos los Symbol de la familia cuyo nombre es "450 x 450"
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector = collector.OfClass(typeof(FamilySymbol));

            string nombreSymbol = "450 x 450";
          
            // Obtener el Id de elemento para el Symbol de la familia
            // que se utilizará para encontrar la instancia de la familias
            var query = from element in collector where element.Name == nombreSymbol select element;
            List<Element> famSyms = query.ToList<Element>();

            //Creamos una lista vacia de element
            ICollection<Element> elementsList = new List<Element>();

            foreach (Element symbol in famSyms)
            {
                // Creamos el filtroFamilyInstanceFilter con el Id de FamilySymbol
                FamilyInstanceFilter filter = new FamilyInstanceFilter(doc, symbol.Id);

                // Aplicamos el filtro a los elementos del documento activo
                collector = new FilteredElementCollector(doc);
                //Concatenamos los elementos filtrados
                elementsList = elementsList.Concat(collector.WherePasses(filter).ToElements()).ToList();

            }

            List<string> names = elementsList.Select(x => x.Name).ToList();

            names.Insert(0, "Elementos que SI pertenecen a un Symbol con ese nombre");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }
    }
}
