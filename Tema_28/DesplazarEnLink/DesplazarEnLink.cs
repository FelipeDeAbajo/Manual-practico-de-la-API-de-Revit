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

namespace DesplazarEnLink
{
    [Transaction(TransactionMode.Manual)]
    public class DesplazarEnLink : IExternalCommand
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
            Reference reference;

            try
            {
                //Seleccionamos un pilar estructural en Link
                reference = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Seleccionar Pilar");
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

            //Obtenemos la Transforn de la RevitLinkInstance
            Transform transform = revitLinkInstance.GetTotalTransform();

            //Creamos XYZ para se�alar
            XYZ xYZSe�alado = XYZ.Zero;

            try
            {
                //Obteneos el XYZ, forzar a intersecci�n. se�alamos en Master.rvt
                xYZSe�alado = uidoc.Selection.PickPoint(ObjectSnapTypes.Intersections, "Punto de inserci�n");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }

            //Transformamos las coordenadas de Master a las de Link
            XYZ xYZSe�aladoTranformado = transform.Inverse.OfPoint(xYZSe�alado);

            //Obtenemos Path de Link
            string pathLink = documentLink.PathName;

            //Obtenemos el Pilar estructural
            Element pilarLink = documentLink.GetElement(reference.LinkedElementId);

            //LocationPoint del Pilar estructural
            XYZ locationPoint = (pilarLink.Location as LocationPoint).Point;

            //Obtenemos el RevitLinkType
            RevitLinkType revitLinkType = doc.GetElement(revitLinkInstance.GetTypeId()) as RevitLinkType;

            //Creamos un Document nul
            Document documentLinkAbierto = null;

            //Obtenemos el ElementId. Obtener antes de Unload
            ElementId elementIdPilar = pilarLink.Id;

            //Descargamos Link. 
            revitLinkType.Unload(null);

            //Leeos el Document del Link
            documentLinkAbierto = uiapp.Application.OpenDocumentFile(pathLink);

            //Definimos Transaction en documentLinkAbierto
            using (Transaction tx = new Transaction(documentLinkAbierto))
            {
                //Iniciamos Transaction
                tx.Start("Transaction DesplazarEnLink");

                //Obtenemos vector
                XYZ vectorDesplazamiento = xYZSe�aladoTranformado - locationPoint;

                //Desplazamos pilar
                ElementTransformUtils.MoveElement(documentLinkAbierto, elementIdPilar, vectorDesplazamiento);

                //Confirmamos Transaction
                tx.Commit();
            }

            //Cerramos Link y salvamos
            documentLinkAbierto.Close(true);

            //Releemos RevitLinkType
            revitLinkType.Load();

            return Result.Succeeded;
        }
    }
}
