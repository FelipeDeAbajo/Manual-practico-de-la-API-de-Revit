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

namespace RotarEnPlanoInclinado
{
    [Transaction(TransactionMode.Manual)]
    public class RotarEnPlanoInclinado : IExternalCommand
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
            #region Eje vertical
            //Creamo un eje verical. Es incorrecto
             Line ejeGiro = Line.CreateBound(xYZgiro, xYZgiro+XYZ.BasisZ);
            #endregion

            #region Eje desde GetTransform().BasisZ
            //Obtenemos la FamilyInstance
            //  FamilyInstance familyInstance = (element as FamilyInstance);
            //Obtenemos la normal desde familyInstance.GetTransform()
            // Line ejeGiro = Line.CreateUnbound(xYZgiro, familyInstance.GetTransform().BasisZ);
            #endregion

            #region Eje desde FaceNormal.Normalize()
            ////Obtenemos la FamilyInstance
            //FamilyInstance familyInstance = (element as FamilyInstance);
            ////Como la viga esta hospedad en plano de cara, podemos obtener la cara
            //Reference referenceFace = familyInstance.HostFace;
            ////Obtenemos la cubierta desde la Reference
            //Element cubierta = doc.GetElement(referenceFace.ElementId);
            ////El plano seleccionado del faldón seleccionado es una cara "plana" luego podemos obtener su PlanarFace
            //PlanarFace face = cubierta.GetGeometryObjectFromReference(referenceFace) as PlanarFace;
            ////Obtenemos la normal de la Face. La normalizamos
            //XYZ normal = face.FaceNormal.Normalize();
            //Line ejeGiro = Line.CreateUnbound(xYZgiro, normal);
            #endregion
            #region Eje desde producto vectorial
            ////Creamos un Plane horizontal por el xYZe0
            //Plane planeH = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, xYZe0);
            ////****Error no es válido
            ////Creamos la proyección de xYZe1 sobre el plano
            ////Este método no es valido. no da una distancia orientada
            //// plane.Project(xYZe0, out UV uVEnPlano, out double distancia);
            ////****Error no es valido

            ////Recurrimos a trigonometría
            ////Vector de un punto en viga a un punto en plano
            //XYZ vector = xYZe1 - planeH.Origin;
            ////Distancia con signo Producto escalar
            //double distancia = planeH.Normal.DotProduct(vector);
            ////XYZ proyeccion del punto en viga sobre el plano horizontal
            //XYZ xYZplaneH = xYZe1 - distancia * planeH.Normal;
            ////XYZ vector del "alero", Producto vectorial (linea sobre plano horizontal * linea viga)
            //XYZ vectorAlero = (xYZplaneH - xYZe1).CrossProduct(xYZe1 - xYZe0);
            ////XYZ normal plano faldon= Producto vectorial (vectorAlero* linea viga)
            //XYZ vectorNormalFaldon = vectorAlero.CrossProduct(xYZe1 - xYZe0);
            ////Eje de giro
            //Line ejeGiro = Line.CreateUnbound(xYZgiro, vectorNormalFaldon);
            #endregion

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");

                //Rotamos el elemento
                ElementTransformUtils.RotateElement(doc, element.Id, ejeGiro, Math.PI / 2);

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
            if (element.Category != null && element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
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
