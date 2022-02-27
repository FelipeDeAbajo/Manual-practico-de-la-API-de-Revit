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

namespace CrearCubierta
{
    [Transaction(TransactionMode.Manual)]
    public class CrearCubierta : IExternalCommand
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

            // Selección actual. Debemos chequear que la selección es correcta
            Selection sel = uidoc.Selection;

            //Construimos un collector para niveles
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Level));
            Level level = col.LastOrDefault() as Level;
            //Construimos un colector para tipos de cubierta
            FilteredElementCollector colRoof = new FilteredElementCollector(doc).OfClass(typeof(RoofType));

            //Construimos un CurveArray, para almacenar el contorno
            CurveArray curveArray = new CurveArray();

            //Obtenemos los elemento seleccionados (Wall o ModelLine)
            ICollection<ElementId> elementIds = sel.GetElementIds();
            foreach (ElementId elementId in elementIds)
            {
                //Obtenemos el Element
                Element element = doc.GetElement(elementId);

                if (element is Wall wall)
                {
                    //Si es Wall LocationCurve 
                    LocationCurve locationCurve = wall.Location as LocationCurve;
                    curveArray.Append(locationCurve.Curve);
                }
                else if (element is ModelLine modelLine)
                {
                    //Si es ModelLine GeometryCurve
                    curveArray.Append(modelLine.GeometryCurve);
                }
            }

            // Obtenemos el tipo por defecto para cubierta
            //No podemos garantizar si es cristalera o no
            RoofType roofType = doc.GetElement(doc.GetDefaultElementTypeId(ElementTypeGroup.RoofType)) as RoofType;

            // Obtenemos el primer tipo para cristalera inclinada
            RoofType roofTypeCristalera = colRoof.ToElements().Where(x => x.get_Parameter(BuiltInParameter.CURTAINGRID_ADJUST_BORDER_1) != null).First() as RoofType;

            //Obtenemos el tipo para Cubierta básica
            roofType = colRoof.ToElements().Where(x => x.get_Parameter(BuiltInParameter.CURTAINGRID_ADJUST_BORDER_1) == null).First() as RoofType;
           
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transactión
                tx.Start("Crear cubiertas");
                #region FootPrintRoof
                //Mueva ModelCurveArray para la salida de la cubierta
                ModelCurveArray modelCurveArray = new ModelCurveArray();
                //Creamos cubierta y salida
                FootPrintRoof footPrintRoof = doc.Create.NewFootPrintRoof(curveArray, level, roofType /*roofTypeCristalera*/, out modelCurveArray);

                ModelCurveArrayIterator modelCurveArrayIterator = modelCurveArray.ForwardIterator();
                modelCurveArrayIterator.Reset();

                while (modelCurveArrayIterator.MoveNext())
                {
                    //Obtenemos la ModelCurve
                    ModelCurve modelCurve = modelCurveArrayIterator.Current as ModelCurve;
                    //Cambiamos pendiente. Intentamos cambiar alero y offset
                    footPrintRoof.set_DefinesSlope(modelCurve, true);
                    footPrintRoof.set_SlopeAngle(modelCurve, Math.PI / 12);
                    footPrintRoof.set_Offset(modelCurve, 10);
                    //  footPrintRoof.set_Overhang(modelCurve, 10);
                }
                #endregion
                #region  ExtrusionRoof
                //Creamos un plano de refrencia.
                //Cada vez que rodemos se creara. Deberiamos buscar antes si existe uno adecuado.
                ReferencePlane referencePlane = doc.Create.NewReferencePlane2(XYZ.Zero, XYZ.BasisX, XYZ.BasisZ, uidoc.ActiveView);
                //Creamos CurveArray para el perfil de extrusión
                CurveArray curveArrayEx = new CurveArray();
                //Incluimos dos Line
                Line line1 = Line.CreateBound(new XYZ(0, 0, 10), new XYZ(10, 0, 20));
                Line line2 = Line.CreateBound(new XYZ(10, 0, 20), new XYZ(30, 0, 10));
                curveArrayEx.Append(line1);
                curveArrayEx.Append(line2);
                //Creamos ExtrusionRoof
                ExtrusionRoof extrusionRoof = doc.Create.NewExtrusionRoof(curveArrayEx, referencePlane, level, roofType, 0, 50);
                #endregion
                tx.Commit();

                TaskDialog.Show("Manual Revit API", "Cubiertas creadas");
            }
            return Result.Succeeded;
        }
    }
}
