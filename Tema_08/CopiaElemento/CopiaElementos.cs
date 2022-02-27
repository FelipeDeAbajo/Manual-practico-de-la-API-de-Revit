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

namespace CopiaElemento
{
    [Transaction(TransactionMode.Manual)]
    public class CopiaElementos : IExternalCommand
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

            // Accedemos a la selección actual
            Selection sel = uidoc.Selection;

            // Creamos el vector de copia
            XYZ vector = new XYZ(10, 10, 10);

            // Creamos transaction
            using (Transaction tx = new Transaction(doc))
            {
                //Iniciamos Transaction
                tx.Start("Transaction Copiar");
                // Creamos una Colleccion para almacenar los nuevos Element
                ICollection<ElementId> elementosCopiados = new List<ElementId>();

                //Obtenemos el numero de objetos de la selección previa
                int numeroObjetos = sel.GetElementIds().Count;

                if (numeroObjetos == 0)
                {
                    message= "Se debe seleccionar algún objeto";
                    return  Result.Failed;
                }
                else if (numeroObjetos == 1)
                {
                    //Si se ha seleccionado 1
                    elementosCopiados = ElementTransformUtils.CopyElement(doc, sel.GetElementIds().First(), vector);
                }
                else
                {
                    //Si se han seleccionado varios
                    elementosCopiados = ElementTransformUtils.CopyElements(doc, sel.GetElementIds(), vector);
                }

                //Confirmamos Transaction
                tx.Commit(); 

                TaskDialog.Show("Manual Revit API", elementosCopiados.Count+" objetos copiados");

            }

            //Mensaje final
            return Result.Succeeded;
        }
    }
}
