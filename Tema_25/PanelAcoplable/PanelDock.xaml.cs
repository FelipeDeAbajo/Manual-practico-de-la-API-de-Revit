using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PanelAcoplable
{
    /// <summary>
    /// The class of our modeless dialog.
    /// </summary>
    /// <remarks>
    /// Besides other methods, it has one method per each command button.
    /// In each of those methods nothing else is done but raising an external
    /// event with a specific request set in the request handler.
    /// </remarks>
    public partial class PanelDock : Page, IDockablePaneProvider
    {
        #region dock Seccion añadida al SDK
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this as FrameworkElement;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Tabbed,
                TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
            };
        }
        #endregion

        // In this sample, the dialog owns the handler and the event objects,
        // but it is not a requirement. They may as well be static properties
        // of the application.

        private RequestHandler m_Handler;
        private ExternalEvent m_ExEvent;

        /// <summary>
        ///   Dialog instantiation
        /// </summary>
        /// 
        public PanelDock(ExternalEvent exEvent, RequestHandler handler)
        {
            InitializeComponent();
            m_Handler = handler;
            m_ExEvent = exEvent;
        }


        /// <summary>
        ///   Control enabler / disabler 
        /// </summary>
        ///
        private void EnableCommands(bool status)
        {
            if (status == false)
            {
                this.GridContenedor.IsEnabled = false;
            }
            else
            {
                this.GridContenedor.IsEnabled = true;
            }
        }

        /// <summary>
        ///   A private helper method to make a request
        ///   and put the dialog to sleep at the same time.
        /// </summary>
        /// <remarks>
        ///   It is expected that the process which executes the request 
        ///   (the Idling helper in this particular case) will also
        ///   wake the dialog up after finishing the execution.
        /// </remarks>
        ///
        private void MakeRequest(RequestId request)
        {
            m_Handler.Request.Make(request);
            m_ExEvent.Raise();
            DozeOff();
        }

        /// <summary>
        ///   DozeOff -> disable all controls (but the Exit button)
        /// </summary>
        /// 
        private void DozeOff()
        {
            EnableCommands(false);
        }

        /// <summary>
        ///   WakeUp -> enable all controls
        /// </summary>
        /// 
        public void WakeUp()
        {
            EnableCommands(true);
        }

        /// <summary>
        ///   Making a door Left
        /// </summary>
        /// 
        private void btnFlipLeft_Click_1(object sender, RoutedEventArgs e)
        {
            var miboton = sender as Button;
            MessageBox.Show(miboton.Name);
            MakeRequest(RequestId.MakeLeft);
        }

        /// <summary>
        ///   Making a door Right
        /// </summary>
        /// 
        private void btnFlipRight_Click_1(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.MakeRight);
        }

        /// <summary>
        ///   Flipping a door between Right and Left
        /// </summary>
        /// 
        private void btnFlipLeftRight_Click_1(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.FlipLeftRight);
        }

        /// <summary>
        ///   Flipping a door between facing In and Out
        /// </summary>
        /// 
        private void btnFlipInOut_Click_1(object sender, RoutedEventArgs e)
        {


            MakeRequest(RequestId.FlipInOut);

        }

        /// <summary>
        ///   Turning a door to face Out
        /// </summary>
        /// 
        private void btnFlipOut_Click_1(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.TurnOut);
        }

        /// <summary>
        ///   Turning a door to face In
        /// </summary>
        /// 
        private void btnFlipIn_Click_1(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.TurnIn);
        }

        /// <summary>
        ///   Turning a door around - flipping both hand and face
        /// </summary>
        /// 
        private void btnRotate_Click_1(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.Rotate);
        }

        /// <summary>
        ///   Deleting a door
        /// </summary>
        /// 
        private void btnDelete_Click_1(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.Delete);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Filtramos todas las puertas
            FilteredElementCollector col = new FilteredElementCollector(App.thisApp.doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_Doors);
          
            //Mostramos el numero de puertas antes de borrar
            TaskDialog.Show("Revit API Manual", "Antes de borrar: " + col.ToElements().Count.ToString());
          
            //Borramos puerta
            MakeRequest(RequestId.Delete);

            //Filtramos todas las puertas
            col = new FilteredElementCollector(App.thisApp.doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_Doors);

            //Mostramos el numero de puertas despues de borrar
            TaskDialog.Show("Revit API Manual", "Despues de borrar: " + col.ToElements().Count.ToString());
        }

    }
}
