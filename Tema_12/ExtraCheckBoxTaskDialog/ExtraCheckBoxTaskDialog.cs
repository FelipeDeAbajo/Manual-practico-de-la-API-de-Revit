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

namespace ExtraCheckBoxTaskDialog
{
    [Transaction(TransactionMode.Manual)]
    public class ExtraCheckBoxTaskDialog : IExternalCommand
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

            // Cramos un TaskDialog de Revit para comunicar información al usuario.
            TaskDialog mainDialog = new TaskDialog("Título: TaskDialog");

            //Alternar valores de TitleAutoPrefix (Optativo)
            mainDialog.TitleAutoPrefix = false;

            mainDialog.MainInstruction = "Extra CheckBox";
            mainDialog.MainContent =
                "Este ejemplo muestra como usar el ExtraCheckBox." +
                "\nDebe seleccionar el CheckBox y pulsar Ok para mostrar siguiene acción";

            mainDialog.ExpandedContent = "Texto de ExpandedContent";

            mainDialog.ExtraCheckBoxText = "¿Realmente desea ejecutar la acción?";

            // Añadimos un icono. (Optativo)
            mainDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
            // Botones comunes y botón por defecto.
            // Si no se añade ningún CommonButton o CommandLink, el TaskDialog mostrará el botón de Close por defecto
            mainDialog.CommonButtons = TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Ok;
            mainDialog.DefaultButton = TaskDialogResult.Cancel;

            // El texto a pie de página se utiliza normalmente para enlazar con el documento de ayuda..
            mainDialog.FooterText =
                "<a href=\"www.linkedin.com/in/felipe-de-abajo-alonso-51794919 \">"
                + "Click para ver perfil en Linkedin</a>";

            TaskDialogResult tResult = mainDialog.Show();

            if (TaskDialogResult.Ok == tResult && mainDialog.WasExtraCheckBoxChecked() == true)
            {
                TaskDialog.Show("ExtraCheckBox", "La acción continúa");
            }
            return Result.Succeeded;
        }

    }
}
