#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace CopiarAVista
{
    [Transaction(TransactionMode.Manual)]
    public class CopiarAVista : IExternalCommand
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

            // Creamos una Colleccion para almacenar los nuevos Element
            ICollection<ElementId> elementosCopiados = new List<ElementId>();

            //Obtenemos el numero de objetos de la selección previa
            int numeroObjetos = sel.GetElementIds().Count;

            if (numeroObjetos == 0)
            {
                message = "Se deben seleccionar más objetos";
                return Result.Cancelled;
            }
            else
            {
                //Creamos variable para destino
                Document documentDestino = null;
                //Obtenemos todo los Document abiertos
                IEnumerator enumerator = doc.Application.Documents.GetEnumerator();
                //Reseteamos el Ebumerador
                enumerator.Reset();
                //Mientras existan Document iteramos
                while (enumerator.MoveNext())
                {
                    //Obtenemos Document y chequemos si es viable como destino
                    documentDestino = enumerator.Current as Document;
                    if (documentDestino.IsFamilyDocument) continue; //Si es Family
                   // if (doc.Equals(documentDestino)) continue; //So es el origen
                    if (documentDestino.IsReadOnly) continue;// Si es de solo lectura
                    if (documentDestino.IsLinked) continue;// Si es un archivo vinculado

                    //Creamos un colector para cada Document
                    FilteredElementCollector col = new FilteredElementCollector(documentDestino).OfClass(typeof(ViewPlan));
                    //Creamos una colección con las ViewPlant que son FloorPlan y no son IsTemplate
                    ICollection<ViewPlan> viewPlans = col.Cast<ViewPlan>().ToList<ViewPlan>()
                        .Where(x => (!x.IsTemplate && x.ViewType == ViewType.FloorPlan)).ToList();

                    //Creamos Transaction para cada Document
                    using (Transaction tx = new Transaction(documentDestino))
                    {
                        //Iniciamos Transaction
                        tx.Start("Copia a destino");
                        // Creamos una nueva CopyPasteOptions
                        CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
                        copyPasteOptions.SetDuplicateTypeNamesHandler(new CopiaTiposDuplicados());
                        //Creamos una Transform de igualdad. 
                        Transform transform = Transform.Identity;
                        //Creamos los objetos en el Document destino
                        foreach (ViewPlan viewPlan in viewPlans)
                        {
                            //Si  la vista origen y destino es la misma saltamos a siguiente
                            if (doc.ActiveView.Id == viewPlan.Id && doc.Equals(documentDestino)) continue;
                            //Copiamos los Element
                            elementosCopiados = ElementTransformUtils.CopyElements(doc.ActiveView, sel.GetElementIds(), viewPlan, transform, copyPasteOptions);
                        }
                        //Confirmamos Transaction
                        tx.Commit();
                    }
                    TaskDialog.Show("Manual Revit API", elementosCopiados.Count + " objetos copiados en :" + documentDestino.Title);
                }
            }

            return Result.Succeeded;
        }

    }

    public class CopiaTiposDuplicados : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
  
}
