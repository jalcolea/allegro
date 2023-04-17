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
using System.Linq;

namespace Allegro.ClassEvents
{
    public class NaturalGasCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* CopyConfigurationData_BeforeConveyanceProcess
        Input validation and calls webmethod */
        public UiEventResult ToolClick_Copy_Configuration_Data_Before_1()
        {
            // History
            // Uptime : 20-10-2015 : a.kokin :: Created.
            // Uptime : 21-10-2015 : a.kokin :: edied.
            
            #region Row validation
            
            var pipelineConveyPane = _view.ViewGrids["pipeline_conveyance"];
            
            if (pipelineConveyPane == null)
                return new UiEventResult(EventStatus.Cancel);
            
            var pipelineConveyRow = pipelineConveyPane.Selected.Rows.Cast<UltraGridRow>();
            var selectedRows = pipelineConveyRow as IList<UltraGridRow> ?? pipelineConveyRow.ToList();
            
            if (!selectedRows.Any())
            {
                ErrorDialog.Show("Error", "Please select a pipeline");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            if (selectedRows.Count != 1)
            {
                ErrorDialog.Show("Error", "Please select only one pipeline");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            #endregion Row validation
            
            #region Cells validation
            
            var cells = selectedRows.First().Cells;
            var effDate = cells["effdate"].Value.ToString();
            var sourcePipeline = cells["sourcepipeline"].Value.ToString();
            var destinationPipeline = cells["destinationpipeline"].Value.ToString();
            
            if (string.IsNullOrEmpty(effDate) || string.IsNullOrEmpty(destinationPipeline))
            {
                ErrorDialog.Show("Error", "Missing End time or Destination Pipeline. Please populate End time and Destination Pipeline");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            #endregion Cells validation
            
            #region Server request
            
            var args = new[] { "sourcePipeline", "destinationPipeline" };
            var parameters = new object[] { sourcePipeline, destinationPipeline };
            try
            {
                var response = Soap.Invoke<string>("NGConveyanceWS.asmx", "CopyConfigurationData", args, parameters);
                MessageBox.Show(response, "Copy Configuration Data");
            }
            catch (Exception ex)
            {
                // TODO: Log exception;
                throw ex;
            }
            
            #endregion Server request
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* CounterpartyAccountConveyance_BeforeConveyanceProcess
        Validation before starting the Counterparty Account Conveyance process */
        public UiEventResult ToolClick_Counterparty_Account_Conveyance_Before_1()
        {
            // History
            // US Tech Desk : 03-10-2014 : siuker       :: Original code
            // EMEA Dev Desk : 08-10-2015 : d.beckham   :: Updated and standardized.
            
            bool endtimeflag = false;
            bool acctflag = false;
            string errormsg = string.Empty;
            DataSet vwdataset = this._view.DataSource;
            
            ViewGrid vwpipelines = this._view.ViewGrids["pipeline"];
            ViewGrid vwpipelineaccount = this._view.ViewGrids["pipelineaccount"];
            if (vwpipelineaccount != null && vwpipelines != null)
            {
                if (vwpipelineaccount.Selected.Rows.Count != 1)
                {
                    ErrorDialog.Show("Error", "Please select one and only one pipeline account row");
                    return new UiEventResult(EventStatus.Cancel);
                }
                
                // siuker - add check for selecting one pipeline
                if (vwpipelines.Selected.Rows.Count != 1)
                {
                    ErrorDialog.Show("Error", "Please select one and only one pipeline row");
                    return new UiEventResult(EventStatus.Cancel);
                }
                
                var cellendtime = vwpipelineaccount.Selected.Rows[0].Cells["accountendtime"];
                if (cellendtime != null)
                {
                    if (string.IsNullOrEmpty(cellendtime.Text))
                    {
                        endtimeflag = true;
                    }
                }
                var cell_acct = vwpipelineaccount.Selected.Rows[0].Cells["destinationcounterpartyaccount"];
                if (cell_acct != null)
                {
                    if (string.IsNullOrEmpty(cell_acct.Text))
                    {
                        acctflag = true;
                    }
                }
                if (endtimeflag && acctflag)
                {
                    errormsg = "Endtime and Destination Counterparty Account are missing. Please verify that the necessary data is properly filled in before trying again.";
                    
                }
                else if (endtimeflag)
                {
                    errormsg = "Endtime is missing. Please verify that the necessary data is properly filled in before trying again.";
                }
                else if (acctflag)
                {
                    errormsg = "Destination Counterparty Account is missing. Please verify that the necessary data is properly filled in before trying again.";
                }
                if (errormsg != string.Empty)
                {
                    ErrorDialog.Show("Error", errormsg);
                    return new UiEventResult(EventStatus.Cancel);
                }
                
                //Get the pipelineaccount column values
                var cellaccount = vwpipelineaccount.Selected.Rows[0].Cells["account"];
                var cellshipper = vwpipelineaccount.Selected.Rows[0].Cells["shipper"];
                
                DateTime endtime = (DateTime)cellendtime.Value;
                string destinationaccount = cell_acct.Value.ToString();
                string shipper = cellshipper.Value.ToString();
                string account = cellaccount.Value.ToString();
                
                //get the pipeline column values
                string pipeline = string.Empty;
                if (vwpipelines.Selected.Rows.Count > 0)
                {
                    var cellpipeline = vwpipelines.Selected.Rows[0].Cells["pipeline"];
                    pipeline = cellpipeline.Value.ToString();
                }
                
                var agrs = new string[] { "accountendtime", "destinationaccount", "shipper", "account", "pipeline", "returndatasource" };
                var parms = new object[] { endtime, destinationaccount, shipper, account, pipeline, vwdataset };
                DataSet returnDS = Soap.Invoke<DataSet>("NGConveyanceWS.asmx", "PipelineAccountConveyance", agrs, parms);
                if (returnDS.Tables.Contains("DtValidation"))
                {
                    string strvalidation = string.Empty;
                    DataTable dtvalidation = returnDS.Tables["DtValidation"];
                    DataRow[] drvalidations = dtvalidation.Select();
                    foreach (DataRow drvalidation in drvalidations)
                    {
                        strvalidation = strvalidation + drvalidation["Message"].ToString() + ".";
                    }
                    MessageBox.Show(strvalidation, "Counterparty Account Conveyance");
                }
            }
            //Validation before Shipper Account Conveyance process starts
            return new UiEventResult(EventStatus.Continue);
        }
        /* NGPointConveyance_BeforeConveyanceProcess
        Validatioxn before xstarting the NG Point Conveyance process */
        public UiEventResult ToolClick_NG_Point_Conveyance_Before_100()
        {
            // History
            // EMEA Dev Desk : 08-10-2015 : d.beckham   :: Updated and standardized.
            
            bool bEffdateflag = false;
            bool bPointflag = false;
            string strErrormsg = string.Empty;
            DataSet vwdataset = this._view.DataSource;
            
            ViewGrid vwPipelines = this._view.ViewGrids["pipeline"];
            ViewGrid vwNgpoint = this._view.ViewGrids["ngpoint"];
            ViewGrid vwNgpointconvey = this._view.ViewGrids["ngpointconvey"];
            if (vwNgpointconvey != null && vwNgpoint != null && vwPipelines != null)
            {
                //Check that only one record is selected
                if (vwNgpointconvey.Selected.Rows.Count != 1)
                {
                    ErrorDialog.Show("Error", "Please select one and only one ng point convey row");
                    return new UiEventResult(EventStatus.Cancel);
                }
                if (vwNgpoint.Selected.Rows.Count != 1)
                {
                    ErrorDialog.Show("Error", "Please select one and only one ng point row");
                    return new UiEventResult(EventStatus.Cancel);
                }
                if (vwPipelines.Selected.Rows.Count != 1)
                {
                    ErrorDialog.Show("Error", "Please select one and only one pipeline row");
                    return new UiEventResult(EventStatus.Cancel);
                }
                
                var celleffdate = vwNgpointconvey.Selected.Rows[0].Cells["effdate"];
                if (celleffdate != null)
                {
                    if (string.IsNullOrEmpty(celleffdate.Text))
                    {
                        bEffdateflag = true;
                    }
                }
                var celldestinationpoint = vwNgpointconvey.Selected.Rows[0].Cells["destinationpoint"];
                if (celldestinationpoint != null)
                {
                    if (string.IsNullOrEmpty(celldestinationpoint.Text))
                    {
                        bPointflag = true;
                    }
                }
                if (bEffdateflag && bPointflag)
                {
                    strErrormsg = "Endtime and Destination NG Point are missing. Please verify that the necessary data is properly filled in before trying again.";
                    
                }
                else if (bEffdateflag)
                {
                    strErrormsg = "Endtime is missing. Please verify that the necessary data is properly filled in before trying again.";
                }
                else if (bPointflag)
                {
                    strErrormsg = "Destination NG Point is missing. Please verify that the necessary data is properly filled in before trying again.";
                }
                if (strErrormsg != string.Empty)
                {
                    ErrorDialog.Show("Error", strErrormsg);
                    return new UiEventResult(EventStatus.Cancel);
                }
                
                //Get the ngpointconvey column values
                DateTime dtEffdate = (DateTime)celleffdate.Value;
                string strSourcepoint = vwNgpointconvey.Selected.Rows[0].Cells["sourcepoint"].Value.ToString();
                string strDestinationpoint = celldestinationpoint.Value.ToString();
                
                //get the pipeline column values
                string strPipeline = string.Empty;
                if (vwPipelines.Selected.Rows.Count > 0)
                {
                    var cellpipeline = vwPipelines.Selected.Rows[0].Cells["pipeline"];
                    strPipeline = cellpipeline.Value.ToString();
                }
                
                var args = new string[] { "effdate", "destinationpoint", "pipeline", "sourcepoint",  "returndatasource" };
                var parms = new object[] { dtEffdate, strDestinationpoint, strPipeline, strSourcepoint, vwdataset };
                DataSet returnDS = Soap.Invoke<DataSet>("NGConveyanceWS.asmx", "NGPointConveyance", args, parms);
                if (returnDS.Tables.Contains("DtValidation"))
                {
                    string strvalidation = string.Empty;
                    DataTable dtvalidation = returnDS.Tables["DtValidation"];
                    DataRow[] drvalidations = dtvalidation.Select();
                    foreach (DataRow drvalidation in drvalidations)
                    {
                        strvalidation = strvalidation + drvalidation["Message"].ToString() + ".";
                    }
                    MessageBox.Show(strvalidation, "NG Point Conveyance");
                }
            }
            //Validation before Shipper Account Conveyance process starts
            return new UiEventResult(EventStatus.Continue);
        }
        /* PipelineConvey_BeforeConveyanceProcess
        Input validatin and calls webmethod */
        public UiEventResult ToolClick_Pipeline_Conveyance_Before_1()
        {
            // History
            // Uptime : 22-10-2015 : a.kokin :: Created.
            
            #region Row validation
            
            var pipelineConveyPane = _view.ViewGrids["pipeline_conveyance"];
            
            if (pipelineConveyPane == null)
                return new UiEventResult(EventStatus.Cancel);
            
            var pipelineConveyRow = pipelineConveyPane.Selected.Rows.Cast<UltraGridRow>();
            var selectedRows = pipelineConveyRow as IList<UltraGridRow> ?? pipelineConveyRow.ToList();
            
            if (!selectedRows.Any())
            {
                ErrorDialog.Show("Error", "Please select a pipeline");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            if (selectedRows.Count != 1)
            {
                ErrorDialog.Show("Error", "Please select only one pipeline");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            #endregion Row validation
            
            #region Cells validation
            
            var cells = selectedRows.First().Cells;
            var effDate = cells["effdate"].Value.ToString();
            var sourcePipeline = cells["sourcepipeline"].Value.ToString();
            var destinationPipeline = cells["destinationpipeline"].Value.ToString();
            var status = cells["status"].Value.ToString();
            
            if (string.IsNullOrEmpty(effDate) || string.IsNullOrEmpty(destinationPipeline))
            {
                ErrorDialog.Show("Error", "Missing End time or Destination Pipeline.");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            // check if configurationdata has been running before
            if (!string.Equals(status, "assigned", StringComparison.CurrentCultureIgnoreCase))
            {
                ErrorDialog.Show("Error", "Configuration data for destination pipeline is not defined. Please run class event CopyConfigurationData first");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            #endregion Cells validation
            
            #region Server request
            
            var args = new[] { "effDate", "sourcePipeline", "destinationPipeline" };
            var parameters = new object[] { DateTime.Parse(effDate), sourcePipeline, destinationPipeline };
            try
            {
                var response = Soap.Invoke<string>("NGConveyanceWS.asmx", "PipelineConveyance", args, parameters);
                MessageBox.Show(response, "Pipeline Conveyance");
            }
            catch (Exception ex)
            {
                // TODO: Log exception;
                throw ex;
            }
            
            #endregion Server request
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

