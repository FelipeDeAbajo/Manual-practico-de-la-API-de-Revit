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

namespace CrearEtiqueta
{
    [Transaction(TransactionMode.Manual)]
    public class CrearEtiqueta : IExternalCommand
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

            //Suponemos la Tag ya cargada
            Wall wall = null;
            Reference reference = null;

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;
            try
            {
                //PickObject siemtre entre try{} catch {}
                reference = sel.PickObject(ObjectType.Element, new WallSelectionFilter(), "Seleccione un muro");
                wall = doc.GetElement(reference) as Wall;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            // O es 2D o 3D bloqueada
            View view = doc.ActiveView;
            if (view is View3D && (view as View3D).IsLocked == false)
            {
                message = "No es posible en 3D, o 3D no bloqueadas";
                return Result.Failed;
            }

            // Definimos modo de etiqueta por categoría y la orientación de la etiqueta
            TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
            TagOrientation tagorn = TagOrientation.Horizontal;

            //Obtenemos LocationCurve y el punto medio
            LocationCurve wallLoc = wall.Location as LocationCurve;
            XYZ wallStart = wallLoc.Curve.GetEndPoint(0);
            XYZ wallEnd = wallLoc.Curve.GetEndPoint(1);
            XYZ wallMid = wallLoc.Curve.Evaluate(0.5, true);
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Etiqueta");

                IndependentTag newTag = IndependentTag.Create(doc, view.Id, reference, true, tagMode, tagorn, wallMid);
                if (null == newTag)
                {
                    throw new Exception("Creación de IndependentTag fallida.");
                }

                // newTag.TagText es de solo lectura, por lo que cambiamos el parámetro de tipo de Type Mark comotexto de la etiqueta.
                // El parámetro de etiqueta para la familia de etiquetas determina qué parámetro de tipo se utiliza para el texto de la etiqueta.

                //Obtenemos el tipo
                WallType type = wall.WallType;
                //Ontenemos el parámetro y le damos valor
                type.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).Set("Hola API");

                if (!(view is View3D))
                {
                    //Si no es 3D
                    // Establecemos el extremo libre, de lo contrario, el punto final se mueve con el codo
                    newTag.LeaderEndCondition = LeaderEndCondition.Free;
                    //Calculamos puntos para el codo y de posición
                    XYZ elbowPnt = wallMid + new XYZ(5.0, 5.0, 0.0);
                    newTag.SetLeaderElbow(reference, elbowPnt);
                    XYZ headerPnt = wallMid + new XYZ(10.0, 10.0, 0.0);
                    newTag.TagHeadPosition = headerPnt;
                }
                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
    public class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            //Solo admitimos la clase Wall
            if (element is Wall)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
}
