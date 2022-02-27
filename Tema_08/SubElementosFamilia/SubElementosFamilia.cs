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

namespace SubElementosFamilia
{
    [Transaction(TransactionMode.Manual)]
    public class SubElementosFamilia : IExternalCommand
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
            if (sel.GetElementIds().FirstOrDefault() == null)
            {
                message = "Se debe seleccionar una FamilyInstance";
                return Result.Failed;
            }
            //Tomamos una familyIstance. La primera
            FamilyInstance familyInstance = doc.GetElement(sel.GetElementIds().FirstOrDefault()) as FamilyInstance;

            // Si no es FamilyInstance Salimos
            if (familyInstance == null)
            {
                message = "Se debe seleccionar una FamilyInstance";
                return Result.Failed;
            }

            //Obtenemos subelementos
            ICollection<ElementId> subelementsId = familyInstance.GetSubComponentIds();
            if (subelementsId.Count > 0)
            {
                string listado = string.Empty;
                foreach (ElementId elementId in subelementsId)
                {
                    listado = listado + doc.GetElement(elementId).Name + "\n";
                }  
                TaskDialog.Show("Manual Revit API", listado);

            }
            else
            {
                TaskDialog.Show("Manual Revit API", "No hay familias anidadas");

            }
            //Obtenemos el padre
            FamilyInstance familyInstancePadre = familyInstance.SuperComponent as FamilyInstance;

            if (familyInstancePadre != null) TaskDialog.Show("Manual Revit API", "Familia padre: " + familyInstancePadre.Name);

            return Result.Succeeded;
        }
    }
}
