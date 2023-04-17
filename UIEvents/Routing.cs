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
    public class RoutingCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Routing_Before_Update_Data
        Routing_Before_Update_Data - DEV - Populate required fields */
        public UiEventResult BeforeUpdateData_2(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string carrier = String.Empty;
            string field = String.Empty;
            string message ="The following information is required: ";
            Boolean ShowMessage = false;
            DataSet ds = new DataSet();
            
            DataRow[] newModifiedrouteplanRows = _view.DataSource.Tables["routeplandetail"].Select("", "", DataViewRowState.ModifiedCurrent);
            
            foreach (DataRow row in _view.DataSource.Tables["routeplandetail"].Select("", "", DataViewRowState.ModifiedCurrent))
            {
                carrier = row["carrier"].ToString();
                
                string sql = "SELECT * FROM cst_shipmentfieldrequired where carrier = '" + carrier + "'";
                SqlHelper.RetrieveData(ds, new[] { "cst_shipmentfieldrequired" }, new[] { "SELECT * FROM cst_shipmentfieldrequired WHERE carrier = '" + carrier + "'" });
                foreach (DataRow rowrequired in ds.Tables["cst_shipmentfieldrequired"].Rows)
                {
                    field = rowrequired["requiredfield"].ToString();
                    if (String.IsNullOrEmpty(row[field].ToString()))
                    {
                        ShowMessage = true;
                        message += field + ",";
                    }
                }
                if (ShowMessage)
                {
                    message = message.Remove(message.Length-1);
                    ErrorDialog.Show("Validation", message);
                    return new UiEventResult(EventStatus.Cancel);
                }
                
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Routing_Apply_Transport_Frequency
        Duplicates the route plan detail record based on the frequency specified on parameter grid. Frequency can be specifed in the days o the week or as E1, E2 which stands for everyday, every two days. */
        public UiEventResult ToolClick_Apply_Transport_Frequency_Before_1()
        {
            ViewGrid routePlandetailGrid = _view.ViewGrids["routeplandetail"];
            if (routePlandetailGrid == null)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            
            if (routePlandetailGrid.Rows.Count <= 0 || (routePlandetailGrid.Selected.Rows.Count <= 0 && _view.ViewGrids["routeplan"].Selected.Rows.Count <= 0))
            {
                ErrorDialog.Show("Validation", "There is no route plan detail to apply transport frequency. Please, select route plan detail or route plan record that has route plan detail.");
                return new UiEventResult(EventStatus.Continue);
            }
            
            
            if (_view.ParameterGrid.Rows[0].Cells.IndexOf("transportfrequency") < 0)
            {
                ErrorDialog.Show("Error", "Transport frequency column is required to be added with value in the view parameter grid.");
                return new UiEventResult(EventStatus.Continue);
            }
            
            
            string frequency = _view.ParameterGrid.Rows[0].Cells["transportfrequency"].Value.ToString();
            
            #region validation of transport frequency
            if (string.IsNullOrEmpty(frequency))
            {
                ErrorDialog.Show("Validation", "Transport frequency in the parameter grid is null." +
                " \nPlease add frequency in the form of either \n \"M,T,W,TR,F,S,SU\" for days of the week \nOr\n E1 for every 1 day. ");
                return new UiEventResult(EventStatus.Continue);
            }
            #endregion validation of transport frequency
            
            List<string> days = new List<string>() { "M", "T", "W", "TR", "F", "S", "SU" };
            
            string[] frequencyArr = frequency.Split(new char[1]{','},StringSplitOptions.RemoveEmptyEntries);
            
            List<DayOfWeek> shipmentDaysOfWeek = new List<DayOfWeek>();
            int frequencyGap = 0;
            List<DateTime> shipmentFrequency = new List<DateTime>();
            
            //if the first frequency input is days, then take days only
            string invalidInput = "Transport frequency could not be recognized as a valid input.\n Please add frequency in the following format:  \n \"M,T,W,TR,F,S,SU\" for days of the week"
            +"\n For Eg: M for Monday or M,T for Mondays and Tuesdays\nor\n E1 for every day or E2 for every second day. ";
            
            #region retrieve the days chosen by the user
            if (days.Contains(frequencyArr[0].ToUpper().Trim()))
            {
                foreach (string input in frequencyArr)
                {
                    if (!days.Contains(input.ToUpper().Trim()))
                    {
                        ErrorDialog.Show("Validation", invalidInput);
                        return new UiEventResult(EventStatus.Continue);
                    }
                    if (input.ToUpper().Trim().StartsWith("M") && !shipmentDaysOfWeek.Contains(DayOfWeek.Monday))
                    {
                        shipmentDaysOfWeek.Add(DayOfWeek.Monday);
                    }
                    else if (input.ToUpper().Trim().StartsWith("T") && !input.ToUpper().Trim().StartsWith("TR")
                        && !shipmentDaysOfWeek.Contains(DayOfWeek.Tuesday))
                    {
                        shipmentDaysOfWeek.Add(DayOfWeek.Tuesday);
                    }
                    else if (input.ToUpper().Trim().StartsWith("W") && !shipmentDaysOfWeek.Contains(DayOfWeek.Wednesday))
                    {
                        shipmentDaysOfWeek.Add(DayOfWeek.Wednesday);
                    }
                    else if ((input.ToUpper().Trim().StartsWith("TR"))
                        && !shipmentDaysOfWeek.Contains(DayOfWeek.Thursday))
                    {
                        shipmentDaysOfWeek.Add(DayOfWeek.Thursday);
                    }
                    else if (input.ToUpper().Trim().StartsWith("F") && !shipmentDaysOfWeek.Contains(DayOfWeek.Friday))
                    {
                        shipmentDaysOfWeek.Add(DayOfWeek.Friday);
                    }
                    else if (input.ToUpper().Trim().StartsWith("S") && !input.ToUpper().Trim().StartsWith("SU") && !shipmentDaysOfWeek.Contains(DayOfWeek.Saturday))
                    {
                        shipmentDaysOfWeek.Add(DayOfWeek.Saturday);
                    }
                    else if (input.ToUpper().Trim().StartsWith("SU") && !shipmentDaysOfWeek.Contains(DayOfWeek.Sunday))
                    {
                        shipmentDaysOfWeek.Add(DayOfWeek.Sunday);
                    }
                }
            }
            
            else if (frequencyArr[0].ToUpper().Trim().StartsWith("E") && frequencyArr[0].Trim().Length>1 && Convert.ToInt32(frequencyArr[0].Trim().Substring(1))>0)
            {
                if (frequencyArr.Length > 1)
                {
                    ErrorDialog.Show("Validation", invalidInput);
                    return new UiEventResult(EventStatus.Continue);
                }
                
                frequencyGap = Convert.ToInt32(frequencyArr[0].Trim().Substring(1));
            }
            else
            {
                ErrorDialog.Show("Validation", invalidInput);
                return new UiEventResult(EventStatus.Continue);
            }
            #endregion retrieve the days chosen by the user
            
            #region get the plandetail rows to be duplicated
            List<DataRow> selectedPlandetailRows = new List<DataRow>();
            
            
            if (routePlandetailGrid.Selected.Rows.Count > 0)
            {
                #region this section go to the last drill and selects the records
                
                
                foreach (UltraGridRow row in routePlandetailGrid.Selected.Rows)
                {
                    string filter = string.Empty;
                    foreach (DrillInfo.DrillColumn drillColumn in routePlandetailGrid.DrillInfo.DrillColumns)
                    {
                        if (row.Cells[drillColumn.ColumnName].Value != DBNull.Value)
                        {
                            filter += (!string.IsNullOrEmpty(filter)) ? " and " : string.Empty;
                            filter += drillColumn.ColumnName + "=" + Expr.Value(row.Cells[drillColumn.ColumnName].Value);
                        }
                    }
                    List<DataRow> plandetailRows = new List<DataRow>();
                    plandetailRows.AddRange(_view.DataSource.Tables["routeplandetail"].Select(filter));
                    
                    if (row.ChildBands != null && row.ChildBands.FirstRow != null)
                    {
                        UltraGridRow parentRow = row;
                        while (parentRow.HasChild())
                        {
                            parentRow = parentRow.ChildBands.FirstRow;
                            if (parentRow.HasChild()) continue;
                            else
                            {
                                UltraGridRow nextParentRow = parentRow.ParentRow.GetSibling(SiblingRow.Next);
                                foreach (UltraGridRow childRow in parentRow.ParentCollection)
                                {
                                    if (plandetailRows.Contains(childRow.GetDataRow()))
                                        selectedPlandetailRows.Add(childRow.GetDataRow());
                                }
                                if (nextParentRow != null) parentRow = nextParentRow;
                            }
                        }
                    }
                    else selectedPlandetailRows.Add(row.GetDataRow());
                }
                #endregion this section go to the last drill and selects the records
            }
            #region if plandetail is not selected try getting the plandetail rows for the selected route plan(s)
                else
            {
                ViewGrid routePlanGrid = _view.ViewGrids["routeplan"];
                List<string> routePlans = new List<string>(); ;
                foreach (UltraGridRow row in routePlanGrid.Selected.Rows)
                {
                    string filter = string.Empty;
                    foreach (DrillInfo.DrillColumn drillColumn in routePlanGrid.DrillInfo.DrillColumns)
                    {
                        if (row.Cells[drillColumn.ColumnName].Value != DBNull.Value)
                        {
                            filter += (!string.IsNullOrEmpty(filter)) ? " and " : string.Empty;
                            filter += drillColumn.ColumnName + "=" + Expr.Value(row.Cells[drillColumn.ColumnName].Value);
                        }
                    }
                    List<DataRow> planRows = new List<DataRow>();
                    planRows.AddRange(_view.DataSource.Tables["routeplan"].Select(filter));
                    if (row.HasChild())
                    {
                        UltraGridRow parentRow = row;
                        while (parentRow.HasChild())
                        {
                            parentRow = parentRow.ChildBands.FirstRow;
                            if (parentRow.HasChild()) continue;
                            else
                            {
                                UltraGridRow nextParentRow = parentRow.ParentRow.GetSibling(SiblingRow.Next);
                                foreach (UltraGridRow childRow in parentRow.ParentCollection)
                                {
                                    if (planRows.Contains(childRow.GetDataRow()))
                                        routePlans.Add(childRow.Cells["routeplan"].Value.ToString());
                                }
                                if (nextParentRow != null) parentRow = nextParentRow;
                            }
                        }
                    }
                    else if (!routePlans.Contains(row.Cells["routeplan"].Value.ToString()))
                        routePlans.Add(row.Cells["routeplan"].Value.ToString());
                }
                foreach (string routeplan in routePlans)
                {
                    selectedPlandetailRows.AddRange(_view.DataSource.Tables["routeplandetail"].Select("routeplan=" + Expr.Value(routeplan), "", DataViewRowState.Unchanged | DataViewRowState.ModifiedCurrent));
                }
            }
            #endregion if plandetail is not selceted try getting the plandetail rows for the selected route plan(s)
                
            #endregion get the plandetail rows to be duplicated
            
            #region duplicate the selected plan detail rows or each transport frequency selected with in the timeperiod starting from the scheddate to the routeplan endtime
            
            #region created dictionary of existing plan detail records
            Dictionary<string, List<DataRow>> existingPlanDetailRowsDic = new Dictionary<string, List<DataRow>>(); //Holds existing plandetail rows so we can check if a record exists for the schedule date before duplicating
            string[] keyColumns = new string[] { "routeplan", "seq", "carriermode" };
            foreach (DataRow row in _view.DataSource.Tables["routeplandetail"].Rows)
            {
                string key = string.Empty;
                for (int i = 0; i < keyColumns.Length; i++) key += row[keyColumns[i]].ToString();
                
                if (!existingPlanDetailRowsDic.ContainsKey(key)) existingPlanDetailRowsDic.Add(key, new List<DataRow>());
                
                existingPlanDetailRowsDic[key].Add(row);
            }
            #endregion created dictionary of existing plan detail records
            
            string validation = null;
            
            int tempRoutePlanDetail = 1;
            foreach (DataRow row in selectedPlandetailRows)
            {
                DataRow routePlanRow = _view.DataSource.Tables["routeplan"].Select("routeplan=" + Expr.Value(row["routeplan"]))[0];
                bool executedRoutePLan = (routePlanRow["status"].ToString().Equals("EXECUTED"));
                if (executedRoutePLan)
                {
                    if (string.IsNullOrEmpty(validation)) validation = "Executed route plan detail was not duplicated.";
                    continue;
                }
                string scheduledateColumn = (!row.IsNull("scheddate")) ? "scheddate" : "destinationscheddate";
                DateTime begtime = Convert.ToDateTime(row[scheduledateColumn]);
                
                DateTime endtime = Convert.ToDateTime(routePlanRow["endtime"]);
                TimeSpan tp = endtime - begtime;
                int duration = tp.Days;
                
                List<DateTime> scheduleDates = new List<DateTime>();
                
                #region assign dates for each days of the week within the time period starting begtime to routplan endtime or based on the frequency gap specified
                if (shipmentDaysOfWeek.Count > 0)
                {
                    foreach (DayOfWeek dayOfWeek in shipmentDaysOfWeek)
                    {
                        for (int i = 1; i < duration; i++)
                        {
                            if (begtime.AddDays(i).DayOfWeek == dayOfWeek && !scheduleDates.Contains(begtime.AddDays(i)))
                                scheduleDates.Add(begtime.AddDays(i));
                        }
                    }
                }
                else if (frequencyGap > 0)
                {
                    for (int i = 0; i + frequencyGap < duration; i += frequencyGap)
                    {
                        if (!scheduleDates.Contains(begtime.AddDays(i + frequencyGap)))
                            scheduleDates.Add(begtime.AddDays(i + frequencyGap));
                    }
                }
                #endregion assign dates for each days of the week within the time period starting begtime to routplan endtime or based on the frequency gap specified
                
                foreach (DateTime scheduledate in scheduleDates)
                {
                    bool duplicate = true;
                    /***don't duplicate if record exists for this schedule date ***/
                    string key = string.Empty;
                    for (int i = 0; i < keyColumns.Length; i++) key += row[keyColumns[i]].ToString();
                    foreach (DataRow existingRow in existingPlanDetailRowsDic[key])
                    {
                        if (Convert.ToDateTime(existingRow[scheduledateColumn]).Equals(scheduledate))
                        {
                            duplicate = false;
                            break;
                        }
                    }
                    //dupliacte row here
                    if (duplicate)
                    {
                        DataRow newRow = _view.DataSource.Tables["routeplandetail"].NewRow();
                        newRow.ItemArray = row.ItemArray;
                        newRow[scheduledateColumn] = scheduledate;
                        if (scheduledateColumn.Equals("scheddate", StringComparison.Ordinal) && !newRow.IsNull("destinationscheddate") && !newRow.IsNull("duration"))
                            newRow["destinationscheddate"] = Convert.ToDateTime(newRow[scheduledateColumn]).AddDays(Convert.ToInt32(newRow["duration"]));
                        
                        //newRow.SetColumnValue("validation", DBNull.Value);
                        
                        newRow["routeplandetail"] = "00" + tempRoutePlanDetail.ToString();
                        tempRoutePlanDetail++;
                        
                        _view.DataSource.Tables["routeplandetail"].Rows.Add(newRow);
                    }
                }
            }
            
            #endregion duplicate the selected plan detail rows
            
            if (!string.IsNullOrEmpty(validation))
                ErrorDialog.Show("Validation", validation);
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

