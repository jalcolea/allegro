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
    public class ConstraintsLogCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* ConstraintsLog - InitView
        Constraint Logs - InitView */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* ConstraintsLog - BeforeProccessFlaggedForUpdate
        Constraint Logs -  BeforeProccessFlaggedForUpdate */
        public UiEventResult ToolClick_ProccessFlaggedForUpdate_Before_1()
        {
            Soap.Invoke<string>("Constraints/ConstraintsToPWS.asmx", "ProcessFlaggedForUpdate", new string[] { }, new object[] {null});
            return new UiEventResult(EventStatus.Continue);
        }
        /* ConstraintsLog  - BeforeProcessCheckRun
        Constraint Logs  - BeforeProcessCheckRun */
        public UiEventResult ToolClick_ProcessCheckRun_Before_1()
        {
            //If Constraints view, capture selected rows.
            if (_view.ViewName.Contains("Constraint Log"))
            {
                SelectedRowsCollection selectedRows = _view.ViewGrids["constraintlog"].Selected.Rows;
                
                //Check to make sure no child bands exist to confirm that a drilled row is not selected.
                if (selectedRows[0].HasChild())
                    return new UiEventResult(EventStatus.Continue);
                
                if (selectedRows.Count != 1)
                {
                    MessageBox.Show("You must select exactly one row for processing", "Trade Constraint Logs");
                    return new UiEventResult(EventStatus.Cancel);
                }
                string[] args = new string[1] { "checkrunid" };
                object[] parms = new object[1] { selectedRows[0].GetCellValue<string>("checkrunid")};
                
                Soap.Invoke<string>("Constraints/ConstraintsToPWS.asmx", "ProcessCheckConstraint", args, parms);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* ConstraintsLog  - BeforeProcessLatestCheckRun
        Constraint Logs  - BeforeProcessLatestCheckRun */
        public UiEventResult ToolClick_ProcessLatestCheckRun_Before_1()
        {
            Soap.Invoke<string>("Constraints/ConstraintsToPWS.asmx", "ProcessCheckConstraint", new string[] { "checkrunid" }, new object[] {null});
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

