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

namespace CrearSubelementosEnSuelo
{
    [Transaction(TransactionMode.Manual)]
    public class CrearSubelementosEnSuelo : IExternalCommand
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

            //Creamos primer conjunto de Curves
            Curve c0 = Line.CreateBound(xYZ0, xYZ1);
            Curve c1 = Line.CreateBound(xYZ1, xYZ2);
            Curve c2 = Line.CreateBound(xYZ2, xYZ3);
            Curve c3 = Line.CreateBound(xYZ3, xYZ0);

#if V2022
            
            //Creamos primer CurveLoop para V2022
            CurveLoop profileSuelo = new CurveLoop();
            profileSuelo.Append(c0);
            profileSuelo.Append(c1);
            profileSuelo.Append(c2);
            profileSuelo.Append(c3);

#else
            //Creamos primer CurveArray para V2021
            CurveArray curveArraySuelo = new CurveArray();
            curveArraySuelo.Append(c0);
            curveArraySuelo.Append(c1);
            curveArraySuelo.Append(c2);
            curveArraySuelo.Append(c3);

#endif
            //Obtenemos tipo de suelo por defecto
            FloorType floorType = doc.GetElement(doc.GetDefaultElementTypeId(ElementTypeGroup.FloorType)) as FloorType;

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Creación Suelos");

                //Creamos suelo arquitectónico
                Floor floor = Floor.Create(doc, new List<CurveLoop> { profileSuelo }, floorType.Id, level.Id);

                ////Opción 1.Regeneramos para forzar cálculo geometría. 
             //   doc.Regenerate();

                ////Opción 2. Desplazomo vector nulo para forzar cálculo geometría. 
                 ElementTransformUtils.MoveElement(doc, floor.Id, XYZ.Zero);

                ////Obtenemos el SlabShapeEditor del suelo arquitectónico
                SlabShapeEditor slabShapeEditor = floor.SlabShapeEditor;

                ////Creamos dos vertices nuevos en slabShapeEditor
                SlabShapeVertex slabShapeVertex0 = slabShapeEditor.DrawPoint(new XYZ(5, 0, 0.3));
                SlabShapeVertex slabShapeVertex1 = slabShapeEditor.DrawPoint(new XYZ(5, 10, 0.3));

                //Creamos linea divisoria en slabShapeEditor
                slabShapeEditor.DrawSplitLine(slabShapeVertex0, slabShapeVertex1);

                //Confirmamos transaction
                tx.Commit();

                TaskDialog.Show("Manual Revit API", "Suelos creados");
            }

            return Result.Succeeded;
        }
    }
}
