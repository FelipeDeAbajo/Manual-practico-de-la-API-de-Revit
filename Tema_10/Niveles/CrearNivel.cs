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

namespace Niveles
{
    [Transaction(TransactionMode.Manual)]
    public class CrearNivel : IExternalCommand
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

            // Selección actual
            Selection sel = uidoc.Selection;

            ICollection<ElementId> elementIdsList = sel.GetElementIds();
            //Cpmprobamos que solo tenemos 1 objeto seleccionado
            if (elementIdsList.Count != 1)
            {
                message = "Debe selecionar solo un elemento Wall.";
                return Result.Failed;
            }
            //Chequeamos si es Wall y almacenamos en wall.
            //Si no es Wall continuamos
            else if (doc.GetElement(elementIdsList.FirstOrDefault()) is Wall wall)
            {
                BoundingBoxXYZ boundingBoxXYZ = null;
                BoundingBoxXYZ boundingBoxXYZView = null;

                //Obtenemos del muro el boundingBoxXYZ
                boundingBoxXYZ = wall.get_BoundingBox(null);

                //Obtenemos el nivel del muro
                Level level = doc.GetElement(wall.LevelId) as Level;
                // obtenemos la vista asociada, si existe
                ElementId elementId = level.FindAssociatedPlanViewId();
                if (elementId != ElementId.InvalidElementId)
                {
                    View viewLevel = doc.GetElement(elementId) as View;
                    boundingBoxXYZView = wall.get_BoundingBox(viewLevel);
                }

                // La elevación la ponemos en la semi suma de las Zs del boundingBoxXYZ
                double elevation = (boundingBoxXYZ.Max.Z + boundingBoxXYZ.Min.Z) / 2;
                double elevationView = (boundingBoxXYZView.Max.Z + boundingBoxXYZView.Min.Z) / 2;

                // Creamos el nivel. Usamos transaction
                using (Transaction tx = new Transaction(doc))
                {
                    tx.SetName("Creación de Nivel");
                    tx.Start();
                    Level levelGeom = Level.Create(doc, elevation);
                    Level levelView = Level.Create(doc, elevationView);

                    // Cambiamos el nombre
                    levelGeom.Name = "Nivel medio (muro)";
                    levelView.Name = "Nivel medio (vista)";

                    tx.Commit();
                }

            }
            else
            {
                message = "Debe selecionar un ejemplar de Wall.";
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
