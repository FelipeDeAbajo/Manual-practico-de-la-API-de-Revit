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

namespace GeometryWallSolid
{
    [Transaction(TransactionMode.Manual)]
    public class GeometryWallSolid : IExternalCommand
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

            Selection sel = uidoc.Selection;
            //Obtenenos la selección
            ICollection<ElementId> elementIds = sel.GetElementIds();

            //Hay algún objeto seleccionado?
            if (elementIds.Count == 0)
            {
                message = "Se debe seleccionar un objeto";
                return Result.Cancelled;
            }

            //Recuperamos el primer objeto
            Element elm = doc.GetElement(elementIds.First());

            //Es de modelo?
            if (elm.Category.CategoryType != CategoryType.Model || elm.Category.IsCuttable == false)
            {
                message = "Se debe seleccionar un objeto de 'modelo' y 'cortable'";
                return Result.Cancelled;
            }

            //Creamos un Options
            Options options = new Options();
            //Asignamos Nivel de detalle Alto
            options.DetailLevel = ViewDetailLevel.Fine;
            //No incluimos objetos no visibles
            options.IncludeNonVisibleObjects = false;
            //No computar References
            options.ComputeReferences = false;

            //Creamos un solid vacio
            Solid solid = null;

            //Obtenemos el GeometryElement
            GeometryElement geometryElement = elm.get_Geometry(options);

            //Iteramos por GeometryObject cada en GeometryElement
            foreach (GeometryObject geometryObject in geometryElement)
            {
                //Chequemos si es Solid y almacenamos en tempWall
                if (geometryObject is Solid tempWall)
                {
                    //Comprobamos si es Solid y su volumen > 0
                    if (tempWall != null && tempWall.Volume > 0)
                    {
                        solid = tempWall;
                        break;
                    }

                }
            }
            #region BoundingBoxXYZ
            // Obtenemos un BoundingBoxXYZ desde el Solid
            BoundingBoxXYZ boundingBoxXYZ = solid.GetBoundingBox();

            //Obtenemos la Transform del BoundingBoxXYZ
            Transform transform = boundingBoxXYZ.Transform;

            //Obtenemos el max del BoundingBoxXYZ y le aplicamos la Transform
            XYZ max = boundingBoxXYZ.Max;
            max = transform.OfPoint(max); //Maximo XYZ
            #endregion

            #region Face
            //Creamos XYZ para almacenar maximo
            XYZ maxEdgeXYZ = null;

            //Obtenemos del solid un FaceArray y un FaceArrayIterator
            FaceArray faceArray = solid.Faces;
            FaceArrayIterator faceArrayIterator = faceArray.ForwardIterator();

            //Iteramos por FaceArrayIterator mientras podamos avanzar
            while (faceArrayIterator.MoveNext())
            {
                //Obtenemos la Face actual
                Face face = faceArrayIterator.Current as Face;
                //es la Face PlanarFace? Es la Z de su Normal >0?
                if (face is PlanarFace planarFace && planarFace.FaceNormal.Z > app.ShortCurveTolerance)
                {
                    //Obtenemos EdgeArrayArray
                    EdgeArrayArray edgeArrayArray = planarFace.EdgeLoops;
                    //Obtenemos el primer bucle de EdgeArrayArray
                    EdgeArray edgeArray = edgeArrayArray.get_Item(0);
                    //Obtenemos el EdgeArrayIterator
                    EdgeArrayIterator edgeArrayIterator = edgeArray.ForwardIterator();

                    //Iteramos por EdgeArrayIterator mientras sea posible
                    while (edgeArrayIterator.MoveNext())
                    {
                        //Obtenemos el Edge actual
                        Edge edge = edgeArrayIterator.Current as Edge;
                        //Obtenemos su Line
                        Line line = edge.AsCurve() as Line;
                        //Obtenemos la lista de puntos ordenanos por Z
                        List<XYZ> xYZs = line.Tessellate().OrderBy(x => x.Z).ToList();
                        //Es maxEdgeXYZ null? La Z es mayor?
                        if (maxEdgeXYZ == null || xYZs.Last().Z > maxEdgeXYZ.Z)
                        {
                            //Actualizamos maxEdgeXYZ
                            maxEdgeXYZ = xYZs.Last();
                        }
                    }
                    //Ha sido la Face PlanarFace y Es la Z de su Normal >0.
                    //Interumpimos el bucle
                    break;
                }
            }
            #endregion
            TaskDialog.Show("Revit API Manual", "La 'Z' máxima, desde el BoundingBoxXYZ es: " + max.Z.ToString("N2") +
                "\rLa 'Z' máxima, desde el Edge es: " + maxEdgeXYZ.Z.ToString("N2"));

            return Result.Succeeded;
        }
    }
}
