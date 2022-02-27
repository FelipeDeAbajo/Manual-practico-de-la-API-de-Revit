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

namespace PickSelectBox
{
    [Transaction(TransactionMode.Manual)]
    public class PickSelectBox : IExternalCommand
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

            try
            {
                #region Construccion suelo. Se supone estar en ua vista de plano
                PickedBox pickedBox = uidoc.Selection.PickBox(PickBoxStyle.Directional, "Seleccionar esquinas para crear Suelo");

                // Los dos marcados por el usuario en pantalla
                XYZ first = pickedBox.Min;
                XYZ third = pickedBox.Max;

                // Construimos las otras dos esquinas
                XYZ second = new XYZ(third.X, first.Y, 0);
                XYZ fourth = new XYZ(first.X, third.Y, 0);

                CurveArray profile = new CurveArray();

                profile.Append(Line.CreateBound(first, second));
                profile.Append(Line.CreateBound(second, third));
                profile.Append(Line.CreateBound(third, fourth));
                profile.Append(Line.CreateBound(fourth, first));

                // La normal debe ser perpendicular a profile
                XYZ normal = XYZ.BasisZ;

                // Chequeamos estar en una vista de plano
                if (uidoc.ActiveView.ViewType != ViewType.FloorPlan)
                {
                    message = "Solo en vistas de plano";
                    return Result.Failed;
                }
                //Se supone estar en una vista de plano. Tomamos el Level
                Level level = doc.GetElement(uidoc.ActiveView.LevelId) as Level;

                // Obtenemos el tipo de suelo por defecto
                FloorType floorType = doc.GetElement(doc.GetDefaultElementTypeId(ElementTypeGroup.FloorType)) as FloorType;
                using (Transaction tx = new Transaction(doc, "Creación suelo"))
                {
                    tx.Start();
                    //Construimos el suelo
                    doc.Create.NewFloor(profile, floorType, level, true, normal);
                    tx.Commit();
                }
                #endregion

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
