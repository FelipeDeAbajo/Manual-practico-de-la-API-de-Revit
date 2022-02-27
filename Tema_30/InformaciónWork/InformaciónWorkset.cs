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

namespace InformaciónWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class InformaciónWorkset : IExternalCommand
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

            //Obtenemos el primer ElementId de la selección
            ElementId elementId = sel.GetElementIds().FirstOrDefault();

            //Si es nulo
            if(elementId ==null)
            {
                message = "Selección no válida";
                return Result.Failed;
            }

            //Creamos un string
            string salida = string.Empty;

            //Añadimos el ElementId del muro.
            salida += "ElementId del muro: " + elementId;

            //Obtenemos el Element
            Element elem = doc.GetElement(elementId);

            //Obtenemos el WorksetId
            WorksetId worksetId = elem.WorksetId;
            salida += ("\n\nWorksetId del muro: " + worksetId.ToString());

            //Obtenemos la tabla de subproyectos del documento
            WorksetTable worksetTable = doc.GetWorksetTable();

            //Buscamos un Workset por su WorksetId
            Workset worksetActual = worksetTable.GetWorkset(worksetId);
            salida += ("\n\nNombre del Workset del muro: " + worksetActual.Name);

            //Obtenemos el ModelUpdatesStatus del muro
            ModelUpdatesStatus updateStatus = WorksharingUtils.GetModelUpdatesStatus(doc, elementId);
            salida += ("\n\nModelUpdatesStatus del muro: " + updateStatus.ToString());

            //Obtenemos el CheckoutStatus del muro
            CheckoutStatus checkoutStatus = WorksharingUtils.GetCheckoutStatus(doc, elementId);
            salida += ("\n\nCheckoutStatus del muro: " + checkoutStatus.ToString());

            //Obtenemos WorksharingTooltipInfo
            WorksharingTooltipInfo tooltipInfo = WorksharingUtils.GetWorksharingTooltipInfo(doc, elementId);
            salida += ("\n\nMuro creado por : " + tooltipInfo.Creator);
            salida += ("\n\nPropietario actual del muro : " + tooltipInfo.Owner);
            salida += ("\n\nUltimo cambio realizado por : " + tooltipInfo.LastChangedBy);

            Autodesk.Revit.UI.TaskDialog.Show("Revit API Manual", salida);

            return Result.Succeeded;
        }
    }
}
