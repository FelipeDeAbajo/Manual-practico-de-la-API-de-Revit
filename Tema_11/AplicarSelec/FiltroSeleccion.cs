#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace FiltroSeleccion
{
    [Transaction(TransactionMode.Manual)]
    public class FiltroSeleccion : IExternalCommand
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

            //Obtenemos vista actual
            View view = uidoc.ActiveView;

            // Creamos ub filtro de clase Floor
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(Floor));
            ICollection<ElementId> elementIds = collector.ToElementIds();

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Filtro selecci�n");

                // Creamos filtro y asignamos los Floor
                SelectionFilterElement filterElement = SelectionFilterElement.Create(doc, "Filtro para Floor");
                filterElement.SetElementIds(elementIds);

                ElementId filterId = filterElement.Id;

                // A�adimos el filtro a la View
                view.AddFilter(filterId);

                //Por norma general debemos regenerar para visualizar el filtro
                doc.Regenerate();

                //Obtenemos configuraci�n de gr�ficos
                OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);

                //Cambiamos la configuraci�n de gr�ficos existente a color azul
                overrideSettings.SetProjectionLineColor(new Color(0x00, 0x00, 0xFF));

                //Sobreescribimos en el filtro en la vista, la configuraci�n de gr�ficos
                view.SetFilterOverrides(filterId, overrideSettings);

                //Confirmamos Transaction
                tx.Commit();
            }

            TaskDialog.Show("API Revit Manual", "Filtro a�adido");

            return Result.Succeeded;
        }
    }
}
