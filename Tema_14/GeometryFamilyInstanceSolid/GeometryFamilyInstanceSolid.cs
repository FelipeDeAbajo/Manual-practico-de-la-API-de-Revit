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

namespace GeometryFamilyInstanceSolid
{
    [Transaction(TransactionMode.Manual)]
    public class GeometryFamilyInstanceSolid : IExternalCommand
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

            Selection sel = uidoc.Selection;
            //Obtenenos la selección
            ICollection<ElementId> elementIds = sel.GetElementIds();

            //Hay algún objeto seleccionado?
            if (elementIds.Count == 0)
            {
                message = "Se debe seleccionar un objeto";
                return Result.Cancelled;
            }

            //Recuperamos el primer objeto
            Element elm = doc.GetElement(elementIds.First());
            //Es de modelo?
            if (elm.Category.CategoryType != CategoryType.Model || elm.Category.IsCuttable == false)
            {
                message = "Se debe seleccionar un objeto de 'modelo' y 'cortable'";
                return Result.Cancelled;
            }

            FamilyInstance familyInstance = null;
            //Creamos un Options
            Options options = new Options();
            //Asignamos Nivel de detalle Alto
            options.DetailLevel = ViewDetailLevel.Fine;
            //No incluimos objetos no visibles
            options.IncludeNonVisibleObjects = false;
            //No computar References
            options.ComputeReferences = false;

            //Creamos un solid vacio
            Solid solidVidrio = null;

            //Obtenemos el GeometryElement
            GeometryElement geometryElement = elm.get_Geometry(options);
            //Iteramos por GeometryObject cada en GeometryElement
            foreach (GeometryObject geomObj in geometryElement)
            {
                //Primero obtenemos GeometryInstance
                GeometryInstance geoInstance = geomObj as GeometryInstance;
                if (null != geoInstance)
                {
                    //Como hemos obtenido GeometryInstance => elm es FamilyInstance
                    familyInstance = elm as FamilyInstance;

                    //Ahora la geometria de la instancia, cada ventana puede tener una altura, anchura etc
                    GeometryElement instanceGeometryElement = geoInstance.GetInstanceGeometry();
                    //Recorremos buscando Solid en cada GeometryObject
                    foreach (GeometryObject o in instanceGeometryElement)
                    {
                        solidVidrio = o as Solid;
                        //Chequemos que es solido (puede ser Face), y que el volumen sea  >0
                        if (solidVidrio != null && solidVidrio.Volume > 0)
                        {
                            //Comprobamos que sea vidrio la cara
                            GraphicsStyle style = doc.GetElement(solidVidrio.GraphicsStyleId) as GraphicsStyle;
                            if (style.Category !=null && style.Category.Id.IntegerValue == (int)BuiltInCategory.OST_WindowsGlassProjection) break;
                        }
                    }
                }

            }

            #region Face

            //Obtenemos del solid un FaceArray y un FaceArrayIterator
            FaceArray faceArray = solidVidrio.Faces;
            FaceArrayIterator faceArrayIterator = faceArray.ForwardIterator();
            //Creamos Face null
            Face faceVidrio = null;
            //Iteramos por FaceArrayIterator mientras podamos avanzar
            while (faceArrayIterator.MoveNext())
            {
                //Obtenemos la Face actual
                faceVidrio = faceArrayIterator.Current as Face;
                //Es la Face PlanarFace? Es normal igual a la FacingOrientation. Perdicular las Host?
                if (faceVidrio is PlanarFace planarFace && planarFace.FaceNormal.IsAlmostEqualTo(familyInstance.FacingOrientation))
                {
                    break;
                }
            }
            #endregion
            TaskDialog.Show("Revit API Manual", "El area del vidrio de la ventana es: " + faceVidrio.Area.ToString("N2"));

            return Result.Succeeded;
        }
    }
}
