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

namespace RegistroEventos
{
    /// <summary>
    /// Command empleado para registrar el evento
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class RegistroEventos : IExternalCommand
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

            //Registramos el evento
            uiapp.Application.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(CommandEvents.OnDocumentChanged);

            return Result.Succeeded;
        }

    }

    /// <summary>
    /// Command empleado para eliminar el registro
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class EliminarRegistroEventos : IExternalCommand
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
            //Eliminamos el registro
            uiapp.Application.DocumentChanged -= new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(CommandEvents.OnDocumentChanged);

            return Result.Succeeded;
        }
    }
    internal class CommandEvents
    {
        internal static void OnDocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            //Obtenemos el document
            Document document = e.GetDocument();

            //Creamos tres listas de ElementId
            ICollection<ElementId> idsAdded = e.GetAddedElementIds();
            ICollection<ElementId> idsMod = e.GetModifiedElementIds();
            ICollection<ElementId> idsDel = e.GetDeletedElementIds();

            //Obtenemos el tipo de operación que genero el evento
            TaskDialog.Show("Revit API Manual", "Operación realizada: " + e.Operation);

            //Obtenemos la lista de transactions y mostramos su nombre
            IList<string> transactions = e.GetTransactionNames();
            TaskDialog.Show("Revit API Manual", "Número de transaciones: " + transactions.Count + ":\n" + String.Join("\n", transactions));

            //Hay elementos modificados?
            if (idsMod.Count > 0)
            {
                List<string> names = idsMod.Select(x => document.GetElement(x).Name).ToList();
                string outString = String.Join("\n", names);

                TaskDialog.Show("Revit API Manual", "Modificados : " + idsMod.Count + " elementos:\n" + outString);

            }

            //Hay elementos añadidos ?
            if (idsAdded.Count > 0)
            {
                List<string> names = idsAdded.Select(x => document.GetElement(x).Name).ToList();
                string outString = String.Join("\n", names);

                TaskDialog.Show("Revit API Manual", "Añadidos : " + idsAdded.Count + " elementos:\n" + outString);
            }

            //Hay elementos borrados?
            if (idsDel.Count > 0)
            {
                Element imposible = document.GetElement(idsDel.FirstOrDefault());

                TaskDialog.Show("Revit API Manual", "Borrados: " + idsDel.Count + " elementos");
            }
        }
    }
}

