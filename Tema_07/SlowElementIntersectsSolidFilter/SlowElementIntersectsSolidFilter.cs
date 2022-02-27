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

namespace SlowElementIntersectsSolidFilter
{
    [Transaction(TransactionMode.Manual)]
    public class SlowElementIntersectsSolidFilter : IExternalCommand
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

            Element columnSelect = null;
            //Seleccionamos un objeto.
            //Mediante un ISelectionFilter asegurarnos que es un Pilar estructural 
            try
            {
                Reference reference = uidoc.Selection.PickObject(ObjectType.Element, new ColumnStructSelectionFilter(), "Seleccionar pilar para intersección.");
                columnSelect = doc.GetElement(reference.ElementId);

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            // Obtenemos geometria de una FamilyInstance. Es distinto de un Element
            //Construimos Options.
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            #region obtener solido pilar
            //Creamos un solid para el pilar
            Solid solidPilar = null;

            //Obtenemos geometria de la FamilyInstance
            GeometryElement geometryElement = columnSelect.get_Geometry(options);

            foreach (GeometryObject geomObj in geometryElement)
            {
                //Primero obtenemos GeometryInstance
                GeometryInstance geoInstance = geomObj as GeometryInstance;
                if (null != geoInstance)
                {
                    //Ahora la geometria de la instancia, cada pilar puede tener una altura, anchura etc
                    GeometryElement instanceGeometryElement = geoInstance.GetInstanceGeometry();
                    //Recorremos buscando Solid en cada GeometryObject
                    foreach (GeometryObject o in instanceGeometryElement)
                    {
                        solidPilar = o as Solid;
                        //Chequemos que es solido (puede ser Face), y que el volumen sea  >0
                        //Si existe el solido salimos del segundo foreach
                        if (solidPilar != null && solidPilar.Volume > 0) break;
                    }
                    //Si existe el solido salimos del primer foreach
                    if (solidPilar != null && solidPilar.Volume > 0) break;
                }

            }
            //Debemos cpmprobar que existen estos muros
            if (solidPilar == null)
            {
                message = "El pilar no debe haber tenido nunca la geometría unida. Cree uno nuevo.";
                return Result.Failed;
            }
            #endregion

            #region obtener solido union de los muros
            //Construimos collector
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            //Creamos el ElementIntersectsSolidFilter para el solido del pilar
            ElementIntersectsSolidFilter elementIntersectsSolidFilter = new ElementIntersectsSolidFilter(solidPilar);

            //Aplicamos el filtro elementIntersectsSolidFilter. Previamente filtro de .OfClass(typeof(Wall)
            IList<Element> walls = collector.OfClass(typeof(Wall)).WherePasses(elementIntersectsSolidFilter).ToElements();

            //Debemos cpmprobar que existen estos muros
            if(walls.Count==0)
            {
                message = "No existen muros intersectantes";
                return Result.Failed;
            }
            //Creamos un Solid nullo para almacenar el solido union de todos los walls
            Solid solidUnionWalls = null;
            
            foreach (Element wall in walls)
            {
                //Obtenemos la geometría de cada wall
                GeometryElement geometryWall = wall.get_Geometry(options);
                foreach (GeometryObject geometryObject in geometryWall)
                {
                    //Chequemos si es Solid y almacenamos en tempWall
                    if (geometryObject is Solid tempWall)
                    {
                        if (tempWall != null && tempWall.Volume > 0)
                        {
                            //Si es el primer muro, no podemos crear la union porque unionWalls = null
                            if (solidUnionWalls == null) solidUnionWalls = BooleanOperationsUtils.ExecuteBooleanOperation(tempWall, tempWall, BooleanOperationsType.Union); 
                            //Si no es el primer muro creamos union
                            else BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solidUnionWalls, tempWall, BooleanOperationsType.Union);
                            break;
                        }

                    }
                }
            }
            #endregion
            #region crear solido intersección
            Solid interseccion= BooleanOperationsUtils.ExecuteBooleanOperation(solidUnionWalls, solidPilar, BooleanOperationsType.Intersect);
            #endregion
            // Creamos nueva instancia del collector
            collector = new FilteredElementCollector(doc);
            //Creamos el ElementIntersectsSolidFilter para el solido interseccion
             elementIntersectsSolidFilter = new ElementIntersectsSolidFilter(interseccion);

            //Creamos lista de exclusión. Muros + pilar
            List<ElementId> idsToExclude = walls.Select(x => x.Id).ToList();
            idsToExclude.Add(columnSelect.Id);

            //Creamos ExclusionFilter
            ExclusionFilter exclusionFilter = new ExclusionFilter(idsToExclude);

            //Aplicamos el filtro elementIntersectsSolidFilter. Previamente pasamos filtro rápido ExclusionFilter. 
            IList<Element> elementsIntersect = collector.WherePasses(exclusionFilter).WherePasses(elementIntersectsSolidFilter).ToElements();

            List<string> names = elementsIntersect.Select(x => x.Name).ToList();
            names.Insert(0, "Elementos que SI intersectan al solido intersección");
            TaskDialog.Show("Manual Revit API", string.Join("\n", names));

            return Result.Succeeded;
        }

    }
    public class ColumnStructSelectionFilter : ISelectionFilter
    {
        //Seccion Element
        public bool AllowElement(Element element)
        {
            // Solo continuamos si es FamilyInstance
            if (element is FamilyInstance column)
            {
                //Obtenemos categoría y comparamos

                //Solo admitimos pilar structural
                if (column.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns) return true;
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
}
