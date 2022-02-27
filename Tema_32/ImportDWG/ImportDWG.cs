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

namespace ImportDWG
{
    [Transaction(TransactionMode.Manual)]
    public class ImportDWG : IExternalCommand
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

            //Creamos un nombre para el fichero
            string nombreFichero = string.Empty;

            //Creamos un formulario de lectura de ficheros
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            //Filtramos tipo de archivos para leer
            openFileDialog.Filter = "Dibujo de AutoCad (*.dwg)|*.dwg";

            //Mostramos el formulario
            System.Windows.Forms.DialogResult dialogResult = openFileDialog.ShowDialog();
            //Si cancelamos o cerramos sin seleccionar archivo válido
            if (dialogResult != System.Windows.Forms.DialogResult.OK)
            {
                message = "Seleccion cancelada";
                return Result.Cancelled;

            }
            //asignamos el nombre del fichero
            nombreFichero = openFileDialog.FileName;

            if (!System.IO.File.Exists(nombreFichero))
            {
                message = "El archivo seleccionado no existe";
                return Result.Failed;
            }

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction ImportDWG");

                try
                {
                    //Creamos DWGImportOptions
                    DWGImportOptions dwgImportOption = new DWGImportOptions();

                    //Configuramos algunas opciones
                    dwgImportOption.ColorMode = ImportColorMode.BlackAndWhite;
                    dwgImportOption.CustomScale = 0.0;// ie 0 = use import units
                    dwgImportOption.Placement = ImportPlacement.Origin;
                    dwgImportOption.ThisViewOnly = true;
                    dwgImportOption.VisibleLayersOnly = false;

                    //Suponemos unidades en milimetros. Segun opciones por defecto
                    dwgImportOption.Unit = ImportUnit.Millimeter;

                    //Importamos DWG
                    doc.Import(nombreFichero, dwgImportOption, doc.ActiveView, out ElementId pElementId);

                    //Confirmamos Transaction
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    //Anulamos Transaction
                    tx.RollBack();

                    message = ex.Message;
                    return Result.Failed;
                }
            }
            return Result.Succeeded;
        }
    }
}
