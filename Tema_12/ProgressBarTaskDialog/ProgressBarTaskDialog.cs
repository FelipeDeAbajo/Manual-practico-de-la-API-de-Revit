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

namespace ProgressBarTaskDialog
{
    [Transaction(TransactionMode.Manual)]
    public class ProgressBarTaskDialog : IExternalCommand
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

            // Cramos un TaskDialog de Revit para comunicar informaci�n al usuario.
            TaskDialog mainDialog = new TaskDialog("T�tulo: TaskDialog");

            mainDialog.TitleAutoPrefix = false;

            mainDialog.MainInstruction = "Mostrar Barra de progreso";
            mainDialog.MainContent =
                "Este ejemplo muestra c�mo utilizar un TaskDialog de mostrando una \"Barra de Progreso\".";


            // mainDialog.ExtraCheckBoxText = "CheckBox extra. Incompatible con Barra de Progreso";

            mainDialog.ExpandedContent = "Texto de ExpandedContent";

            // A�adimos un icono. (Optativo)
            mainDialog.MainIcon = TaskDialogIcon.TaskDialogIconShield;
            // Botones comunes y bot�n por defecto.
            // Si no se a�ade ning�n CommonButton o CommandLink, el TaskDialog mostrar� el bot�n de Close por defecto
            mainDialog.CommonButtons = TaskDialogCommonButtons.Close | TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Ok;
            mainDialog.DefaultButton = TaskDialogResult.Close;

            mainDialog.EnableMarqueeProgressBar = true;

            // El texto a pie de p�gina se utiliza normalmente para enlazar con el documento de ayuda..
            mainDialog.FooterText =
                "<a href=\"www.linkedin.com/in/felipe-de-abajo-alonso-51794919 \">"
                + "Click para ver perfil en Linkedin</a>";

            TaskDialogResult tResult = mainDialog.Show();


            return Result.Succeeded;
        }
    }
}
