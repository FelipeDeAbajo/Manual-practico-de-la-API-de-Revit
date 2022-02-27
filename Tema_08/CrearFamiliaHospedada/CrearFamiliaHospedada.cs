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

namespace CrearFamiliaHospedada
{
    [Transaction(TransactionMode.Manual)]
    public class CrearFamiliaHospedada : IExternalCommand
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

            List<FamilyInstance> familyInstances = new List<FamilyInstance>();
            //Seleccionamos vista actual
            View view = doc.ActiveView;
            //Solo vistas en planta
            if (view.ViewType != ViewType.FloorPlan)
            {
                message = "La vista no es de planta";
                return Result.Failed;
            }

            //Siempre que usemos Selection.PickObject ponemos try{}
            try
            {
                Reference reference = uidoc.Selection.PickObject(ObjectType.Element, new WallSelectionFilter(), "Seleccionar un muro");
                Element wallTemp = doc.GetElement(reference.ElementId);
                if (wallTemp is Wall wall) //Conversión redundante. Siempre será muro
                {
                    FilteredElementCollector col = new FilteredElementCollector(doc);
                    //Buscamos todos los FamilySymbol puertas 
                    ICollection<Element> simbols = col.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Doors).ToElements();

                    //Obtenemos la LocationCurve del Wall
                    LocationCurve locationCurve = wall.Location as LocationCurve;
                    //Obtenemos la Line desde LocationCurve
                    Line curve = locationCurve.Curve as Line;

                    //si LocationCurve is UnBound da problemas. siempre mejor Tessellate
                    IList<XYZ> xYZs = curve.Tessellate();

                    curve = Line.CreateBound(xYZs[0], xYZs[1]);
                    //Establecemos un desfase inicial de 6 unidades internas
                    double desfase = 6;

                    //Obtenemos el Level asociado a la View
                    Level level = doc.ActiveView.GenLevel;

                    //Creamos un contador pa incrementar el punto de inserción
                    int n = 0;

                    //Creamos la Transaction
                    using (Transaction tx = new Transaction(doc))
                    {
                        //Iniciamos la Transaction
                        tx.Start("Inserción de Puertas");
                        //Para ca da tipo intentamos una inserción
                        foreach (Element element in simbols)
                        {
                            n++;
                            // Seleccionamos FamilySymbol
                            FamilySymbol familySymbol = element as FamilySymbol;
                            //Antes de crear una FamilyInstance hay que activar el tipo
                            familySymbol.Activate();
                            //Obpenemos XYZ desde la curve. Consideramos su verdadera longitud (no entre 0 y 1)
                            XYZ xYZ = curve.Evaluate(desfase * n, false);
                            if (desfase * n > curve.ApproximateLength)
                            {
                                //estamos ya fuera del muro. No podemos colocar la Puerta
                                break;
                            }
                            //Añadimos FamilyInstance a la lista
                            familyInstances.Add(doc.Create.NewFamilyInstance(xYZ, familySymbol, wall, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural));

                        }
                        //Confirmamos la Transaction
                        tx.Commit();
                    }

                }
                else
                {
                    message = "No has seleccionado muro";
                    return Result.Cancelled;
                }
            }
            catch
            {
                message = "No has seleccionado muro";
                return Result.Cancelled;
            }
            TaskDialog.Show("API", "Perfecto. " + familyInstances.Count + " puertas creadas.");

            return Result.Succeeded;
        }
    }
    public class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.IsValidObject && element.Category != null && element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls)
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
