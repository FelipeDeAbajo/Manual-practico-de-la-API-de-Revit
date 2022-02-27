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

namespace ElementosEnLink
{
    [Transaction(TransactionMode.Manual)]
    public class ElementosEnLink : IExternalCommand
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

            //Definimos Reference
            Reference reference = null;

            //Nueva instancia de ISelectionFilter
            ISelectionFilter selectionFilterColumn = new ColumnStructSelectionFilter();

            try
            {
                //Seleccionamos pilar. imposible utilizar ISelectionFilter
                reference = uidoc.Selection.PickObject(ObjectType.LinkedElement, /*selectionFilterColumn,*/ "Seleccionar Pilar estructural.");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }

            //Obtenemos RevitLinkInstance desde Reference
            RevitLinkInstance revitLinkInstance = doc.GetElement(reference.ElementId) as RevitLinkInstance;

            //Obtenemos Document de Link.rvt
            Document documentLink = revitLinkInstance.GetLinkDocument();

            //Obtenemos Path de Link.rvt
            string pathLink = documentLink.PathName;
            string salida = "Archivo: " + pathLink + "\n";

            //Obtenemos el Pilar
            Element pilarLink = documentLink.GetElement(reference.LinkedElementId);

            salida += "ElemenId del pilar: " + pilarLink.Id.IntegerValue + "\n";

            //Obtenemos dimensiones (asumimos nombre "b" y "h"
            double sizeb = documentLink.GetElement(pilarLink.GetTypeId()).LookupParameter("b").AsDouble();
            double sizeh = documentLink.GetElement(pilarLink.GetTypeId()).LookupParameter("h").AsDouble();

            salida += "Dimensiones del pilar (pies): " + sizeb.ToString("N2") + " x " + sizeh.ToString("N2") + "\n";

            //Obtenemos Location en coordenadas de Link.rvt
            XYZ locationXYZ = (pilarLink.Location as LocationPoint).Point;

            salida += "Coordenadas del pilar (pies): " + locationXYZ.X.ToString("N2") + ", " + locationXYZ.Y.ToString("N2") + "\n";

            TaskDialog.Show("Revit API Manual", salida);

            return Result.Succeeded;
        }
    }
    public class ColumnStructSelectionFilter : ISelectionFilter
    {
        //Seccion Element
        public bool AllowElement(Element element)
        {
            // Solo continuamos si es FamilyInstance
            if (element is FamilyInstance column)
            {
                //Obtenemos categoría y comparamos

                //Solo admitimos pilar structural
                if (column.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns) return true;
            }
            return false;
        }
        // Seccion Reference
        public bool AllowReference(Reference refer, XYZ point)
        {
            //No filtramos ninguna Reference, siempre retornamos false
            return false;
        }

    }
}
