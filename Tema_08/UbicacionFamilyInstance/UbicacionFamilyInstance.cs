#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace UbicacionFamilyInstance
{
    [Transaction(TransactionMode.Manual)]
    public class UbicacionFamilyInstance : IExternalCommand
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

            //Obtenemos todas las fases del proyecto
            PhaseArray phaseArray = doc.Phases;
            // Creamos tres listas de Room. Una familyInstance puede estar o lindar con una Room por fase.
            List<Room> listRoomFrom = new List<Room>();
            List<Room> listRoomTo = new List<Room>();
            List<Room> listRoom = new List<Room>();
            //Creamos una lista para XYZ
            IList<XYZ> puntosEnHabitaciones = new List<XYZ>();

            // Si cumple es Puerta o Ventana
            if (familyInstance.HasSpatialElementFromToCalculationPoints)
            {
                //   Obtenemos la lista de dos XYZ para calcular las Room
                puntosEnHabitaciones = familyInstance.GetSpatialElementFromToCalculationPoints();
            }
            //Si cumple es que tiene Punto de cálculo de habitación
            else if (familyInstance.HasSpatialElementCalculationPoint)
            {
                //   Obtenemos el XYZ de cáculo
                puntosEnHabitaciones.Add(familyInstance.GetSpatialElementCalculationPoint());
            }
            //Iteramos por cada Fase
            foreach (Phase phase in phaseArray)
            {

                try
                {
                    //Si la Room esta en la ultima Fase true, si no null
                    Room roomFa = familyInstance.FromRoom;
                    //Intentamos obtener la Room de esta Fase, si no existe genera error
                    Room roomFrom = familyInstance.get_FromRoom(phase);
                    //Si es posible la añado a la lista
                    if (roomFrom != null) listRoomFrom.Add(roomFrom);

                }
                catch { }
                try
                {
                    //Si la Room esta en la ultima Fase true, si no null
                    Room roomTa = familyInstance.ToRoom;
                    //Intentamos obtener la Room de esta Fase, si no existe genera error
                    Room roomTo = familyInstance.get_ToRoom(phase);
                    //Si es posible la añado a la lista
                    if (roomTo != null) listRoomTo.Add(roomTo);
                }
                catch { }

                try
                {
                    //Intentamos obtener la Room de esta Fase, si no existe genera error
                    Room room = familyInstance.get_Room(phase);
                    //Si es posible la añado a la lista
                    if (room != null) listRoom.Add(room);
                }
                catch { }
                //}
            }
            string listado = "FromRoom: " + string.Join(", ", listRoomFrom.Select(x => x.Name));
            listado = listado + "\n" + "ToRoom: " + string.Join(", ", listRoomTo.Select(x => x.Name));
            listado = listado + "\n" + "InRoom: " + string.Join(", ", listRoom.Select(x => x.Name));
            listado = listado + "\n";
            listado = listado + "\n" + "FamilyInstance con: " + puntosEnHabitaciones.Count + " puntos de cálculo";

            TaskDialog.Show("Manual Revit API", listado);

            return Result.Succeeded;
        }
    }
}