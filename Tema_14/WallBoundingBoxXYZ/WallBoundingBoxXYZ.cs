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

namespace WallBoundingBoxXYZ
{
    [Transaction(TransactionMode.Manual)]
    public class WallBoundingBoxXYZ : IExternalCommand
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
            if(elementIds.Count==0)
            {
                message = "Se debe seleccionar un objeto";
                return Result.Cancelled;
            }

            //Recuperamos el primer objeto
            Element elm = doc.GetElement(elementIds.First());

            //Es de modelo?
            if (elm.Category.CategoryType != CategoryType.Model || elm.Category.IsCuttable ==false)
            {
                message = "Se debe seleccionar un objeto de 'modelo' y 'cortable'";
                return Result.Cancelled;
            }

            View view = doc.ActiveView;
           
            //BoundingBoxXYZ de la vista
            BoundingBoxXYZ sectionBox = elm.get_BoundingBox(view);

            //Coordenadas de BoundingBoxXYZ absoluto
            XYZ max = sectionBox.Max; // Coordenadas máximas (esquina superior derecha frontal de la caja). 
            XYZ min = sectionBox.Min; // Coordenadas mínimas (esquina inferior izquierda trasera de la caja)

            TaskDialog.Show("Revit API Manual", "La 'Z' máxima es: " + max.Z.ToString("N2"));

            return Result.Succeeded;
        }

    }
}
