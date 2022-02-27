//
// (C) Copyright 2003-2019 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Events;

namespace Ribbon
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication,   
    /// show user how to create RibbonItems by API in Revit.
    /// we add one RibbonPanel:
    /// 1. contains a SplitButton for user to create Non-Structural or Structural Wall;
    /// 2. contains a StackedButton which is consisted with one PushButton and two Comboboxes, 
    /// PushButton is used to reset all the RibbonItem, Comboboxes are use to select Level and WallShape
    /// 3. contains a RadioButtonGroup for user to select WallType.
    /// 4. Adds a Slide-Out Panel to existing panel with following functionalities:
    /// 5. a text box is added to set mark for new wall, mark is a instance parameter for wall, 
    /// Eg: if user set text as "wall", then Mark for each new wall will be "wall1", "wall2", "wall3"....
    /// 6. a StackedButton which consisted of a PushButton (delete all the walls) and a PulldownButton (move all the walls in X or Y direction)
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class Ribbon : IExternalApplication
    {
        // ExternalCommands assembly path
        static string AddInPath = typeof(Ribbon).Assembly.Location;
        // Button icons directory
        static string ButtonIconsFolder = Path.GetDirectoryName(AddInPath);
        // uiApplication
        static UIApplication uiApplication = null;

        #region IExternalApplication Members
        /// <summary>
        /// Implement this method to implement the external application which should be called when 
        /// Revit starts before a file or default template is actually loaded.
        /// </summary>
        /// <param name="application">An object that is passed to the external application 
        /// which contains the controlled application.</param>
        /// <returns>Return the status of the external application. 
        /// A result of Succeeded means that the external application successfully started. 
        /// Cancelled can be used to signify that the user cancelled the external operation at 
        /// some point.
        /// If Failed is returned then Revit should inform the user that the external application 
        /// failed to load and the release the internal reference.</returns>
        public Autodesk.Revit.UI.Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // create customer Ribbon Items
                CreateRibbonSamplePanel(application);

                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ribbon Sample", ex.ToString());

                return Autodesk.Revit.UI.Result.Failed;
            }
        }

        /// <summary>
        /// Implement this method to implement the external application which should be called when 
        /// Revit is about to exit, Any documents must have been closed before this method is called.
        /// </summary>
        /// <param name="application">An object that is passed to the external application 
        /// which contains the controlled application.</param>
        /// <returns>Return the status of the external application. 
        /// A result of Succeeded means that the external application successfully shutdown. 
        /// Cancelled can be used to signify that the user cancelled the external operation at 
        /// some point.
        /// If Failed is returned then the Revit user should be warned of the failure of the external 
        /// application to shut down correctly.</returns>
        public Autodesk.Revit.UI.Result OnShutdown(UIControlledApplication application)
        {
            //remove events
#if NewTab
            List<RibbonPanel> myPanels = application.GetRibbonPanels("Revit API Manual");
#else
            List<RibbonPanel> myPanels = application.GetRibbonPanels();
#endif
            Autodesk.Revit.UI.ComboBox comboboxLevel = (Autodesk.Revit.UI.ComboBox)(myPanels.Where(x => x.Name == "Ribbon Sample").First().GetItems()[2]);
            application.ControlledApplication.DocumentCreated -= new EventHandler<
               Autodesk.Revit.DB.Events.DocumentCreatedEventArgs>(DocumentCreated);
            Autodesk.Revit.UI.TextBox textBox = myPanels.Where(x => x.Name == "Ribbon Sample").First().GetItems()[5] as Autodesk.Revit.UI.TextBox;
            textBox.EnterPressed -= new EventHandler<
               Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs>(SetTextBoxValue);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
        #endregion

        /// <summary>
        /// This method is used to create RibbonSample panel, and add wall related command buttons to it:
        /// 1. contains a SplitButton for user to create Non-Structural or Structural Wall;
        /// 2. contains a StackedBotton which is consisted with one PushButton and two Comboboxes, 
        /// PushButon is used to reset all the RibbonItem, Comboboxes are use to select Level and WallShape
        /// 3. contains a RadioButtonGroup for user to select WallType.
        /// 4. Adds a Slide-Out Panel to existing panel with following functionalities:
        /// 5. a text box is added to set mark for new wall, mark is a instance parameter for wall, 
        /// Eg: if user set text as "wall", then Mark for each new wall will be "wall1", "wall2", "wall3"....
        /// 6. a StackedButton which consisted of a PushButton (delete all the walls) and a PulldownButton (move all the walls in X or Y direction)
        /// </summary>
        /// <param name="application">An object that is passed to the external application 
        /// which contains the controlled application.</param>
        private void CreateRibbonSamplePanel(UIControlledApplication application)
        {
            // create a Ribbon panel which contains three stackable buttons and one single push button. 
            string firstPanelName = "Ribbon Sample";
#if NewTab
            // Create a custom ribbon tab 
            application.CreateRibbonTab("Revit API Manual");
            RibbonPanel ribbonSamplePanel = application.CreateRibbonPanel("Revit API Manual", firstPanelName);        
#else
            // create a Ribbon panel which contains three stackable buttons and one single push button.
            RibbonPanel ribbonSamplePanel = application.CreateRibbonPanel(firstPanelName);
#endif

            #region Create a SplitButton for user to create Non-Structural or Structural Wall
            SplitButtonData splitButtonData = new SplitButtonData("NewWallSplit", "Create Wall");
            SplitButton splitButton = ribbonSamplePanel.AddItem(splitButtonData) as SplitButton;
            PushButton pushButton = splitButton.AddPushButton(new PushButtonData("WallPush", "Wall", AddInPath, "Ribbon.CreateWall"));
            pushButton.LargeImage = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.CreateWall.png");
            pushButton.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.CreateWall-S.png");
            pushButton.ToolTip = "Creates a partition wall in the building model.";

            #region Availability
            pushButton.AvailabilityClassName = "Ribbon.CreateWall";
            #endregion

            #region Ayuda y descripción larga
            pushButton.LongDescription = "Esta es la descripción larga.";
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://help.autodesk.com/view/RVT/2022/ESP/?guid=Revit_API_Revit_API_Developers_Guide_Introduction_Add_In_Integration_Ribbon_Panels_and_Controls_html");
            pushButton.SetContextualHelp(contextHelp);
            #endregion

            pushButton.ToolTipImage = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.CreateWallTooltip.bmp"); 
            pushButton = splitButton.AddPushButton(new PushButtonData("StrWallPush", "Structure Wall", AddInPath, "Ribbon.CreateStructureWall"));
            pushButton.LargeImage = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.StrcturalWall.png"); 
            pushButton.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.StrcturalWall-S.png"); 
            #endregion

            ribbonSamplePanel.AddSeparator();

            #region Add a StackedButton which is consisted of one PushButton and two Comboboxes
            PushButtonData pushButtonData = new PushButtonData("Reset", "Reset", AddInPath, "Ribbon.ResetSetting");
            ComboBoxData comboBoxDataLevel = new ComboBoxData("LevelsSelector");
            ComboBoxData comboBoxDataShape = new ComboBoxData("WallShapeComboBox");
            IList<RibbonItem> ribbonItemsStacked = ribbonSamplePanel.AddStackedItems(pushButtonData, comboBoxDataLevel, comboBoxDataShape);
            ((PushButton)(ribbonItemsStacked[0])).Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.Reset.png");
            //Add options to WallShapeComboBox
            Autodesk.Revit.UI.ComboBox comboboxWallShape = (Autodesk.Revit.UI.ComboBox)(ribbonItemsStacked[2]);
            ComboBoxMemberData comboBoxMemberData = new ComboBoxMemberData("RectangleWall", "RectangleWall");
            ComboBoxMember comboboxMember = comboboxWallShape.AddItem(comboBoxMemberData);
            comboboxMember.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.RectangleWall.png"); 
            comboBoxMemberData = new ComboBoxMemberData("CircleWall", "CircleWall");
            comboboxMember = comboboxWallShape.AddItem(comboBoxMemberData);
            comboboxMember.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.CircleWall.png"); 
            comboBoxMemberData = new ComboBoxMemberData("TriangleWall", "TriangleWall");
            comboboxMember = comboboxWallShape.AddItem(comboBoxMemberData);
            comboboxMember.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.TriangleWall.png"); 
            comboBoxMemberData = new ComboBoxMemberData("SquareWall", "SquareWall");
            comboboxMember = comboboxWallShape.AddItem(comboBoxMemberData);
            comboboxMember.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.SquareWall.png"); 
            #endregion

            ribbonSamplePanel.AddSeparator();

            #region Add a RadioButtonGroup for user to select WallType
            RadioButtonGroupData radioButtonGroupData = new RadioButtonGroupData("WallTypeSelector");
            RadioButtonGroup radioButtonGroup = (RadioButtonGroup)(ribbonSamplePanel.AddItem(radioButtonGroupData));
            ToggleButton toggleButton = radioButtonGroup.AddItem(new ToggleButtonData("Generic8", "Genérico - 200 mm", AddInPath, "Ribbon.Dummy"));
            toggleButton.LargeImage = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.Generic8.png"); 
            toggleButton.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.Generic8-S.png"); 
            toggleButton = radioButtonGroup.AddItem(new ToggleButtonData("ExteriorBrick", "Exterior - Enlucido en ladrillo en bloque", AddInPath, "Ribbon.Dummy"));
            toggleButton.LargeImage = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.ExteriorBrick.png"); 
            toggleButton.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.ExteriorBrick-S.png"); 
            #endregion

            //slide-out panel:
            ribbonSamplePanel.AddSlideOut();

            #region add a Text box to set the mark for new wall
            TextBoxData testBoxData = new TextBoxData("WallMark");
            Autodesk.Revit.UI.TextBox textBox = (Autodesk.Revit.UI.TextBox)(ribbonSamplePanel.AddItem(testBoxData));
            textBox.Value = "new wall"; //default wall mark
            textBox.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.WallMark.png"); 
            textBox.ToolTip = "Set the mark for new wall";
            textBox.ShowImageAsButton = true;
            textBox.EnterPressed += new EventHandler<Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs>(SetTextBoxValue);
            #endregion

            ribbonSamplePanel.AddSeparator();

            #region Create a StackedButton which consisted of a PushButton (delete all the walls) and a PulldownButton (move all the walls in X or Y direction)
            PushButtonData deleteWallsButtonData = new PushButtonData("deleteWalls", "Delete Walls", AddInPath, "Ribbon.DeleteWalls");
            deleteWallsButtonData.ToolTip = "Delete all the walls created by the Create Wall tool.";
            deleteWallsButtonData.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.DeleteWalls.png"); 

            PulldownButtonData moveWallsButtonData = new PulldownButtonData("moveWalls", "Move Walls");
            moveWallsButtonData.ToolTip = "Move all the walls in X or Y direction";
            moveWallsButtonData.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.MoveWalls.png"); 

            // create stackable buttons
            IList<RibbonItem> ribbonItems = ribbonSamplePanel.AddStackedItems(deleteWallsButtonData, moveWallsButtonData);

            // add two push buttons as sub-items of the moveWalls PulldownButton. 
            PulldownButton moveWallItem = ribbonItems[1] as PulldownButton;

            PushButton moveX = moveWallItem.AddPushButton(new PushButtonData("XDirection", "X Direction", AddInPath, "Ribbon.XMoveWalls"));
            moveX.ToolTip = "move all walls 10 feet in X direction.";
            moveX.LargeImage = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.MoveWallsXLarge.png"); 

            PushButton moveY = moveWallItem.AddPushButton(new PushButtonData("YDirection", "Y Direction", AddInPath, "Ribbon.YMoveWalls"));
            moveY.ToolTip = "move all walls 10 feet in Y direction.";
            moveY.LargeImage = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.MoveWallsYLarge.png"); 
            #endregion

            ribbonSamplePanel.AddSeparator();

            application.ControlledApplication.DocumentCreated += new EventHandler<Autodesk.Revit.DB.Events.DocumentCreatedEventArgs>(DocumentCreated);
        }

        /// <summary>
        /// Insert Level into ComboBox - LevelsSelector
        /// </summary>
        /// <param name="evnetArgs">Autodesk.Revit.DB.Events.DocumentCreatedEventArgs</param>
        public void DocumentCreated(object sender, Autodesk.Revit.DB.Events.DocumentCreatedEventArgs e)
        {
            uiApplication = new UIApplication(e.Document.Application);
#if NewTab
            List<RibbonPanel> myPanels = uiApplication.GetRibbonPanels("Revit API Manual");
#else
            List<RibbonPanel> myPanels = uiApplication.GetRibbonPanels();
#endif

            Autodesk.Revit.UI.ComboBox comboboxLevel = (Autodesk.Revit.UI.ComboBox)(myPanels.Where(x => x.Name == "Ribbon Sample").First().GetItems()[2]);
            if (null == comboboxLevel) { return; }
            FilteredElementCollector collector = new FilteredElementCollector(uiApplication.ActiveUIDocument.Document);
            ICollection<Element> founds = collector.OfClass(typeof(Level)).ToElements();
            foreach (Element elem in founds)
            {
                Level level = elem as Level;
                ComboBoxMemberData comboBoxMemberData = new ComboBoxMemberData(level.Name, level.Name);
                ComboBoxMember comboboxMember = comboboxLevel.AddItem(comboBoxMemberData);
                comboboxMember.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.LevelsSelector.png");
            }
            //refresh level list (in case user created new level after document created)
            comboboxLevel.DropDownOpened += new EventHandler<ComboBoxDropDownOpenedEventArgs>(AddNewLevels);
        }

        /// <summary>
        /// Bind to combobox's DropDownOpened Event, add new levels that created by user.
        /// </summary>
        /// <param name="evnetArgs">Autodesk.Revit.UI.Events.ComboBoxDropDownOpenedEventArgs</param>
        public void AddNewLevels(object sender, ComboBoxDropDownOpenedEventArgs args)
        {
            Autodesk.Revit.UI.ComboBox comboboxLevel = sender as Autodesk.Revit.UI.ComboBox;
            if (null == comboboxLevel) { return; }
            FilteredElementCollector collector = new FilteredElementCollector(uiApplication.ActiveUIDocument.Document);
            ICollection<Element> founds = collector.OfClass(typeof(Level)).ToElements();
            foreach (Element elem in founds)
            {
                Level level = elem as Level;
                bool alreadyContained = false;
                foreach (ComboBoxMember comboboxMember in comboboxLevel.GetItems())
                {
                    if (comboboxMember.Name == level.Name)
                    {
                        alreadyContained = true;
                    }
                }
                if (!alreadyContained)
                {
                    ComboBoxMemberData comboBoxMemberData = new ComboBoxMemberData(level.Name, level.Name);
                    ComboBoxMember comboboxMember = comboboxLevel.AddItem(comboBoxMemberData);
                    comboboxMember.Image = GetIconFromDll.GetEmbeddedImage("Ribbon.Resources.LevelsSelector.png");
                }
            }

        }

        /// <summary>
        /// Bind to text box's EnterPressed Event, show a dialogue tells user value of test box changed.
        /// </summary>
        /// <param name="evnetArgs">Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs</param>
        public void SetTextBoxValue(object sender, TextBoxEnterPressedEventArgs args)
        {
            TaskDialog.Show("TextBox EnterPressed Event", "New wall's mark changed.");
        }

    }
}
