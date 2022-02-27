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

namespace CrearLink
{
    [Transaction(TransactionMode.Manual)]
    public class CrearLink : IExternalCommand
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

            //Obtenemos la carpeta actual del proyecto Master
            string folder = System.IO.Path.GetDirectoryName(doc.PathName);

            //Componemos path completo deLink
            string nameLink = folder + "\\Link.rvt";

            //Buscamos RevitLinkType coincidente
            FilteredElementCollector col = new FilteredElementCollector(doc);
            Element revitLinkType = col.OfClass(typeof(RevitLinkType)).Where(x => x.Name == "Link.rvt").FirstOrDefault();

            //Creamos ElementId para RevitLinkType
            ElementId revitLinkTypeId = ElementId.InvalidElementId;

            //Definimos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name CrearLink");

                //Si existe
                if (revitLinkType != null)
                {
                   TaskDialog.Show("Revit API Manual", "Ya está definido el RevitLinkType");
                    //Obtenemos ElementId
                    revitLinkTypeId = revitLinkType.Id;
                }
                else
                {
                    #region Crear RevitLinkType
                    //Construimos FilePath
                    FilePath path = new FilePath(nameLink);
                    //Ruta asoluta
                    RevitLinkOptions options = new RevitLinkOptions(false);
                    //Creamos el RevitLinkType
                    LinkLoadResult result = RevitLinkType.Create(doc, path, options);
                    //Obtenemos ElementId
                    revitLinkTypeId = result.ElementId;
                    #endregion
                }


                #region 
                //Creamos una RevitLinkInstance en el origen
                RevitLinkInstance instance1 = RevitLinkInstance.Create(doc, revitLinkTypeId);

                //Creamos otra RevitLinkInstance en el origen
                RevitLinkInstance instance2 = RevitLinkInstance.Create(doc, revitLinkTypeId);

                //Desplazamos la 2º RevitLinkInstance
                Location location = instance2.Location;
                location.Move(new XYZ(0, -100, 0));
                #endregion

                //Confirmamos Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
