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

namespace SincronizarCentral
{
    [Transaction(TransactionMode.Manual)]
    public class SincronizarCentral : IExternalCommand
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

            //Establecemos nueva TransactWithCentralOptions. Utilizamos prederminada
            TransactWithCentralOptions twcOpts = new TransactWithCentralOptions();

            //Opciones de sincronización. Agregamos un comentario
            SynchronizeWithCentralOptions swcOpts = new SynchronizeWithCentralOptions();
            swcOpts.Comment = "Sincronizado desde la API.";

            TaskDialog mainDialog = new TaskDialog("Revit API Manual");
            mainDialog.MainInstruction = "Elegir modo para sincronizar.";

            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "No sincronizar.");
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,"Sincronizar, pero manteniendo los subproyectos.");
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3,  "Sincronizar y renunciar a todos los elementos.");
            mainDialog.DefaultButton = TaskDialogResult.CommandLink1;

            TaskDialogResult taskDialogResult = mainDialog.Show();

            switch (taskDialogResult)
            {
                //No sincronizar
                case TaskDialogResult.CommandLink1:
                default:
                    {
                        //No hacemos nada
                        break;
                    }
                    //Sincronizar y no renunciar
                case TaskDialogResult.CommandLink2:
                    {

                        //Configurar RelinquishOptions
                        RelinquishOptions rOptions = new RelinquishOptions(false);
                        rOptions.UserWorksets = false;//True se renuncian. False no se renuncian
                        //Por defecto salvamos copia local
                        swcOpts.SetRelinquishOptions(rOptions);
                        //Sincronizamos
                        doc.SynchronizeWithCentral(twcOpts, swcOpts);

                        break;
                    }
                //Sincronizar y renunciar
                case TaskDialogResult.CommandLink3:
                    {
                        //Configurar RelinquishOptions
                        RelinquishOptions rOptions = new RelinquishOptions(false);
                        rOptions.UserWorksets = true;//True se renuncian. False no se renuncian
                        swcOpts.SetRelinquishOptions(rOptions);
                        //No salvar despues de sincronizar
                        swcOpts.SaveLocalAfter = false;
                        //Sincronizamos
                        doc.SynchronizeWithCentral(twcOpts, swcOpts);

                        break;
                    }
               
               
            }

            return Result.Succeeded;
        }
    }
}
