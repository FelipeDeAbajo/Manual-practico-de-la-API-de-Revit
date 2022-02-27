#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

#endregion

namespace RegistroEventos
{
    class RegistroEventosApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            try
            {
                // Registramos el evento
                a.ControlledApplication.DocumentOpened += new EventHandler
                    <Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(OnDocumentOpened);
            }
            catch (Exception)
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            //Cancelamos el registro
            a.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            return Result.Succeeded;
        }
        public void OnDocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            // Obtenemos el Document desde args
            Document doc = args.Document;
            //Creamos Transaction
            using (Transaction transaction = new Transaction(doc, "Transaction Registro Eventos"))
            {
                //Iniciamos Transaction
                if (transaction.Start() == TransactionStatus.Started)
                {
                    //Modificamos el Document. 
                    doc.ProjectInformation.Address = "Revit API Manual. Madrid";
                    //Confirmamos Transaction
                    transaction.Commit();

                    TaskDialog.Show("Revit API Manual", "Proyecto actualizado");
                }
            }
        }
    }
}
