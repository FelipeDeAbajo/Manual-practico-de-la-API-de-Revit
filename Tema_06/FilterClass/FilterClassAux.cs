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

namespace FilterClassAux
{
    // Implementamos la interfaz ISelectionFilter
  public  class WallSelectionFilter : ISelectionFilter
    {
        //Seccion Element    
        public bool AllowElement(Element element)
        {
            // Solo continuamos si es Wall
            if (element is Wall wall)
            {
                // Obtenemos si es muro apilado
                bool isStacked = wall.IsStackedWall;
                // Si apilado false
                if (isStacked) return false;
                // Obtenemos la altura del muro
                double altura = element.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                // Convertimos la altura de u.i. a metros
                altura = UnitUtils.ConvertFromInternalUnits(altura, UnitTypeId.Meters);
                //Solo admitimos muros con altura superior a 5 metros
                if (altura > 5) return true;
            }
            return false;
        }
        // Seccion Reference
        public bool AllowReference(Reference refer, XYZ point)
        {
            //No filtramos ninguna Reference, siempre retornamos false
            return false;
        }

    }

    // Implementamos la interfaz ISelectionFilter

   public class PlanarFaceSelectionFilter : ISelectionFilter
    {
        Document document = null;
        public PlanarFaceSelectionFilter(Document document)
        {
            this.document = document;
        }
        public bool AllowElement(Element element)
        {
            //No filtramos ninguna Element, siempre retornamos true
            return true;
        }
        public bool AllowReference(Reference reference, XYZ point)
        {
            // Obtenemos el Element
            Element element = document.GetElement(reference.ElementId);
            // Obtenemos el GeometryObject
            GeometryObject geometryObject = element.GetGeometryObjectFromReference(reference);
            // Parseamos a PlanarFace.
            PlanarFace planarFace = geometryObject as PlanarFace;
            //Solo admitimos caras planas. 
            if (planarFace != null) return true;

            return false;
        }
    }
}
