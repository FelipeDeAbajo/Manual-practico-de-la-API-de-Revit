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

namespace OperacionesBasicas
{
    [Transaction(TransactionMode.Manual)]
    public class OperacionesBasicas : IExternalCommand
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

            // Accedemos con un objeto seleccionado
            Selection sel = uidoc.Selection;

            //Obtenemos el ElementId desde la selección
            ElementId id = sel.GetElementIds().FirstOrDefault();
          
            //Obtenemos el valor entero desde el ElementId
            TaskDialog.Show("Manual Revit API", "El valor entero del ElementId es: "+ id.IntegerValue.ToString());
  
            //Obtenemos el Element desde el ElemenId
            Element element = doc.GetElement(id);
   
            //Obtenemos el nombre del Element
            TaskDialog.Show("Manual Revit API", "El nombre del Element: " + element.Name);
   
            //Obtenemos el ElementId del tipo del Element
            ElementId idTipo = element.GetTypeId();
    
            //Obtenemos el Tipo desde su ElementId
            ElementType elementTipo = doc.GetElement(idTipo) as ElementType;
  
            //Obtenemos desde el Element Inicial el ElementId de su Nivel
            ElementId levelId = element.LevelId;

            //Obtenemos el Nivel
            Level level = doc.GetElement(levelId) as Level;

            //Obtenemos el nombre del Nuvel
            TaskDialog.Show("Manual Revit API", "El nombre del Level: " + level.Name);

            // Obtenemos el ElementId desde del Element inicial
            ElementId elementId = element.Id;

            // Obtenemos el UniqueId desde del Element inicial
            string uniqueId = element.UniqueId;

            // Obtenemos el VersionGuid desde del Element inicial
            Guid elementGuid = element.VersionGuid;

            TaskDialog.Show("Manual Revit API", "El ElementId es: " + elementId.IntegerValue + "\n"+
               "El UniqueId es: " +uniqueId+ "\n"+
               "El VersionGuid es: "+ elementGuid);


            return Result.Succeeded;
        }
    }
}
