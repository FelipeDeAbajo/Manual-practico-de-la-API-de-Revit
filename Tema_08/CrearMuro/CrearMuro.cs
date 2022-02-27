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

namespace CrearMuro
{
    [Transaction(TransactionMode.Manual)]
    public class CrearMuro : IExternalCommand
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
            XYZ xYZ1 = XYZ.Zero;
            XYZ xYZ2 = new XYZ(10, 0, 0);
            XYZ xYZ3 = new XYZ(10, 10, 0);
            XYZ xYZ4 = new XYZ(0, 10, 0);

            //Creamos un vector vertical. Altura muro
            XYZ vectorVertical = new XYZ(0, 0, 10);

            //Curva para crear muro por linea
            Curve c1 = Line.CreateBound(xYZ1, xYZ2);

            //Construimos 4 curvas para perímero del muro.
            //Podemos crear cualquier perimetro
            Curve c2Planta = Line.CreateBound(xYZ2, xYZ3);
            Curve c2V1 = Line.CreateBound(xYZ2 + vectorVertical, xYZ2);
            Curve c2V2 = Line.CreateBound(xYZ3, xYZ3 + vectorVertical);
            Curve c2Techo = Line.CreateBound(xYZ2 + vectorVertical, xYZ3 + vectorVertical);

            //Construimos lista de curvas para perímetro
            List<Curve> perimetroVertical = new List<Curve>() { c2Planta, c2V1, c2Techo, c2V2 };

            //Creamos transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos transaction
                tx.Start("Transaction Name");
                //Creamos muro por curve
              Wall wallPorLinea = Wall.Create(doc, c1, level.Id, true);
                //Creamos muro por perímetro
               Wall wallPorPerimetro = Wall.Create(doc, perimetroVertical, true);
                //Confirmamos transaction
                tx.Commit();
            }
            TaskDialog.Show("Manual Revit API", "Muros creados.");

            return Result.Succeeded;
        }
    }
}
