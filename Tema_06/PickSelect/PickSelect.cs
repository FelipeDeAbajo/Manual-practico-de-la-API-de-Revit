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

namespace PickSelect
{
    [Transaction(TransactionMode.Manual)]
    public class PickSelect : IExternalCommand
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

            // Debemos emplear siempre con la selección por usuari try {} catch {}
            try
            {
                #region Seleccionar arista en un elemento
                // Obtenemos una Referencia a la selección
                // En este caso Edge
                Reference reference = uidoc.Selection.PickObject(ObjectType.Edge, "Selecciona arista en elemento");
                if (reference != null)
                {
                    // Obtenemos el Element desde la Reference
                    Element element = doc.GetElement(reference.ElementId);
                    // Recuperamos el objeto seleccionado (Edge), desde el element
                    GeometryObject geometryObject = element.GetGeometryObjectFromReference(reference);
                    // Lo parseamos a Edge
                    Edge edge = geometryObject as Edge;
                    TaskDialog.Show("Manual Revit API", "Longitud en ud. internas: " + edge.ApproximateLength.ToString("N2"));
                }
                #endregion

                #region Seleccionar varios elementos
                // Obtenemos una coleccion de Elements
                IList<Reference> references = uidoc.Selection.PickObjects(ObjectType.Element, "Seleccionar varios elementos");
               
                if (references.Count > 0)
                {
                    // Seleccionamos los Name de los Element
                    List<string> names = references.Select(x => doc.GetElement(x.ElementId).Name).ToList();
                    // Convertimos la lista en un solo string
                    TaskDialog.Show("Manual Revit API", string.Join("\n", names));
                }
                #endregion

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
