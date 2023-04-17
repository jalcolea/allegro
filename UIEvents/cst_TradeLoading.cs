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
    public class cst_TradeLoadingCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        private int GetReadyToInvoice(string position, string trade)
        {
            
            string sql = string.Format("select count(*) from position where cst_readytoinvoice=1 and cst_thothposition='{0}' and cst_thothtrade='{1}'", position, trade);
            int readytoinvoice = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return readytoinvoice;
        }
        
        private int GetCountCounterpartyContract(string counterparty, string contract)
        {
            string sql = string.Format("select count(*) from contractparty where counterparty='{0}' and contract='{1}'", counterparty, contract);
            int countcounterpartycontract = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countcounterpartycontract;
        }
        
        private int GetCountCounterparty(string counterparty)
        {
            string sql = string.Format("select count(*) from counterparty where counterparty='{0}' ", counterparty);
            int counterpartycount = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return counterpartycount;
        }
        
        private int GetCountTrade(string trade)
        {
            string sql = string.Format("select count(*) from trade where trade='{0}' ", trade);
            int counttrade = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return counttrade;
        }
        
        private int GetCountThothTrade(string thothtrade)
        {
            string sql = string.Format("select count(*) from trade where cst_thothtrade='{0}' ", thothtrade);
            int counttrade = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return counttrade;
        }
        
        private string GetTrader(string userid)
        {
            string sql = string.Format("select top 1 name from userid where  userid='{0}' ", userid);
            string trader = Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return trader;
        }
        
        private int GetCountContractPaymenttermsPhysical(string paymentterms, string contract, string product, string pricecurrency)
        {
            string sql = string.Format("select count(*) from contractacctg where paymentterms='{0}' and contract='{1}' and product='{2}' and currency ='{3}' ", paymentterms, contract, product, pricecurrency);
            int countcontractpaymentterms = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countcontractpaymentterms;
        }
        
        private int GetCountContractPaymenttermsFinancial(string paymentterms, string contract, string pricecurrency)
        {
            string sql = string.Format("select count(*) from contractacctg where paymentterms='{0}' and contract='{1}' and product = 'FINANCIAL SWAP BILATERAL' and currency ='{2}' ", paymentterms, contract, pricecurrency);
            int countcontractpaymentterms = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countcontractpaymentterms;
        }
        
        private int GetCountContractPaymenttermsLNGCargo(string paymentterms, string contract, string pricecurrency)
        {
            string sql = string.Format("select count(*) from contractacctg where paymentterms='{0}' and contract='{1}' and product = 'LNG' and currency ='{2}' ", paymentterms, contract, pricecurrency);
            int countcontractpaymentterms = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countcontractpaymentterms;
        }
        
        private int GetCountUnit(string unit)
        {
            string sql = string.Format("select count(*) from unit where unit='{0}' ", unit);
            int countunit = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countunit;
        }
        
        private int GetCountPriceIndex(string priceindex)
        {
            string sql = string.Format("select count(*) from priceindex where priceindex='{0}' ", priceindex);
            int countpriceindex = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpriceindex;
        }
        
        private int GetCountStrategyid(string strategyid)
        {
            string sql = string.Format("select count(*) from cst_strategymanagement where strategyid='{0}' ", strategyid);
            int countpriceindex = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpriceindex;
        }
        
        private int GetCountUserid(string userid)
        {
            string sql = string.Format("select count(*) from userid where userid='{0}' ", userid);
            int countuserid = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countuserid;
        }
        
        private int GetCountCurrency(string currency)
        {
            string sql = string.Format("select count(*) from currency where currency='{0}' ", currency);
            int countcurrency = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countcurrency;
        }
        
        private int GetCountPipeline(string pipeline)
        {
            string sql = string.Format("select count(*) from pipeline where pipeline='{0}' ", pipeline);
            int countpipeline = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpipeline;
        }
        
        private int GetCountPipelinePoint(string pipeline, string point)
        {
            string sql = string.Format("select count(*) from ngpoint where pipeline='{0}' and point='{1}'", pipeline, point);
            int countpipelinepoint = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpipelinepoint;
        }
        
        private int GetCountLocation(string location)
        {
            string sql = string.Format("select count(*) from location where location='{0}' ", location);
            int countlocation = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countlocation;
        }
        
        private int GetCountSetcurrency(string setcurrency)
        {
            string sql = string.Format("select count(*) from currency where currency='{0}' ", setcurrency);
            int countsetcurrency = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countsetcurrency;
        }
        
        private int GetCountSetunit(string setunit)
        {
            string sql = string.Format("select count(*) from unit where unit='{0}' ", setunit);
            int countsetunit = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countsetunit;
        }
        
        private int GetCountPricecurrency(string pricecurrency)
        {
            string sql = string.Format("select count(*) from currency where currency='{0}' ", pricecurrency);
            int countpricecurrency = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpricecurrency;
        }
        
        private int GetCountPriceunit(string priceunit)
        {
            string sql = string.Format("select count(*) from unit where unit='{0}' ", priceunit);
            int countpriceunit = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpriceunit;
        }
        
        private int GetCountTargetstrategy(string targetstrategyid)
        {
            string sql = string.Format("select count(*) from cst_strategymanagement where strategyid='{0}' ", targetstrategyid);
            int countpriceunit = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpriceunit;
        }
        
        private int GetCountTargetuser(string targetuserid)
        {
            string sql = string.Format("select count(*) from userid where useryid='{0}' ", targetuserid);
            int countpriceunit = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countpriceunit;
        }
        
        private int GetCountOrigin(string origin)
        {
            string sql = string.Format("select count(*) from location where location='{0}' ", origin);
            int countorigin = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countorigin;
        }
        
        private int GetCountIncoterms(string incoterms)
        {
            string sql = string.Format("select count(*) from incoterms where incoterms='{0}' ", incoterms);
            int countincoterms = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countincoterms;
        }
        
        private int GetCountProduct(string product)
        {
            string sql = string.Format("select count(*) from product where product='{0}' ", product);
            int countproduct = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countproduct;
        }
        
        private int GetCountMarketArea(string marketarea)
        {
            
            string sql = string.Format("select count(*) from location where market=1 and marketarea='{0}' ", marketarea);
            int countmarketarea = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            return countmarketarea;
        }
        #endregion
        
        /* TradeImporter - Avoid delete
        Trade Importer -DEV- Avoid delete completed rows */
        public UiEventResult BeforeRowsDeleted_cst_demandimporter_1(object sender, BeforeRowsDeletedEventArgs e)
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
            List<DataRow> ctTradeImporterRowsToDelete = new List<DataRow>();
            for (int i = 0; i < e.Rows.Length; i++)
            {
                ctTradeImporterRowsToDelete.Add(((ViewGrid)sender).GetBindRow(e.Rows[i]));
            }
            
            foreach (DataRow cstTradeImporterRow in ctTradeImporterRowsToDelete)
            {
                if (cstTradeImporterRow.Field<string>("status").ToLower() == "completed" && !string.IsNullOrEmpty(cstTradeImporterRow.Field<string>("trade")))
                {
                    ErrorDialog.Show("Error", "This row cannot be deleted because it was processed succesfully previously.");
                    
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeImporter - TradeLoader_beforeupdatedata
        Trade Importer - DEV - Before update information into Trade Loading */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
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
            bool deleted = false;
            #region MIBGAS
            if (_view.Name == "Automatic Mibgas Interface")
            {
                foreach (DataRow row in _view.DataSource.Tables["cst_loadermibgasauto"].Select("", "", DataViewRowState.Deleted))
                {
                    row.RejectChanges();
                    deleted = true;
                }
                if (deleted)
                {
                    MessageBox.Show("It is not permitted to remove existing rows, action reverted.");
                    this._view.RetrieveData();
                }
            }
            #endregion
            
            #region Trade Importer - NOM
            else if (_view.Name == "Trade Importer - NOM")
            {
                foreach (UltraGridRow gridrow in this._view.ViewGrids[0].Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    //if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error" && string.IsNullOrEmpty(gR.Cells["creationname"].Value.ToString().Trim()))
                        //{
                        //    if (gR.Cells["idposition"].Value != DBNull.Value)
                            //    {
                            //        string position = gR.Cells["idposition"].Value.ToString().Trim();
                            //        string trade = gR.Cells["idtrade"].Value.ToString().Trim();
                            
                            //        int readytoinvoice = GetReadyToInvoice(position, trade);
                            //        if (readytoinvoice == 1)
                                //        {
                                //            gR.Cells["status"].Value = "Error";
                                //            gR.Cells["validation"].Value += "The position can't be updated, Ready To Invoice field is checked. Please modify this field in Trade Execution view.";
                                
                            //        }
                        //    }
                        
                        
                    //}
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        
                        string counterparty = gR.Cells["counterparty"].Value.ToString().Trim().Trim();
                        string contract = gR.Cells["contract"].Value.ToString().Trim();
                        string currency = gR.Cells["currency"].Value.ToString().Trim();
                        string unit = gR.Cells["unit"].Value.ToString().Trim();
                        string priceindex = gR.Cells["priceindex"].Value.ToString().Trim();
                        string strategyid = gR.Cells["strategyid"].Value.ToString().Trim();
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        string idposition = gR.Cells["idposition"].Value.ToString().Trim();
                        string cst_readytoinvoice = gR.Cells["cst_readytoinvoice"].Value.ToString().Trim();
                        
                        
                        if (!string.IsNullOrEmpty(idposition))
                        {
                            string position = gR.Cells["idposition"].Value.ToString().Trim();
                            string trade = gR.Cells["idtrade"].Value.ToString().Trim();
                            
                            int readytoinvoice = GetReadyToInvoice(position, trade);
                            if (readytoinvoice == 1)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "The position can't be updated, Ready To Invoice field is checked. Please modify this field in Trade Execution view.";
                                
                            }
                        }
                        
                        if (string.IsNullOrEmpty(cst_readytoinvoice))
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Ready to Invoice  can't be null";
                            
                            
                        }
                        else if (cst_readytoinvoice != "True" && cst_readytoinvoice != "False")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Ready to Invoice  " + cst_readytoinvoice + " not valid";
                        }
                        
                        
                        
                        
                        int countcounterpartycontract = GetCountCounterpartyContract(counterparty, contract);
                        if (countcounterpartycontract == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Contract " + contract + " doesn't exist for Counterparty " + counterparty;
                            
                        }
                        
                        int countcounterparty = GetCountCounterparty(counterparty);
                        if (countcounterparty == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Counterparty " + counterparty + " doesn't exist";
                            
                        }
                        
                        
                        int countunit = GetCountUnit(unit);
                        if (countunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Unit " + unit + " doesn't exist";
                            
                        }
                        if (!string.IsNullOrEmpty(priceindex))
                        {
                            int countpriceindex = GetCountPriceIndex(priceindex);
                            if (countpriceindex == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Price Index  " + priceindex + " doesn't exist";
                                
                            }
                        }
                        int countstrategyid = GetCountStrategyid(strategyid);
                        if (countstrategyid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "ID Strategy  " + strategyid + " doesn't exist";
                            
                        }
                        
                        int countuserid = GetCountUserid(userid);
                        if (countuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                            
                        }
                        
                        if (!string.IsNullOrEmpty(currency))
                        {
                            int countcurrency = GetCountCurrency(currency);
                            if (countcurrency == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Currency  " + currency + " doesn't exist";
                                
                            }
                        }
                    }
                    
                    
                    
                    
                }
            }
            #endregion
            
            #region Trade Importer - PHYSICAL
            else if (_view.Name == "Trade Importer - PHYSICAL")
            {
                foreach (UltraGridRow gridrow in this._view.ViewGrids[0].Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        
                        string counterparty = gR.Cells["counterparty"].Value.ToString().Trim();
                        string contract = gR.Cells["contract"].Value.ToString().Trim();
                        string paymentterms = gR.Cells["paymentterms"].Value.ToString().Trim();
                        string product = gR.Cells["product"].Value.ToString().Trim();
                        string unit = gR.Cells["unit"].Value.ToString().Trim();
                        string priceindex = gR.Cells["priceindex"].Value.ToString().Trim();
                        string strategyid = gR.Cells["strategyid"].Value.ToString().Trim();
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        string tradetype = gR.Cells["tradetype"].Value.ToString().Trim();
                        string postype = gR.Cells["postype"].Value.ToString().Trim();
                        string timeunit = gR.Cells["timeunit"].Value.ToString().Trim();
                        string pipeline = gR.Cells["pipeline"].Value.ToString().Trim();
                        string point = gR.Cells["point"].Value.ToString().Trim();
                        string location = gR.Cells["location"].Value.ToString().Trim();
                        string priceunit = gR.Cells["priceunit"].Value.ToString().Trim();
                        string pricecurrency = gR.Cells["pricecurrency"].Value.ToString().Trim();
                        string settlementunit = gR.Cells["settlementunit"].Value.ToString().Trim();
                        string settlementcurrency = gR.Cells["settlementcurrency"].Value.ToString().Trim();
                        
                        int countcounterparty = GetCountCounterparty(counterparty);
                        if (countcounterparty == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Counterparty " + counterparty + " doesn't exist";
                            
                        }
                        
                        int countcounterpartycontract = GetCountCounterpartyContract(counterparty, contract);
                        if (countcounterpartycontract == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Contract " + contract + " doesn't exist for Counterparty " + counterparty;
                            
                        }
                        if (!string.IsNullOrEmpty(paymentterms))
                        {
                            int countcontractpaymentterms = GetCountContractPaymenttermsPhysical(paymentterms, contract, product, pricecurrency);
                            if (countcontractpaymentterms == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Payment Terms " + paymentterms + " doesn't exist for Contract " + contract;
                                
                            }
                        }
                        
                        int countunit = GetCountUnit(unit);
                        if (countunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Unit " + unit + " doesn't exist";
                            
                        }
                        if (!string.IsNullOrEmpty(priceindex))
                        {
                            int countpriceindex = GetCountPriceIndex(priceindex);
                            if (countpriceindex == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Price Index  " + priceindex + " doesn't exist";
                                
                            }
                        }
                        int countstrategyid = GetCountStrategyid(strategyid);
                        if (countstrategyid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "ID Strategy  " + strategyid + " doesn't exist";
                            
                        }
                        
                        int countuserid = GetCountUserid(userid);
                        if (countuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                            
                        }
                        
                        if (product == "NG" && (string.IsNullOrEmpty(pipeline) || string.IsNullOrEmpty(point)))
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Pipeline and Point are mandatory for product " + product;
                            
                        }
                        
                        if (tradetype != "PHYSICAL BILATERAL" && tradetype != "PHYSICAL SWAP" && tradetype != " ORIGINATION PHYSICAL BILATERAL")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Tradetype  " + tradetype + " is not allowed fot PHYSICAL Trade Importer";
                        }
                        
                        if (postype != "BUY" && postype != "SELL")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Postype  " + postype + " doesn't exist";
                        }
                        if (!string.IsNullOrEmpty(timeunit))
                        {
                            if (timeunit != "MONTH" && timeunit != "DAY" && timeunit != "TOTAL" && timeunit != "HOUR")
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Time Unit  " + timeunit + " doesn't exist";
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(pipeline))
                        {
                            int countpipeline = GetCountPipeline(pipeline);
                            if (countpipeline == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Pipeline  " + pipeline + " doesn't exist";
                                
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(pipeline) && !string.IsNullOrEmpty(point))
                        {
                            int countpipelinepoint = GetCountPipelinePoint(pipeline, point);
                            if (countpipelinepoint == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Point " + point + " doesn't exist for pipeline " + pipeline;
                                
                            }
                        }
                        if (!string.IsNullOrEmpty(location))
                        {
                            int countlocation = GetCountLocation(location);
                            if (countlocation == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Location  " + location + " doesn't exist";
                                
                            }
                        }
                        
                        if (string.IsNullOrEmpty(location) && product == "LNG")
                        {
                            
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Location can't be empty for product " + product;
                            
                            
                        }
                        
                        int countproduct = GetCountProduct(product);
                        if (countproduct == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Product  " + product + " doesn't exist";
                            
                        }
                        
                        int countsetcurrency = GetCountSetcurrency(settlementcurrency);
                        if (countsetcurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement currency  " + pricecurrency + " doesn't exist";
                            
                        }
                        
                        int countsetunit = GetCountSetunit(settlementunit);
                        if (countsetunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement Unit  " + settlementunit + " doesn't exist";
                            
                        }
                        
                        int countpricecurrency = GetCountPricecurrency(pricecurrency);
                        if (countpricecurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price currency  " + pricecurrency + " doesn't exist";
                            
                        }
                        
                        int countpriceunit = GetCountPriceunit(priceunit);
                        if (countpriceunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price Unit  " + priceunit + " doesn't exist";
                            
                        }
                        
                        if (product == "NG" && !string.IsNullOrEmpty(location))
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Location must be empty for product  " + product;
                        }
                        
                        if (product == "LNG" && (!string.IsNullOrEmpty(pipeline) || !string.IsNullOrEmpty(point)))
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Point and/or Pipeline must be empty for product  " + product;
                        }
                    }
                    
                }
            }
            #endregion
            
            #region Trade Importer - FINANCIAL
            else if (_view.Name == "Trade Importer - FINANCIAL")
            {
                foreach (UltraGridRow gridrow in this._view.ActiveGrid.Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        
                        string counterparty = gR.Cells["counterparty"].Value.ToString().Trim();
                        string contract = gR.Cells["contract"].Value.ToString().Trim();
                        string paymentterms = gR.Cells["paymentterms"].Value.ToString().Trim();
                        
                        string unit = gR.Cells["unit"].Value.ToString().Trim();
                        string priceindex = gR.Cells["priceindex"].Value.ToString().Trim();
                        string strategyid = gR.Cells["strategyid"].Value.ToString().Trim();
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        string timeunit = gR.Cells["timeunit"].Value.ToString().Trim();
                        string priceunit = gR.Cells["priceunit"].Value.ToString().Trim();
                        string pricecurrency = gR.Cells["pricecurrency"].Value.ToString().Trim();
                        string settlementunit = gR.Cells["settlementunit"].Value.ToString().Trim();
                        string settlementcurrency = gR.Cells["settlementcurrency"].Value.ToString().Trim();
                        
                        int countcounterparty = GetCountCounterparty(counterparty);
                        if (countcounterparty == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Counterparty " + counterparty + " doesn't exist";
                            
                        }
                        
                        int countcounterpartycontract = GetCountCounterpartyContract(counterparty, contract);
                        if (countcounterpartycontract == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Contract " + contract + " doesn't exist for Counterparty " + counterparty;
                            
                        }
                        
                        
                        int countcontractpaymentterms = GetCountContractPaymenttermsFinancial(paymentterms, contract, pricecurrency);
                        if (countcontractpaymentterms == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Payment Terms " + paymentterms + " doesn't exist for Contract " + contract;
                            
                        }
                        
                        int countunit = GetCountUnit(unit);
                        if (countunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Unit " + unit + " doesn't exist";
                            
                        }
                        if (!string.IsNullOrEmpty(priceindex))
                        {
                            int countpriceindex = GetCountPriceIndex(priceindex);
                            if (countpriceindex == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Price Index  " + priceindex + " doesn't exist";
                                
                            }
                        }
                        int countstrategyid = GetCountStrategyid(strategyid);
                        if (countstrategyid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "ID Strategy  " + strategyid + " doesn't exist";
                            
                        }
                        
                        int countuserid = GetCountUserid(userid);
                        if (countuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                            
                        }
                        
                        if (timeunit != "MONTH" && timeunit != "DAY" && timeunit != "TOTAL" && timeunit != "HOUR")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Time Unit  " + timeunit + " doesn't exist";
                        }
                        
                        int countsetcurrency = GetCountSetcurrency(settlementcurrency);
                        if (countsetcurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement currency  " + settlementcurrency + " doesn't exist";
                            
                        }
                        
                        int countsetunit = GetCountSetunit(settlementunit);
                        if (countsetunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement Unit  " + settlementunit + " doesn't exist";
                            
                        }
                        
                        int countpricecurrency = GetCountPricecurrency(pricecurrency);
                        if (countpricecurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price currency  " + pricecurrency + " doesn't exist";
                            
                        }
                        
                        int countpriceunit = GetCountPriceunit(priceunit);
                        if (countpriceunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price Unit  " + priceunit + " doesn't exist";
                            
                        }
                        
                        
                    }
                    
                }
            }
            #endregion
            
            #region Trade Importer - VIRTUAL FINSWAP
            else if (_view.Name == "Trade Importer - VIRTUAL FINSWAP")
            {
                foreach (UltraGridRow gridrow in this._view.ActiveGrid.Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        string unit = gR.Cells["unit"].Value.ToString().Trim();
                        string priceindex = gR.Cells["priceindex"].Value.ToString().Trim();
                        string strategyid = gR.Cells["strategyid"].Value.ToString().Trim();
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        string timeunit = gR.Cells["timeunit"].Value.ToString().Trim();
                        string priceunit = gR.Cells["priceunit"].Value.ToString().Trim();
                        string pricecurrency = gR.Cells["pricecurrency"].Value.ToString().Trim();
                        string settlementunit = gR.Cells["settlementunit"].Value.ToString().Trim();
                        string settlementcurrency = gR.Cells["settlementcurrency"].Value.ToString().Trim();
                        string targetstrategyid = gR.Cells["targetstrategyid"].Value.ToString().Trim();
                        string targetuserid = gR.Cells["targettrader"].Value.ToString().Trim();
                        
                        
                        
                        int countunit = GetCountUnit(unit);
                        if (countunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Unit " + unit + " doesn't exist";
                            
                        }
                        if (!string.IsNullOrEmpty(priceindex))
                        {
                            int countpriceindex = GetCountPriceIndex(priceindex);
                            if (countpriceindex == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Price Index  " + priceindex + " doesn't exist";
                                
                            }
                        }
                        int countstrategyid = GetCountStrategyid(strategyid);
                        if (countstrategyid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "ID Strategy  " + strategyid + " doesn't exist";
                            
                        }
                        
                        int countuserid = GetCountUserid(userid);
                        if (countuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                            
                        }
                        
                        if (timeunit != "MONTH" && timeunit != "DAY" && timeunit != "TOTAL" && timeunit != "HOUR")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Time Unit  " + timeunit + " doesn't exist";
                        }
                        
                        int countsetcurrency = GetCountSetcurrency(settlementcurrency);
                        if (countsetcurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement currency  " + settlementcurrency + " doesn't exist";
                            
                        }
                        
                        int countsetunit = GetCountSetunit(settlementunit);
                        if (countsetunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement Unit  " + settlementunit + " doesn't exist";
                            
                        }
                        
                        int countpricecurrency = GetCountPricecurrency(pricecurrency);
                        if (countpricecurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price currency  " + pricecurrency + " doesn't exist";
                            
                        }
                        
                        int countpriceunit = GetCountPriceunit(priceunit);
                        if (countpriceunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price Unit  " + priceunit + " doesn't exist";
                            
                        }
                        
                        int counttargetstrategy = GetCountTargetstrategy(targetstrategyid);
                        if (counttargetstrategy == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Target Strategy ID  " + targetstrategyid + " doesn't exist";
                            
                        }
                        
                        int counttargetuser = GetCountUserid(targetuserid);
                        if (counttargetuser == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Target Trader  " + targetuserid + " doesn't exist";
                            
                        }
                    }
                    
                }
            }
            #endregion
            
            #region Trade Importer - VIRTUAL PHYSESP
            else if (_view.Name == "Trade Importer - VIRTUAL PHYSESP")
            {
                foreach (UltraGridRow gridrow in this._view.ActiveGrid.Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        
                        string priceindex = gR.Cells["priceindex"].Value.ToString().Trim();
                        string strategyid = gR.Cells["strategyid"].Value.ToString().Trim();
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        string product = gR.Cells["operationproduct"].Value.ToString().Trim();
                        string operationtype = gR.Cells["operationtype"].Value.ToString().Trim();
                        string targetstrategyid = gR.Cells["targetstrategyid"].Value.ToString().Trim();
                        string targettrader = gR.Cells["targettrader"].Value.ToString().Trim();
                        
                        if (!string.IsNullOrEmpty(priceindex))
                        {
                            int countpriceindex = GetCountPriceIndex(priceindex);
                            if (countpriceindex == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Price Index  " + priceindex + " doesn't exist";
                                
                            }
                        }
                        int countstrategyid = GetCountStrategyid(strategyid);
                        if (countstrategyid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "ID Strategy  " + strategyid + " doesn't exist";
                            
                        }
                        
                        int countuserid = GetCountUserid(userid);
                        if (countuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                            
                        }
                        
                        int countproduct = GetCountProduct(product);
                        if (countproduct == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Product  " + product + " doesn't exist";
                            
                        }
                        
                        int counttargetstrategy = GetCountTargetstrategy(targetstrategyid);
                        if (counttargetstrategy == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Target Strategy ID  " + targetstrategyid + " doesn't exist";
                            
                        }
                        
                        int counttargetuserid = GetCountUserid(targettrader);
                        if (counttargetuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Target trader  " + targettrader + " doesn't exist";
                            
                        }
                        
                        
                        if (operationtype != "Compra" && operationtype != "Venta")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Operation type  " + operationtype + " doesn't exist";
                        }
                    }
                    
                }
            }
            #endregion
            
            #region Trade Importer - TACTICAL IW
            else if (_view.Name == "Trade Importer - TACTICAL IW")
            {
                foreach (UltraGridRow gridrow in this._view.ViewGrids[0].Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        string targettrader = gR.Cells["targettrader"].Value.ToString().Trim();
                        
                        string product = gR.Cells["product"].Value.ToString().Trim();
                        string postype = gR.Cells["postype"].Value.ToString().Trim();
                        string unit = gR.Cells["unit"].Value.ToString().Trim();
                        string strategyid = gR.Cells["strategyid"].Value.ToString().Trim();
                        //  string targetstrategyid = gR.Cells["targetstrategyid"].Value.ToString().Trim();
                        string timeunit = gR.Cells["timeunit"].Value.ToString().Trim();
                        
                        string pipeline = gR.Cells["pipeline"].Value.ToString().Trim();
                        string point = gR.Cells["point"].Value.ToString().Trim();
                        string location = gR.Cells["location"].Value.ToString().Trim();
                        
                        
                        
                        string referencetrade = gR.Cells["referencefirmtrade"].Value.ToString().Trim();
                        string[] firmTrades = referencetrade.Split(',');
                        foreach (string trade in firmTrades)
                        {
                            int counttrade = GetCountThothTrade(trade);
                            if (counttrade == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Reference Thoth Trade " + referencetrade + " doesn't exist.";
                                
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(userid))
                        {
                            int countuserid = GetCountUserid(userid);
                            if (countuserid == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                                
                            }
                            
                            int counttargetuserid = GetCountUserid(targettrader);
                            if (counttargetuserid == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Target trader  " + targettrader + " doesn't exist";
                                
                            }
                            
                            
                            int countproduct = GetCountProduct(product);
                            if (countproduct == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Product  " + product + " doesn't exist";
                                
                            }
                            
                            if (postype != "BUY" && postype != "SELL")
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Postype  " + postype + " doesn't exist";
                            }
                            
                            int countunit = GetCountUnit(unit);
                            if (countunit == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Unit " + unit + " doesn't exist";
                                
                            }
                            
                            int countstrategyid = GetCountStrategyid(strategyid);
                            if (countstrategyid == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "ID Strategy  " + strategyid + " doesn't exist";
                                
                            }
                            
                            //int counttargetstrategy = GetCountTargetstrategy(targetstrategyid);
                            //if (counttargetstrategy == 0)
                                //{
                                //    gR.Cells["status"].Value = "Error";
                                //    gR.Cells["validation"].Value += "Strategy ID  " + targetstrategyid + " doesn't exist";
                                
                            //}
                            
                            if (!string.IsNullOrEmpty(timeunit))
                            {
                                if (timeunit != "MONTH" && timeunit != "DAY" && timeunit != "TOTAL" && timeunit != "HOUR")
                                {
                                    gR.Cells["status"].Value = "Error";
                                    gR.Cells["validation"].Value += "Time Unit  " + timeunit + " doesn't exist";
                                }
                            }
                            
                            if (!string.IsNullOrEmpty(pipeline))
                            {
                                int countpipeline = GetCountPipeline(pipeline);
                                if (countpipeline == 0)
                                {
                                    gR.Cells["status"].Value = "Error";
                                    gR.Cells["validation"].Value += "Pipeline  " + pipeline + " doesn't exist";
                                    
                                }
                            }
                            
                            if (!string.IsNullOrEmpty(pipeline) && !string.IsNullOrEmpty(point))
                            {
                                int countpipelinepoint = GetCountPipelinePoint(pipeline, point);
                                if (countpipelinepoint == 0)
                                {
                                    gR.Cells["status"].Value = "Error";
                                    gR.Cells["validation"].Value += "Point " + point + " doesn't exist for pipeline " + pipeline;
                                    
                                }
                            }
                            if (!string.IsNullOrEmpty(location))
                            {
                                int countlocation = GetCountLocation(location);
                                if (countlocation == 0)
                                {
                                    gR.Cells["status"].Value = "Error";
                                    gR.Cells["validation"].Value += "Location  " + location + " doesn't exist";
                                    
                                }
                            }
                            
                            if (string.IsNullOrEmpty(location) && product == "LNG")
                            {
                                
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Location can't be empty for product " + product;
                                
                                
                            }
                            
                            
                            if (product == "NG" && !string.IsNullOrEmpty(location))
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Location must be empty for product  " + product;
                            }
                            
                            if (product == "LNG" && (!string.IsNullOrEmpty(pipeline) || !string.IsNullOrEmpty(point)))
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Point and/or Pipeline must be empty for product  " + product;
                            }
                            
                            if (product == "NG" && (string.IsNullOrEmpty(pipeline) || string.IsNullOrEmpty(point)))
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Pipeline and Point are mandatory for product " + product;
                                
                            }
                            
                            
                        }
                        
                    }
                    
                }
            }
            #endregion
            
            #region Trade Importer - DEMAND
            else if (_view.Name == "Trade Importer - DEMAND")
            {
                foreach (UltraGridRow gridrow in this._view.ActiveGrid.Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        string priceindex = gR.Cells["priceindex"].Value.ToString().Trim();
                        string unit = gR.Cells["unit"].Value.ToString().Trim();
                        
                        int countuserid = GetCountUserid(userid);
                        if (countuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                            
                        }
                        if (!string.IsNullOrEmpty(priceindex))
                        {
                            int countpriceindex = GetCountPriceIndex(priceindex);
                            if (countpriceindex == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Price Index  " + priceindex + " doesn't exist";
                                
                            }
                        }
                        
                        int countunit = GetCountUnit(unit);
                        if (countunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Unit " + unit + " doesn't exist";
                            
                        }
                        
                    }
                    
                }
            }
            
            
            
            #endregion
            
            #region Trade Importer - LNG CARGO
            else if (_view.Name == "Trade Importer - LNG CARGO")
            {
                foreach (UltraGridRow gridrow in this._view.ActiveGrid.Rows)
                {
                    UltraGridRow gR = gridrow;
                    DataRow row = gR.GetDataRow();
                    
                    if (gR.Cells["status"].Value.ToString().Trim() != "Completed" && gR.Cells["status"].Value.ToString().Trim() != "Error")
                    {
                        
                        string counterparty = gR.Cells["counterparty"].Value.ToString().Trim();
                        string contract = gR.Cells["contract"].Value.ToString().Trim();
                        string paymentterms = gR.Cells["paymentterms"].Value.ToString().Trim();
                        string tradeclass = gR.Cells["tradeclass"].Value.ToString().Trim();
                        string termspot = gR.Cells["termspot"].Value.ToString().Trim();
                        string postype = gR.Cells["postype"].Value.ToString().Trim();
                        string location = gR.Cells["location"].Value.ToString().Trim();
                        string origin = gR.Cells["origin"].Value.ToString().Trim();
                        string strategyid = gR.Cells["strategyid"].Value.ToString().Trim();
                        string destination = gR.Cells["destination"].Value.ToString().Trim();
                        string incoterms = gR.Cells["incoterms"].Value.ToString().Trim();
                        string cst_export = gR.Cells["cst_export"].Value.ToString().Trim();
                        string unit = gR.Cells["unit"].Value.ToString().Trim();
                        string marketarea = gR.Cells["marketarea"].Value.ToString().Trim();
                        string cst_acqclass = gR.Cells["cst_acqclass"].Value.ToString().Trim();
                        string priceindex = gR.Cells["priceindex"].Value.ToString().Trim();
                        string priceunit = gR.Cells["priceunit"].Value.ToString().Trim();
                        string pricecurrency = gR.Cells["pricecurrency"].Value.ToString().Trim();
                        string settlementunit = gR.Cells["settlementunit"].Value.ToString().Trim();
                        string settlementcurrency = gR.Cells["settlementcurrency"].Value.ToString().Trim();
                        string userid = gR.Cells["userid"].Value.ToString().Trim();
                        
                        
                        
                        int countuserid = GetCountUserid(userid);
                        if (countuserid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "User ID  " + userid + " doesn't exist";
                            
                        }
                        int countcounterparty = GetCountCounterparty(counterparty);
                        if (countcounterparty == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Counterparty " + counterparty + " doesn't exist";
                            
                        }
                        int countcounterpartycontract = GetCountCounterpartyContract(counterparty, contract);
                        if (countcounterpartycontract == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Contract " + contract + " doesn't exist for Counterparty " + counterparty;
                            
                        }
                        
                        int countcontractpaymenttermsLNGCargo = GetCountContractPaymenttermsLNGCargo(paymentterms, contract, pricecurrency);
                        if (countcontractpaymenttermsLNGCargo == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Payment Terms " + paymentterms + " doesn't exist for Contract " + contract;
                            
                        }
                        
                        if (tradeclass != "ATR PROCUREMENT" && tradeclass != "PROCUREMENT")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "tradeclass  " + tradeclass + " is not allowed in LNG CARGO Importer";
                        }
                        
                        if (termspot != "TERM" && termspot != "SPOT")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "termspot  " + tradeclass + " doesn't exist";
                        }
                        
                        if (postype != "BUY" && postype != "SELL")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Postype  " + postype + " doesn't exist";
                        }
                        
                        int countstrategyid = GetCountStrategyid(strategyid);
                        if (countstrategyid == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "ID Strategy  " + strategyid + " doesn't exist";
                            
                        }
                        if (!string.IsNullOrEmpty(location))
                        {
                            int countlocation = GetCountLocation(location);
                            if (countlocation == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Location  " + location + " doesn't exist";
                                
                            }
                        }
                        if (!string.IsNullOrEmpty(origin))
                        {
                            int countorigin = GetCountOrigin(origin);
                            if (countorigin == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Origin  " + origin + " doesn't exist";
                                
                            }
                        }
                        if (!string.IsNullOrEmpty(destination))
                        {
                            int countdestination = GetCountLocation(destination);
                            if (countdestination == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Destination  " + destination + " doesn't exist";
                                
                            }
                        }
                        
                        int countincoterms = GetCountIncoterms(incoterms);
                        if (countincoterms == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Incoterms  " + incoterms + " doesn't exist";
                            
                        }
                        if (string.IsNullOrEmpty(cst_export))
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Export  can't be null";
                            
                            
                        }
                        
                        else if (cst_export != "True" && cst_export != "False")
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Export  " + cst_export + " not valid";
                        }
                        
                        int countunit = GetCountUnit(unit);
                        if (countunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Unit " + unit + " doesn't exist";
                            
                        }
                        
                        int countmarketarea = GetCountMarketArea(marketarea);
                        if (countmarketarea == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Market Area  " + marketarea + " doesn't exist";
                            
                        }
                        
                        if (cst_acqclass != "FIRM" && !string.IsNullOrEmpty(cst_acqclass))
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "ACQ Class  " + cst_acqclass + " not valid";
                        }
                        
                        if (!string.IsNullOrEmpty(priceindex))
                        {
                            int countpriceindex = GetCountPriceIndex(priceindex);
                            if (countpriceindex == 0)
                            {
                                gR.Cells["status"].Value = "Error";
                                gR.Cells["validation"].Value += "Price Index  " + priceindex + " doesn't exist";
                                
                            }
                        }
                        int countsetcurrency = GetCountSetcurrency(settlementcurrency);
                        if (countsetcurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement currency  " + settlementcurrency + " doesn't exist";
                            
                        }
                        
                        int countsetunit = GetCountSetunit(settlementunit);
                        if (countsetunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Settlement Unit  " + settlementunit + " doesn't exist";
                            
                        }
                        
                        int countpricecurrency = GetCountPricecurrency(pricecurrency);
                        if (countpricecurrency == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price currency  " + pricecurrency + " doesn't exist";
                            
                        }
                        
                        int countpriceunit = GetCountPriceunit(priceunit);
                        if (countpriceunit == 0)
                        {
                            gR.Cells["status"].Value = "Error";
                            gR.Cells["validation"].Value += "Price Unit  " + priceunit + " doesn't exist";
                            
                        }
                        
                        
                        
                    }
                    
                }
            }
            #endregion
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeImporter - ImportExcelDealsMethod
        Trade Importer - DEV - Import Deals from Excel Datasource */
        public string ImportDeals(bool automaticupdate)
        {
            /******************************************************************************
            NAME:  TradeImporter
            
            PURPOSE/DESCRIPTION/NOTES:    Trade Importer
            
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
            2.0        23/08/2021  (SSA)DZL         TradeImporter - Virtual Physical Esp
            3.0        30/09/2021  (SSA)DZL         TradeImporter - NOM
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            System.IO.FileStream stream = null;
            
            
            try
            {
                //DZL 17/08/2021 Include other views for TRADE IMPORTER - VIRTUAL PHYSICAL ESP
                if (_view.Name == "Trade Importer - DEMAND")
                {
                    
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "Excel Files (*.xls; *.xlsx; *.xlsm)| *.xls; *.xlsx; *.xlsm";
                    openDialog.Multiselect = false;
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        System.Xml.XmlNode result = Soap.Invoke("cst_ExternalSourceWS.asmx", "RetrieveConfiguration", new string[] { "datasource" }, new object[] { _view.ViewName });
                        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExternalDataSourceView));
                        var conf = (ExternalDataSourceView)serializer.Deserialize(new System.IO.StringReader(result.InnerXml.Remove(23, 39)));
                        
                        InfragisticsLibWrapper wrapper = new InfragisticsLibWrapper();
                        var mapper = new ExternalExcelFile(wrapper);
                        
                        stream = new System.IO.FileStream(openDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                        
                        //DataSet dsData = mapper.MapFile(openDialog.FileName, conf);
                        DataSet dsData = mapper.MapFile(stream, conf);
                        
                        int id = 1;
                        int idposition = 1;
                        int idtrade = 1;
                        
                        foreach (var pane in conf.PaneConfiguration)
                        {
                            
                            var ExcelGrouping = from g in dsData.Tables[pane.AllegroDataTable].AsEnumerable()
                                group g by new
                            {
                                //counterparty = g.Field<string>("counterparty"),
                                point = g.Field<string>("point"),
                                year = g.Field<DateTime>("begtime").Year
                            }
                            into gm
                            select new
                            {
                                //Counterparty = gm.Key.counterparty,
                                Point = gm.Key.point,
                                Year = gm.Key.year
                                };
                                
                                foreach (var ExcelGroupingRow in ExcelGrouping)
                                {
                                    idposition = 1;
                                    foreach (DataRow drRow in dsData.Tables[pane.AllegroDataTable].AsEnumerable().Where(x =>
                                    //x.Field<string>("counterparty") == ExcelGroupingRow.Counterparty &&
                                    x.Field<string>("point") == ExcelGroupingRow.Point
                                    && x.Field<DateTime>("begtime").Year == ExcelGroupingRow.Year))
                                    {
                                        UltraGridRow row = _view.ViewGrids[pane.ViewPane].AppendRow();
                                        
                                        foreach (DataColumn col in dsData.Tables[pane.AllegroDataTable].Columns)
                                        {
                                            var viewDefault = (from vc in _view.MetaData.viewcolumn
                                            where !vc.IsviewdefaultNull() &&
                                            vc.viewpane == pane.ViewPane &&
                                            vc.viewcolumn == col.ColumnName
                                            select vc.viewdefault).FirstOrDefault()
                                                ;
                                            
                                            if (!string.IsNullOrEmpty(viewDefault))
                                            {
                                                row.Cells[col.ColumnName].Value = viewDefault;
                                            }
                                            
                                            if (!drRow.IsNull(col))
                                            {
                                                if (drRow[col] == "")
                                                {
                                                    row.Cells[col.ColumnName].Value = DBNull.Value;
                                                }
                                                else
                                                {
                                                    row.Cells[col.ColumnName].Value = drRow[col];
                                                }
                                            }
                                        }
                                        
                                        row.Cells["iddeal"].Value = "00" + id.ToString();
                                        id++;
                                        
                                        //  row.Cells["idtrade"].Value = idtrade.ToString();
                                        
                                        //   row.Cells["idposition"].Value = idposition.ToString();
                                        idposition++;
                                    }
                                    idtrade++;
                                    
                                }
                                
                            }
                            stream.Close();
                            if (automaticupdate)
                            {
                                _view.UpdateData();
                                this.ToolClick_Insert_Trades_After_1();
                            }
                        }
                    }
                    else if (_view.Name == "Trade Importer - VIRTUAL PHYSESP")
                    {
                        
                        OpenFileDialog openDialog = new OpenFileDialog();
                        openDialog.Filter = "Excel Files (*.xls; *.xlsx; *.xlsm)| *.xls; *.xlsx; *.xlsm";
                        openDialog.Multiselect = false;
                        
                        if (openDialog.ShowDialog() == DialogResult.OK)
                        {
                            System.Xml.XmlNode result = Soap.Invoke("cst_ExternalSourceWS.asmx", "RetrieveConfiguration", new string[] { "datasource" }, new object[] { _view.ViewName });
                            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExternalDataSourceView));
                            var conf = (ExternalDataSourceView)serializer.Deserialize(new System.IO.StringReader(result.InnerXml.Remove(23, 39)));
                            
                            InfragisticsLibWrapper wrapper = new InfragisticsLibWrapper();
                            var mapper = new ExternalExcelFile(wrapper);
                            
                            stream = new System.IO.FileStream(openDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                            
                            //DataSet dsData = mapper.MapFile(openDialog.FileName, conf);
                            DataSet dsData = mapper.MapFile(stream, conf);
                            
                            int id = 1;
                            int idposition = 1;
                            int idtrade = 1;
                            
                            foreach (var pane in conf.PaneConfiguration)
                            {
                                foreach (DataRow drRow in dsData.Tables[pane.AllegroDataTable].Rows)
                                {
                                    UltraGridRow row = _view.ViewGrids[pane.ViewPane].AppendRow();
                                    
                                    foreach (DataColumn col in dsData.Tables[pane.AllegroDataTable].Columns)
                                    {
                                        var viewDefault = (from vc in _view.MetaData.viewcolumn
                                        where !vc.IsviewdefaultNull() &&
                                        vc.viewpane == pane.ViewPane &&
                                        vc.viewcolumn == col.ColumnName
                                        select vc.viewdefault).FirstOrDefault()
                                            ;
                                        
                                        if (!string.IsNullOrEmpty(viewDefault))
                                        {
                                            row.Cells[col.ColumnName].Value = viewDefault;
                                        }
                                        
                                        if (!drRow.IsNull(col))
                                        {
                                            if (drRow[col] == "")
                                            {
                                                row.Cells[col.ColumnName].Value = DBNull.Value;
                                            }
                                            else
                                            {
                                                row.Cells[col.ColumnName].Value = drRow[col];
                                            }
                                        }
                                    }
                                    
                                    row.Cells["iddeal"].Value = "00" + id.ToString();
                                    id++;
                                    
                                    //  row.Cells["trade"].Value = idtrade.ToString();
                                    
                                    //  row.Cells["position"].Value = idposition.ToString();
                                    idtrade++;
                                    idposition++;
                                }
                                
                                
                                
                                
                            }
                            stream.Close();
                            if (automaticupdate)
                            {
                                _view.UpdateData();
                                this.ToolClick_Insert_Trades_After_1();
                            }
                        }
                        
                        
                    }
                    else if (_view.Name == "Trade Importer - VIRTUAL FINSWAP" || _view.Name == "Trade Importer - LNG CARGO" || _view.Name == "Trade Importer - FINANCIAL" || _view.Name == "Trade Importer - PHYSICAL" || _view.Name == "Trade Importer - FIRM IW")
                    {
                        
                        OpenFileDialog openDialog = new OpenFileDialog();
                        openDialog.Filter = "Excel Files (*.xls; *.xlsx; *.xlsm)| *.xls; *.xlsx; *.xlsm";
                        openDialog.Multiselect = false;
                        
                        if (openDialog.ShowDialog() == DialogResult.OK)
                        {
                            System.Xml.XmlNode result = Soap.Invoke("cst_ExternalSourceWS.asmx", "RetrieveConfiguration", new string[] { "datasource" }, new object[] { _view.ViewName });
                            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExternalDataSourceView));
                            var conf = (ExternalDataSourceView)serializer.Deserialize(new System.IO.StringReader(result.InnerXml.Remove(23, 39)));
                            
                            InfragisticsLibWrapper wrapper = new InfragisticsLibWrapper();
                            var mapper = new ExternalExcelFile(wrapper);
                            
                            stream = new System.IO.FileStream(openDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                            
                            //DataSet dsData = mapper.MapFile(openDialog.FileName, conf);
                            DataSet dsData = mapper.MapFile(stream, conf);
                            
                            int id = 1;
                            //int idposition = 1;
                            //int idtrade = 1;
                            
                            foreach (var pane in conf.PaneConfiguration)
                            {
                                foreach (DataRow drRow in dsData.Tables[pane.AllegroDataTable].Rows)
                                {
                                    UltraGridRow row = _view.ViewGrids[pane.ViewPane].AppendRow();
                                    
                                    foreach (DataColumn col in dsData.Tables[pane.AllegroDataTable].Columns)
                                    {
                                        var viewDefault = (from vc in _view.MetaData.viewcolumn
                                        where !vc.IsviewdefaultNull() &&
                                        vc.viewpane == pane.ViewPane &&
                                        vc.viewcolumn == col.ColumnName
                                        select vc.viewdefault).FirstOrDefault()
                                            ;
                                        
                                        if (!string.IsNullOrEmpty(viewDefault))
                                        {
                                            row.Cells[col.ColumnName].Value = viewDefault;
                                        }
                                        
                                        if (!drRow.IsNull(col))
                                        {
                                            if (drRow[col] == "")
                                            {
                                                row.Cells[col.ColumnName].Value = DBNull.Value;
                                            }
                                            else
                                            {
                                                row.Cells[col.ColumnName].Value = drRow[col];
                                            }
                                        }
                                    }
                                    
                                    row.Cells["iddeal"].Value = "00" + id.ToString();
                                    id++;
                                    
                                    
                                    
                                    //row.Cells["idtrade"].Value = idtrade.ToString();
                                    
                                    //row.Cells["idposition"].Value = idposition.ToString();
                                    ////idtrade++;
                                    //idposition++;
                                }
                                
                                
                                
                                
                            }
                            stream.Close();
                            if (automaticupdate)
                            {
                                _view.UpdateData();
                                this.ToolClick_Insert_Trades_After_1();
                            }
                        }
                        
                        
                    }
                    else if (_view.Name == "Trade Importer - TACTICAL IW")
                    {
                        
                        OpenFileDialog openDialog = new OpenFileDialog();
                        openDialog.Filter = "Excel Files (*.xls; *.xlsx; *.xlsm)| *.xls; *.xlsx; *.xlsm";
                        openDialog.Multiselect = false;
                        
                        if (openDialog.ShowDialog() == DialogResult.OK)
                        {
                            System.Xml.XmlNode result = Soap.Invoke("cst_ExternalSourceWS.asmx", "RetrieveConfiguration", new string[] { "datasource" }, new object[] { _view.ViewName });
                            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExternalDataSourceView));
                            var conf = (ExternalDataSourceView)serializer.Deserialize(new System.IO.StringReader(result.InnerXml.Remove(23, 39)));
                            
                            InfragisticsLibWrapper wrapper = new InfragisticsLibWrapper();
                            var mapper = new ExternalExcelFile(wrapper);
                            
                            stream = new System.IO.FileStream(openDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                            
                            //DataSet dsData = mapper.MapFile(openDialog.FileName, conf);
                            DataSet dsData = mapper.MapFile(stream, conf);
                            
                            int id = 1;
                            //int idposition = 1;
                            //int idtrade = 1;
                            
                            foreach (var pane in conf.PaneConfiguration)
                            {
                                foreach (DataRow drRow in dsData.Tables[pane.AllegroDataTable].Rows)
                                {
                                    UltraGridRow row = _view.ViewGrids[pane.ViewPane].AppendRow();
                                    
                                    foreach (DataColumn col in dsData.Tables[pane.AllegroDataTable].Columns)
                                    {
                                        var viewDefault = (from vc in _view.MetaData.viewcolumn
                                        where !vc.IsviewdefaultNull() &&
                                        vc.viewpane == pane.ViewPane &&
                                        vc.viewcolumn == col.ColumnName
                                        select vc.viewdefault).FirstOrDefault()
                                            ;
                                        
                                        if (!string.IsNullOrEmpty(viewDefault))
                                        {
                                            row.Cells[col.ColumnName].Value = viewDefault;
                                        }
                                        
                                        if (!drRow.IsNull(col))
                                        {
                                            if (drRow[col] == "")
                                            {
                                                row.Cells[col.ColumnName].Value = DBNull.Value;
                                            }
                                            else
                                            {
                                                row.Cells[col.ColumnName].Value = drRow[col];
                                            }
                                        }
                                    }
                                    
                                    row.Cells["iddeal"].Value = "00" + id.ToString();
                                    id++;
                                    if (row.Cells["idtrade"].Value.ToString() == "0")
                                    {
                                        row.Cells["idtrade"].Value = DBNull.Value;
                                        row.Cells["status"].Value = "Firm";
                                    }
                                    else
                                    {
                                        row.Cells["targettrader"].Value = row.Cells["userid"].Value;
                                    }
                                    //row.Cells["idtrade"].Value = idtrade.ToString();
                                    
                                    //row.Cells["idposition"].Value = idposition.ToString();
                                    ////idtrade++;
                                    //idposition++;
                                }
                                
                                
                                
                                
                            }
                            stream.Close();
                            if (automaticupdate)
                            {
                                _view.UpdateData();
                                this.ToolClick_Insert_Trades_After_1();
                            }
                        }
                        
                        
                    }
                    else if (_view.Name == "Trade Importer - NOM")
                    {
                        
                        OpenFileDialog openDialog = new OpenFileDialog();
                        openDialog.Filter = "Excel Files (*.xls; *.xlsx; *.xlsm)| *.xls; *.xlsx; *.xlsm";
                        openDialog.Multiselect = false;
                        
                        if (openDialog.ShowDialog() == DialogResult.OK)
                        {
                            System.Xml.XmlNode result = Soap.Invoke("cst_ExternalSourceWS.asmx", "RetrieveConfiguration", new string[] { "datasource" }, new object[] { _view.ViewName });
                            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExternalDataSourceView));
                            var conf = (ExternalDataSourceView)serializer.Deserialize(new System.IO.StringReader(result.InnerXml.Remove(23, 39)));
                            
                            InfragisticsLibWrapper wrapper = new InfragisticsLibWrapper();
                            var mapper = new ExternalExcelFile(wrapper);
                            
                            stream = new System.IO.FileStream(openDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                            
                            //DataSet dsData = mapper.MapFile(openDialog.FileName, conf);
                            DataSet dsData = mapper.MapFile(stream, conf);
                            
                            int id = 1;
                            //int idposition = 1;
                            //int idtrade = 1;
                            
                            foreach (var pane in conf.PaneConfiguration)
                            {
                                foreach (DataRow drRow in dsData.Tables[pane.AllegroDataTable].Rows)
                                {
                                    UltraGridRow row = _view.ViewGrids[pane.ViewPane].AppendRow();
                                    
                                    foreach (DataColumn col in dsData.Tables[pane.AllegroDataTable].Columns)
                                    {
                                        var viewDefault = (from vc in _view.MetaData.viewcolumn
                                        where !vc.IsviewdefaultNull() &&
                                        vc.viewpane == pane.ViewPane &&
                                        vc.viewcolumn == col.ColumnName
                                        select vc.viewdefault).FirstOrDefault()
                                            ;
                                        
                                        if (!string.IsNullOrEmpty(viewDefault))
                                        {
                                            row.Cells[col.ColumnName].Value = viewDefault;
                                        }
                                        
                                        if (!drRow.IsNull(col))
                                        {
                                            row.Cells[col.ColumnName].Value = drRow[col];
                                            
                                            if (col.ColumnName == "idtrade")
                                            {
                                                row.Cells["trade"].Value = drRow[col];
                                            }
                                            else if (col.ColumnName == "idposition")
                                            {
                                                row.Cells["position"].Value = drRow[col];
                                                
                                            }
                                            
                                        }
                                    }
                                    
                                    row.Cells["iddeal"].Value = "00" + id.ToString();
                                    id++;
                                    
                                    //  row.Cells["idtrade"].Value = idtrade.ToString();
                                    
                                    //  row.Cells["idposition"].Value = idposition.ToString();
                                    ////idtrade++;
                                    //idposition++;
                                }
                                
                                
                                
                                
                            }
                            stream.Close();
                            if (automaticupdate)
                            {
                                _view.UpdateData();
                                this.ToolClick_Insert_Trades_After_1();
                            }
                        }
                        
                        
                    }
                    return "OK";
                }
                catch (ExternalSourceException ex)
                {
                    ErrorDialog.Show("Error", ex.Message, ex);
                    return "ERROR";
                }
                
                catch (Exception ex)
                {
                    ErrorDialog.Show("Error", "Error importing Excel file.", ex);
                    return "ERROR";
                }
            }
            /* TradeImporter - Demand_DuplicateRowEvent
            Trade Importer - DEV -  Clean up data when a row is duplicated for Demand */
            public UiEventResult DuplicateRowEvent_cst_demandimporter_1(object sender, NewRowEventArgs e)
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
                e.NewRow.Cells["idtrade"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["idposition"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["posdetail"].SetValue(DBNull.Value, false);
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - VirtualPhys_DuplicateRowEvent
            Trade Importer - DEV -  Clean up data when a row is duplicated for Virtual Phys Esp */
            public UiEventResult DuplicateRowEvent_cst_virtualphysimporter_1(object sender, NewRowEventArgs e)
            {
                /******************************************************************************
                NAME:  TradeImporter
                
                PURPOSE/DESCRIPTION/NOTES:    Trade Importer
                
                REVISIONS:
                Ver        Date        Author           Description
                ---------  ----------  ---------------  ------------------------------------
                1.0        23/08/2021  (SSA)DZL         TradeImporter - Initial Version
                ---------  ----------  ---------------  ------------------------------------
                *********************************************************************************/
                e.NewRow.Cells["trade"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["position"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["status"].SetValue("New", false);
                e.NewRow.Cells["validation"].SetValue(DBNull.Value, false);
                
                e.NewRow.Cells["mirrorposition"].SetValue(DBNull.Value, false);
                e.NewRow.Cells["mirrortrade"].SetValue(DBNull.Value, false);
                
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - Init View
            Trade Importer - DEV - InitView */
            public UiEventResult InitView_1()
            {
                if (_view.Name == "Trade Importer - DEMAND")
                {
                    _view.ToolbarsManager.Tools["Import_Insert Deals"].SharedProps.Visible = true;
                    _view.ToolbarsManager.Tools["Insert Trades"].SharedProps.Visible = false;
                    _view.ToolbarsManager.Tools["Import Deals"].SharedProps.Visible = false;
                }
                else if (_view.Name == "Trade Importer - VIRTUAL PHYSESP")
                {
                    _view.ToolbarsManager.Tools["Import_Insert Deals"].SharedProps.Visible = true;
                    _view.ToolbarsManager.Tools["Insert Trades"].SharedProps.Visible = true;
                    _view.ToolbarsManager.Tools["Import Deals"].SharedProps.Visible = false;
                }
                else if (_view.Name == "Trade Importer - NOM")
                {
                    _view.ToolbarsManager.Tools["Import_Insert Deals"].SharedProps.Visible = true;
                    _view.ToolbarsManager.Tools["Insert Trades"].SharedProps.Visible = true;
                    _view.ToolbarsManager.Tools["Import Deals"].SharedProps.Visible = true;
                }
                else if (_view.Name == "Trade Importer - ICE")
                {
                    _view.ToolbarsManager.Tools["Import_Insert Deals"].SharedProps.Visible = false;
                    _view.ToolbarsManager.Tools["Import Deals"].SharedProps.Visible = true;
                    
                    _view.ToolbarsManager.Tools["Insert Trades"].SharedProps.Visible = true;
                }
                else if (_view.Name == "Trade Importer - PHYSICAL")
                {
                    _view.ToolbarsManager.Tools["Import_Insert Deals"].SharedProps.Visible = false;
                    _view.ToolbarsManager.Tools["Import Deals"].SharedProps.Visible = true;
                    
                    _view.ToolbarsManager.Tools["Insert Trades"].SharedProps.Visible = true;
                }
                
                
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - demandimporter_newrow_surrogate
            Trade Importer - DEV - Populates surrogate column with temp ID when into new row DEMAND */
            public UiEventResult NewRowEvent_cst_demandimporter_1(object sender, NewRowEventArgs e)
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
                
                foreach (var row in _view.ViewGrids[0].Rows)
                {
                    if (row.Cells["iddeal"].Value != null && row.Cells["iddeal"].Value.ToString().StartsWith("0"))
                    {
                        rowcount++;
                    }
                }
                
                e.NewRow.Cells["iddeal"].SetValue("00" + (_view.DataSource.Tables["cst_demandimporter"].Columns["iddeal"].AutoIncrementStep * rowcount).ToString(), false);
                
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - virtualphysimporter_newrow
            Trade Importer - DEV - Populates surrogate column with temp ID when into new row VIRTUAL PHYS */
            public UiEventResult NewRowEvent_cst_virtualphysimporter_1(object sender, NewRowEventArgs e)
            {
                /******************************************************************************
                NAME:  TradeImporter
                
                PURPOSE/DESCRIPTION/NOTES:    Trade Importer
                
                REVISIONS:
                Ver        Date        Author           Description
                ---------  ----------  ---------------  ------------------------------------
                1.0        09/09/2021  (SSA)DZL         TradeImporter - Initial Version
                ---------  ----------  ---------------  ------------------------------------
                *********************************************************************************/
                if (((ViewGrid)sender).GetBindRow(e.NewRow).Equals(null))
                    return new UiEventResult(EventStatus.Continue);
                
                int rowcount = 1;
                
                foreach (var row in _view.ViewGrids["cst_virtualphysimporter"].Rows)
                {
                    if (row.Cells["iddeal"].Value != null && row.Cells["iddeal"].Value.ToString().StartsWith("0"))
                    {
                        rowcount++;
                    }
                }
                
                e.NewRow.Cells["iddeal"].SetValue("00" + (_view.DataSource.Tables["cst_virtualphysimporter"].Columns["iddeal"].AutoIncrementStep * rowcount).ToString(), false);
                
                return new UiEventResult(EventStatus.Continue);
            }
            /* TradeImporter - ImportExcelDemandDeals
            Trade Importer - DEV - Imports dels from Demand Excel datasource */
            public UiEventResult ToolClick_Import_Deals_After_2()
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
                string result = ImportDeals(false);
                if (result == "OK")
                {
                    return new UiEventResult(EventStatus.Continue);
                }
                else
                {
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            /* TradeImporter - Import_InsertExcelDeal
            Trade Importer - DEV - Auto update Imports deals from Excel datasource */
            public UiEventResult ToolClick_Import_Insert_Deals_After_1()
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
                string result = ImportDeals(true);
                if (result == "OK")
                {
                    return new UiEventResult(EventStatus.Continue);
                }
                else
                {
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            /* TradeImporter - InserDemandTrades
            Trade Importer - DEV - Insert Demand Trades into the system */
            public UiEventResult ToolClick_Insert_Trades_After_1()
            {
                /******************************************************************************
                NAME:  TradeImporter
                
                PURPOSE/DESCRIPTION/NOTES:    Trade Importer
                
                REVISIONS:
                Ver        Date        Author           Description
                ---------  ----------  ---------------  ------------------------------------
                1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
                2.0        09/05/2022  (SSA)DZL         TradeImporter - Add FirmIW trades
                ---------  ----------  ---------------  ------------------------------------
                *********************************************************************************/
                try
                {
                    
                    List<UltraGridRow> selection = new List<UltraGridRow>();
                    //Add Selection Firm
                    List<UltraGridRow> selectionFirm = new List<UltraGridRow>();
                    
                    foreach (UltraGridRow gridrow in this._view.ViewGrids[0].Rows)
                    {
                        UltraGridRow gR = gridrow;
                        
                        if (gR.Cells["status"].Value.ToString() == "New" || gR.Cells["status"].Value.ToString() == "Update")
                        {
                            selection.Add(gR);
                        }
                        //Add Selection Firm
                        else if (gR.Cells["status"].Value.ToString() == "Firm")
                        {
                            selectionFirm.Add(gR);
                            //   gR.Cells["status"].Value = "Running";
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
                        
                        //Firm Trades
                        List<string> idFirmDeals = new List<string>();
                        
                        foreach (UltraGridRow row in selectionFirm)
                        {
                            idFirmDeals.Add(row.Cells["iddeal"].Value.ToString());
                        }
                        if (idFirmDeals.Count > 0)
                        {
                            //Call webservice
                            Soap.Invoke("cst_TradesToStagingWS.asmx", "ImportToStaging", new string[] { "idDeals" }, new object[] { idFirmDeals.ToArray() });
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
            
            
        }
    }
    
