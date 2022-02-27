#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;

#endregion

namespace PanelAcoplable
{
    class App : IExternalApplication
    {
        // class instance
        internal static App thisApp = null;

        //Instancias de UIApplication y del Document
        internal UIApplication uiapp = null;
        internal Document doc = null; 
        
        //Instancia del formulario del panel
        internal PanelDock myPanelDock = null;
     
      
        #region IExternalApplication Members
        public Result OnShutdown(UIControlledApplication application)
        {
            //Eliminamos registro de OnViewActivated
            try
            {
                application.ViewActivated -= new EventHandler<ViewActivatedEventArgs>(OnViewActivated);
            }
            catch (Exception)
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            thisApp = this;  // static access to this application instance

            try
            {
                //Registramos OnViewActivated
                application.ViewActivated += new EventHandler<ViewActivatedEventArgs>(OnViewActivated);
            }
            catch (Exception)
            {
                return Result.Failed;
            }
            #region Dock
            // A new handler to handle request posting by the dialog
            RequestHandler handler = new RequestHandler();

            // External Event for the dialog to use (to post requests)
            ExternalEvent exEvent = ExternalEvent.Create(handler);
            myPanelDock = new PanelDock(exEvent, handler);

            //Datos de posicionamiento del Panel
            DockablePaneProviderData data = new DockablePaneProviderData
            {
                FrameworkElement = myPanelDock as System.Windows.FrameworkElement,
                InitialState = new DockablePaneState
                {
                    DockPosition = DockPosition.Tabbed,//En una pestaña
                    TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser//Anclado con el Panel Propiedades
                }
            };

            //Registramos el Panel
            try
            {
                //Guid unico para toda la vida del plugin
                thisApp.RegisterDockableWindow(application, (new Guid("{77C963CE-B7CA-426A-8D51-6E8254D21199}")));
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Revit API Manual", ex.Message);
            }
            #endregion
            return Result.Succeeded;
        }
        #endregion

        #region Registro dock      
        public void RegisterDockableWindow(UIControlledApplication application, Guid mainPageGuid)
        {
            //Creamos el DockablePaneId con el GUID
            DockablePaneId sm_UserDockablePaneId = new DockablePaneId(mainPageGuid);

            //Registramos el Panel en la Application, Guid, nombre y formulario
            application.RegisterDockablePane(sm_UserDockablePaneId, "Mi Aplicacion Dock", myPanelDock);
        }
        #endregion

        #region eventos
        //Debemos controlar el cambio de Document al cambiar de ventana en Revit
        void OnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            //Actualizamos las instancias de UIApplication y del Document
            uiapp = sender as UIApplication;
            doc = e.Document;

        }
        #endregion
    }
}
