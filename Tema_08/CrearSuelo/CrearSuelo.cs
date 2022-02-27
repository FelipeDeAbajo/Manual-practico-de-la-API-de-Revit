#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace CreaSuelo
{
    [Transaction(TransactionMode.Manual)]
    public class CrearSuelo : IExternalCommand
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
            XYZ desfase = new XYZ(10, 10, 0);

            //Creamos 4 puntos en planta. Cuadricula 10*10
            //el xYZ5 es igual al xYZ3
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

#if V2022
            
            //Creamos primer CurveLoop para V2022
            CurveLoop profileSuelo = new CurveLoop();
            profileSuelo.Append(c0);
            profileSuelo.Append(c1);
            profileSuelo.Append(c2);
            profileSuelo.Append(c3);

            //Creamos segundo CurveLoop para V2022
            CurveLoop profileLosa = new CurveLoop();
            profileLosa.Append(c4);
            profileLosa.Append(c5);
            profileLosa.Append(c6);
            profileLosa.Append(c7);
#else
            //Creamos primer CurveArray para V2021
            CurveArray curveArraySuelo = new CurveArray();
            curveArraySuelo.Append(c0);
            curveArraySuelo.Append(c1);
            curveArraySuelo.Append(c2);
            curveArraySuelo.Append(c3);
            
            //Creamos segundo CurveArray para V2021
            CurveArray curveArrayLosa = new CurveArray();
            curveArrayLosa.Append(c4);
            curveArrayLosa.Append(c5);
            curveArrayLosa.Append(c6);
            curveArrayLosa.Append(c7);
#endif
            //Obtenemos tipo de suelo por defecto
            FloorType floorType = doc.GetElement(doc.GetDefaultElementTypeId(ElementTypeGroup.FloorType)) as FloorType;
            //Obtenemos tipo losa de cimentación por defecto
            FloorType floorTypeSlab = doc.GetElement(doc.GetDefaultElementTypeId(ElementTypeGroup.FootingSlabType)) as FloorType;

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Creación Suelos");

#if V2022
                //Creamos suelo arquitectónico
                Floor floor = Floor.Create(doc, new List<CurveLoop> { profileSuelo }, floorType.Id, level.Id);
                //Creamos suelo con pendiente. Le creamos estructural
                Floor losa = Floor.Create(doc, new List<CurveLoop> { profileLosa }, floorType.Id, level.Id, true, c0 as Line, 1);
                //Creamos losa (depende del tipo elegudo), sin pendiente
                Floor losaCimen = Floor.Create(doc, new List<CurveLoop> { profileLosa }, floorTypeSlab.Id, level.Id, true, null, 1);

#else
                Floor floor = doc.Create.NewFloor(curveArraySuelo, floorType, level, true);
                Floor losa = doc.Create.NewSlab(curveArrayLosa, level, c0 as Line, 1, true);
                Floor losaCimen = doc.Create.NewFoundationSlab(curveArrayLosa, floorTypeSlab, level, true, XYZ.BasisZ);

#endif
                //Confirmamos transaction
                tx.Commit();

                TaskDialog.Show("Manual Revit API", "Suelos creados");
            }

            return Result.Succeeded;
        }
    }
}
