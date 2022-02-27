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

namespace CrearPlataformaNivelacion
{
    [Transaction(TransactionMode.Manual)]
    public class CrearPlataformaNivelacion : IExternalCommand
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

            //Filtramos los niveles, metodo abreviado. Filtro de clase
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Level));
            //Seleccionamos el primer nivel de la colección
            Level level = col.First() as Level;

            //Creamos 4 puntos en planta. Cuadricula 20*20
            XYZ xYZ0 = new XYZ(0, 0, -1);
            XYZ xYZ1 = new XYZ(20, 0, -1);
            XYZ xYZ2 = new XYZ(20, 20, -1);
            XYZ xYZ3 = new XYZ(0, 20, -1);

            //Creamos 4 puntos en planta. Cuadricula 10*10
            XYZ xYZ5 = new XYZ(5, 5, -1);
            XYZ xYZ6 = new XYZ(15, 5, -1);
            XYZ xYZ7 = new XYZ(15, 15, -1);
            XYZ xYZ8 = new XYZ(5, 15, -1);

            //Creamos primer conjunto de Curves
            Curve c0 = Line.CreateBound(xYZ0, xYZ1);
            Curve c1 = Line.CreateBound(xYZ1, xYZ2);
            Curve c2 = Line.CreateBound(xYZ2, xYZ3);
            Curve c3 = Line.CreateBound(xYZ3, xYZ0);

            //Creamos segundo conjunto de Curves
            Curve c4 = Line.CreateBound(xYZ5, xYZ6);
            Curve c5 = Line.CreateBound(xYZ6, xYZ7);
            Curve c6 = Line.CreateBound(xYZ7, xYZ8);
            Curve c7 = Line.CreateBound(xYZ8, xYZ5);

            IList<CurveLoop> curveLoops = new List<CurveLoop>();

            CurveLoop curvesA = new CurveLoop();
            curvesA.Append(c0);
            curvesA.Append(c1);
            curvesA.Append(c2);
            curvesA.Append(c3);

            CurveLoop curvesB = new CurveLoop();

            curvesB.Append(c4);
            curvesB.Append(c5);
            curvesB.Append(c6);
            curvesB.Append(c7);
            //Añadimos las dos CurveLoop a  IList<CurveLoop>. La segunda es interior a la primera
            curveLoops.Add(curvesA);
            curveLoops.Add(curvesB);
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transactión
                tx.Start("Transaction Name");

                ElementId buiddingId = doc.GetDefaultElementTypeId(ElementTypeGroup.BuildingPadType);
                //Creamos la plataforma. Necesitamos tener creada una Topografia capaz de hospedar la plataforma
                Autodesk.Revit.DB.Architecture.BuildingPad buildingPad = Autodesk.Revit.DB.Architecture.
                    BuildingPad.Create(doc, buiddingId, level.Id, curveLoops);
                ElementTransformUtils.MoveElement(doc, buildingPad.Id, new XYZ(0, 0, -1));
                //Obtenemos la Topografía generada
                Autodesk.Revit.DB.Architecture.TopographySurface topo = doc.GetElement(buildingPad.AssociatedTopographySurfaceId)
                    as Autodesk.Revit.DB.Architecture.TopographySurface;
                //Confirmamos Transaction
                tx.Commit();

                TaskDialog.Show("Manual Revit API", "Plataforma de nivelación creada");
            }

            return Result.Succeeded;
        }
    }
}
