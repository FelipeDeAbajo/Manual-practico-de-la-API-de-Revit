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

namespace FiltroReglas
{
    [Transaction(TransactionMode.Manual)]
    public class FiltroReglas : IExternalCommand
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

            //Obtenemos la vista actual
            View view = uidoc.ActiveView;

            //Creamos una coleccion con las categorias que deseamos filtrar. Walls
            ISet<ElementId> categories = new HashSet<ElementId>() { new ElementId(BuiltInCategory.OST_Walls) };

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Filtro reglas");

                //Obtenemos el parametro Uso Estructura 0 = no es estructura, > 0 = si es estructura
                ElementId exteriorParamId = new ElementId(BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM);
                //Creamos el filtro de ElementParameterFilter
                ElementParameterFilter filter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateGreaterRule(exteriorParamId,0));

                // Creamos filtro asociado a las categorías de entrada (Wall)
                if (ParameterFilterElement.ElementFilterIsAcceptableForParameterFilterElement(doc, categories, filter))
                {
                    ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(doc, "Filtro muros estructurales", categories);
                    parameterFilterElement.SetElementFilter(filter);
                    // Aplicamos filtro a la vista
                    view.AddFilter(parameterFilterElement.Id);
                    //Los objetos incluidos NO son visibles
                    view.SetFilterVisibility(parameterFilterElement.Id, false);
                }
                else
                {
                    message = "El filtro no puede usarse";
                    return Result.Cancelled;
                }
                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
