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

namespace ModificarFiltro
{
    [Transaction(TransactionMode.Manual)]
    public class ModificarFiltro : IExternalCommand
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
            // Find any filter with overrides setting cut color to Red

            //Obtenemos los filtros de la vista actual
            foreach (ElementId filterId in view.GetFilters())
            {
                //Obtenemos configuración de gráficos
                OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
                //Obtenemos el color de las lineas de proyección
                Color lineColor = overrideSettings.ProjectionLineColor;
                //Si no tiene color 
                if (!lineColor.IsValid || lineColor == Color.InvalidColorValue)
                    continue;

                // Si el color es azul, lo cambiamos a verde
                if (lineColor.Red == 0x00 && lineColor.Green == 0x00 && lineColor.Blue == 0xFF)
                {
                    overrideSettings.SetProjectionLineColor(new Color(0x00, 0xFF, 0x00));

                    //Crteamos Transaction
                    using (Transaction tx = new Transaction(doc))
                    {
                        //Iniciamos Transaction
                        tx.Start("Transaction Name");

                        //Sobrescribimos en la vista, para el filtro seleccionado la configuración
                        view.SetFilterOverrides(filterId, overrideSettings);

                        //Confirmamos Transaction
                        tx.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
