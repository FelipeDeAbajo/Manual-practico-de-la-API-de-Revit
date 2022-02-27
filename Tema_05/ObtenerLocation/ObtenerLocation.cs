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

namespace ObtenerLocation
{
    [Transaction(TransactionMode.Manual)]
    public class ObtenerLocation : IExternalCommand
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

            // Accedemos con un objeto seleccionado
            Selection sel = uidoc.Selection;

            //Obtenemos el ElementId desde la selección
            ElementId id = sel.GetElementIds().FirstOrDefault();

            //Obtenemos el Element desde el ElemenId
            Element element = doc.GetElement(id);

            //Obtenemos su Location
            Location location = element.Location;

            //Cuatro posibilidades
            if (location == null)
            {
                TaskDialog.Show("Manual Revit API",  "Su Location es null");

            }
            else if (location is LocationCurve locationCurve)
            {
                Curve curve= locationCurve.Curve;
                XYZ xYZa = curve.GetEndPoint(0);
                XYZ xYZb = curve.GetEndPoint(1);
                TaskDialog.Show("Manual Revit API", string.Format("Su Location es una Curve.\nX: {0}, Y: {1} y Z: {2}.\nX: {3}, Y: {4} y Z: {5}",
                    xYZa.X.ToString("N2"), xYZa.Y.ToString("N2"), xYZa.X.ToString("N2"),
                                    xYZb.X.ToString("N2"), xYZb.Y.ToString("N2"), xYZb.X.ToString("N2")));

            }

            else if (location is LocationPoint locationPoint)
            {
                XYZ xYZ = locationPoint.Point;

                TaskDialog.Show("Manual Revit API", string.Format("Su Location es un XYZ.\nX: {0}, Y: {1} y Z: {2}", 
                    xYZ.X.ToString("N2"), xYZ.Y.ToString("N2"), xYZ.X.ToString("N2")));

            }

            else
            {
                TaskDialog.Show("Manual Revit API", "Tiene Location, pero no tiene valor");

            }


            return Result.Succeeded;
        }
    }
}
