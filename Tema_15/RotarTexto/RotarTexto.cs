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

namespace RotarTexto
{
    [Transaction(TransactionMode.Manual)]
    public class RotarTexto : IExternalCommand
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

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;
            if (sel.GetElementIds().Count != 1)
            {
                message = "Debe seleccionar solo un muro";
                return Result.Failed;
            }
            //Nos aseguramos que sea Wall
            if (doc.GetElement(sel.GetElementIds().FirstOrDefault()) is Wall wall)
            {
                //Obtenemos LocationCurve desde wall
                LocationCurve locationCurve = wall.Location as LocationCurve;
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
                    message = "Solo muros rectilineos";
                    return Result.Failed;
                }
                double angle = 0;
               //Obtenemos el ángulo de line, comparado con el eje X
               //Si (Producto vectorial).Z >0 debemos restas de 180º
                if (XYZ.BasisX.CrossProduct(line.Direction).Z > 0)
                {
                    angle = (-XYZ.BasisX).AngleOnPlaneTo(line.Direction, XYZ.BasisZ);

                }
                else
                {
                    angle = XYZ.BasisX.AngleOnPlaneTo(line.Direction, XYZ.BasisZ);

                } 
                
                // Creamos transaction
                using (Transaction tx = new Transaction(doc))
                {
                    //Iniciamos Transaction
                    tx.Start("Rotar texto");
                    //Obtenemos la vista actual
                    View view = uidoc.ActiveView;
                    try
                    {
                        //Creamos ISelectionFilter solo admitimos BuiltInCategory.OST_TextNotes
                        ISelectionFilter selFilter = new TextSelectionFilter();

                        //Seleccionamos un texto. Siempre encerrado en un try{} catch{}
                        Reference reference = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Seleccionar texto");
                        //Obtenemos el texto desde la Reference. La conversión es posible por el selFilter
                        TextNote textNoteOld = doc.GetElement(reference.ElementId) as TextNote;
                        // Creamos TextNoteOptions
                        var testOptions = new TextNoteOptions() { HorizontalAlignment = HorizontalTextAlignment.Center, Rotation = angle, TypeId = textNoteOld.TextNoteType.Id };
                        //Creamos el nuevo texto
                        var text = TextNote.Create(doc, view.Id, textNoteOld.Coord, textNoteOld.Text, testOptions);
                        //Borramos el anterior
                        doc.Delete(textNoteOld.Id);
                        //Confirmamos Transaction
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Si falla la selección de texto
                        message = ex.Message;
                        return Result.Failed;
                    }
                }
            }
            else
            {
                //Si el seleccionado no es muro
                message = "Debe seleccionar solo un muro";
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
    public class TextSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element is TextNote)
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
