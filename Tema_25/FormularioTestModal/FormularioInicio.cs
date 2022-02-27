using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormularioTestModal
{
    public partial class FormularioInicio : Form
    {
        //creamos los objetos accesibles a la clase
        Autodesk.Revit.DB.Document doc = null;
        Autodesk.Revit.UI.UIDocument uidoc = null;
        public FormularioInicio(Autodesk.Revit.UI.UIDocument uiDocument)
        {
            //Damos valor a los objetos
            uidoc = uiDocument;
            doc = uiDocument.Document;

            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Obtenemos el primer ElemenId
            Autodesk.Revit.DB.ElementId id = uidoc.Selection.GetElementIds().FirstOrDefault();
           
            //Si no seleccionamos ninguno
            if (id == null)
            {
                this.Close();
                return;
            }
            Autodesk.Revit.DB.Transaction tx = new Autodesk.Revit.DB.Transaction(doc);
            tx.Start("API");
            tx.Commit();
            //Obtenemos id y Name
            Autodesk.Revit.DB.Element element = doc.GetElement(id);
            Autodesk.Revit.UI.TaskDialog.Show("Revit API Manual", element.Name);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }
    }
}
