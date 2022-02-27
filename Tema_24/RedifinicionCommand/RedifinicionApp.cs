#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;

#endregion

namespace RedifinicionCommand
{
    class RedifinicionApp : IExternalApplication
    {
        //Identificador del Commnand
        static String s_commandToDisable = "ID_EDIT_DESIGNOPTIONS";

        //ID del Command
        static RevitCommandId s_commandId;
        public Result OnStartup(UIControlledApplication a)
        {
            //Buscamos el comando deseado por nombre
            s_commandId = RevitCommandId.LookupCommandId(s_commandToDisable);

            //Confirmamos que el comando se puede anular
            if (!s_commandId.CanHaveBinding)
            {
                TaskDialog.Show("Revit API Manual", "El command " + s_commandToDisable + " seleccionado para desactivar no se puede anular.");
                return Result.Failed;
            }

            //Anulamos el Command
            try
            {
                AddInCommandBinding commandBinding = a.CreateAddInCommandBinding(s_commandId);
                //Lo anulamos
              //  commandBinding.CanExecute += new EventHandler<CanExecuteEventArgs>(DisableEvent);

                //Lo redifinimos
                commandBinding.Executed += new EventHandler<Autodesk.Revit.UI.Events.ExecutedEventArgs>(NewCommandEvent); 
            }
            catch (Exception es)
            {
                TaskDialog.Show("Revit API Manual", es.Message);
            }

            return Result.Succeeded;
        }


        public Result OnShutdown(UIControlledApplication application)
        {
            // Eliminamos la redifinición
            if (s_commandId.HasBinding)
                application.RemoveAddInCommandBinding(s_commandId);
            return Result.Succeeded;
        }
        //Creamos nueva defición
        private void NewCommandEvent(object sender, ExecutedEventArgs args)
        {
            TaskDialog.Show("Revit API Manual", "El uso de este comando ha sido deshabilitado.");
        }
        //Anulamos el Command
        private void DisableEvent(object sender, CanExecuteEventArgs args)
        {
            args.CanExecute = false;
        }

    }
}