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

namespace DesplazarEnVista
{
    [Transaction(TransactionMode.Manual)]
    public class DesplazarEnVista : IExternalCommand
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

            //Accedemos a la selección
            Selection sel = uidoc.Selection;
            //Accedemos a la view actual
            View view = uidoc.ActiveView;

            Reference edgeRef = null;
            try
            {
                // PickObject siempre entre try{ç catch {}
                //Llamamos a ISelectionFilter solo Edge de RoofBase
                edgeRef = sel.PickObject(ObjectType.Edge, new RoofSelectionFilter(doc), "Seleccionar cubierta");

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;

            }
            //Obtenemos elemento desde Reference
            Element roof = doc.GetElement(edgeRef.ElementId);

            //Construimos coleccion con el seleccionado
            ICollection<ElementId> els = new List<ElementId>() { roof.Id };

            //Comprobamos que pueda desplazarse, en caso contrario salimos
            bool isDesplazable = DisplacementElement.CanElementsBeDisplaced(view, els);
            if (!isDesplazable)
            {
                message = "El objeto seleccionado no se puede desplazar";
                return Result.Cancelled;
            }

            //Creamos Transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Name");
                // 

                //Creamos desplazamiento 
                DisplacementElement dispElem = DisplacementElement.Create(doc, els, new XYZ(10, 0, 20), view, null);

                // Creamos el camino asociado al elemento
               //DisplacementPath.Create(doc, dispElem, edgeRef, 0);
                //Confirmamos Transaction
                tx.Commit();
            }


            return Result.Succeeded;
        }

    }


    public class RoofSelectionFilter : ISelectionFilter
    {
        Document document = null;
        public RoofSelectionFilter(Document document)
        {
            this.document = document;
        }
        public bool AllowElement(Element element)
        {
            if (element is RoofBase)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            Element element = document.GetElement(refer.ElementId);
            if (element is RoofBase) return true;

            return false;
        }
    }

}
