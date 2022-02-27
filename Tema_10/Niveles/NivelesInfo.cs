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
    public class NivelesInfo : IExternalCommand
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

            // Access current selection

            Selection sel = uidoc.Selection;

            ICollection<ElementId> elementIdsList = sel.GetElementIds();
            if (elementIdsList.Count != 1)
            {
                message = "Debe selecionar solo un elemento Wall.";
                return Result.Failed;
            }
            else if (doc.GetElement(elementIdsList.FirstOrDefault()) is Wall wall)
            {
                Level level = doc.GetElement(wall.LevelId) as Level;
                string msg = string.Empty;


                //obtenenos el nombre
                msg = msg + ("\nNombre del nivel: " + level.Name);

                //obtenemos la elevación sobre el origen de coordenadas
                msg = msg + ("\nElevación sobre el origen de coordenadas (unidades internas): " + level.Elevation.ToString("N2"));

                // obtenemos la elevación sobre el proyecto
                msg = msg + ("\nElevación sobre el proyecto (unidades internas): " + level.ProjectElevation.ToString("N2"));

                // obtenemos la vista asociada, si existe
                ElementId elementId = level.FindAssociatedPlanViewId();
                if (elementId != ElementId.InvalidElementId)
                    msg = msg + ("\nVista asociada: " + doc.GetElement(elementId).Name);
                else
                    msg = msg + ("\n\tNo existe vista asociada");

                //Obtenemos el tipo y buscamos se Base de elevación
                LevelType levelType = doc.GetElement(level.GetTypeId()) as LevelType;
                msg = msg + ("\n\n\"Base de elevación\" del tipo: " +
                    levelType.get_Parameter(BuiltInParameter.LEVEL_RELATIVE_BASE_TYPE).AsValueString());

                //Solo en versiones 2022 y posteriores
                double cotaParaBuscarNivel = -0.55; //en unidades internas
                //Obtenemos el nivel mas cercano a la cota cotaParaBuscarNivel
                ElementId id = Level.GetNearestLevelId(doc, cotaParaBuscarNivel, out double diferencia);
                Level nivelCercano = doc.GetElement(id) as Level;
                //Obtenemos el nivel y tambien la diferencia de cotas en unidades internas
                msg = msg + ("\n\nEl nivel mas cercano a la cota (uninades internas): " + cotaParaBuscarNivel +
                    " es: " + nivelCercano.Name +
                    "\nLa diferencia de cota es (uninades internas): " + diferencia.ToString("N2"));

                //Mostramos toda la información
                TaskDialog.Show("Manual Revit API", msg);
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
