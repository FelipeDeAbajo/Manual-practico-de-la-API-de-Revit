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

namespace OpcionesDiseño
{
    [Transaction(TransactionMode.Manual)]
    public class OpcionesDiseño : IExternalCommand
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

            //Selección actual
            Selection sel = uidoc.Selection;

            ICollection<ElementId> elementIdsList = sel.GetElementIds();
            if (elementIdsList.Count != 1)
            {
                message = "Debe selecionar solo un elemento Pilar structural.";
                return Result.Failed;
            }
            else if (doc.GetElement(elementIdsList.FirstOrDefault()) is FamilyInstance pilar
                //además de ser Familiinstance debe tener categoría OST_StructuralColumns
                && pilar.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns)
            {
                string msg = "Pilar : " + pilar.Name;
                //Obtenemos la opción de diseño del pilar
                DesignOption designOption = pilar.DesignOption;
                msg = msg + "\nOpción de diseño del pilar: " + designOption.Name;

                // Obtenemos si es opción primaria
                msg = msg + "\n" + designOption.Name+ ", es primaria: "+ designOption.IsPrimary;

                // Obtenemos el ElementID del Conjunto de opciones de designOption
                ElementId idOptionSet =  designOption.get_Parameter(BuiltInParameter.OPTION_SET_ID).AsElementId();

                // Obtenemos el  Conjunto de opciones
                Element optionSet = doc.GetElement(idOptionSet);
                msg = msg + "\n" + designOption.Name + ", pertenece a: " + optionSet.Name;

                TaskDialog.Show("Manual Revit API", msg);
            }
            else
            {
                message = "Debe selecionar un ejemplar de Pilar estructural.";
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
