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

namespace CrearWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class CrearWorkset : IExternalCommand
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

            #region Habilitar Colaborar
           //Si no es posible Colaborar
            if(!doc.CanEnableWorksharing())
            {
                message = "Imposible habilitar Colaborar.";
                return Result.Failed;
            }

            //Si NO esta habilitado
            if (!doc.IsWorkshared)
            {
                doc.EnableWorksharing("Niveles y rejillas compartidos", "Subproyecto1");
            }
            
            #endregion

            #region Workset actual
            //Obtenemos la tabla de subproyectos del documento
            WorksetTable worksetTable = doc.GetWorksetTable();

            //Obtenemos el WorksetId del Workset actual
            WorksetId activeId = worksetTable.GetActiveWorksetId();

            //Buscamos un Workset por su WorksetId
            Workset worksetActual = worksetTable.GetWorkset(activeId);

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamo Transaction
                tx.Start("Transaction RenombrarWorkset");

                //Renombramos el subproyecto
                WorksetTable.RenameWorkset(doc, activeId, "Workset renombrado");

                //Confirmamos Transaction
                tx.Commit();
            }

            #endregion     

            #region crear Workset
            Workset newWorkset = null;
            //Worksets solo se pueden crear en un documento con el trabajo compartido habilitado
            string worksetName = "Nuevo Workset";
            // El nombre no debe estar en uso por otro subproyecto
            if (WorksetTable.IsWorksetNameUnique(doc, worksetName))
            {
                //Definimos Transaction
                using (Transaction tx = new Transaction(doc))
                {
                    //Iniciamo Transaction
                    tx.Start("Transaction CrearWorkset");
                    newWorkset = Workset.Create(doc, worksetName);

                    //Confirmamos Transaction
                    tx.Commit();
                }

            }
            #endregion

            #region Información
            string salida = string.Empty;
            
            //Obtenemos subproyectos e información básica para cada uno
            FilteredWorksetCollector collector = new FilteredWorksetCollector(doc);

            //Buscamos todos los subproyectos de usuario
            collector.OfKind(WorksetKind.UserWorkset);
            IList<Workset> worksets = collector.ToWorksets();

            //Obtenemos la información para cada subproyecto. Propiedades de solo lectura
            foreach (Workset workset in worksets)
            {
                salida += "\n";
                salida += "\nSubproyecto : " + workset.Name;
                salida += "\nUnique Id : " + workset.UniqueId;
                salida += "\nPropietario : " + workset.Owner;
                salida += "\nTipo : " + workset.Kind;
                salida += "\nSubproyecto activo : " + workset.IsDefaultWorkset;
                salida += "\nEditable : " + workset.IsEditable;
                salida += "\nAbierto : " + workset.IsOpen;
                salida += "\nVisible por defecto : " + workset.IsVisibleByDefault;
            }

            //Mostramos información
            TaskDialog.Show("Revit API Manual", salida);
            #endregion

            return Result.Succeeded;
        }
    }
}
