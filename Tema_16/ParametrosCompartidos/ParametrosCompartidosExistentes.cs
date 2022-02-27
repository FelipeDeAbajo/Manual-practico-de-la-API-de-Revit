#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

#endregion

namespace ParametrosCompartidosExistentes
{
    [Transaction(TransactionMode.Manual)]
    public class ParametrosCompartidosExistentes : IExternalCommand
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

            //Declaramos una ruta. Considere OpenFileDialog()
            string sharedParameterFile = @"C:\Users\felip\Documents\RevitAPIManual.txt";
            if (!File.Exists(sharedParameterFile))
            {
                //Si no exisre creamos el fichero
                System.IO.FileStream fileStream = System.IO.File.Create(sharedParameterFile);
                fileStream.Close();
            }
            // Establecemos ruta del archivo de parámetros compartidos en Revit
            app.SharedParametersFilename = sharedParameterFile;
            //Abrimos el fichero
            DefinitionFile definitionFile = app.OpenSharedParameterFile();
            #region Informacion
            StringBuilder fileInformation = new StringBuilder();

            //Obtenemos nonmbre fichero para mensaje de info
            fileInformation.AppendLine("Nombre archivo: " + definitionFile.Filename);

            //Iteramos en todos los DefinitionGroup en el fichero
            foreach (DefinitionGroup myGroupTemp in definitionFile.Groups)
            {
                //Obtenemos nombre del Group
                fileInformation.AppendLine("Nombre Grupo: " + myGroupTemp.Name);

                //Iteramos en cada Definition
                foreach (Definition definition in myGroupTemp.Definitions)
                {
                    //Obtenemos nombre de Definition
                    fileInformation.AppendLine("Definición parámetro: " + definition.Name);
                }
            }
            TaskDialog.Show("Revit API Manual", fileInformation.ToString());
            #endregion

            #region Modificar Definition
            //Obtenemos ExternalDefinition desde el fichero
            DefinitionGroups myGroups = definitionFile.Groups;
            //Obtenemos el Group "GrupoA"
            DefinitionGroup grupoA = myGroups.get_Item("GrupoA");
            //Obtenemos el GroupB
            DefinitionGroup grupoB = myGroups.get_Item("GrupoB");
            if (grupoA != null && grupoB != null)
            {
                //Obtenemos el "ParametroA"
                ExternalDefinition myExtDef = grupoA.Definitions.get_Item("ParametroA") as ExternalDefinition;
                if (myExtDef != null)
                {
                    //Cambiamos el parámetro del GrupoA al GrupoB y el valor de HideWhenNoValue
                    myExtDef.OwnerGroup = grupoB;
                    myExtDef.HideWhenNoValue = true;
                }
            }           
            #endregion
            return Result.Succeeded;
        }       
    }
}
