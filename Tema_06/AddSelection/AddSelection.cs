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

namespace AddSelection
{
    [Transaction(TransactionMode.Manual)]
    public class AddSelection : IExternalCommand
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
            //Cramos una instancia de WallSelectionFilter, muros con h >5 m
            ISelectionFilter selectionFilter = new FilterClassAux.WallSelectionFilter();
            //Creamos una IList<Reference> que popularemos con la selección actual
            IList<Reference> referencesPre = new List<Reference>();
            // Obtenemos id seleccionados
            ICollection<ElementId> elementIds = uidoc.Selection.GetElementIds();
            // De los id anterioers a IList<Reference> MODO LARGO
            foreach (ElementId elementId in elementIds)
            {
                Element element = uidoc.Document.GetElement(elementId);
                Reference reference = Reference.ParseFromStableRepresentation(uidoc.Document, element.UniqueId);
                referencesPre.Add(reference);
            }

            // De los id anterioers a IList<Reference> MODO CORTO 1 LINEA
            //    referencesPre =  elementIds.Select(x => Reference.ParseFromStableRepresentation
            //    (uidoc.Document, uidoc.Document.GetElement(x).UniqueId)).ToList();

            //Nueva selección de objetos. Partimos de preselcción anterior
            IList<Reference> references = uidoc.Selection.PickObjects(ObjectType.Element,
                selectionFilter, "Seleccionar elementos", referencesPre);
            // Podemos eliminar la selacción actual
            elementIds.Clear();
            // Necesitamos iDs. MODO LARGO
            foreach (Reference referenceTotal in references)
            {
                elementIds.Add(referenceTotal.ElementId);
            }
            // Necesitamos iDs. MODO CORTO 1 LINEA
            //  elementIds = references.Select(X => X.ElementId).ToList();

            // Mostramos en pantalla la selección actual
            uidoc.ShowElements(elementIds);

            // Incorporamos los IDs a selección actual
           uidoc.Selection.SetElementIds(elementIds);
            return Result.Succeeded;
        }
    }
}
