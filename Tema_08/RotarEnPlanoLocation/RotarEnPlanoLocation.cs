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

namespace RotarEnPlanoLocation
{
    [Transaction(TransactionMode.Manual)]
    public class RotarEnPlanoLocation : IExternalCommand
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

            //Creamos una Reference para poder seleccionar la viga
            Reference reference = null;
            //Creamos ISelectionFilter solo admitimos BuiltInCategory.OST_StructuralFraming
            ISelectionFilter beamSeleccionFilter = new BeamSeleccionFilter();

            try
            {
                //Siempre PickObject en estructura try{ç carch{}
                reference = uidoc.Selection.PickObject(ObjectType.Element, beamSeleccionFilter, "Seleccionar viga");
            }
            catch (Exception ex)
            {
                // Si falla la selección de viga
                message = ex.Message;
                return Result.Failed;

            }
            //Obtenemos el Elemen, dado que PickObject no ha fallado
            Element element = doc.GetElement(reference.ElementId);

            //Obtenemos Location. como es viga lo convertimos a LocationCurve
            LocationCurve locationCurve = element.Location as LocationCurve;
            //Obtenemos curve
            Curve curve = locationCurve.Curve;
            //Comprobamos que es segmento rectilineo
            Line line = null;
            if (curve is Line)
            {
                line = curve as Line;
            }
            else
            {
                message = "Solo vigas rectilineos";
                return Result.Failed;
            }
            //Obtenemos los puntos inicial y final.
            //Considere utilizar Tessellate() en lugar de GetEndPoint()
            //IList<XYZ> xYZs  line.Tessellate();

            XYZ xYZe0 = line.GetEndPoint(0);//Punto inicial
            XYZ xYZe1 = line.GetEndPoint(1);//Punto final

            //Obtenemos punto medio
            XYZ xYZgiro = (xYZe0 + xYZe1) / 2;
            //Creamos eje de giro desde el punto medio
            Line ejeGiro = Line.CreateUnbound(xYZgiro, XYZ.BasisZ);

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //Rotamos el elemento
                locationCurve.Rotate(ejeGiro, Math.PI / 4);

                //Confirmamos Transaction
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }

    public class BeamSeleccionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            //Filtramos a Armazon estructural
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
            {
                return true;
            }
            return false;
        }
        public bool AllowReference(Reference reference, XYZ xyz)
        {
            return false;
        }
    }
}
