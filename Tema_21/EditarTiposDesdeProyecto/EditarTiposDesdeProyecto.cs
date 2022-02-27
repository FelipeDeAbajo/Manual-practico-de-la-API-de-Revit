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

namespace EditarTiposDesdeProyecto
{
    [Transaction(TransactionMode.Manual)]
    public class EditarTiposDesdeProyecto : IExternalCommand
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

           
            Selection sel = uidoc.Selection;

            //Debemos comprobar que tenemos un solo objeto seleccionado y que es el correcto. Pilar
            FamilyInstance familyInstance = doc.GetElement(sel.GetElementIds().FirstOrDefault()) as FamilyInstance;

            // Obtenemos la Family asociada a la FamilyInstance
            Family family = familyInstance.Symbol.Family;
            if (null == family)
            {
                message = "No se ha podido obtener la Family";
                return Result.Failed;    
            }

            // Accedemos al FamilyDocument
            Document familyDoc = doc.EditFamily(family);
            if (null == familyDoc)
            {
                message = "No se ha podido abrir la Family para editar";
                return Result.Failed;
            }

            //Accedemos al FamilyManager
            FamilyManager familyManager = familyDoc.FamilyManager;
            if (null == familyManager)
            {
                message = "No se ha podido acceder al Familymanager";
                return Result.Failed;
            }

            // Definimos Transaction en el ---FAMILYDOCUMENT---
            using (Transaction txFamily = new Transaction(familyDoc, "Añadir Tipo"))
            {
                //Iniciamos Transaction en el ---FAMILYDOCUMENT---
                txFamily.Start();

                //Añadimos nuevo tipo
                FamilyType newFamilyType = familyManager.NewType("2X2 Pies");

                if (newFamilyType != null)
                {
                    //Obtenemos el parametro 'b' y le asignamos 2 unidades internas                  
                    FamilyParameter familyParam = familyManager.get_Parameter("b");
                    if (null != familyParam)
                    {
                        familyManager.Set(familyParam, 2.0);
                    }

                }

                //Confirmamos Transaction en el ---FAMILYDOCUMENT---
                txFamily.Commit();

            }

            //Actualicamos el proyecto de Revit con la Family, que tiene un nuevo tipo
            LoadOptsMin loadOptions = new LoadOptsMin();

            //Necesatamos volver a cargar la familia editada
            family = familyDoc.LoadFamily(doc, loadOptions);

            //Cerramos el Document de Family
             familyDoc.Close(false);

            return Result.Succeeded;
        }
    }
    class LoadOptsMin : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }
}
