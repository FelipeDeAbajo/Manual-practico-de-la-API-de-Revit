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

namespace ObtenerFamilyType
{
    [Transaction(TransactionMode.Manual)]
    public class ObtenerFamilyType : IExternalCommand
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

            // Access current selection

            Selection sel = uidoc.Selection;

            //Si no es FamilyDocument
            if (!doc.IsFamilyDocument)
            {
                message = "No es documento FamilyDocument";
                return  Result.Failed;
            }

            //Si no existe OwnerFamily
            if (doc.OwnerFamily == null)
            {
                message = "No es existe OwnerFamily";
                return Result.Failed;
            }

            //Obtenemos el FamilyManager
            FamilyManager familyManager = doc.FamilyManager;

            // Obtenemos los tipos
            string types = "Tipos de la Family: ";
            FamilyTypeSet familyTypes = familyManager.Types;

            //Iteramos entre los tipos
            FamilyTypeSetIterator familyTypesItor = familyTypes.ForwardIterator();
            familyTypesItor.Reset();
            while (familyTypesItor.MoveNext())
            {
                FamilyType familyType = familyTypesItor.Current as FamilyType;
                //Obtenemos nombre
                types += "\n" + familyType.Name;
            }

            TaskDialog.Show("Revit API Manual", types);

            return Result.Succeeded;
        }
    }
}
