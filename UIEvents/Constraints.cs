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
using System.Collections;
using System.Linq;

namespace Allegro.ClassEvents
{
    public class ConstraintsCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Constraints - BeforeCellActivate_position
        Limit position constraint to relevant trade */
        public UiEventResult BeforeCellActivate_constraintdefinition_position_1000(object sender, CancelableCellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (_view.Name.Equals("Constraint Definition"))
            {
                ViewGrid viewGrid = sender as ViewGrid;
                
                string trade = (viewGrid.ActiveRow.Cells["trade"].Column.Key.Length > 0
                && viewGrid.ActiveRow.Cells["trade"].Value != DBNull.Value) ?
                viewGrid.ActiveRow.GetCellValue<string>("trade") : string.Empty;
                
                string sql = string.Empty;
                if (!string.IsNullOrEmpty(trade))
                {
                    sql = string.Format("SELECT DISTINCT position FROM position WHERE positionmode='PHYSICAL' AND trade = '{0}' ORDER BY position", trade);
                }
                else
                {
                    sql = string.Format("SELECT DISTINCT position FROM position WHERE positionmode='PHYSICAL' ORDER BY position", trade);
                }
                viewGrid.SetColumnStyle(viewGrid.ActiveRow.Cells["position"].Column, sql);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Constraints - BeforeUpdateData
        ensure all view constraints are updeld */
        public UiEventResult BeforeUpdateData_1000(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (DataRow row in _view.DataSource.Tables["tradeconstraint"].Rows)
            {
                
                if (!string.IsNullOrEmpty(row.GetColumnValue<string>("position")) &&
                (!string.IsNullOrEmpty(row.GetColumnValue<string>("pipeline")) ||
                !string.IsNullOrEmpty(row.GetColumnValue<string>("ngpoint"))))
                {
                    MessageBox.Show("Position entry cannot exist with entry for Pipeline or Point",
                    "Constraint Entry Validation", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return new UiEventResult(EventStatus.Cancel);
                    
                }
                //D.BECKHAM - 2017.03.151 :: Restriction removed.
                //if (!string.IsNullOrEmpty(row.GetColumnValue<string>("pipeline")) &&
                //string.IsNullOrEmpty(row.GetColumnValue<string>("ngpoint")))
                    //{
                    //    MessageBox.Show("Pipeline cannot be defined without a Point", "Constraint Entry Validation",
                    //    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    //    return new UiEventResult(EventStatus.Cancel);
                    
                //}
                
                if (row.GetColumnValue<decimal>("min") > row.GetColumnValue<decimal>("max") &&
                !string.IsNullOrEmpty(row.GetColumnValue<string>("min")) &&
                !string.IsNullOrEmpty(row.GetColumnValue<string>("max")))
                {
                    MessageBox.Show(
                    "Constraints " + row.GetColumnValue<string>("constraintid") +
                    " 'min' value is not allowed to be larger than 'max'", "Constraint Entry Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return new UiEventResult(EventStatus.Cancel);
                }
                
                //d.beckham 29-09-2015 : Loadshape constraint.
                if (!string.IsNullOrEmpty(row.GetColumnValue<string>("minls")) ||
                !string.IsNullOrEmpty(row.GetColumnValue<string>("maxls")))
                {
                    if ((!string.IsNullOrEmpty(row.GetColumnValue<string>("minls")) &&
                    string.IsNullOrEmpty(row.GetColumnValue<string>("maxls"))) ||
                    (string.IsNullOrEmpty(row.GetColumnValue<string>("minls")) &&
                    !string.IsNullOrEmpty(row.GetColumnValue<string>("maxls"))))
                    {
                        MessageBox.Show("Loadshape based constraints must have both min/max loadshapes defined.",
                        "Constraint Entry Validation", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return new UiEventResult(EventStatus.Cancel);
                    }
                    else if (!string.IsNullOrEmpty(row.GetColumnValue<string>("constrainttype")) &&
                    !row.GetColumnValue<string>("constrainttype").Contains("QUANTITY"))
                    {
                        MessageBox.Show("Loadshape based constraints must be of Constraint Type QUANTITY.",
                        "Constraint Entry Validation", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return new UiEventResult(EventStatus.Cancel);
                    }
                    else if (!string.IsNullOrEmpty(row.GetColumnValue<string>("timeunit")) &&
                    !(row.GetColumnValue<string>("timeunit").Contains("HOUR") ||
                    row.GetColumnValue<string>("timeunit").Contains("DAY") ||
                    row.GetColumnValue<string>("timeunit").Contains("MONTH")))
                    {
                        MessageBox.Show("Loadshape based constraints must have time unit of HOUR, DAY or MONTH.",
                        "Constraint Entry Validation", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
                
                //D.BECKHAM - 2017.03.30 :: ToP constraint must also have Log Success = true.
                if (row.GetColumnValue<bool>("takeorpay") && !row.GetColumnValue<bool>("logsuccess"))
                {
                    MessageBox.Show("ToP constraint must also have Log Success checked.",
                    "Constraint Entry Validation", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Constraints - InitView
        Constraints custom class initialize view */
        public UiEventResult InitView_1000()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* Constraints - ToolClick_Before
        Button to execute constraints report from Constraints Definition view */
        public UiEventResult ToolClick_Execute_Constraints_Before_1000()
        {
            //D.BECKHAM - 2017.03.07
            
            //If Constraints view, capture selected rows.
            if (_view.ViewName.Contains("Constraint Definition"))
            {
                try
                {
                    SelectedRowsCollection selectedRows = _view.ViewGrids["constraintdefinition"].Selected.Rows;
                    
                    //Check to make sure no child bands exist to confirm that a drilled row is not selected.
                    if (selectedRows[0].HasChild())
                        return new UiEventResult(EventStatus.Continue);
                    
                    int rowCountSelected = selectedRows.Count;
                    
                    //Check if group parameter is populated
                    string group = string.Empty;
                    string constraints = string.Empty;
                    if (_view.ParameterGrid.Rows.Any())
                    {
                        group = _view.ParameterGrid.Rows[0].GetCellValue<string>("ConstraintGroup");
                    }
                    
                    string msg_ex = string.Empty;
                    string msg_res = string.Empty;
                    
                    if (rowCountSelected == 0)
                    {
                        //If no constraints selected, check for Group parameter. Else execute for all active.
                        msg_ex = string.Format(string.IsNullOrEmpty(group)
                            ? "Run Trade Contraints Report for all Active definitions?"
                        : "Run Trade Contraints Report for the following Constraint Group?\n{0}", group);
                        if (MessageBox.Show(msg_ex, "Trade Constraints", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                            return new UiEventResult(EventStatus.Cancel);
                        
                        if (string.IsNullOrEmpty(group))
                        {
                            msg_res = "Full constraint report execution added to grid queue.";
                        }
                        else
                        {
                            msg_res = string.Format("Report execution added to grid queue for active constraints in Group {0}.", group);
                        }
                        
                    }
                    else
                    {
                        //Submit selected row constraint IDs for execution
                        List<string> constraintList = (from UltraGridRow row in selectedRows select row.Cells["constraintid"].GetValue<string>()).ToList();
                        constraintList.Sort();
                        
                        if (constraintList.Any())
                        {
                            //Capture string of constraint IDs and send to web method
                            constraints = string.Join(", ", constraintList);
                            
                            //Supercedes group parameter
                            group = string.Empty;
                            
                            msg_ex = string.Format("Run Trade Contraints Report for the following Constraint IDs?\n{0}", constraints);
                            
                            if (MessageBox.Show(msg_ex, "Trade Constraints", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                                return new UiEventResult(EventStatus.Cancel);
                            
                            msg_res = string.Format("Report execution added to grid queue for the following Constraint IDs:\n{0}", constraints);
                        }
                        else
                        {
                            msg_res = "Unable to capture selected constraint rows. Please reselect.";
                        }
                    }
                    string[] args = new string[2] { "groups_in", "constraints_in" };
                    object[] parms = new object[2] { group, constraints };
                    string result = Soap.Invoke<string>("ConstraintsWS.asmx", "StartTaskConstraints", args, parms);
                    
                    if (result == "ERROR")
                    {
                        msg_res = string.Format("Constraint report execution encountered errors.\nPlease review grid log.");
                    }
                    
                    MessageBox.Show(msg_res, "Trade Constraints", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ShowMessage("ERROR", ex.ToString());
                }
            }
            //return a continue UI event
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

