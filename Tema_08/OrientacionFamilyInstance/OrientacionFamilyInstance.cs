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

namespace OrientacionFamilyInstance
{
    [Transaction(TransactionMode.Manual)]
    public class OrientacionFamilyInstance : IExternalCommand
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
            if (sel.GetElementIds().FirstOrDefault() == null)
            {
                message = "Se debe seleccionar una FamilyInstance";
                return Result.Failed;
            }
            //Tomamos una familyIstance. La primera
            FamilyInstance familyInstance = doc.GetElement(sel.GetElementIds().FirstOrDefault()) as FamilyInstance;

            // Si no es FamilyInstance Salimos
            if (familyInstance == null)
            {
                message = "Se debe seleccionar una FamilyInstance";
                return Result.Failed;
            }

            //Obtenemos sus dos vectores de orientación
            string listado = "HandOrientation: " + familyInstance.HandOrientation.X.ToString("N2") + " # " + familyInstance.HandOrientation.Y.ToString("N2");
            listado = listado + "\n" +
                "FacingOrientation: " + familyInstance.FacingOrientation.X.ToString("N2") + " # " + familyInstance.FacingOrientation.Y.ToString("N2");

            TaskDialog.Show("Manual Revit API", listado);

            //Creamos la Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Abrimos la Transaction
                tx.Start("Volteo");
                //Si es posible la rotación la efectuamos
                if (familyInstance.CanRotate)
                {
                    familyInstance.rotate();
                    //Obtenemos los nuevos vectores
                    listado = "HandOrientation: " + familyInstance.HandOrientation.X.ToString("N2") + ", " + familyInstance.HandOrientation.Y.ToString("N2");
                    listado = listado + "\n" +
                        "FacingOrientation: " + familyInstance.FacingOrientation.X.ToString("N2") + " # " + familyInstance.FacingOrientation.Y.ToString("N2");

                    TaskDialog.Show("Manual Revit API", listado);
                }
                //Confirmamos la Transaction
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
