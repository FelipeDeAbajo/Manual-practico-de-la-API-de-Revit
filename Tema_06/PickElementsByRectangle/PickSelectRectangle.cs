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

namespace PickElementsByRectangle
{
    [Transaction(TransactionMode.Manual)]
    public class PickSelectRectangle : IExternalCommand
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

            try
            {
                #region Seleccionar muros h >5 por rectangulo
                ISelectionFilter selectionFilterWallAdd = new WallSelectionFilter();

                List<Element> elementWalls = uidoc.Selection.PickElementsByRectangle(selectionFilterWallAdd, "Selecciona muros por rectángulo");
                if (reference != null)
                {
                    Element element = doc.GetElement(reference.ElementId);
                    GeometryObject geometryObject = element.GetGeometryObjectFromReference(reference);
                    Edge edge = geometryObject as Edge;
                    TaskDialog.Show("Manual Revit API", "Longitud en ud. internas: " + edge.ApproximateLength.ToString("N2"));
                }
                #endregion

               

            }
            catch
            {
                //TODO rellenar con un mensaje por lo menos
                message = "Operación cancelada. No se puede continuar";
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
