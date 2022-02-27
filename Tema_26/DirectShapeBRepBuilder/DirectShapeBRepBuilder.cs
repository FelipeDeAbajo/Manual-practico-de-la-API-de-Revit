#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace DirectShapeBRepBuilder
{
    [Transaction(TransactionMode.Manual)]
    public class DirectShapeBRepBuilder : IExternalCommand
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

            //Asumimos ejes en direccion estandard

            //Construimos BRepBuilder Solido
            BRepBuilder brepBuilder = new BRepBuilder(BRepType.Solid);

            //Definimos Frame y comprobamos 
            Frame frame = new Frame(new XYZ(50, -100, 0), new XYZ(0, 1, 0), new XYZ(-1, 0, 0), new XYZ(0, 0, 1));
            if (Frame.CanDefineRevitGeometry(frame) == false)
            {
                message = "Imposible crear DirectShape";
                return Result.Failed;
            }

            //Creamos geometria 4 semicirculos y dos generatrices de cilindro
            BRepBuilderEdgeGeometry frontEdgeBottom = BRepBuilderEdgeGeometry.Create(Arc.Create(new XYZ(0, -100, 0), new XYZ(100, -100, 0), new XYZ(50, -50, 0)));
            BRepBuilderEdgeGeometry backEdgeBottom = BRepBuilderEdgeGeometry.Create(Arc.Create(new XYZ(100, -100, 0), new XYZ(0, -100, 0), new XYZ(50, -150, 0)));

            BRepBuilderEdgeGeometry frontEdgeTop = BRepBuilderEdgeGeometry.Create(Arc.Create(new XYZ(0, -100, 100), new XYZ(100, -100, 100), new XYZ(50, -50, 100)));
            BRepBuilderEdgeGeometry backEdgeTop = BRepBuilderEdgeGeometry.Create(Arc.Create(new XYZ(0, -100, 100), new XYZ(100, -100, 100), new XYZ(50, -150, 100)));

            BRepBuilderEdgeGeometry linearEdgeFront = BRepBuilderEdgeGeometry.Create(new XYZ(100, -100, 0), new XYZ(100, -100, 100));
            BRepBuilderEdgeGeometry linearEdgeBack = BRepBuilderEdgeGeometry.Create(new XYZ(0, -100, 0), new XYZ(0, -100, 100));

            //Añadimos los 6 Edges
            BRepBuilderGeometryId frontEdgeBottomId = brepBuilder.AddEdge(frontEdgeBottom);
            BRepBuilderGeometryId frontEdgeTopId = brepBuilder.AddEdge(frontEdgeTop);
            BRepBuilderGeometryId linearEdgeFrontId = brepBuilder.AddEdge(linearEdgeFront);
            BRepBuilderGeometryId linearEdgeBackId = brepBuilder.AddEdge(linearEdgeBack);
            BRepBuilderGeometryId backEdgeBottomId = brepBuilder.AddEdge(backEdgeBottom);
            BRepBuilderGeometryId backEdgeTopId = brepBuilder.AddEdge(backEdgeTop);

            // Las superficies de las cuatro Faces. 
            //Cilindrica
            CylindricalSurface cylSurf = CylindricalSurface.Create(frame, 50);
            //Planas superior e inferior
            Plane top = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), new XYZ(0, 0, 100));  //Normal hacia afuera
            Plane bottom = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), new XYZ(0, 0, 0)); //Normal hacia adentro

            //Añadimos las 4 Face
            BRepBuilderGeometryId frontCylFaceId = brepBuilder.AddFace(BRepBuilderSurfaceGeometry.Create(cylSurf, null), false);
            BRepBuilderGeometryId backCylFaceId = brepBuilder.AddFace(BRepBuilderSurfaceGeometry.Create(cylSurf, null), false);
            BRepBuilderGeometryId topFaceId = brepBuilder.AddFace(BRepBuilderSurfaceGeometry.Create(top, null), false);
            BRepBuilderGeometryId bottomFaceId = brepBuilder.AddFace(BRepBuilderSurfaceGeometry.Create(bottom, null), true);

            //Añadimos Loops de las 4 Face
            BRepBuilderGeometryId loopId_Top = brepBuilder.AddLoop(topFaceId);
            BRepBuilderGeometryId loopId_Bottom = brepBuilder.AddLoop(bottomFaceId);
            BRepBuilderGeometryId loopId_Front = brepBuilder.AddLoop(frontCylFaceId);
            BRepBuilderGeometryId loopId_Back = brepBuilder.AddLoop(backCylFaceId);

            //Añadimos coEdge para el Loop de la Face frontal
            brepBuilder.AddCoEdge(loopId_Front, linearEdgeBackId, false);
            brepBuilder.AddCoEdge(loopId_Front, frontEdgeTopId, false);
            brepBuilder.AddCoEdge(loopId_Front, linearEdgeFrontId, true);
            brepBuilder.AddCoEdge(loopId_Front, frontEdgeBottomId, true);
            brepBuilder.FinishLoop(loopId_Front);
            brepBuilder.FinishFace(frontCylFaceId);

            //Añadimos coEdge para el Loop de la Face trasera
            brepBuilder.AddCoEdge(loopId_Back, linearEdgeBackId, true);
            brepBuilder.AddCoEdge(loopId_Back, backEdgeBottomId, true);
            brepBuilder.AddCoEdge(loopId_Back, linearEdgeFrontId, false);
            brepBuilder.AddCoEdge(loopId_Back, backEdgeTopId, true);
            brepBuilder.FinishLoop(loopId_Back);
            brepBuilder.FinishFace(backCylFaceId);

            //Añadimos coEdge para el Loop de la Face superior
            brepBuilder.AddCoEdge(loopId_Top, backEdgeTopId, false);
            brepBuilder.AddCoEdge(loopId_Top, frontEdgeTopId, true);
            brepBuilder.FinishLoop(loopId_Top);
            brepBuilder.FinishFace(topFaceId);

            //Añadimos coEdge para el Loop de la Face inferior
            brepBuilder.AddCoEdge(loopId_Bottom, frontEdgeBottomId, false);
            brepBuilder.AddCoEdge(loopId_Bottom, backEdgeBottomId, false);
            brepBuilder.FinishLoop(loopId_Bottom);
            brepBuilder.FinishFace(bottomFaceId);

            //Finalizamos construcción
            brepBuilder.Finish();

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name BRepBuilder");

                //Creamos una DirectShape en el doc categoría muros
                Autodesk.Revit.DB.DirectShape ds = Autodesk.Revit.DB.DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Walls));

                //Completamos datos
                ds.ApplicationId = "Revit API Manual";
                ds.ApplicationDataId = "Revit API Manual. Creación Tessellated";
                //Asignamos geometría
                ds.SetShape(brepBuilder);

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
