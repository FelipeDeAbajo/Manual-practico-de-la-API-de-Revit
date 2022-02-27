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

            // Creamos un TaskDialog de Revit para comunicar información al usuario.
            TaskDialog mainDialog = new TaskDialog("Título: TaskDialog");

            //Alternar valores de TitleAutoPrefix (Optativo)
            mainDialog.TitleAutoPrefix = false;

            mainDialog.MainInstruction = "Información para mostrar";
            mainDialog.MainContent =
                "Este ejemplo muestra cómo utilizar un TaskDialog de Revit para comunicarse con el usuario."
                + "\nLos siguientes enlaces de comandos abren TaskDialog adicionales con más información." +
                "\n\nOK mostrara ambos mensajes";

            // Añadimos commmandLink. Podemos añadir hasta 4 Link (Optativo)
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                                      "Muestra información sobre la instalación de Revit.",
                                      "Linea opcional ampliando información.");
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                                      "Muestra información sobre el documento activo.",
                                      "Linea opcional ampliando información.");
            /* mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3,
                                      "No usado en este ejemplo");
             mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4,
                                      "No usado en este ejemplo");*/

            mainDialog.ExpandedContent = "Texto de ExpandedContent";

            // Añadimos un icono. (Optativo)
            mainDialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;

            // Botones comunes y botón por defecto.
            // Si no se añade ningún CommonButton o CommandLink, el TaskDialog mostrará el botón de Close por defecto
            mainDialog.CommonButtons = TaskDialogCommonButtons.Close | TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Ok;
            mainDialog.DefaultButton = TaskDialogResult.Close;

            // El texto a pie de página se utiliza normalmente para enlazar con el documento de ayuda..
            mainDialog.FooterText =
                "<a href=\"www.linkedin.com/in/felipe-de-abajo-alonso-51794919 \">"
                + "Click para ver perfil en Linkedin</a>";

            TaskDialogResult tResult = mainDialog.Show();

            // Si el usuario hace clic en el primer link, un TaskDialog con sólo un botón Close,
            // muestra información sobre la instalación de Revit. 
            if (TaskDialogResult.CommandLink1 == tResult)
            {
                MostrarInfoRevit(app);
            }

            // Si el usuario hace clic en el primer link, un TaskDialog
            // creado mediante un método static muestra información sobre el documento activo
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
                TaskDialog.Show("Resultado final", "No se ha pulsado ninguna opción controlada." +
                    "\nEl resultado final es: " + tResult.ToString());
            }
            return Result.Succeeded;
        }
        private void MostrarInfoRevit(Application app)
        {
            TaskDialog dialog_CommandLink1 = new TaskDialog("Instalación de Revit");
            dialog_CommandLink1.MainInstruction =
                "Versión Revit. Nombre: " + app.VersionName + "\n"
                + "Versión Revit. Número: " + app.VersionNumber + "\n"
                + "Versión Revit. Compilación: " + app.VersionBuild;

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
