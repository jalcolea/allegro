using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinDock;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinGrid;
using Allegro;
using Allegro.UI;
using Allegro.UI.ClassEvents;
using Allegro.UI.Controls;
using Allegro.UI.Dialogs;
using Allegro.UI.Forms;
using Allegro.Core.ClassEvents;
using Allegro.Core.Config;
using Allegro.Core.Criteria;
using Allegro.Core.DataModel;
using Allegro.Core.Extensions;
using Allegro.Core.Logging;
using Allegro.Core.Utils;
using Domain = Allegro.Core.ClassEvents.Domain;

namespace Allegro.ClassEvents
{
    public class ToolbarHelper
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Toolbar helper add button
        Toolbar helper add button */
        public static void AddButton(ViewForm view, string name, bool stateful, string icon)
        {
            if (view.ToolbarsManager.Tools.Exists(name))
            {
                return;
            }
            
            ToolBase button = stateful ? new StateButtonTool(name) : new ButtonTool(name);
            button.SharedProps.AppearancesSmall.Appearance.Image = AppManager.MainForm.SmallImageList.Images[icon];
            button.SharedProps.AppearancesSmall.PressedAppearance.Image = AppManager.MainForm.SmallImageList.Images[icon];
            button.SharedProps.AppearancesLarge.Appearance.Image = AppManager.MainForm.LargeImageList.Images[icon];
            button.SharedProps.AppearancesLarge.PressedAppearance.Image = AppManager.MainForm.LargeImageList.Images[icon];
            button.SharedProps.Caption = name;
            button.SharedProps.Category = "FUNCTION";
            button.SharedProps.ToolTipText = name;
            button.SharedProps.Visible = true;
            
            RibbonTab methodsRibbon;
            if (view.ToolbarsManager.Ribbon.Tabs.IndexOf(view.ViewName + " Methods") >= 0)
            {
                methodsRibbon = view.ToolbarsManager.Ribbon.Tabs[view.ViewName + " Methods"];
            }
            else
            {
                methodsRibbon = view.ToolbarsManager.Ribbon.Tabs.Add(view.ViewName + " Methods");
                methodsRibbon.Caption = view.ViewName;
            }
            
            RibbonGroup functionsRibbonGroup;
            if (methodsRibbon.Groups.IndexOf("functionsRibbonGroup") >= 0)
            {
                functionsRibbonGroup = methodsRibbon.Groups["functionsRibbonGroup"];
            }
            else
            {
                functionsRibbonGroup = methodsRibbon.Groups.Add("functionsRibbonGroup");
                functionsRibbonGroup.Caption = "Functions";
                functionsRibbonGroup.PreferredToolSize = RibbonToolSize.Large;
            }
            
            view.ToolbarsManager.Tools.Add(button);
            functionsRibbonGroup.Tools.Insert(functionsRibbonGroup.Tools.Count, button);
            
            if (view.ToolbarsManager.MdiParentManager != null)
            {
                view.ToolbarsManager.MdiParentManager.RefreshMerge();
            }
        }
        /* Toolbar helper check state button
        Toolbar helper check state button */
        public static bool CheckStateButton(ViewForm view, string name)
        {
            StateButtonTool tool = null;
            if (view != null && view.ToolbarsManager.Tools.IndexOf(name) >= 0)
            {
                tool = view.ToolbarsManager.Tools[name] as StateButtonTool;
            }
            else if (AppManager.MainForm.ToolbarsManager.Tools.IndexOf(name) >= 0)
            {
                tool = AppManager.MainForm.ToolbarsManager.Tools[name] as StateButtonTool;
            }
            return tool != null && tool.Checked;
        }
        
        
    }
}

