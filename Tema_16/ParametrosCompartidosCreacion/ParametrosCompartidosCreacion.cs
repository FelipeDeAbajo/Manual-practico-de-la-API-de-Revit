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
using System.Linq;
using System.Text;

#endregion

namespace ParametrosCompartidosCreacion
{
    [Transaction(TransactionMode.Manual)]
    public class ParametrosCompartidosCreacion : IExternalCommand
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

            //Nombres de parámetros
            string nParamEjemplar = "ParametroEjemplar";
            string nParamTipo = "ParametroTipo";

            //Filtro para ParameterElement
            IEnumerable<ParameterElement> parameters = new FilteredElementCollector(doc).OfClass(typeof(ParameterElement)).Cast<ParameterElement>();
       
            //Parámetros obtenidos
            ParameterElement parameterEjemplar= parameters.Where(e => e.Name.Equals(nParamEjemplar)).FirstOrDefault();
            ParameterElement parameterTipo = parameters.Where(e => e.Name.Equals(nParamTipo)).FirstOrDefault();

            if(parameterEjemplar !=null && parameterTipo !=null)
            {
                message = "Los parámetros ya existen. ";
               // TaskDialog.Show("Revit API Manual", message);
                return Result.Cancelled;
            }

            //Declaramos una ruta. Considere OpenFileDialog()
            string sharedParameterFile = @"C:\Users\felip\Documents\RevitAPIManual.txt";
            if (File.Exists(sharedParameterFile))
            {
                //Si existe borramos el fichero
                System.IO.File.Delete(sharedParameterFile);
            }
            System.IO.FileStream fileStream = System.IO.File.Create(sharedParameterFile);
            fileStream.Close();
            // Establecemos ruta del archivo de parámetros compartidos en Revit
            app.SharedParametersFilename = sharedParameterFile;
            //Abrimos el fichero
            DefinitionFile definitionFile = app.OpenSharedParameterFile();

            DefinitionGroup grupoC = definitionFile.Groups.Create("GrupoC");
            #region Asociación
            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Compartidos");

                // Creamos un CategorySet e insertamos la categoría Wall
                CategorySet myCategories = app.Create.NewCategorySet();
                // Usamos BuiltInCategory para obtener la categoría
                Category myCategory = Category.GetCategory(doc, BuiltInCategory.OST_Walls);
                //Insertamos la categoría
                myCategories.Insert(myCategory);

                // Obtenemos el BingdingMap del Document
                BindingMap bindingMap = doc.ParameterBindings;
               
                // Creamos ExternalDefinitionCreationOptions para parametro de ejemplar
#if V2022
                ExternalDefinitionCreationOptions option = new ExternalDefinitionCreationOptions(nParamEjemplar, SpecTypeId.String.Text);
#else
                ExternalDefinitionCreationOptions option = new ExternalDefinitionCreationOptions(nParamEjemplar, ParameterType.Text);

#endif
                //Establecemos que no sea modificable
                option.UserModifiable = false;
                //Establecemos que no sea visible
                option.Visible = false;
                //Creamos tooltip de información
                option.Description = "Wall parametro por Ejemplar";


                // Creamos Definition para parametro de ejemplar
                Definition definitionEjemplar = grupoC.Definitions.Create(option);
                #endregion


                //Creamos una instancia de InstanceBinding
                InstanceBinding instanceBinding = app.Create.NewInstanceBinding(myCategories);
                //  Parameter parameterInstance= doc.ParameterBindings.
                //Insertamos un nuevo vinculo para el document
                bool instanceBindOK = bindingMap.Insert(definitionEjemplar, instanceBinding, BuiltInParameterGroup.PG_TEXT);

                // Creamos ExternalDefinitionCreationOptions para parametro de tipo
#if V2022
                option = new ExternalDefinitionCreationOptions(nParamTipo, SpecTypeId.String.Text);
#else
                option = new ExternalDefinitionCreationOptions(nParamTipo, ParameterType.Text);

#endif
                //Establecemos que si sea modificable
                option.UserModifiable = true;

                //Creamos tooltip de información
                option.Description = "Wall parametro por Tipo";
                Definition definitionTipo = grupoC.Definitions.Create(option);

                //Creamos una instancia de TypeBinding
                TypeBinding typeBinding = app.Create.NewTypeBinding(myCategories);

                //Insertamos nuevos vinculo para el document
                bool typeBindOK = bindingMap.Insert(definitionTipo, typeBinding, BuiltInParameterGroup.PG_TEXT);
                //Confirmamos Transaction
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
