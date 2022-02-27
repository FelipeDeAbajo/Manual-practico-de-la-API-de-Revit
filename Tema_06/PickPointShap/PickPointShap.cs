#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace PickPointShap
{
    [Transaction(TransactionMode.Manual)]
    public class PickPointShap : IExternalCommand
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

            string xyzCoordenadas = string.Empty;
            XYZ puntoMarcado = null;
            string mensaje = "Seleciona un punto...";

            #region Sin mensaje. 
            //puntoMarcado = uidoc.Selection.PickPoint();
            //xyzCoordenadas = string.Format("Coordenada x: {0} \nCoordenada y: {1} \nCoordenada z: {2}",
            //    puntoMarcado.X, puntoMarcado.Y, puntoMarcado.Z);
            //TaskDialog.Show("Manual Revit API", xyzCoordenadas);
            #endregion

            #region Con mensaje. Sin forzar cursor
            //puntoMarcado = PuntoPorUsuario(uidoc, mensaje);
            //xyzCoordenadas = string.Format("Coordenada x: {0} \nCoordenada y: {1} \nCoordenada z: {2}",
            //    puntoMarcado.X, puntoMarcado.Y, puntoMarcado.Z);
            //TaskDialog.Show("Manual Revit API", xyzCoordenadas);
            #endregion

            #region Con mensaje. Forzando cursor. Gestionando cancelación por usuario
            puntoMarcado = PuntoPorUsuarioSnap(uidoc, mensaje);
            if (puntoMarcado == null)
            {
                message = "Operación cancelada. No se puede continuar";
                return Result.Cancelled;
            }
            xyzCoordenadas = string.Format("Coordenada x: {0} \nCoordenada y: {1} \nCoordenada z: {2}",
                puntoMarcado.X, puntoMarcado.Y, puntoMarcado.Z);
            TaskDialog.Show("Manual Revit API", xyzCoordenadas);
            #endregion

            return Result.Succeeded;
        }
        public XYZ PuntoPorUsuario(UIDocument uidoc, string mensaje)
        {
            return uidoc.Selection.PickPoint(mensaje);
        }
        public XYZ PuntoPorUsuarioSnap(UIDocument uidoc, string mensaje)
        {
            XYZ xYZ = null;
            ObjectSnapTypes objectSnapTypes = ObjectSnapTypes.Endpoints | ObjectSnapTypes.Midpoints;
            try
            {
                xYZ = uidoc.Selection.PickPoint(objectSnapTypes, mensaje);
            }
            catch
            {

            }
            return xYZ;
        }
    }
}
