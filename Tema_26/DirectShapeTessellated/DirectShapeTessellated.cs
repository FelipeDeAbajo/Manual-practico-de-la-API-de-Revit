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

namespace DirectShapeTessellated
{
    [Transaction(TransactionMode.Manual)]
    public class DirectShapeTessellated : IExternalCommand
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

            //Filtro de clase material. Obtenemos el primero
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Material));
            IEnumerable<Material> materialsEnum = col.ToElements().Cast<Material>();
            ElementId materialId = materialsEnum.First().Id;

            //Nuevo TessellatedShapeBuilder
            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            //Definimos sólido
            builder.OpenConnectedFaceSet(true); //Indica si es solido vacio

            //Piramide base cuadrada. Lado 4 y altura 5
            double length = 4.0;
            double height = 5.0;

            //Base
            XYZ basePt1 = XYZ.Zero;
            XYZ basePt2 = new XYZ(length, 0, 0);
            XYZ basePt3 = new XYZ(length, length, 0);
            XYZ basePt4 = new XYZ(0, length, 0);

            //Vertice superior
            XYZ vertice = new XYZ(length / 2, length / 2, height);

            //Lista de XYZ 4 vertices
            List<XYZ> loopVertices = new List<XYZ>();
            loopVertices.Add(basePt1);
            loopVertices.Add(basePt2);
            loopVertices.Add(basePt3);
            loopVertices.Add(basePt4);

            //Construimos Face de base y añadimos al TessellatedShapeBuilder
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            //Limpiamos loopVertices
            loopVertices.Clear();
            //Lista de XYZ 3 vertices
            loopVertices.Add(basePt1);
            loopVertices.Add(vertice);
            loopVertices.Add(basePt2);

            //Construimos Face de lado 1 y añadimos al TessellatedShapeBuilder
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            //Limpiamos loopVertices
            loopVertices.Clear();
            //Lista de XYZ 3 vertices
            loopVertices.Add(basePt2);
            loopVertices.Add(vertice);
            loopVertices.Add(basePt3);

            //Construimos Face de lado 2 y añadimos al TessellatedShapeBuilder
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            //Limpiamos loopVertices
            loopVertices.Clear();
            //Lista de XYZ 3 vertices
            loopVertices.Add(basePt3);
            loopVertices.Add(vertice);
            loopVertices.Add(basePt4);

            //Construimos Face de lado 3 y añadimos al TessellatedShapeBuilder
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            //Limpiamos loopVertices
            loopVertices.Clear();
            //Lista de XYZ 3 vertices
            loopVertices.Add(basePt4);
            loopVertices.Add(vertice);
            loopVertices.Add(basePt1);

            //Construimos Face de lado 4 y añadimos al TessellatedShapeBuilder
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            //Cerramos y unimos Faces
            builder.CloseConnectedFaceSet();

            //Geometria Mesh
            builder.Target = TessellatedShapeBuilderTarget.Solid;
            builder.Fallback = TessellatedShapeBuilderFallback.Abort;

            //Construimos TessellatedShapeBuilder
            builder.Build();

            TessellatedShapeBuilderResult result = builder.GetBuildResult();

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name Tessellated");

                //Creamos una DirectShape en el doc categoría modelos genericos
                Autodesk.Revit.DB.DirectShape ds = Autodesk.Revit.DB.DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                //Completamos datos
                ds.ApplicationId = "Revit API Manual";
                ds.ApplicationDataId = "Revit API Manual. Creación Tessellated";
                //Asignamos geometría
                ds.SetShape(result.GetGeometricalObjects());

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
