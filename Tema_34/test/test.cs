#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace test
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class TestRebar : IExternalCommand
    {
        // https://thebuildingcoder.typepad.com/blog/2018/10/resources-rotated-rebar-stirrups-and-steel-stuff.html#5
        //https://thebuildingcoder.typepad.com/blog/2021/03/boundary-elements-and-stirrup-constraints.html


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
            ElementId elementId = sel.GetElementIds().FirstOrDefault();


            FamilyInstance familyInstance = doc.GetElement(elementId) as FamilyInstance;
            if (familyInstance == null)
            {
                message = "Debe seleccionar un Pilar estructural";
                return Result.Failed;
            }
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(RebarShape));
            RebarShape shape = col.Where(x => x.Name == "M_T1").FirstOrDefault() as RebarShape;

            if (shape == null)
            {
                message = "RebarShape no encontrado";
                return Result.Failed;
            }
            col = new FilteredElementCollector(doc).OfClass(typeof(RebarBarType));

            RebarBarType rebarType = col.FirstOrDefault() as RebarBarType;

            if (rebarType == null)
            {
                message = "RebarBarType no encontrado";
                return Result.Failed;
            }

            GeometryElement geometryElement = familyInstance.get_Geometry(new Options());

            // this will get the edges in family coordinates 
            // not the global coordinates

            IList<Curve> edges = getFaceEdges(geometryElement);

            Transform trf = familyInstance.GetTransform();

            // SOLUTION 1

            {
                XYZ origin, xAxisDir, yAxisDir, xAxisBox, yAxisBox;
                getOriginXandYvecFromFaceEdges(edges, out origin, out xAxisDir, out yAxisDir, out xAxisBox, out yAxisBox);

                // we obtained origin, xAxis, yAxis in family 
                // coordinates. Now we will transform them in 
                // global coordinates

                origin = trf.OfPoint(origin);
                xAxisDir = trf.OfVector(xAxisDir);
                yAxisDir = trf.OfVector(yAxisDir);

                xAxisBox = trf.OfVector(xAxisBox);
                yAxisBox = trf.OfVector(yAxisBox);

                using (Transaction tr = new Transaction(doc))
                {
                    tr.Start("Transaction Rebar");
                    Rebar createdStirrupRebar = Rebar.CreateFromRebarShape(doc, shape, rebarType, familyInstance, origin, xAxisDir, yAxisDir);
                   
                    RebarShapeDrivenAccessor rebarStirrupShapeDrivenAccessor = createdStirrupRebar.GetShapeDrivenAccessor();
                    rebarStirrupShapeDrivenAccessor.SetLayoutAsFixedNumber(5, 10, true, true, true);
                    rebarStirrupShapeDrivenAccessor.ScaleToBox(origin, xAxisBox, yAxisBox);
                   
                    tr.Commit();
                }
            }


            return Result.Succeeded;
        }

        private void getOriginXandYvecFromFaceEdges(IList<Curve> edges, out XYZ origin, out XYZ xAxisDir, out XYZ yAxisDir, out XYZ xAxisBox, out XYZ yAxisBox)
        {
            origin = new XYZ();
            xAxisDir = new XYZ();
            yAxisDir = new XYZ();
            xAxisBox = new XYZ();
            yAxisBox = new XYZ();

            double minZ = double.MaxValue;

            for (int ii = 0; ii < edges.Count; ii++)
            {
                Line edgeLine = edges[ii] as Line;
                if (edgeLine == null)
                    continue;

                int nextii = (ii + 1) % edges.Count;
                Line edgeLineNext = edges[nextii] as Line;
                if (edgeLineNext == null) continue;

                XYZ pntEnd = edgeLine.Evaluate(1, true);
                if (pntEnd.Z < minZ)
                {
                    minZ = pntEnd.Z;
                    origin = pntEnd;
                    // These two will be used by Rebar.CreateFromRebarShape.
                    // For this the length is irrelevant:
                    xAxisDir = edgeLine.Direction * -1;
                    yAxisDir = edgeLineNext.Direction;
                    // These two  will be used at Rebar.ScaleToBox.
                    // For this, the length is important, because it
                    // will represent the length of box segment.
                    xAxisBox = xAxisDir * edgeLine.ApproximateLength;
                    yAxisBox = yAxisDir * edgeLineNext.ApproximateLength;

                    // Here you can also remove from the length 
                    // the value of the cover.
                }
            }
        }

        private IList<Curve> getFaceEdges(GeometryElement geometryElement)
        {
            foreach (GeometryObject geometryObject in geometryElement)
            {
                Solid solid = geometryObject as Solid;
                if (solid != null)
                {
                    FaceArray faces = solid.Faces;
                    foreach (Face face in faces)
                    {
                        Plane plane = face.GetSurface() as Plane;
                        if (AreEqual(plane.Normal.DotProduct(XYZ.BasisZ.Negate()), -1)) // vecs are parallel 
                        {
                            // There should be onlu one curve loop.
                            // It can be multiple if the face have a hole.
                            if (face.GetEdgesAsCurveLoops().Count != 1)
                                return null;

                            IList<Curve> edgesArr = new List<Curve>();
                            CurveLoopIterator cli = face
                              .GetEdgesAsCurveLoops()[0]
                              .GetCurveLoopIterator();

                            while (cli.MoveNext())
                            {
                                Curve edge = cli.Current;
                                edgesArr.Add(edge);
                            }
                            return edgesArr;
                        }
                    }
                }
                else
                {
                    GeometryInstance geometryInstance = geometryObject as GeometryInstance;

                    if (geometryInstance != null)
                    {
                        return getFaceEdges(geometryInstance.GetSymbolGeometry());
                    }
                }
            }
            return null;
        }




        public static bool AreEqual(
          double firstValue, double secondValue,
          double tolerance = 1.0e-9)
        {
            return (secondValue - tolerance < firstValue
              && firstValue < secondValue + tolerance);
        }

    }
}