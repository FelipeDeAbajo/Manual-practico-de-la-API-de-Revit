#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

#endregion

namespace PublicarFallo
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            //Nueva instancia del DMU
            WallWarnUpdater wallUpdater = new WallWarnUpdater(a.ActiveAddInId);
            //Registramos el DMU
            UpdaterRegistry.RegisterUpdater(wallUpdater);
            //Establecemos filtro de clase para el DMU. Muros
            ElementClassFilter filter = new ElementClassFilter(typeof(Wall));
            //Establecemos dos desencadenadores del DMU
            UpdaterRegistry.AddTrigger(wallUpdater.GetUpdaterId(), filter, Element.GetChangeTypeGeometry());

            UpdaterRegistry.AddTrigger(wallUpdater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());

            //Definir una nueva identificación de falla para una advertencia sobre muros
            FailureDefinitionId warnId = new FailureDefinitionId(new Guid("bB4F5AF3-42BB-5571-B559-FB1648D5B4D1"));

            //Registro de la nueva advertencia usando FailureDefinition
            FailureDefinition failDefAdv = FailureDefinition.CreateFailureDefinition(warnId, FailureSeverity.Warning, "El muro es demasiado alto (>20 pies). Producción dificil.");

            //Definir una nueva identificación de falla para un error sobre muros
            FailureDefinitionId failId = new FailureDefinitionId(new Guid("b91E5825-93DC-4f5c-9290-8072A4B631BC"));

            //Registro del nuevo error usando FailureDefinition
            FailureDefinition failDefError = FailureDefinition.CreateFailureDefinition(failId, FailureSeverity.Error, "El muro es demasiado alto (>30 pies). Producción imposible.");
 
            //Guardamos ids para referencia posterior
            wallUpdater.WarnId = warnId;
            wallUpdater.FailureId = failId;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            //Eliminamos registro del DMU
            UpdaterRegistry.UnregisterUpdater(WallWarnUpdater.m_updaterId);

            return Result.Succeeded;
        }
    }
    public class WallWarnUpdater : IUpdater
    {
        static AddInId m_appId;
        internal static UpdaterId m_updaterId;
        FailureDefinitionId m_failureId = null;
        FailureDefinitionId m_warnId = null;

        //Constructor. AddInId del plugin asociado con este Updater
        public WallWarnUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("554df89c-8905-4317-b73c-33bb09c918e4"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;

            //Iteramos para da Wall creado
            foreach (ElementId id in data.GetAddedElementIds())
            {
                //Obtenemos el muro
                Wall wall = doc.GetElement(id) as Wall;

                //Obtenemos el parámetro Altura desconectada
                Parameter p = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (p != null)
                {
                    if (p.AsDouble() > 30)//Unidaedes internas
                    {
                        FailureMessage failMessage = new FailureMessage(FailureId);
                        failMessage.SetFailingElement(id);
                        doc.PostFailure(failMessage);
                    }
                    else if (p.AsDouble() > 20) //Unidaedes internas
                    {
                        FailureMessage failMessage = new FailureMessage(WarnId);
                        failMessage.SetFailingElement(id);
                        doc.PostFailure(failMessage);
                    }
                }
            }
            //Iteramos para cada Wall modificado
            foreach (ElementId id in data.GetModifiedElementIds())
            {
                //Obtenemos el muro
                Wall wall = doc.GetElement(id) as Wall;

                //Obtenemos el parámetro Altura desconectada
                Parameter p = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (p != null)
                {
                    if (p.AsDouble() > 30)//Unidaedes internas
                    {
                        FailureMessage failMessage = new FailureMessage(FailureId);
                        failMessage.SetFailingElement(id);
                        doc.PostFailure(failMessage);
                    }
                    else if (p.AsDouble() > 20) //Unidaedes internas
                    {
                        FailureMessage failMessage = new FailureMessage(WarnId);
                        failMessage.SetFailingElement(id);
                        doc.PostFailure(failMessage);
                    }
                }
            }
        }

        //Propiedad FailureId
        public FailureDefinitionId FailureId
        {
            get { return m_failureId; }
            set { m_failureId = value; }
        }

        //Propiedad  WarnId
        public FailureDefinitionId WarnId
        {
            get { return m_warnId; }
            set { m_warnId = value; }
        }

        public string GetAdditionalInformation()
        {
            return "Desencadena advertencia si un muro es demasiado alto";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "Comprobación de la altura de muro";
        }
    }

}
