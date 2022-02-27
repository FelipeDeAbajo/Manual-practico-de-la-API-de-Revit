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

namespace Rejillas
{
    [Transaction(TransactionMode.Manual)]
    public class RejillasInfo : IExternalCommand
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
                string marca = pilar.get_Parameter(BuiltInParameter.COLUMN_LOCATION_MARK).AsString();
                // Doble Split, primero seleccionamos rejilla, y despues el nombre sin el desfase
                string nombreRejilla = marca.Split('-')[0].Split('(')[0];
                if (String.IsNullOrEmpty(nombreRejilla))
                {
                    message = "El Pilar estructural no está asociado a ninguna rejilla.";
                    return Result.Failed;
                }
                // Filtramos para obtener Grids del proyecto
                FilteredElementCollector col = new FilteredElementCollector(doc);
                ICollection<Element> grids = col.OfClass(typeof(Grid)).ToElements();

                // Obtenemos la primera y unica rejilla que cumple
                Grid grid = grids.Where(x => x.Name == nombreRejilla).FirstOrDefault() as Grid;

                string msg = "Rejilla : " + grid.Name;

                // Obtenenos si es arco
                msg += "\nLa rejilla es Arc: " + grid.IsCurved;

                // Obtenemos la Curve
                Autodesk.Revit.DB.Curve curve = grid.Curve;
                if (grid.IsCurved)
                {
                    //Si es Arc, obtenemos centro y radio
                    Autodesk.Revit.DB.Arc arc = curve as Autodesk.Revit.DB.Arc;
                    msg += "\nRadio del Arc: " + arc.Radius;
                    msg += "\nCentro del Arc:  (" + XYZString(arc.Center);
                }
                else
                {
                    // Si es Line, obtenemos longitud
                    Autodesk.Revit.DB.Line line = curve as Autodesk.Revit.DB.Line;
                    msg += "\nLongitud de la Line: " + line.Length;
                }
                // Punto inicial
                msg += "\nPunto inicial: " + XYZString(curve.GetEndPoint(0));
                // Punto final
                msg += "\nPunto final: " + XYZString(curve.GetEndPoint(1));

                // Punto inicial
                XYZString(curve.Tessellate()[0]);
                // Punto final
                XYZString(curve.Tessellate()[1]);

                TaskDialog.Show("Manual Revit API", msg);
            }
            else
            {
                message = "Debe selecionar un ejemplar de Pilar estructural.";
                return Result.Failed;
            }
            return Result.Succeeded;
        }
        // Metodo auxiliar para listar las coordenadas de punto
        private string XYZString(XYZ point)
        {
            return "(" + point.X + ", " + point.Y + ", " + point.Z + ")";
        }
    }
}
