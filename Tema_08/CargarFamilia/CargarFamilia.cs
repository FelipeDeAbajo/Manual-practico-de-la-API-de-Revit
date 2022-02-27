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

namespace CargarFamilia
{
    [Transaction(TransactionMode.Manual)]
    public class CargarFamilia : IExternalCommand
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

            //Obtenemos todas las bibliotecas instaladas
            IDictionary<string, string> keyValuePair = doc.Application.GetLibraryPaths();
            //Obtenemos la que coincide con nuestro nombre
            //Lo pasamos como carpeta inicial al formulario
            openFileDialog.InitialDirectory = keyValuePair["API Revit Manual"];

            //Filtramos tipo de archivos para leer
            openFileDialog.Filter = "Familias de Revit (*.rfa)|*.rfa";

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
            //Creamos una Family
            Family family = null;
            //Creamos una Transactión. 
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos una Transaction
                tx.Start("Transaction Name");
                //Leemos la familia, 
                bool leida = doc.LoadFamily(nombreFichero, new OpcionesCargaFamiliasExt(), out family);
                if (family != null)
                {
                    TaskDialog.Show("API Revit Manual", "Familia leida: " + family.Name);
                }
                else
                {
                    TaskDialog.Show("API Revit Manual", "Familia NO leida");
                }
                // Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
        //Interface minima con  escritura de parametros
        class OpcionesCargaFamiliasMin : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                //Sobreescribimos los parametros existentes, si ya esta leida
                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                //Devolvemos la Family recien cargada
                source = FamilySource.Family;
                //Sobreescribimos los parametros existentes, si ya esta leida
                overwriteParameterValues = true;
                return true;
            }
           
        }
        //Interface ampliada, con preguntas al usuario sobre la escritura de parametros
        public class OpcionesCargaFamiliasExt : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInuse, out bool overwriteParameters)
            {
                //Miramos si hay ejemplares. La familia esta leida
                string status = string.Empty;
                if (!familyInuse)
                {
                    status = "La familia existe. No hay ejemplares.";
                }
                else
                {
                    status = "La familia existe. Si hay ejemplares";
                }

                //Preguntamos al usuario
                TaskDialog mainDialog = new TaskDialog("API")
                {
                    Title = "API Revit Manual ",
                    TitleAutoPrefix = false,
                    MainInstruction = "Lectura de familias.",
                    MainContent = status + "\n.Seleccione un metodo de lectura:",
                    MainIcon = TaskDialogIcon.TaskDialogIconInformation
                };
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Abandonar lectura");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Sobreescribir geometría");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Sobreescribir geometría y parámetros");

                TaskDialogResult taskDialogResult = mainDialog.Show();
                //Abandonamos lectura
                if (taskDialogResult == TaskDialogResult.CommandLink1)
                {
                    overwriteParameters = false;
                    return false;
                }
                //Leemos pero no sobreescribimos parámetros
                else if (taskDialogResult == TaskDialogResult.CommandLink2)
                {
                    overwriteParameters = false;
                    return true;
                }
                //Leemos y sobreescribimos parámetros
                else if (taskDialogResult == TaskDialogResult.CommandLink3)
                {
                    overwriteParameters = true;
                    return true;
                }
                overwriteParameters = false;
                return false;

            }
            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInuse, out FamilySource source, out bool overwriteParameters)
            {
                //Miramos si hay ejemplares. La familia esta leida
                string status = string.Empty;
                if (!familyInuse)
                {
                    status = "La familia existe. No hay ejemplares.";
                }
                else
                {
                    status = "La familia existe. Si hay ejemplares";
                }

                //Preguntamos al usuario

                TaskDialog mainDialog = new TaskDialog("API")
                {
                    Title = "API Revit Manual ",
                    TitleAutoPrefix = false,
                    MainInstruction = "Lectura de familias compartidas. Familia: " + sharedFamily.Name,
                    MainContent = status + "\n.Seleccione un metodo de lectura:",
                    MainIcon = TaskDialogIcon.TaskDialogIconInformation
                };
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Abandonar lectura de familias compartida y padre.");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Abandonar lectura de familia compartida. Continuar con familia padre.");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Sobreescribir geometría de familia conpartida.");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Sobreescribir geometría y parámetros de familia conpartida.");

                TaskDialogResult taskDialogResult = mainDialog.Show();
                //AAbandonar lectura de familias compartida y padre
                if (taskDialogResult == TaskDialogResult.CommandLink1)
                {
                    overwriteParameters = false;
                    source = FamilySource.Project;
                    return false;
                }
                //Abandonar lectura de familia compartida. Continuar con familia padre.
                else if (taskDialogResult == TaskDialogResult.CommandLink2)
                {
                    overwriteParameters = false;
                    source = FamilySource.Project;
                    return true;
                }
                //Sobreescribir geometría de familia conpartida
                else if (taskDialogResult == TaskDialogResult.CommandLink3)
                {
                    overwriteParameters = false;
                    source = FamilySource.Family;
                    return true;
                }
                //Sobreescribir geometría y parámetros de familia conpartida
                else if (taskDialogResult == TaskDialogResult.CommandLink4)
                {
                    overwriteParameters = true;
                    source = FamilySource.Family;
                    return true;
                }
                //AAbandonar lectura de familias compartida y padre
                overwriteParameters = false;
                source = FamilySource.Project;
                return false;
            }
        }

    }
}
