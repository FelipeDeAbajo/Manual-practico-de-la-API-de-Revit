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
                #region Seleccionar muros h > 5 por rectangulo

                // Creamos una instancia de WallSelectionFilter
                ISelectionFilter selectionFilterWallAdd = new FilterClassAux.WallSelectionFilter();
                // Obtenemos una Ilist de los Element, con el WallSelectionFilter
                IList<Element> elementWalls = uidoc.Selection.
                    PickElementsByRectangle(selectionFilterWallAdd, "Selecciona muros por rectángulo");
                if (elementWalls.Count > 0)
                {
                    // Obtenemos una lista de string con el nombre y altura de cada muro
                    List<string> namesHeights = elementWalls.Select(x => x.Name + " | " +
                    x.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble().ToString("N2")).ToList();
                    // Pasamos la lista a un solo string, separando por salto de linea
                    TaskDialog.Show("Manual Revit API", string.Join("\n", namesHeights));
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
