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

namespace CambiarCota
{
    [Transaction(TransactionMode.Manual)]
    public class CambiarCota : IExternalCommand
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

            bool isCota = false;

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;
            //Creamos Reference
            Reference reference = null;
            //           
            List<Dimension> dimensions = sel.GetElementIds().Select(x => doc.GetElement(x)).Cast<Dimension>().ToList();
            if (dimensions.Count != 1)
            {
                message = "Se debe seleccionar una sola Dimension";
                return Result.Failed;
            }
            //Obtenemos Dimension
            Dimension dimension = dimensions.FirstOrDefault();

            //Creamos mensaje
            string mensaje = "Dimension : ";
            //Obtenemos el nombre 
            mensaje += "\nEl nombre es : " + dimension.Name;
            //Obtenemos la Curve
            Autodesk.Revit.DB.Curve curve = dimension.Curve;
            if (curve != null && curve.IsBound)
            {
                //Obtenemos punto inicial y final
                mensaje += "\nLínea punto inicial:(" + curve.GetEndPoint(0).X + ", " + curve.GetEndPoint(0).Y + ", " + curve.GetEndPoint(0).Z + ")";
                mensaje += "; Línea punto final:(" + curve.GetEndPoint(1).X + ", " + curve.GetEndPoint(1).Y + ", " + curve.GetEndPoint(1).Z + ")";
            }
            //Obtenemos el nombre del tipo
            mensaje += "\nNombre del tipo de Dimension : " + dimension.DimensionType.Name;
           
            //Obtenemos eumero de Reference
            mensaje += "\nNumero de Reference en la Dimension " + dimension.References.Size;
            //Obtenemos Dimensón/Restricción
            if ((int)BuiltInCategory.OST_Dimensions == dimension.Category.Id.IntegerValue)
            {
                mensaje += "\nLa Dimension es cota.";
                //Obtenemos el nombre de la vista. Solo si es cota
                mensaje += "\nNombre de la vista : " + dimension.View.Name;
                isCota = true;
            }
            else if ((int)BuiltInCategory.OST_Constraints == dimension.Category.Id.IntegerValue)
            {
                mensaje += "\nla Dimension es restricción.";
            }

            TaskDialog.Show("Manual Revit API", mensaje);
            
            if(!isCota)
            {
                TaskDialog.Show("Manual Revit API", "No se puede modificar");
                return Result.Succeeded;

            }
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Cota");

                // Compruebe si tenemos una dimensión que no sea de varios segmentos y si la posición del texto es ajustable
                if (dimension.NumberOfSegments == 0 && dimension.IsTextPositionAdjustable())
                {
                    // Obtener la posición actual XYZ del texto
                    XYZ currentTextPosition = dimension.TextPosition;
                    // Calcule una nueva posición XYZ transformando la posición actual del texto
                    XYZ newTextPosition = Transform.CreateTranslation(new XYZ(0.0, 1.0, 0.0)).OfPoint(currentTextPosition);
                    // Establecer la nueva posición del texto
                    dimension.TextPosition = newTextPosition;
                }
                else if (dimension.NumberOfSegments > 0)
                {
                    foreach (DimensionSegment currentSegment in dimension.Segments)
                    {
                        if (currentSegment != null && currentSegment.IsTextPositionAdjustable())
                        {
                            // Obtener la posición actual XYZ del texto
                            XYZ currentTextPosition = currentSegment.TextPosition;
                            // Calcule una nueva posición XYZ transformando la posición actual del texto
                            XYZ newTextPosition = Transform.CreateTranslation(new XYZ(0, 1, 0)).OfPoint(currentTextPosition);
                            // Establecer la nueva posición del texto para el texto del segmento
                            currentSegment.TextPosition = newTextPosition;
                        }
                    }
                }
                tx.Commit();
                TaskDialog.Show("Manual Revit API", "Cota modificada");
            }

            return Result.Succeeded;
        }
    }
}
