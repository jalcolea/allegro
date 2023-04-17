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
    public class cst_loadermibgasinputCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        private int GetContractCount(string company, string counterparty)
        {
            int count = 0;
            DataTable contractDS = new DataTable("contract");
            
            string sql = "select count(*) " +
            "from contract c " +
            "inner join contractparty company on company.contract = c.contract " +
            "inner join contractparty counterparty on counterparty.contract = c.contract " +
            "where (company.endtime >= getdate() or company.endtime is null) " +
            "and (counterparty.endtime >= getdate() or counterparty.endtime is null) " +
            "and (c.termdate is null or c.termdate >= cast(getdate() as Date)) " +
            "and c.status = 'ACTIVE' and company.counterparty = '" + company + "' and counterparty.counterparty = '" + counterparty + "'";
            count = Soap.Invoke<int>("cst_loadermibgasinputWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return count;
        }
        
        private int GetMarketAreaCount(string pipeline, string point)
        {
            int count = 0;
            DataTable marketareaDS = new DataTable("ngpoint");
            string sql = "select count(*) from ngpoint " +
            "where pipeline = '" + pipeline + "' and point = '" + point + "'";
            count = Soap.Invoke<int>("cst_loadermibgasinputWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return count;
        }
        
        private string GetCompany()
        {
            string companyStr = string.Empty;
            string sql = "select allegrovalue from cst_tradeloaderdefaultdata where tradeloader = '100001' and target = 'Position.Company'";
            companyStr = Soap.Invoke<string>("cst_loadermibgasinputWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return companyStr;
        }
        #endregion
        
        /* TradeImporter - loadermibgasinput_afterupdatedata
        Trade Importer - DEV - After update information into Staging Table */
        public UiEventResult AfterUpdateData_1(object sender, EventArgs e)
        {
            /******************************************************************************
            NAME:  TradeImporter
            
            PURPOSE/DESCRIPTION/NOTES:    Trade Importer
            
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            try
            {
                List<UltraGridRow> selection = new List<UltraGridRow>();
                foreach (UltraGridRow gridrow in _view.ViewGrids["cst_mibgasimporter"].Rows)
                {
                    UltraGridRow gR = gridrow;
                    
                    if (gR.Cells["status"].Value.ToString() == "New")
                    {
                        selection.Add(gR);
                    }
                    
                }
                
                string idLoader = string.Empty;
                
                foreach (MetaDataDS.viewcolumnRow vc in this._view.MetaData.viewcolumn)
                {
                    if (vc.viewcolumn == "idloader" && !vc.IsviewdefaultNull())
                    {
                        idLoader = vc.viewdefault;
                        break;
                    }
                }
                
                if (!string.IsNullOrEmpty(idLoader))
                {
                    List<string> idDeals = new List<string>();
                    
                    foreach (UltraGridRow row in selection)
                    {
                        idDeals.Add(row.Cells["iddeal"].Value.ToString());
                    }
                    
                    bool value = Soap.Invoke<bool>(this._view.ClassUrl, "InsertTrades", new string[] { "idLoader", "idDeals" }, new object[] { idLoader, idDeals.ToArray() });
                    if (!value)
                    {
                        throw new Exception("Error when executing InsertTrades");
                    }
                }
                else
                {
                    List<string> idDeals = new List<string>();
                    
                    bool value = true;
                    
                    foreach (IGrouping<string, UltraGridRow> group in selection.GroupBy(r => r.Cells["idloader"].Value.ToString()))
                    {
                        if (!string.IsNullOrEmpty(group.Key))
                        {
                            idDeals.Clear();
                            
                            foreach (var row in group)
                            {
                                idDeals.Add(row.Cells["iddeal"].Value.ToString());
                            }
                            
                            if (!Soap.Invoke<bool>(this._view.ClassUrl, "InsertTrades", new string[] { "idLoader", "idDeals" }, new object[] { group.Key, idDeals.ToArray() }))
                            {
                                value = false;
                            }
                        }
                    }
                    
                    if (!value)
                    {
                        throw new Exception("Error when executing InsertTrades");
                    }
                }
                
                return new UiEventResult(EventStatus.Continue);
            }
            catch (Exception ex)
            {
                ErrorDialog.Show("Error", ex);
                
                return new UiEventResult(EventStatus.Cancel);
            }
        }
        /* TradeImporter - loadermibgasinput_populateids
        Trade Importer - DEV -Populates TransactionId and OrderId fields */
        public UiEventResult BeforeUpdateData_100(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /******************************************************************************
            NAME:  TradeImporter
            
            PURPOSE/DESCRIPTION/NOTES:    Trade Importer
            
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
            2.0        28/08/2019  (SSA)SAF         TradeImporter - The marketarea part is commented, it will be set in table cst_tradeloadertranslation
            3.0        17/02/2020  (SSA)SAF         TradeImporter - The counterparty is obtained from codtrade instead of from counterparty
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            foreach (var ugr in _view.ViewGrids["cst_mibgasimporter"].Rows)
            {
                if (ugr.Cells["transactionid"].Value == DBNull.Value)
                {
                    ugr.Cells["transactionid"].Value = ugr.Cells["codtrade"].Value.ToString().Substring(0, ugr.Cells["codtrade"].Value.ToString().IndexOf('_'));
                }
                
                if (ugr.Cells["orderid"].Value == DBNull.Value)
                {
                    ugr.Cells["orderid"].Value = ugr.Cells["codposition"].Value.ToString().Substring(0, ugr.Cells["codposition"].Value.ToString().IndexOf('_'));
                }
            }
            
            try
            {
                List<UltraGridRow> selection = new List<UltraGridRow>();
                
                string company = GetCompany();
                foreach (UltraGridRow gridrow in _view.ViewGrids["cst_mibgasimporter"].Rows)
                {
                    UltraGridRow gR = gridrow;
                    
                    if (gR.Cells["status"].Value.ToString() == "New")
                    {
                        string counterpartyST = gR.Cells["codtrade"].Value.ToString(); // The trade code contains the product, with the product we obtain MIBGAS or MIBGAS DERIVATIVES
                        string sqlCounterparty = "select allegrovalue from cst_tradeloadertranslation where target = 'Counterparty' and '" + counterpartyST + "' like  '%'+externvalue+'%'";
                        
                        string counterparty = Soap.Invoke<string>("cst_loadermibgasinputWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sqlCounterparty });
                        
                        /*string pipelineDesc = gR.Cells["logisticelement"].Value.ToString(); // 'Punto Virtual de Balance'
                        
                        string sqlPipeline = "select allegrovalue from cst_tradeloadertranslation where target = 'Pipeline'and externvalue = '"+ pipelineDesc + "'";
                        string pipeline = Soap.Invoke<string>("cst_loadermibgasinputWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sqlPipeline });
                        string sqlPoint = "select allegrovalue from cst_tradeloadertranslation where target = 'Point'and externvalue = '" + pipelineDesc + "'";
                        string point = Soap.Invoke<string>("cst_loadermibgasinputWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sqlPoint });
                        */
                        
                        int contract = GetContractCount(company, counterparty);
                        //int marketarea = GetMarketAreaCount(pipeline, point);
                        
                        if (contract != 1)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value = "Input data is not valid to generate a trade. Company must have only 1 contract with the counterparty selected.";
                        }
                        /*if (marketarea != 1)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value = "Input data is not valid to generate a trade. Point doesn't belong to the selected pipeline.";
                            }*/
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorDialog.Show("Error", ex);
                    
                    return new UiEventResult(EventStatus.Cancel);
                }
                
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - loadermibgasinput_DuplicateRowEvent
            Trade Importer - DEV -  Clean up data when a row is duplicated */
            public UiEventResult DuplicateRowEvent_cst_mibgasimporter_1(object sender, NewRowEventArgs e)
            {
                /******************************************************************************
                NAME:  TradeImporter
                
                PURPOSE/DESCRIPTION/NOTES:    Trade Importer
                
                REVISIONS:
                Ver        Date        Author           Description
                ---------  ----------  ---------------  ------------------------------------
                1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
                ---------  ----------  ---------------  ------------------------------------
                *********************************************************************************/
                e.NewRow.Cells["trade"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["position"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["status"].SetValue("New", false);
                e.NewRow.Cells["validation"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["transactionid"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["orderid"].SetValue(DBNull.Value, false);
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - loadermibgasinput_newrow_surrogate
            Trade Importer - DEV - Populates surrogate column with temp ID when into new row */
            public UiEventResult NewRowEvent_cst_mibgasimporter_1(object sender, NewRowEventArgs e)
            {
                /******************************************************************************
                NAME:  TradeImporter
                
                PURPOSE/DESCRIPTION/NOTES:    Trade Importer
                
                REVISIONS:
                Ver        Date        Author           Description
                ---------  ----------  ---------------  ------------------------------------
                1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
                ---------  ----------  ---------------  ------------------------------------
                *********************************************************************************/
                if (((ViewGrid)sender).GetBindRow(e.NewRow).Equals(null))
                    return new UiEventResult(EventStatus.Continue);
                
                int rowcount = 1;
                
                foreach (var row in _view.ViewGrids["cst_mibgasimporter"].Rows)
                {
                    if(row.Cells["iddeal"].Value != null && row.Cells["iddeal"].Value.ToString().StartsWith("0"))
                    {
                        rowcount++;
                    }
                }
                
                e.NewRow.Cells["iddeal"].SetValue("00" + (_view.DataSource.Tables["cst_mibgasimporter"].Columns["iddeal"].AutoIncrementStep * rowcount).ToString(), false);
                
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - loadermibgasinput_ImportData_ToolClick
            Trade Importer - DEV -  Import external file when clicking import button on tab */
            public UiEventResult ToolClick_Import_data_After_1()
            {
                /******************************************************************************
                NAME:  TradeImporter
                
                PURPOSE/DESCRIPTION/NOTES:    Trade Importer
                
                REVISIONS:
                Ver        Date        Author           Description
                ---------  ----------  ---------------  ------------------------------------
                1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
                1.1        12/07/2019  (SSA)SAF         TradeImporter - Condition GMES-GQES-GSES-GYES
                1.2        07/01/2020  (SSA)SAF         TradeImporter - Condition GBoMES
                ---------  ----------  ---------------  ------------------------------------
                *********************************************************************************/
                System.IO.FileStream stream = null;
                
                try
                {
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "Excel Files (*.xls; *.xlsx; *xlsm)| *.xls; *.xlsx; *.xlsm";
                    openDialog.Multiselect = false;
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        var idloader = (from vc in _view.MetaData.viewcolumn where vc.viewcolumn == "idloader" select vc.viewdefault).Single();
                        // BMR (SSA) Trade Importer MIBGAS
                        stream = new System.IO.FileStream(openDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                        var workbook = Infragistics.Documents.Excel.Workbook.Load(stream);
                        
                        //var workbook = Infragistics.Documents.Excel.Workbook.Load(openDialog.FileName);
                        var mibgasRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[5].Value != null && !r.Cells[5].Value.ToString().Contains("GMAES") && r.Index > 0 &&
                        !r.Cells[5].Value.ToString().Contains("GWEES")
                            && !r.Cells[5].Value.ToString().Contains("GMES")
                            && !r.Cells[5].Value.ToString().Contains("GBoMES")
                            && !r.Cells[5].Value.ToString().Contains("GQES")
                            && !r.Cells[5].Value.ToString().Contains("GSES")
                            && !r.Cells[5].Value.ToString().Contains("GYES")
                            
                        select r
                        ;
                        
                        foreach (var row in mibgasRows)
                        {
                            DateTime? operationdate;
                            if(row.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)row.Cells[0].Value);
                            }
                            else if(row.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(row.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = row.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = row.Cells[2].Value;
                            ugr.Cells["quantity"].Value = row.Cells[3].Value;
                            ugr.Cells["price"].Value = row.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = row.Cells[5].Value;
                            ugr.Cells["codposition"].Value = row.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = row.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = row.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = row.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = row.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        
                        //GMAES product
                        
                        var gmaesRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[7].Value != null && r.Cells[7].Value.ToString().Contains("GMAES")
                            group r by r.Cells[7].Value into g
                        select g;
                        
                        foreach(var row in gmaesRows)
                        {
                            var sampleRow = (from r in row
                            where (r.Cells[0].Value is double && DateTime.FromOADate((double)r.Cells[0].Value).Day == 1) ||
                            (r.Cells[0].Value is string && Convert.ToDateTime(r.Cells[0].Value).Day == 1)
                                select r).First()
                                ;
                            
                            DateTime? operationdate;
                            if (sampleRow.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)sampleRow.Cells[0].Value);
                            }
                            else if (sampleRow.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(sampleRow.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = sampleRow.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = sampleRow.Cells[2].Value;
                            // ugr.Cells["quantity"].Value = row.Sum(c => Convert.ToDecimal(c.Cells[3].Value));
                            ugr.Cells["quantity"].Value = Convert.ToDecimal(sampleRow.Cells[3].Value);
                            ugr.Cells["price"].Value = sampleRow.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = sampleRow.Cells[5].Value;
                            ugr.Cells["codposition"].Value = sampleRow.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = sampleRow.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = sampleRow.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = sampleRow.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = sampleRow.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        //GWEES product
                        var gweesRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[7].Value != null && r.Cells[7].Value.ToString().Contains("GWEES")
                            group r by r.Cells[7].Value into g
                        select g;
                        
                        foreach (var row in gweesRows)
                        {
                            var sampleRow = (from r in row
                            where (r.Cells[0].Value is double && DateTime.FromOADate((double)r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday) ||
                            (r.Cells[0].Value is string && Convert.ToDateTime(r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday)
                                select r).First()
                                ;
                            
                            DateTime? operationdate;
                            if (sampleRow.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)sampleRow.Cells[0].Value);
                            }
                            else if (sampleRow.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(sampleRow.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = sampleRow.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = sampleRow.Cells[2].Value;
                            // ugr.Cells["quantity"].Value = row.Sum(c =&gt; Convert.ToDecimal(c.Cells[3].Value));
                            ugr.Cells["quantity"].Value = Convert.ToDecimal(sampleRow.Cells[3].Value);
                            ugr.Cells["price"].Value = sampleRow.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = sampleRow.Cells[5].Value;
                            ugr.Cells["codposition"].Value = sampleRow.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = sampleRow.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = sampleRow.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = sampleRow.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = sampleRow.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        
                        //GMES product
                        var gmesRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[7].Value != null && r.Cells[7].Value.ToString().Contains("GMES")
                            group r by r.Cells[7].Value into g
                        select g;
                        
                        foreach (var row in gmesRows)
                        {
                            var sampleRow = (from r in row
                            where (r.Cells[0].Value is double && DateTime.FromOADate((double)r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday) ||
                            (r.Cells[0].Value is string && Convert.ToDateTime(r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday)
                                select r).First()
                                ;
                            
                            DateTime? operationdate;
                            if (sampleRow.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)sampleRow.Cells[0].Value);
                            }
                            else if (sampleRow.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(sampleRow.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = sampleRow.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = sampleRow.Cells[2].Value;
                            // ugr.Cells["quantity"].Value = row.Sum(c =&gt; Convert.ToDecimal(c.Cells[3].Value));
                            ugr.Cells["quantity"].Value = Convert.ToDecimal(sampleRow.Cells[3].Value);
                            ugr.Cells["price"].Value = sampleRow.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = sampleRow.Cells[5].Value;
                            ugr.Cells["codposition"].Value = sampleRow.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = sampleRow.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = sampleRow.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = sampleRow.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = sampleRow.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        
                        //GBoMES product
                        var GbomesRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[7].Value != null && r.Cells[7].Value.ToString().Contains("GBoMES")
                            group r by r.Cells[7].Value into g
                        select g;
                        
                        foreach (var row in GbomesRows)
                        {
                            var sampleRow = (from r in row
                            where (r.Cells[0].Value is double && DateTime.FromOADate((double)r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday) ||
                            (r.Cells[0].Value is string && Convert.ToDateTime(r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday)
                                select r).First()
                                ;
                            
                            DateTime? operationdate;
                            if (sampleRow.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)sampleRow.Cells[0].Value);
                            }
                            else if (sampleRow.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(sampleRow.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = sampleRow.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = sampleRow.Cells[2].Value;
                            // ugr.Cells["quantity"].Value = row.Sum(c =&gt; Convert.ToDecimal(c.Cells[3].Value));
                            ugr.Cells["quantity"].Value = Convert.ToDecimal(sampleRow.Cells[3].Value);
                            ugr.Cells["price"].Value = sampleRow.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = sampleRow.Cells[5].Value;
                            ugr.Cells["codposition"].Value = sampleRow.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = sampleRow.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = sampleRow.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = sampleRow.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = sampleRow.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        
                        //GQES product
                        var gqesRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[7].Value != null && r.Cells[7].Value.ToString().Contains("GQES")
                            group r by r.Cells[7].Value into g
                        select g;
                        
                        foreach (var row in gqesRows)
                        {
                            var sampleRow = (from r in row
                            where (r.Cells[0].Value is double && DateTime.FromOADate((double)r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday) ||
                            (r.Cells[0].Value is string && Convert.ToDateTime(r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday)
                                select r).First()
                                ;
                            
                            DateTime? operationdate;
                            if (sampleRow.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)sampleRow.Cells[0].Value);
                            }
                            else if (sampleRow.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(sampleRow.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = sampleRow.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = sampleRow.Cells[2].Value;
                            // ugr.Cells["quantity"].Value = row.Sum(c =&gt; Convert.ToDecimal(c.Cells[3].Value));
                            ugr.Cells["quantity"].Value = Convert.ToDecimal(sampleRow.Cells[3].Value);
                            ugr.Cells["price"].Value = sampleRow.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = sampleRow.Cells[5].Value;
                            ugr.Cells["codposition"].Value = sampleRow.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = sampleRow.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = sampleRow.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = sampleRow.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = sampleRow.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        
                        //GSES product
                        var gsesRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[7].Value != null && r.Cells[7].Value.ToString().Contains("GSES")
                            group r by r.Cells[7].Value into g
                        select g;
                        
                        foreach (var row in gsesRows)
                        {
                            var sampleRow = (from r in row
                            where (r.Cells[0].Value is double && DateTime.FromOADate((double)r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday) ||
                            (r.Cells[0].Value is string && Convert.ToDateTime(r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday)
                                select r).First()
                                ;
                            
                            DateTime? operationdate;
                            if (sampleRow.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)sampleRow.Cells[0].Value);
                            }
                            else if (sampleRow.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(sampleRow.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = sampleRow.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = sampleRow.Cells[2].Value;
                            // ugr.Cells["quantity"].Value = row.Sum(c =&gt; Convert.ToDecimal(c.Cells[3].Value));
                            ugr.Cells["quantity"].Value = Convert.ToDecimal(sampleRow.Cells[3].Value);
                            ugr.Cells["price"].Value = sampleRow.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = sampleRow.Cells[5].Value;
                            ugr.Cells["codposition"].Value = sampleRow.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = sampleRow.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = sampleRow.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = sampleRow.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = sampleRow.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        
                        //GYES product
                        var gyesRows = from r in workbook.Worksheets[0].Rows
                        where r.Cells[7].Value != null && r.Cells[7].Value.ToString().Contains("GYES")
                            group r by r.Cells[7].Value into g
                        select g;
                        
                        foreach (var row in gyesRows)
                        {
                            var sampleRow = (from r in row
                            where (r.Cells[0].Value is double && DateTime.FromOADate((double)r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday) ||
                            (r.Cells[0].Value is string && Convert.ToDateTime(r.Cells[0].Value).DayOfWeek == DayOfWeek.Saturday)
                                select r).First()
                                ;
                            
                            DateTime? operationdate;
                            if (sampleRow.Cells[0].Value is double)
                            {
                                operationdate = DateTime.FromOADate((double)sampleRow.Cells[0].Value);
                            }
                            else if (sampleRow.Cells[0].Value is string)
                            {
                                operationdate = Convert.ToDateTime(sampleRow.Cells[0].Value);
                            }
                            else
                            {
                                operationdate = null;
                            }
                            
                            UltraGridRow ugr = _view.ViewGrids["cst_mibgasimporter"].AppendRow();
                            ugr.Cells["idloader"].Value = idloader;
                            ugr.Cells["operationdate"].Value = operationdate;
                            ugr.Cells["operationtype"].Value = sampleRow.Cells[1].Value;
                            ugr.Cells["logisticelement"].Value = sampleRow.Cells[2].Value;
                            // ugr.Cells["quantity"].Value = row.Sum(c =&gt; Convert.ToDecimal(c.Cells[3].Value));
                            ugr.Cells["quantity"].Value = Convert.ToDecimal(sampleRow.Cells[3].Value);
                            ugr.Cells["price"].Value = sampleRow.Cells[4].Value;
                            ugr.Cells["codtrade"].Value = sampleRow.Cells[5].Value;
                            ugr.Cells["codposition"].Value = sampleRow.Cells[7].Value;
                            ugr.Cells["counterparty"].Value = sampleRow.Cells[8].Value;
                            ugr.Cells["operationstatus"].Value = sampleRow.Cells[9].Value;
                            ugr.Cells["sentstatus"].Value = sampleRow.Cells[10].Value;
                            ugr.Cells["conciliate"].Value = sampleRow.Cells[11].Value;
                            ugr.Cells["status"].Value = "New";
                        }
                        
                        
                        // BMR (SSA) - Trade Importer MIBGAS
                        stream.Close();
                        _view.UpdateData();
                    }
                    
                    return new UiEventResult(EventStatus.Continue);
                }
                catch (Exception ex)
                {
                    ErrorDialog.Show("Error", "Error importing Excel file.", ex);
                    return new UiEventResult(EventStatus.Cancel);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
            /* TradeImporter - loadermibgasinput_insertrades
            Trade Importer - DEV - Insert trades into Trade Execution */
            public UiEventResult ToolClick_Insert_Trades_After_1()
            {
                /******************************************************************************
                NAME:  TradeImporter
                
                PURPOSE/DESCRIPTION/NOTES:    Trade Importer
                
                REVISIONS:
                Ver        Date        Author           Description
                ---------  ----------  ---------------  ------------------------------------
                1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
                ---------  ----------  ---------------  ------------------------------------
                *********************************************************************************/
                try
                {
                    List<UltraGridRow> selection = _view.ViewGrids["cst_mibgasimporter"].GetSelectedRows();
                    string idLoader = string.Empty;
                    
                    foreach (MetaDataDS.viewcolumnRow vc in _view.MetaData.viewcolumn)
                    {
                        if (vc.viewcolumn == "idloader" && !vc.IsviewdefaultNull())
                        {
                            idLoader = vc.viewdefault;
                            break;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(idLoader))
                    {
                        List<string> idDeals = new List<string>();
                        
                        foreach (UltraGridRow row in selection)
                        {
                            idDeals.Add(row.Cells["iddeal"].Value.ToString());
                        }
                        
                        bool value = Soap.Invoke<bool>(_view.ClassUrl, "InsertTrades", new string[] { "idLoader", "idDeals" }, new object[] { idLoader, idDeals.ToArray() });
                        if (!value)
                        {
                            ErrorDialog.Show("Error", "Error when inserting trades in the system.", "Please, check the Allegro log to see further details.");
                            return new UiEventResult(EventStatus.Cancel);
                        }
                    }
                    else
                    {
                        List<string> idDeals = new List<string>();
                        
                        bool value = true;
                        
                        foreach (IGrouping<string, UltraGridRow> group in selection.GroupBy(r => r.Cells["idloader"].Value.ToString()))
                        {
                            if (!string.IsNullOrEmpty(group.Key))
                            {
                                idDeals.Clear();
                                
                                foreach (var row in group)
                                {
                                    idDeals.Add(row.Cells["iddeal"].Value.ToString());
                                }
                                
                                if (!Soap.Invoke<bool>(_view.ClassUrl, "InsertTrades", new string[] { "idLoader", "idDeals" }, new object[] { group.Key, idDeals.ToArray() }))
                                {
                                    value = false;
                                }
                            }
                        }
                        
                        if (!value)
                        {
                            ErrorDialog.Show("Error", "Error when inserting trades in the system.", "Please, check the Allegro log to see further details.");
                            return new UiEventResult(EventStatus.Cancel);
                        }
                    }
                    
                    return new UiEventResult(EventStatus.Continue);
                }
                catch (Exception ex)
                {
                    ErrorDialog.Show("Error", ex);
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            
            
        }
    }
    
