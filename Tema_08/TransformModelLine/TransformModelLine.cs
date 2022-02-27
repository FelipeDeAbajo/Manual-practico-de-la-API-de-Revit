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

namespace TransformModelLine
{
    [Transaction(TransactionMode.Manual)]
    public class TransformModelLine : IExternalCommand
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

       //Creamos cuatro puntos cuadricula 10*10
            XYZ xYZ1 = XYZ.Zero;
            XYZ xYZ2 = new XYZ(10, 0, 0);
            XYZ xYZ3 = new XYZ(10, 10, 0);
            XYZ xYZ4 = new XYZ(0, 10, 0);

            //Creamos dos Transform
            Transform transform1 = Transform.CreateTranslation(new XYZ(20, 20, 0));
            Transform transform2 = Transform.CreateRotationAtPoint(XYZ.BasisZ, Math.PI / 4, XYZ.Zero);

            //Creamos cuatro Lines
            Line line1 = Line.CreateBound(xYZ1, xYZ2);
            Line line2 = Line.CreateBound(xYZ2, xYZ3);
            Line line3 = Line.CreateBound(xYZ3, xYZ4);
            Line line4 = Line.CreateBound(xYZ4, xYZ1);

            //Creamos un nuevo conjunto de Lines acumulando las Transform
            line1 = line1.CreateTransformed(transform1.Multiply(transform2)) as Line;
            line2 = line2.CreateTransformed(transform1.Multiply(transform2)) as Line;
            line3 = line3.CreateTransformed(transform1.Multiply(transform2)) as Line;
            line4 = line4.CreateTransformed(transform1.Multiply(transform2)) as Line;

            //Creamos un nuevo plano. Coincidente con el horizontal del Nivel 1
         //   Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Inicioamos Transaction
                tx.Start("Transaction transform");

                //Creamos SketchPlane
                //   SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                //Construccion alternativa desde PlanView
                //Necesario una ViewPlan
                ViewPlan viewPlan = doc.ActiveView as ViewPlan;
                SketchPlane sketchPlane = viewPlan.SketchPlane;

                //Creamos las 4 ModelLine
                ModelLine modelLine1 = doc.Create.NewModelCurve(line1, sketchPlane) as ModelLine;
                ModelLine modelLine2 = doc.Create.NewModelCurve(line2, sketchPlane) as ModelLine;
                ModelLine modelLine3 = doc.Create.NewModelCurve(line3, sketchPlane) as ModelLine;
                ModelLine modelLine4 = doc.Create.NewModelCurve(line4, sketchPlane) as ModelLine;

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
