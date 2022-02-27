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

namespace CrearRegion
{
    [Transaction(TransactionMode.Manual)]
    public class CrearRegion : IExternalCommand
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

            //Creamos 4 puntos en planta. Cuadricula 10*10
            XYZ xYZ0 = XYZ.Zero;
            XYZ xYZ1 = new XYZ(10, 0, 0);
            XYZ xYZ2 = new XYZ(10, 10, 0);
            XYZ xYZ3 = new XYZ(0, 10, 0);

            //Creamos un vector de desplazamiento. a 45 º
            XYZ desfase = new XYZ(15, 15, 0);

            //Creamos 4 puntos en planta. Cuadricula 10*10
            XYZ xYZ5 = XYZ.Zero + desfase;
            XYZ xYZ6 = xYZ1 + desfase;
            XYZ xYZ7 = xYZ2 + desfase;
            XYZ xYZ8 = xYZ3 + desfase;

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

            //Creamos primer CurveLoop 
            CurveLoop profileA = new CurveLoop();
            profileA.Append(c0);
            profileA.Append(c1);
            profileA.Append(c2);
            profileA.Append(c3);

            //Creamos segundo CurveLoop 
            CurveLoop profileB = new CurveLoop();
            profileB.Append(c4);
            profileB.Append(c5);
            profileB.Append(c6);
            profileB.Append(c7);

            //Creamos una lista de CurveLoop
            List<CurveLoop> curveLoops = new List<CurveLoop>() { profileA, profileB };
            //Obtenemos el ElementType por defecto
            //No es posible crear region de mascara
            ElementId id = doc.GetDefaultElementTypeId(ElementTypeGroup.FilledRegionType);
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction región");
                //Creamos la región
                FilledRegion filledRegion = FilledRegion.Create(doc, id, uidoc.ActiveView.Id, curveLoops);
                //Obtenemos las DetailCurve del contorno
                IList<ElementId> idsCurves = filledRegion.GetDependentElements(new ElementClassFilter(typeof(CurveElement)));
                List<Element> curves = idsCurves.Select(x => doc.GetElement(x)).ToList();

                //Podriamos cambiar el Estilo de linea de eastas Lineas de detalle

                TaskDialog.Show("Manual Revit API", "Region creada con " + curves.Count + " Lineas de detalle");
                //Confirmamos transaction
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
