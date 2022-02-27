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

namespace EncadenarTaskDialog
{
    [Transaction(TransactionMode.Manual)]
    public class EncadenarTaskDialog : IExternalCommand
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

            // Creamos un TaskDialog de Revit para comunicar informaci�n al usuario.
            TaskDialog mainDialog = new TaskDialog("T�tulo: TaskDialog");

            //Alternar valores de TitleAutoPrefix (Optativo)
            mainDialog.TitleAutoPrefix = false;

            mainDialog.MainInstruction = "Informaci�n para mostrar";
            mainDialog.MainContent =
                "Este ejemplo muestra c�mo utilizar un TaskDialog de Revit para comunicarse con el usuario."
                + "\nLos siguientes enlaces de comandos abren TaskDialog adicionales con m�s informaci�n." +
                "\n\nOK mostrara ambos mensajes";

            // A�adimos commmandLink. Podemos a�adir hasta 4 Link (Optativo)
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                                      "Muestra informaci�n sobre la instalaci�n de Revit.",
                                      "Linea opcional ampliando informaci�n.");
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                                      "Muestra informaci�n sobre el documento activo.",
                                      "Linea opcional ampliando informaci�n.");
            /* mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3,
                                      "No usado en este ejemplo");
             mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4,
                                      "No usado en este ejemplo");*/

            mainDialog.ExpandedContent = "Texto de ExpandedContent";

            // A�adimos un icono. (Optativo)
            mainDialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;

            // Botones comunes y bot�n por defecto.
            // Si no se a�ade ning�n CommonButton o CommandLink, el TaskDialog mostrar� el bot�n de Close por defecto
            mainDialog.CommonButtons = TaskDialogCommonButtons.Close | TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Ok;
            mainDialog.DefaultButton = TaskDialogResult.Close;

            // El texto a pie de p�gina se utiliza normalmente para enlazar con el documento de ayuda..
            mainDialog.FooterText =
                "<a href=\"www.linkedin.com/in/felipe-de-abajo-alonso-51794919 \">"
                + "Click para ver perfil en Linkedin</a>";

            TaskDialogResult tResult = mainDialog.Show();

            // Si el usuario hace clic en el primer link, un TaskDialog con s�lo un bot�n Close,
            // muestra informaci�n sobre la instalaci�n de Revit. 
            if (TaskDialogResult.CommandLink1 == tResult)
            {
                MostrarInfoRevit(app);
            }

            // Si el usuario hace clic en el primer link, un TaskDialog
            // creado mediante un m�todo static muestra informaci�n sobre el documento activo
            else if (TaskDialogResult.CommandLink2 == tResult)
            {
                MostrarInfoDoc(doc);
            }
            // Si el usuario hace clic en OK, se muestran los dos Taskdialog anteriores

            else if (TaskDialogResult.Ok == tResult)
            {
                MostrarInfoRevit(app);
                MostrarInfoDoc(doc);
            }
            else
            {
                TaskDialog.Show("Resultado final", "No se ha pulsado ninguna opci�n controlada." +
                    "\nEl resultado final es: " + tResult.ToString());
            }
            return Result.Succeeded;
        }
        private void MostrarInfoRevit(Application app)
        {
            TaskDialog dialog_CommandLink1 = new TaskDialog("Instalaci�n de Revit");
            dialog_CommandLink1.MainInstruction =
                "Versi�n Revit. Nombre: " + app.VersionName + "\n"
                + "Versi�n Revit. N�mero: " + app.VersionNumber + "\n"
                + "Versi�n Revit. Compilaci�n: " + app.VersionBuild;

            dialog_CommandLink1.Show();
        }
        private void MostrarInfoDoc(Document doc)
        {
            TaskDialog.Show("Documento activo",
                    "Documento activo: " + doc.Title + "\n"
                    + "Nombre de vista activa: " + doc.ActiveView.Name);
        }
    }
}
