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

namespace AbrirProyectoCompartido
{
    [Transaction(TransactionMode.Manual)]
    public class AbrirProyectoCompartido : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
           //Document doc = uidoc.Document;

            //Creamos un formulario de lectura de ficheros
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            //Filtramos tipo de archivos para leer
            openFileDialog.Filter = "Proyectos de Revit (*.rvt)|*.rvt";

            //Mostramos el formulario
            System.Windows.Forms.DialogResult dialogResult = openFileDialog.ShowDialog();
            //Si cancelamos o cerramos sin seleccionar archivo válido
            if (dialogResult != System.Windows.Forms.DialogResult.OK)
            {
                message = "Seleccion cancelada";
                return Result.Cancelled;

            }
            //Creamos y asignamos el nombre del fichero
            string nombreFichero = openFileDialog.FileName;
            if (!System.IO.File.Exists(nombreFichero))
            {
                message = "El archivo seleccionado no existe";
                return Result.Failed;
            }

            //Obtenemos el ModelPath del archivo seleccionado
            ModelPath projectPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(nombreFichero);
            try
            {
                //Obtenemos todos los subproyectos de usuario
                IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(projectPath);

                //Creamos una lita vacia...
                IList<WorksetId> worksetPropiosIds = new List<WorksetId>();

                //Nuevas opciones de apertura
                OpenOptions openOptions = new OpenOptions();

                //Creamos nueva WorksetConfiguration y cerramos todos los subproyectos
                WorksetConfiguration openConfig = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);

                //Creamos un TaskDialog con cuatro opciones
                TaskDialog mainDialog = new TaskDialog("Revit API Manual");
                mainDialog.TitleAutoPrefix = false;
                mainDialog.MainInstruction = "Seleccione modo de apertura:";
                // Añadimos cuatro commmandLink. 
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Ultimo subproyecto.");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Los subproyectos propios.");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "No crear copia. Abrir desenlazado de archivo central.");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Si crear copia. Crear nuevo archivo local.");

                mainDialog.CommonButtons = TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Ok;
                mainDialog.DefaultButton = TaskDialogResult.Cancel;

                TaskDialogResult tResult = mainDialog.Show();

                //Abrimos el ultimo subproyecto
                if (TaskDialogResult.CommandLink1 == tResult)
                {
                    openConfig = new WorksetConfiguration(WorksetConfigurationOption.OpenLastViewed);
                    //Establecemos en el OpenOptions la WorksetConfiguration
                    openOptions.SetOpenWorksetsConfiguration(openConfig);
                }

                //Abrimos los subproyectos del usuario actual 
                else if (TaskDialogResult.CommandLink2 == tResult)
                {
                    //Recorremos subproyectos y buscamos del usuario actual
                    foreach (WorksetPreview worksetPrev in worksets)
                    {
                        if (worksetPrev.Owner == app.Username)
                        {
                            //Incluimos WorksetId
                            worksetPropiosIds.Add(worksetPrev.Id);
                        }
                    }
                    //Añadimos al WorksetConfiguration la lista de WorksetId
                    openConfig.Open(worksetPropiosIds);
                    //Establecemos en el OpenOptions la WorksetConfiguration
                    openOptions.SetOpenWorksetsConfiguration(openConfig);

                }
                //Abrimos desenlazado
                else if (TaskDialogResult.CommandLink3 == tResult)
                {
                    //Desenlacamos de central. OpenOptions vacias
                    openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets;

                }
                //Creamos nueva copia local
                else if (TaskDialogResult.CommandLink4 == tResult)
                {
                    //Creamos un nuevo nombre y lo convertimos en ModelPath
                    string newName = System.IO.Path.GetDirectoryName(nombreFichero) + "\\copia.rvt";
                    ModelPath projectPathCopia = ModelPathUtils.ConvertUserVisiblePathToModelPath(newName);
                  
                    //Creamos nuevo archivo local
                    WorksharingUtils.CreateNewLocal(projectPath, projectPathCopia);

                    //Establecemos en el OpenOptions la WorksetConfiguration
                    //en este caso vacía. Podemos establecer los criterios anteriores
                    openOptions.SetOpenWorksetsConfiguration(openConfig);

                    //Establecemos fichero de apertura
                    projectPath = projectPathCopia;
                }
                else
                {
                    message = "Operación cancelada";
                    return Result.Cancelled;
                }
                //Abrimos y activamos el Document
                UIDocument newUIdoc = uiapp.OpenAndActivateDocument(projectPath, openOptions, false);
            }
            catch (Exception e)
            {
                TaskDialog.Show("Revit API Manual", "Imposible abrir archivo. " + e.Message);
            }

            return Result.Succeeded;
        }

    }
}
