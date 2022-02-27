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

namespace PickSelectFilter
{
    [Transaction(TransactionMode.Manual)]
    public class PickSelectFilter : IExternalCommand
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
                #region Seleccionar un elemento Muro
                // Creamos una instancia de WallSelectionFilter
                ISelectionFilter selectionFilterWall = new FilterClassAux.WallSelectionFilter();
                // Obtenemos una Referencia a la selección
                // En este caso Element
                Reference referenceWall = uidoc.Selection.PickObject(ObjectType.Element, selectionFilterWall, "Selecciona un Muro");
 
                if (referenceWall != null)
                {   
                    // Obtenemos el Element, desde la Reference
                    Element element = doc.GetElement(referenceWall.ElementId);
                    TaskDialog.Show("Manual Revit API", "Seleccionado muro: " + element.Name);
                }
                #endregion

                #region Seleccionar una Face plana
                // Creamos una instancia de PlanarFaceSelectionFilter
                ISelectionFilter selectionFilterFace = new FilterClassAux.PlanarFaceSelectionFilter(uidoc.Document);
                // Obtenemos una Referencia a la selección
                // En este caso Face
                Reference referenceFace = uidoc.Selection.PickObject(ObjectType.Face, selectionFilterFace, "Selecciona una cara");
                if (referenceFace != null)
                {
                    // Obtenemos el Element, desde la Reference
                    Element element = doc.GetElement(referenceFace.ElementId);
                    // Recuperamos el objeto seleccionado (Face), desde el element
                    GeometryObject geometryObject = element.GetGeometryObjectFromReference(referenceFace);
                    // Lo parseamos a Face
                    Face face = geometryObject as Face;

                    string msg = "Seleccionado muro: " + element.Name;
                    msg = msg + "\nFace seleccionada, área (unidades internas):" + face.Area.ToString("N2");
                    TaskDialog.Show("Manual Revit API", "Seleccionado muro: " + msg);

                }
                #endregion

            }
            catch
            {
                message = "Operación cancelada. No se puede continuar";
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
