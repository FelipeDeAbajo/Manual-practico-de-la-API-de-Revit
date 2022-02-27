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

namespace MurosComposicion
{
    [Transaction(TransactionMode.Manual)]
    public class MurosComposicion : IExternalCommand
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

            //Iniciamos con un Wall seleccionado
            Selection sel = uidoc.Selection;
            //Obtenemos el muro
            Wall wall = doc.GetElement(sel.GetElementIds().FirstOrDefault()) as Wall;
            if (wall is null)
            {
                message = "Se debe iniciar con un muro seleccionado";
                return Result.Cancelled;
            }

            //Obtenemos el tipo. Los muros cortina no tiene materiales ni capas
            WallType wallType = wall.WallType;
            if (wallType.Kind != WallKind.Basic)
            {
                message = "Debe ser un muro básico";
                return Result.Cancelled;
            }

            //Obtenemos el material por categoría
            Categories categories = doc.Settings.Categories;
            categories.get_Item(BuiltInCategory.OST_Walls);
            Material materialCategoria = wall.Category.Material;

            string mensaje = "Datos de composición del muro:";
            mensaje = mensaje + "\nEl material por categoría es: " + materialCategoria.Name;

            //Obtenemos la composición
            CompoundStructure compoundStructure = wallType.GetCompoundStructure();
            foreach (CompoundStructureLayer compoundStructureLayer in compoundStructure.GetLayers())
            {
                //Iteramos entre cada material.Obtenemos material y función
                Material layerMaterial = doc.GetElement(compoundStructureLayer.MaterialId) as Material;
                MaterialFunctionAssignment materialFunctionAssignment = compoundStructureLayer.Function;

                //Es funcion de aislamiento?
                if (materialFunctionAssignment == MaterialFunctionAssignment.Insulation)
                {
                    //Obtenemos propiedades del aislamiento
                    mensaje = mensaje + "\n\nAislamento. Material = " + layerMaterial.Name;
                    mensaje = mensaje + "\nEspesor= " + compoundStructureLayer.Width.ToString("N3");
                    double areaMate = wall.GetMaterialArea(layerMaterial.Id, false);
                    mensaje = mensaje + "\nArea= " + areaMate.ToString("N3");
                    double volumenMate = wall.GetMaterialVolume(layerMaterial.Id);
                    mensaje = mensaje + "\nVolumen= " + volumenMate.ToString("N3");
                }

            }

            //Obtenemos los materiales de Pintura
            ICollection<ElementId> elementIdsMateriales = wall.GetMaterialIds(true);
            //Obtenemos area del material pintado. Y volumen
            double areaMatePintado = wall.GetMaterialArea(elementIdsMateriales.First(), true);
            double volumenMatePintado = wall.GetMaterialVolume(elementIdsMateriales.First());

            mensaje = mensaje + "\n\nMaterial Pintado. " + doc.GetElement(elementIdsMateriales.First()).Name +
                           "\nArea= " + areaMatePintado.ToString("N3") +
                           "\nVolumen= " + volumenMatePintado.ToString("N3");

            //Obtenemos indice de la primera capa del nucle
            mensaje = mensaje + "\n\nIndice capa nucleo: " + compoundStructure.GetCoreBoundaryLayerIndex(ShellLayerType.Exterior);

            #region Caras extremas
            //Obtenemos References Exterior. Tomamos 1ª. Suponemos es muro Plano
            IList<Reference> sideFacesE = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);
            Reference referenceFaceE = sideFacesE[0];
            PlanarFace faceE = wall.Document.GetElement(referenceFaceE).GetGeometryObjectFromReference(referenceFaceE) as PlanarFace;

            //Obtenemos References Interior. Tomamos 1ª. Suponemos es muro Plano
            IList<Reference> sideFacesI = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
            Reference referenceFaceI = sideFacesI[0];
            PlanarFace faceI = wall.Document.GetElement(referenceFaceI).GetGeometryObjectFromReference(referenceFaceI) as PlanarFace;

            //Obtenemos area de caras y por defecto
            mensaje = mensaje + "\n\nDatos de caras extremas:";
            mensaje = mensaje + "\nCara Exterior. Area: " + faceE.Area.ToString("N3");
            mensaje = mensaje + "\nCara Interior. Area: " + faceI.Area.ToString("N3");
            mensaje = mensaje + "\nPor defecto muro. Area: " + wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble().ToString("N3");

            #endregion
            TaskDialog.Show("Revit API Manual", mensaje);
           
            return Result.Succeeded;
        }
    }
}
