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

namespace BoundingBoxIntersects
{
    [Transaction(TransactionMode.Manual)]
    public class BoundingBoxIntersects : IExternalCommand
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

            // Use el filtro BoundingBoxIntersects  para encontrar elementos con un cuadro
            // delimitador que intersecte o este dentro del Outline. 

            // Creamos un Outline, usamos dos puntos XYZ minimo y maximo. 
            //Convertimos 10 m a unidades internas, o manejamos 10 pies
            double valor = 10; /*UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Meters);*/
            Outline myOutLn = new Outline(new XYZ(0, 0, 0), new XYZ(valor, valor, valor));

            // Creamos el filtro BoundingBoxIntersects con el Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

            // Aplicamos el filtro a los elementos del documento activo

            // Este filtro excluye todos los objetos derivados de View y los objetos derivados de ElementType
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elementsList = collector.OfClass(typeof(Wall)).WherePasses(filter).ToElements();

            List<string> names = elementsList.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI estan estan dentro o intersectan Outline");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            // Buscamos  elementos con BoundingBox que este fuera del Outline. 
            BoundingBoxIntersectsFilter invertFilter = new BoundingBoxIntersectsFilter(myOutLn, true); // filtro inverso
            collector = new FilteredElementCollector(doc);
            IList<Element> notIntersectWalls =
                collector.OfClass(typeof(Wall)).WherePasses(invertFilter).ToElements();

           names = notIntersectWalls.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que NO estan estan dentro o intersectan Outline");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));


            return Result.Succeeded;
        }
    }
}
