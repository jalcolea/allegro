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
using Allegro.Core.Reporting.DocumentBase;
using Allegro.Core.Serialization;

namespace Allegro.ClassEvents
{
    public class TradeExecutionCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* FeeDetail_Counterparty_AfterCellUpdate_1
        Refresh fee contract */
        public UiEventResult AfterCellUpdate_feedetail_counterparty_1(object sender, CellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string contract = Soap.Invoke<string>(_view.Class.Url, "GetContract", new string[] { "currentContract", "tradetype", "counterparty", "company", "positiontype", "positionmode", "product", "begtime", "endtime" },
            new object[] { e.Cell.Row.Cells["feecontract"].GetValue<string>(), _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(),
            e.Cell.Row.Cells["counterparty"].GetValue<string>(), e.Cell.Row.Cells["company"].GetValue<string>(),
            _view.ViewGrids["tradedetail"].ActiveRow.Cells["positiontype"].GetValue<string>(), _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(),
            e.Cell.Row.Cells["begtime"].GetValue<DateTime>(), e.Cell.Row.Cells["endtime"].GetValue<DateTime>() });
            
            if (!string.IsNullOrEmpty(contract))
            {
                DataSet ds = new DataSet();
                
                SqlHelper.RetrieveData(ds, new[] { "contract" }, new[] { "SELECT * FROM contract WHERE contract = '" + contract + "'" });
                
                DataRow contractRow = ds.Tables["contract"].Rows[0];
                
                if (contractRow.Field<string>("contractstatus") != "TERMINATED" && contractRow.Field<string>("contractstatus") != "PENDING")
                {
                    
                    e.Cell.Row.Cells["feecontract"].Value = contract;
                }
                else
                {
                    e.Cell.Row.Cells["feecontract"].Value = DBNull.Value;
                }
                
            }
            else
            {
                
                e.Cell.Row.Cells["feecontract"].Value = DBNull.Value;
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* FeeDetail_FeeContract_AfterCellUpdate_1
        Refresh payment terms */
        public UiEventResult AfterCellUpdate_feedetail_feecontract_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string paymentTerms = Soap.Invoke<string>(_view.Class.Url, "GetPaymentTerms", new string[] { "currentPaymentTerms", "currency", "contract" , "product"},
            new object[] { _view.ViewGrids["tradedetail"].ActiveRow.Cells["paymentterms"].GetValue<string>(), e.Cell.Row.Cells["currency"].GetValue<string>(),
            e.Cell.Row.Cells["feecontract"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>()});
            if (!string.IsNullOrEmpty(paymentTerms)) e.Cell.Row.Cells["paymentterms"].Value = paymentTerms;
            else e.Cell.Row.Cells["paymentterms"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* FeeDetail_Paymentterms_AfterCellUpdate
        Update prepaid field if paymentterms is "PREPAID" */
        public UiEventResult AfterCellUpdate_feedetail_paymentterms_1(object sender, CellEventArgs e)
        {
            Boolean marcado = false;
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            List<String> listPrepaid = new List<string>();
            DataSet ds = new DataSet();
            string paymentterms = e.Cell.Text.ToString();
            SqlHelper.RetrieveData(ds, new[] { "cst_prepaidpaymentterms" }, new[] { "SELECT * FROM cst_prepaidpaymentterms" });
            foreach (DataRow prepaidrows in ds.Tables["cst_prepaidpaymentterms"].Rows)
            {
                listPrepaid.Add(prepaidrows["paymentterms"].ToString());
            }
            
            if (listPrepaid.Contains(paymentterms))
            {
                _view.ViewGrids["feedetail"].ActiveRow.Cells["cst_prepaid"].SetValue(true, false);
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_prepaid"].SetValue(true, false);
            }
            else
            {
                _view.ViewGrids["feedetail"].ActiveRow.Cells["cst_prepaid"].SetValue(false, false);
                foreach (UltraGridRow row in _view.ViewGrids["feedetail"].Rows)
                {
                    if (Convert.ToBoolean(row.Cells["cst_prepaid"].Value))
                        marcado = true;
                }
                if (!marcado && !listPrepaid.Contains(_view.ViewGrids["tradedetail"].ActiveRow.Cells["paymentterms"].Value))
                    _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_prepaid"].SetValue(false, false);
            }
            
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* FeeDetail_Priceindex_AfterCellUpdate_1
        Refresh pricelevel */
        public UiEventResult AfterCellUpdate_feedetail_priceindex_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string indexType = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex", e.Cell.Row.Cells["priceindex"].GetValue<string>(), "indextype" });
            if (indexType.Equals("FORWARD")) e.Cell.Row.Cells["pricelevel"].Value = "SETTLE";
            else if (indexType.Equals("POSTING")) e.Cell.Row.Cells["pricelevel"].Value = "POSTED";
            else e.Cell.Row.Cells["pricelevel"].Value = "AVG";
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* FeeDetail_Priceindex_AfterCellUpdate_2
        Refresh feemode */
        public UiEventResult AfterCellUpdate_feedetail_priceindex_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (!string.IsNullOrEmpty(e.Cell.Row.Cells["priceindex"].GetValue<string>())) e.Cell.Row.Cells["feemode"].Value = "VARIABLE";
            else e.Cell.Row.Cells["feemode"].Value = "FIXED";
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_AfterCellUpdate_Multi_1
        Refresh expiration date
        Targets: trade/timeperiod,tradedetail/exchange,tradedetail/marketarea,tradedetail/optionposition,tradedetail/optionstyle,tradedetail/product,trade/begtime,trade/endtime,tradedetail/begtime,tradedetail/endtime */
        public UiEventResult AfterCellUpdate_multi_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if ((e.Cell.Row.Cells["exchange"].GetValue<string>() == null || e.Cell.Row.Cells["exchange"].GetValue<string>().Length <= 0) &&
            e.Cell.Row.Cells["optionposition"].GetValue<string>() != null && !e.Cell.Row.Cells["optionposition"].GetValue<bool>())
            {
                if (e.Cell.Row.Cells["expirationdate"].GetValue<string>() != null) e.Cell.Row.Cells["expirationdate"].Value = DBNull.Value;
                return new UiEventResult(EventStatus.Continue);
            }
            DateTime begtime = (e.Cell.Row.Cells["begtime"].GetValue<string>() == null) ? DateTime.MinValue : e.Cell.Row.Cells["begtime"].GetValue<DateTime>();
            DateTime endtime = (e.Cell.Row.Cells["endtime"].GetValue<string>() == null) ? DateTime.MaxValue : e.Cell.Row.Cells["endtime"].GetValue<DateTime>();
            string timePeriod = TimePeriod.FindTimePeriod(begtime, endtime);
            string marketarea = string.Empty;
            
            //Request347700
            //if (e.Cell.Row.Cells.Contains("marketarea") && e.Cell.Row.Cells["marketarea"].GetValue<string>() != null)
                //    marketarea = e.Cell.Row.Cells["marketarea"].GetValue<string>();
            DataRow tradedetailRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            DataRow tradeRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["trade"].ActiveRow);
            if (tradedetailRow.Table.Columns.Contains("marketarea") && !tradedetailRow.IsNull("marketarea"))
                marketarea = tradedetailRow["marketarea"].ToString();
            if (string.IsNullOrEmpty(marketarea))
            {
                if (tradeRow.Table.Columns.Contains("marketarea") && !tradeRow.IsNull("marketarea"))
                    marketarea = tradeRow["marketarea"].ToString();
            }
            DateTime expirationDate = Soap.Invoke<DateTime>(_view.Class.Url, "GetExpirationDate",
            new string[] { "optionposition", "positionmode", "exchange", "optionstyle", "product", "marketarea", "timeperiod", "tradeDate", "endtime" },
            new object[] { e.Cell.Row.Cells["optionposition"].GetValue<bool>(),
            e.Cell.Row.Cells["positionmode"].GetValue<string>(),
            e.Cell.Row.Cells["exchange"].GetValue<string>(),
            e.Cell.Row.Cells["optionstyle"].GetValue<string>(),
            e.Cell.Row.Cells["product"].GetValue<string>(),
            marketarea,
            timePeriod,
            _view.ViewGrids["trade"].ActiveRow.Cells["tradedate"].GetValue<DateTime>(),
            e.Cell.Row.Cells["endtime"].GetValue<DateTime>()});
            
            if (!expirationDate.Equals(DateTime.MinValue)) e.Cell.Row.Cells["expirationdate"].Value = expirationDate;
            else if (!begtime.Equals(DateTime.MinValue)) e.Cell.Row.Cells["expirationdate"].Value = begtime;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_AfterCellUpdate_Multi_3
        Refresh qualitybasis
        Targets: positionquality/product,positionquality/quality */
        public UiEventResult AfterCellUpdate_multi_3(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            DataRow positionqualityrow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["positionquality"].ActiveRow);
            if (positionqualityrow == null) return new UiEventResult(EventStatus.Continue);
            if (positionqualityrow.IsNull("product") || positionqualityrow.IsNull("quality")) return new UiEventResult(EventStatus.Continue);
            
            string whereclause = "product=" + Expr.Value(positionqualityrow["product"]) + " and quality=" + Expr.Value(positionqualityrow["quality"]);
            string qualitybasis = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
            new object[] { "productquality", "qualitybasis", whereclause });
            if (!string.IsNullOrEmpty(qualitybasis) && !qualitybasis.Contains(",")) e.Cell.Row.Cells["qualitybasis"].Value = qualitybasis;
            else e.Cell.Row.Cells["qualitybasis"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* EFP_Sync_Trade_Tradedetail
        Sync Trade and Tradedetail
        Targets: efpphys_begtime,efpphys_endtime,efpfin_begtime,efpfin_endtime,efpfin_timeunit,efpfin_unit,efpphys_timeunit,efpphys_unit,efpphys_pricediff,efpphys_priceindex,efpfin_pricediff,efpfin_priceindex,efpphys_positiontype,efpfin_positiontype,efpfin_quantity,efpfin_product,efpphys_quantity,efpphys_product,efpphys_carrier,efpphys_cycle,efpphys_delmethod,efpphys_incoterms,efpphys_location,efpphys_paymentterms,efpphys_spec,efpfin_contractquantity,efpfin_postprice,efpfin_postdate,begtime,endtime,positiontype,quantity,contractquantity,product,timeunit,unit,carrier,cycle,delmethod,incoterms,location,paymentterms,spec,fee_postprice,fee_postdate */
        public UiEventResult AfterCellUpdate_multi_4(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            DataRow tradedetailRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            if (tradedetailRow == null) return new UiEventResult(EventStatus.Continue);
            if (tradedetailRow.IsNull("pipeline")) return new UiEventResult(EventStatus.Continue);
            
            string colname = e.Cell.Column.Key;
            if (colname == "pipeline")
            {
                if (e.Cell.Row.Cells.Exists("shipperaccount") && !tradedetailRow.IsNull("company"))
                {
                    string whereclause = "pipeline=" + Expr.Value(tradedetailRow["pipeline"]) + " and shipper=" + Expr.Value(tradedetailRow["company"]);
                    string account = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "pipelineaccount", "account", whereclause });
                    
                    if (!string.IsNullOrEmpty(account) && !account.Contains(",")) e.Cell.Row.Cells["shipperaccount"].Value = account;
                    else e.Cell.Row.Cells["shipperaccount"].Value = DBNull.Value;
                }
                if (e.Cell.Row.Cells.Exists("counterpartyaccount") && !tradedetailRow.IsNull("counterparty"))
                {
                    string whereclause = "pipeline=" + Expr.Value(tradedetailRow["pipeline"]) + " and shipper=" + Expr.Value(tradedetailRow["counterparty"]);
                    string account = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "pipelineaccount", "account", whereclause });
                    
                    if (!string.IsNullOrEmpty(account) && !account.Contains(",")) e.Cell.Row.Cells["counterpartyaccount"].Value = account;
                    else e.Cell.Row.Cells["counterpartyaccount"].Value = DBNull.Value;
                }
            }
            else if (colname == "company" && !tradedetailRow.IsNull("company"))
            {
                if (e.Cell.Row.Cells.Exists("shipperaccount"))
                {
                    string whereclause = "pipeline=" + Expr.Value(tradedetailRow["pipeline"]) + " and shipper=" + Expr.Value(tradedetailRow["company"]);
                    string account = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "pipelineaccount", "account", whereclause });
                    
                    if (!string.IsNullOrEmpty(account) && !account.Contains(",")) e.Cell.Row.Cells["shipperaccount"].Value = account;
                    else e.Cell.Row.Cells["shipperaccount"].Value = DBNull.Value;
                }
            }
            else if (colname == "counterparty" && !tradedetailRow.IsNull("counterparty"))
            {
                if (e.Cell.Row.Cells.Exists("counterpartyaccount"))
                {
                    string whereclause = "pipeline=" + Expr.Value(tradedetailRow["pipeline"]) + " and shipper=" + Expr.Value(tradedetailRow["counterparty"]);
                    string account = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "pipelineaccount", "account", whereclause });
                    
                    if (!string.IsNullOrEmpty(account) && !account.Contains(",")) e.Cell.Row.Cells["counterpartyaccount"].Value = account;
                    else e.Cell.Row.Cells["counterpartyaccount"].Value = DBNull.Value;
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* EFP_TradeDetail_AfterCellUPdate
        EFP After Cell Update
        Targets: fee_postprice,efpseq,begtime,endtime,positiontype,quantity,contractquantity,fee_priceindex,fee_pricediff,product,timeunit,unit,carrier,cycle,delmethod,incoterms,location,paymentterms,spec */
        public UiEventResult AfterCellUpdate_multi_5(object sender, CellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            UltraGridRow gridRow = e.Cell.Row;
            string colname = e.Cell.Column.Key;
            
            if (grid.Name.ToLower() != "tradedetail") return new UiEventResult(EventStatus.Continue);
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            DataRow[] tradeRow = ((ViewGrid)sender).GetBindRow(e.Cell.Row).GetParentRows("trade_tradedetail");
            if (tradeRow == null || tradeRow.Length <= 0 || tradeRow[0].IsNull("tradetype") || !tradeRow[0]["tradetype"].ToString().Contains("EFP")) return new UiEventResult(EventStatus.Continue);
            
            if (e.Cell.Row.Cells["positionmode"].Value == DBNull.Value) return new UiEventResult(EventStatus.Continue);
            
            string positionmode = e.Cell.Row.Cells["positionmode"].Value.ToString().ToUpper();
            
            DataRow[] detailRows_phy = _view.DataSource.Tables["tradedetail"].Select("trade=" + Expr.Value(e.Cell.Row.Cells["trade"].Value)+" and positionmode='PHYSICAL'", "position, posdetail");
            
            DataRow[] detailRows_fin = _view.DataSource.Tables["tradedetail"].Select("trade=" + Expr.Value(e.Cell.Row.Cells["trade"].Value) + " and positionmode='FINANCIAL'", "position, posdetail");
            
            string match_position = "", match_posdetail = "";
            if (positionmode == "PHYSICAL")
            {
                int counter = 0;
                foreach (DataRow r in detailRows_phy)
                {
                    if (r["position"].ToString() == e.Cell.Row.Cells["position"].Value.ToString() && r["posdetail"].ToString() == e.Cell.Row.Cells["posdetail"].Value.ToString())
                    {
                        if (detailRows_fin.Length <= counter) break;
                        match_position = detailRows_fin[counter]["position"].ToString();
                        match_posdetail = detailRows_fin[counter]["posdetail"].ToString();
                        break;
                    }
                    counter++;
                }
            }
            else if (positionmode == "FINANCIAL")
            {
                int counter = 0;
                foreach (DataRow r in detailRows_fin)
                {
                    if (r["position"].ToString() == e.Cell.Row.Cells["position"].Value.ToString() && r["posdetail"].ToString() == e.Cell.Row.Cells["posdetail"].Value.ToString())
                    {
                        if (detailRows_phy.Length <= counter) break;
                        match_position = detailRows_phy[counter]["position"].ToString();
                        match_posdetail = detailRows_phy[counter]["posdetail"].ToString();
                        break;
                    }
                    counter++;
                }
            }
            
            if (match_position.Length <= 0 || match_posdetail.Length <= 0) return new UiEventResult(EventStatus.Continue);
            
            
            if (colname == "efpseq")
            {
                //if (e.Cell.Row.Cells[colname].OriginalValue != DBNull.Value)
                    //{
                    //    decimal orig_epfseq = (e.Cell.Row.Cells[colname].OriginalValue != DBNull.Value) ? (decimal)e.Cell.Row.Cells[colname].OriginalValue : (decimal)e.Cell.Row.Cells[colname].Value;
                    //    foreach (UltraGridRow detailRow in _view.ViewGrids["tradedetail"].Rows)
                        //    {
                        //        if (detailRow.Cells["trade"].Value.ToString() != e.Cell.Row.Cells["trade"].Value.ToString()) continue;
                        //        if (detailRow.Cells["efpseq"].Value.ToString() != orig_epfseq.ToString()) continue;
                        //        detailRow.Cells["efpseq"].Value = e.Cell.Row.Cells[colname].Value;
                    //    }
                //}
            }
            else if (colname == "fee_postprice")
            {
                e.Cell.Row.Cells[colname.Substring(0, colname.IndexOf("_")) + "_postdate"].Value = DateTime.Now;
            }
            else if (colname == "fee_priceindex" && e.Cell.Row.Cells[colname].Value != DBNull.Value)
            {
                e.Cell.Row.Cells["feemode"].Value = "VARIABLE";
                
                string price_unit = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                new object[] { "priceindex", "unit", "priceindex=" + SqlHelper.SqlValue(e.Cell.Row.Cells[colname].Value.ToString()) });
                if (!string.IsNullOrEmpty(price_unit)) e.Cell.Row.Cells["fee_unit"].Value = price_unit;
            }
            else if (colname == "unit")
            {
                //e.Cell.Row.Cells["fee_unit"].Value = e.Cell.Value;
            }
            
            if (positionmode == "PHYSICAL")
            {
                #region
                string marketarea = string.Empty;
                if (colname == "fee_priceindex")
                {
                    marketarea = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "exchproduct", "marketarea", "priceindex=" + SqlHelper.SqlValue(e.Cell.Row.Cells["fee_priceindex"].GetValue<string>()) });
                    if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
                }
                
                foreach (UltraGridRow detailRow in _view.ViewGrids["tradedetail"].Rows)
                {
                    if (detailRow.Cells["trade"].Value.ToString() != e.Cell.Row.Cells["trade"].Value.ToString()) continue;
                    //if (detailRow.Cells["efpseq"].Value.ToString() != e.Cell.Row.Cells["efpseq"].Value.ToString()) continue;
                    if (detailRow.Cells["positionmode"].Value.ToString().ToUpper() == "PHYSICAL") continue;
                    if (detailRow.Cells["position"].Value.ToString() != match_position || detailRow.Cells["posdetail"].Value.ToString() != match_posdetail) continue;
                    
                    if (detailRow.Cells["positionmode"].Value.ToString().ToUpper() == "FINANCIAL")
                    {
                        if (!string.IsNullOrEmpty(marketarea)) detailRow.Cells["marketarea"].Value = marketarea;
                        if (colname == "quantity")
                        {
                            # region
                            if (e.Cell.Row.Cells["quantity"].Value != DBNull.Value)
                            {
                                decimal efpphys_quantity = (decimal)e.Cell.Row.Cells["quantity"].Value;
                                string phys_unit = e.Cell.Row.Cells["unit"].Value.ToString();
                                
                                decimal convertQuantity = Soap.Invoke<decimal>("TradeExecutionWS.asmx", "UnitConversion", new string[] { "unitFrom", "unitTo", "quantity" },
                                new object[] { phys_unit, detailRow.Cells["unit"].GetValue<string>(), efpphys_quantity });
                                
                                decimal quantity = Soap.Invoke<decimal>(_view.Class.Url, "GetContractQuantity",
                                new string[] { "exchange", "product", "marketarea", "unit", "contractquantity", "criteria" },
                                new object[6] { detailRow.Cells["exchange"].GetValue<string>(), detailRow.Cells["product"].GetValue<string>(),
                                detailRow.Cells["marketarea"].GetValue<string>(), detailRow.Cells["unit"].GetValue<string>(), 1, _view.GetCriteriaFromViewSelect() });
                                
                                if (quantity > 0)
                                {
                                    detailRow.Cells["contractquantity"].Value = Math.Ceiling(convertQuantity / quantity);
                                    
                                    quantity = Soap.Invoke<decimal>(_view.Class.Url, "GetContractQuantity",
                                    new string[] { "exchange", "product", "marketarea", "unit", "contractquantity", "criteria" },
                                    new object[6] { detailRow.Cells["exchange"].GetValue<string>(), detailRow.Cells["product"].GetValue<string>(),
                                    detailRow.Cells["marketarea"].GetValue<string>(), detailRow.Cells["unit"].GetValue<string>(),
                                    detailRow.Cells["contractquantity"].Value, _view.GetCriteriaFromViewSelect() });
                                    
                                    if (quantity > 0)
                                    {
                                        detailRow.Cells["quantity"].Value = quantity;
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (colname == "unit")
                        {
                            //detailRow.Cells["unit"].Value = e.Cell.Value;
                        }
                        else if (colname == "begtime")
                        {
                            string timeperiod = Soap.Invoke<string>("TradeExecutionWS.asmx", "DefaultTimeperiod", new string[] { "exchange", "product", "begtime" },
                            new object[] { SqlHelper.SqlValue(detailRow.Cells["exchange"].GetValue<string>()),
                            SqlHelper.SqlValue(detailRow.Cells["product"].GetValue<string>()), SqlHelper.SqlValue(e.Cell.Row.Cells["begtime"].Value) });
                            
                            if (!string.IsNullOrEmpty(timeperiod))
                            {
                                string begtime = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                                new object[] { "timeperiod", "begtime", "timeperiod=" + SqlHelper.SqlValue(timeperiod) });
                                if (!string.IsNullOrEmpty(begtime)) detailRow.Cells["begtime"].Value = Convert.ToDateTime(begtime);
                                
                                string endtime = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                                new object[] { "timeperiod", "endtime", "timeperiod=" + SqlHelper.SqlValue(timeperiod) });
                                if (!string.IsNullOrEmpty(endtime)) detailRow.Cells["endtime"].Value = Convert.ToDateTime(endtime);
                            }
                            else
                            if (detailRow.Cells["begtime"].Value == DBNull.Value) detailRow.Cells["begtime"].Value = e.Cell.Value;
                        }
                        else if (colname == "endtime")
                        {
                            if (detailRow.Cells["endtime"].Value == DBNull.Value) detailRow.Cells["endtime"].Value = e.Cell.Value;
                        }
                        else if (colname == "timeunit")
                        {
                            if (detailRow.Cells["timeunit"].Value == DBNull.Value) detailRow.Cells["timeunit"].Value = e.Cell.Value;
                        }
                        else if (colname == "positiontype")
                        {
                            if (e.Cell.Value.ToString() == "BUY")
                                detailRow.Cells["positiontype"].Value = "SELL";
                            else
                            detailRow.Cells["positiontype"].Value = "BUY";
                        }
                        else if (colname == "product")
                        {
                            string unit = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                            new object[] { "product", "product", e.Cell.Row.Cells["product"].GetValue<string>(), "unit" });
                            if (!string.IsNullOrEmpty(unit)) e.Cell.Row.Cells["unit"].Value = unit;
                            
                            //detailRow.Cells["product"].Value = e.Cell.Value;
                        }
                        else if (colname == "fee_priceindex")
                        {
                            #region
                            DataRow[] priceRows = ((ViewGrid)sender).GetBindRow(detailRow).GetChildRows("tradedetail_pricedetail");
                            if (priceRows.Length > 0)
                                detailRow.Cells["fee_priceindex"].Value = e.Cell.Row.Cells["fee_priceindex"].Value;
                            else
                            {
                                detailRow.Activate();
                                _view.RetrieveChilds(_view.ViewGrids["tradedetail"], new List<DataRelation> { _view.DataSource.Relations["tradedetail_pricedetail"] });
                                detailRow.Cells["fee_priceindex"].Value = e.Cell.Row.Cells["fee_priceindex"].Value;
                            }
                            if (tradeRow[0].Table.Columns.Contains("efpfin_priceindex")) tradeRow[0]["efpfin_priceindex"] = e.Cell.Row.Cells["fee_priceindex"].Value;
                            
                            
                            string exchange = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                            new object[] { "exchproduct", "exchange", "priceindex=" + SqlHelper.SqlValue(e.Cell.Row.Cells["fee_priceindex"].GetValue<string>()) });
                            //if (!string.IsNullOrEmpty(exchange)) e.Cell.Row.Cells["exchange"].Value = exchange;
                            
                            if (e.Cell.Row.Cells["quantity"].Value != DBNull.Value)
                            {
                                decimal efpphys_quantity = (decimal)e.Cell.Row.Cells["quantity"].Value;
                                string phys_unit = e.Cell.Row.Cells["unit"].Value.ToString();
                                decimal convertQuantity = Soap.Invoke<decimal>("TradeExecutionWS.asmx", "UnitConversion", new string[] { "unitFrom", "unitTo", "quantity" },
                                new object[] { phys_unit, detailRow.Cells["unit"].GetValue<string>(), efpphys_quantity });
                                
                                decimal quantity = Soap.Invoke<decimal>(_view.Class.Url, "GetContractQuantity",
                                new string[] { "exchange", "product", "marketarea", "unit", "contractquantity", "criteria" },
                                new object[6] { detailRow.Cells["exchange"].GetValue<string>(), detailRow.Cells["product"].GetValue<string>(),
                                detailRow.Cells["marketarea"].GetValue<string>(), detailRow.Cells["unit"].GetValue<string>(), 1, _view.GetCriteriaFromViewSelect() });
                                
                                if (quantity > 0)
                                {
                                    detailRow.Cells["contractquantity"].Value = Math.Ceiling(convertQuantity / quantity);
                                    
                                    quantity = Soap.Invoke<decimal>(_view.Class.Url, "GetContractQuantity",
                                    new string[] { "exchange", "product", "marketarea", "unit", "contractquantity", "criteria" },
                                    new object[6] { detailRow.Cells["exchange"].GetValue<string>(), detailRow.Cells["product"].GetValue<string>(),
                                    detailRow.Cells["marketarea"].GetValue<string>(), detailRow.Cells["unit"].GetValue<string>(),
                                    detailRow.Cells["contractquantity"].Value, _view.GetCriteriaFromViewSelect() });
                                    
                                    if (quantity > 0)
                                    {
                                        detailRow.Cells["quantity"].Value = quantity;
                                    }
                                }
                            }
                            
                            string timeperiod = Soap.Invoke<string>("TradeExecutionWS.asmx", "DefaultTimeperiod", new string[] { "exchange", "product", "begtime" },
                            new object[] { SqlHelper.SqlValue(detailRow.Cells["exchange"].GetValue<string>()),
                            SqlHelper.SqlValue(detailRow.Cells["product"].GetValue<string>()), SqlHelper.SqlValue(e.Cell.Row.Cells["begtime"].Value) });
                            
                            if (!string.IsNullOrEmpty(timeperiod))
                            {
                                string begtime = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                                new object[] { "timeperiod", "begtime", "timeperiod=" + SqlHelper.SqlValue(timeperiod) });
                                if (!string.IsNullOrEmpty(begtime)) detailRow.Cells["begtime"].Value = Convert.ToDateTime(begtime);
                                
                                string endtime = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                                new object[] { "timeperiod", "endtime", "timeperiod=" + SqlHelper.SqlValue(timeperiod) });
                                if (!string.IsNullOrEmpty(endtime)) detailRow.Cells["endtime"].Value = Convert.ToDateTime(endtime);
                            }
                            
                            #endregion
                        }
                    }
                }
                #endregion
            }
            else if (positionmode == "FINANCIAL")
            {
                #region
                
                if (colname == "fee_priceindex")
                {
                    string exchange = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "exchproduct", "exchange", "priceindex=" + SqlHelper.SqlValue(e.Cell.Row.Cells["fee_priceindex"].GetValue<string>()) });
                    if (!string.IsNullOrEmpty(exchange)) e.Cell.Row.Cells["exchange"].Value = exchange;
                    
                    string product = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "exchproduct", "product", "priceindex=" + SqlHelper.SqlValue(e.Cell.Row.Cells["fee_priceindex"].GetValue<string>()) });
                    if (!string.IsNullOrEmpty(product)) e.Cell.Row.Cells["product"].Value = product;
                    
                    string unit = Soap.Invoke<string>("TradeExecutionWS.asmx", "ValidateSQL", new string[] { "table", "column", "whereclause" },
                    new object[] { "exchproduct", "volumeunit", "priceindex=" + SqlHelper.SqlValue(e.Cell.Row.Cells["fee_priceindex"].GetValue<string>()) });
                    if (!string.IsNullOrEmpty(unit)) e.Cell.Row.Cells["unit"].Value = unit;
                }
                
                foreach (UltraGridRow detailRow in _view.ViewGrids["tradedetail"].Rows)
                {
                    if (detailRow.Cells["trade"].Value.ToString() != e.Cell.Row.Cells["trade"].Value.ToString()) continue;
                    //if (detailRow.Cells["efpseq"].Value.ToString() != e.Cell.Row.Cells["efpseq"].Value.ToString()) continue;
                    if (detailRow.Cells["positionmode"].Value.ToString().ToUpper() == "FINANCIAL") continue;
                    if (detailRow.Cells["position"].Value.ToString() != match_position || detailRow.Cells["posdetail"].Value.ToString() != match_posdetail) continue;
                    
                    if (colname == "unit")
                    {
                        //detailRow.Cells["unit"].Value = e.Cell.Value;
                    }
                    else if (colname == "begtime")
                    {
                        //detailRow.Cells["begtime"].Value = e.Cell.Value;
                    }
                    else if (colname == "endtime")
                    {
                        //detailRow.Cells["endtime"].Value = e.Cell.Value;
                    }
                    else if (colname == "timeunit")
                    {
                        detailRow.Cells["timeunit"].Value = e.Cell.Value;
                    }
                    else if (colname == "positiontype")
                    {
                        if (e.Cell.Value.ToString() == "BUY")
                            detailRow.Cells["positiontype"].Value = "SELL";
                        else
                        detailRow.Cells["positiontype"].Value = "BUY";
                    }
                    else if (colname == "fee_postdate")
                    {
                        detailRow.Cells["fee_postdate"].Value = e.Cell.Value;
                    }
                }
                #endregion
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Validate_Begtime_Endtime
        Validate_Begtime_Endtime
        Targets: trade/begtime,trade/endtime,tradedetail/begtime,tradedetail/endtime,pricedetail/begtime,pricedetail/endtime,feedetail/begtime,feedetail/endtime */
        public UiEventResult AfterCellUpdate_multi_6(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            ViewGrid grid = (ViewGrid)sender;
            string colname = e.Cell.Column.Key;
            
            if (e.Cell.Row.Cells[colname].Value == DBNull.Value || e.Cell.Row.Cells[colname].GetValue<DateTime>() == DateTime.MinValue) return new UiEventResult(EventStatus.Continue);
            
            if (e.Cell.Row.Cells["begtime"].Value != DBNull.Value && e.Cell.Row.Cells["endtime"].Value != DBNull.Value &&
            Convert.ToDateTime(e.Cell.Row.Cells["begtime"].Value) >= Convert.ToDateTime(e.Cell.Row.Cells["endtime"].Value))
            {
                ErrorDialog.Show("Error", "Begtime must be before Endtime");
                e.Cell.Row.Cells[colname].Value = e.Cell.Row.Cells[colname].OriginalValue;
                return new UiEventResult(EventStatus.Cancel);
            }
            
            if (grid.Name.ToLower() == "pricedetail" || grid.Name.ToLower() == "feedetail")
            {
                DateTime tradebegtime = DateTime.MinValue;
                DateTime tradeendtime = DateTime.MinValue;
                DateTime positionbegtime = DateTime.MinValue;
                DateTime positionendtime = DateTime.MinValue;
                string position = string.Empty;
                bool evergreen = false;
                if (_view.ViewGrids["trade"] != null && _view.ViewGrids["trade"].ActiveRow != null)
                {
                    if (_view.ViewGrids["trade"].ActiveRow.Cells["evergreen"].Value != DBNull.Value) evergreen = _view.ViewGrids["trade"].ActiveRow.Cells["evergreen"].GetValue<bool>();
                    if (_view.ViewGrids["trade"].ActiveRow.Cells["begtime"].Value != DBNull.Value) tradebegtime = _view.ViewGrids["trade"].ActiveRow.Cells["begtime"].GetValue<DateTime>();
                    if (_view.ViewGrids["trade"].ActiveRow.Cells["endtime"].Value != DBNull.Value) tradeendtime = _view.ViewGrids["trade"].ActiveRow.Cells["endtime"].GetValue<DateTime>();
                }
                if (_view.ViewGrids["tradedetail"] != null && _view.ViewGrids["tradedetail"].ActiveRow != null)
                {
                    if (_view.ViewGrids["tradedetail"].ActiveRow.Cells["position"].Value != DBNull.Value) position = _view.ViewGrids["tradedetail"].ActiveRow.Cells["position"].GetValue<string>();
                    //Comment out per request 345994
                    //if (_view.ViewGrids["tradedetail"].ActiveRow.Cells["begtime"].Value != DBNull.Value) positionbegtime = _view.ViewGrids["tradedetail"].ActiveRow.Cells["begtime"].GetValue<DateTime>();
                    //if (_view.ViewGrids["tradedetail"].ActiveRow.Cells["endtime"].Value != DBNull.Value) positionendtime = _view.ViewGrids["tradedetail"].ActiveRow.Cells["endtime"].GetValue<DateTime>();
                    //Add per request 345994
                    bool isnewrow = false;
                    for (int i = 0; i < _view.ViewGrids["tradedetail"].Rows.Count; i ++)
                    {
                        if (_view.ViewGrids["tradedetail"].Rows[i].Cells["position"].Value.ToString().Length <= 3)
                        {
                            isnewrow = true;
                            break;
                        }
                    }
                    foreach (UltraGridRow viewrow in _view.ViewGrids["tradedetail"].Rows)
                    {
                        if (position != viewrow.Cells["position"].Value.ToString() && viewrow.Cells["position"].Value.ToString().Length > 3 && !isnewrow)
                            continue;
                        if (positionbegtime == DateTime.MinValue)
                            positionbegtime = Convert.ToDateTime(viewrow.Cells["begtime"].Value);
                        else
                        {
                            DateTime posbegtime = Convert.ToDateTime(viewrow.Cells["begtime"].Value);
                            if (posbegtime < positionbegtime)
                                positionbegtime = posbegtime;
                        }
                        if (positionendtime == DateTime.MinValue)
                            positionendtime = Convert.ToDateTime(viewrow.Cells["endtime"].Value);
                        else
                        {
                            DateTime posendtime = Convert.ToDateTime(viewrow.Cells["endtime"].Value);
                            if (posendtime > positionendtime)
                                positionendtime = posendtime;
                        }
                    }
                    
                }
                if (colname == "begtime")
                {
                    if ((tradebegtime != DateTime.MinValue && tradebegtime > e.Cell.Row.Cells["begtime"].GetValue<DateTime>()) ||
                    (evergreen == false && tradeendtime != DateTime.MinValue && tradeendtime <= e.Cell.Row.Cells["begtime"].GetValue<DateTime>()) ||
                    (positionbegtime != DateTime.MinValue && positionbegtime > e.Cell.Row.Cells["begtime"].GetValue<DateTime>()) ||
                    (evergreen == false && positionendtime != DateTime.MinValue && positionendtime <= e.Cell.Row.Cells["begtime"].GetValue<DateTime>()))
                    {
                        ErrorDialog.Show("Error", "Price/Fee time range needs to be within position time range");
                        grid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, false);
                        e.Cell.Row.Cells[colname].Value = e.Cell.Row.Cells[colname].OriginalValue;
                        grid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, true);
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
                else if (colname == "endtime")
                {
                    if ((evergreen == false && tradeendtime != DateTime.MinValue && tradeendtime < e.Cell.Row.Cells["endtime"].GetValue<DateTime>()) ||
                    (tradebegtime != DateTime.MinValue && tradebegtime >= e.Cell.Row.Cells["endtime"].GetValue<DateTime>()) ||
                    (evergreen == false && positionendtime != DateTime.MinValue && positionendtime < e.Cell.Row.Cells["endtime"].GetValue<DateTime>()) ||
                    (positionbegtime != DateTime.MinValue && positionbegtime >= e.Cell.Row.Cells["endtime"].GetValue<DateTime>()))
                    {
                        ErrorDialog.Show("Error", "Price/Fee time range needs to be within position time range");
                        grid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, false);
                        e.Cell.Row.Cells[colname].Value = e.Cell.Row.Cells[colname].OriginalValue;
                        grid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, true);
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Default_GasPlantContract
        Default_GasPlantContract
        Targets: tradedetail/pipeline,tradedetail/point */
        public UiEventResult AfterCellUpdate_multi_7(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row).Table.Columns.Contains("gasplantcontract") == false) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["company"].Value == DBNull.Value || e.Cell.Row.Cells["counterparty"].Value == DBNull.Value)
            {
                e.Cell.Row.Cells["gasplantcontract"].Value = DBNull.Value;
                return new UiEventResult(EventStatus.Continue);
            }
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters",
            new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            
            if (commodityclass == "GAS")
            {
                string whereclause = " position.position = ngposition.position and ngposition.position = processcontractpoint.position" +
                " and position.company = " + Expr.Value(e.Cell.Row.GetCellValue<string>("company")) +
                " and position.counterparty=" + Expr.Value(e.Cell.Row.GetCellValue<string>("counterparty"));
                if (e.Cell.Row.Cells["pipeline"].Value != DBNull.Value) whereclause += " and processcontractpoint.pipeline=" + Expr.Value(e.Cell.Row.GetCellValue<string>("pipeline"));
                if (e.Cell.Row.Cells["point"].Value != DBNull.Value) whereclause += " and processcontractpoint.point=" + Expr.Value(e.Cell.Row.GetCellValue<string>("point"));
                
                string gasplantcontract = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
                new object[] { "position, ngposition, processcontractpoint", "gasplantcontract", whereclause });
                if (!string.IsNullOrEmpty(gasplantcontract) && !gasplantcontract.Contains(",")) e.Cell.Row.Cells["gasplantcontract"].Value = gasplantcontract;
                else e.Cell.Row.Cells["gasplantcontract"].Value = DBNull.Value;
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Emission_CertificateType_Constraint
        Emission certificate type and position type constraint
        Targets: tradedetail/positiontype,tradedetail/certificatetype,tradedetail/product */
        public UiEventResult AfterCellUpdate_multi_9(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            DataRow tradedetailRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            if (tradedetailRow == null) return new UiEventResult(EventStatus.Continue);
            if (tradedetailRow.IsNull("product")) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters",
            new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            
            if (commodityclass != "EMISSION" && commodityclass != "RENEWABLE") return new UiEventResult(EventStatus.Continue);
            if (tradedetailRow.IsNull("certificatetype")) return new UiEventResult(EventStatus.Continue);
            
            string certificatetype = tradedetailRow["certificatetype"].ToString();
            string positiontype = tradedetailRow["positiontype"].ToString();
            
            bool b_internal = false;
            UltraGridRow tradeGRow = _view.ViewGrids["trade"].ActiveRow;
            if (tradeGRow != null) b_internal = Convert.ToBoolean(tradeGRow.Cells["internal"].Value);
            
            string colname = e.Cell.Column.Key;
            if (certificatetype.Length > 0)
            {
                if (colname == "certificatetype" || colname == "product")
                {
                    if (b_internal)
                    {
                        if (certificatetype != "PURCHASED" && certificatetype != "SOLD")
                        {
                            e.Cell.Row.Cells["certificatetype"].Value = DBNull.Value;
                            ErrorDialog.Show("Validation", "Only PURCHASED/SOLD certificate type allowed for emission internal trade");
                            return new UiEventResult(EventStatus.Cancel);
                        }
                    }
                    else
                    {
                        if ((certificatetype == "PURCHASED" || certificatetype == "GRANTED") && positiontype != "BUY")
                        {
                            e.Cell.Row.Cells["positiontype"].Value = "BUY";
                            if (tradedetailRow.Table.Columns.Contains("quantitytype")) tradedetailRow["quantitytype"] = "RECEIPT";
                        }
                        else if ((certificatetype == "EXPIRED" || certificatetype == "SOLD" || certificatetype == "SURRENDERED") && positiontype != "SELL")
                        {
                            e.Cell.Row.Cells["positiontype"].Value = "SELL";
                            if (tradedetailRow.Table.Columns.Contains("quantitytype")) tradedetailRow["quantitytype"] = "DELIVERY";
                        }
                    }
                }
                else if (colname == "positiontype")
                {
                    if (positiontype == "BUY" && (certificatetype != "PURCHASED" && certificatetype != "GRANTED"))
                    {
                        e.Cell.Row.Cells["certificatetype"].Value = "PURCHASED";
                        if (tradedetailRow.Table.Columns.Contains("quantitytype")) tradedetailRow["quantitytype"] = "RECEIPT";
                    }
                    else if (positiontype == "SELL" && (certificatetype != "EXPIRED" && certificatetype != "SOLD" && certificatetype != "SURRENDERED"))
                    {
                        e.Cell.Row.Cells["certificatetype"].Value = "SOLD";
                        if (tradedetailRow.Table.Columns.Contains("quantitytype")) tradedetailRow["quantitytype"] = "DELIVERY";
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Validate_NG_wellhead_position
        Validate new NG wellhead position
        Targets: tradedetail/property,tradedetail/begtime,tradedetail/endtime,tradedetail/pipeline,tradedetail/point,tradedetail/product */
        public UiEventResult AfterCellUpdate_multi_10(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            DataRow tradeDetailRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            if (tradeDetailRow == null) return new UiEventResult(EventStatus.Continue);
            if (tradeDetailRow.IsNull("product") || tradeDetailRow.IsNull("property")) return new UiEventResult(EventStatus.Continue);
            if (!tradeDetailRow.IsNull("trade") && tradeDetailRow["trade"].ToString().Length > 4 && tradeDetailRow.RowState != DataRowState.Added) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters",
            new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            if (commodityclass != "GAS") return new UiEventResult(EventStatus.Continue);
            
            string colname = e.Cell.Column.Key;
            
            string whereclause = "quantitystatus not in ('TRADE', 'FORECAST') and position.position=ngquantity.position and position.trade='" + e.Cell.Row.Cells["trade"].GetValue<string>() + "'";
            whereclause += " and begtime<" + SqlHelper.SqlDate(e.Cell.Row.Cells["endtime"].GetValue<DateTime>());
            whereclause += " and endtime>" + SqlHelper.SqlDate(e.Cell.Row.Cells["begtime"].GetValue<DateTime>());
            string quantitystatus = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
            new object[] { "position,ngquantity", "quantitystatus", whereclause });
            
            if (quantitystatus.Length > 0)
            {
                MessageBox.Show("There is a nominated record for the time period.", "New row error!");
                if (colname != "product")
                {
                    _view.ViewGrids["tradedetail"].EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, false);
                    e.Cell.Value = DBNull.Value;
                    _view.ViewGrids["tradedetail"].EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, true);
                }
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Validate transport contract
        Validate transport contract
        Targets: tradedetail/pipeline,tradedetail/recpoint,tradedetail/delpoint,tradedetail/quantity */
        public UiEventResult AfterCellUpdate_multi_11(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            DataRow tradeDetailRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            if (tradeDetailRow == null) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["quantity"].Value == DBNull.Value || e.Cell.Row.Cells["quantity"].GetValue<decimal>() == 0) return new UiEventResult(EventStatus.Continue);
            if (tradeDetailRow.IsNull("product") || tradeDetailRow.IsNull("pipeline")) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["recpoint"].Value != DBNull.Value || e.Cell.Row.Cells["delpoint"].Value != DBNull.Value) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters",
            new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            if (commodityclass != "GAS") return new UiEventResult(EventStatus.Continue);
            
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters",
            new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
            if (positionclass != "TRANSPORT") return new UiEventResult(EventStatus.Continue);
            
            string whereclause = "pipeline=" + Expr.Value(e.Cell.Row.Cells["pipeline"].GetValue<string>());
            string modeltype = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
            new object[] { "pipeline", "modeltype", whereclause });
            
            if (modeltype == "ENTRY/EXIT")
            {
                if (e.Cell.Row.Cells["recpoint"].Value == DBNull.Value && e.Cell.Row.Cells["delpoint"].Value == DBNull.Value
                && e.Cell.Row.Cells["quantity"].Value != DBNull.Value && e.Cell.Row.Cells["quantity"].GetValue<decimal>() != 0)
                {
                    e.Cell.Row.Cells["quantity"].Value = Convert.ToDecimal(0);
                    MessageBox.Show("Capacity should be zero for system transport contract.", "Validation error!");
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Refresh premium due date
        Refresh premium due date
        Targets: tradedetail/optionposition,trade/tradetype,trade/tradedate,trade/holidaycalendar */
        public UiEventResult AfterCellUpdate_multi_12(object sender, CellEventArgs e)
        {
            
            if (_view.ViewGrids["trade"].ActiveRow == null) return new UiEventResult(EventStatus.Continue);
            if (_view.ViewGrids["tradedetail"].Rows.Count == 0) return new UiEventResult(EventStatus.Continue);
            bool assignPremiumduedate = false;
            for (int x = 0; x < _view.ViewGrids["tradedetail"].Rows.Count; x++)
            {
                if (_view.ViewGrids["tradedetail"].Rows[x].Cells["optionposition"].GetValue<string>() != null && _view.ViewGrids["tradedetail"].Rows[x].Cells["optionposition"].GetValue<bool>())
                {
                    assignPremiumduedate = true;
                    break;
                }
            }
            if (assignPremiumduedate)
            {
                DateTime premiumDueDate = Soap.Invoke<DateTime>(_view.Class.Url, "GetPremiumDueDate",
                new string[] { "tradetype", "tradedate", "holidaycalendar" },
                new object[] { _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(),
                _view.ViewGrids["trade"].ActiveRow.Cells["tradedate"].GetValue<DateTime>(),
                _view.ViewGrids["trade"].ActiveRow.Cells["holidaycalendar"].GetValue<string>() });
                if (!premiumDueDate.Equals(DateTime.MinValue))
                {
                    //_view.ViewGrids["tradedetail"].ActiveRow.Cells["premiumduedate"].Value = premiumDueDate;
                    for (int i = 0; i < _view.ViewGrids["tradedetail"].Rows.Count; i ++)
                    {
                        _view.ViewGrids["tradedetail"].Rows[i].Cells["premiumduedate"].Value = premiumDueDate;
                    }
                }
            }
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Refresh quantity for exchange product
        Refresh quantity for exchange product
        Targets: tradedetail/contractquantity,tradedetail/exchange,tradedetail/product,tradedetail/marketarea,tradedetail/unit */
        public UiEventResult AfterCellUpdate_multi_13(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            decimal quantity = Soap.Invoke<decimal>(_view.Class.Url, "GetContractQuantity",
            new string[] { "exchange", "product", "marketarea", "unit", "contractquantity", "criteria" },
            new object[6] { e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(),
            e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["unit"].GetValue<string>(),
            e.Cell.Row.Cells["contractquantity"].GetValue<decimal>(), _view.GetCriteriaFromViewSelect() });
            if (quantity > 0) e.Cell.Row.Cells["quantity"].Value = quantity;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Refresh pricetype
        Refresh pricetype
        Targets: trade/fee_priceindex,trade/priceindex,trade/fee_pricediff,trade/pricediff */
        public UiEventResult AfterCellUpdate_multi_14(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            ViewGrid tradeGrid = (ViewGrid)sender;
            
            string whereclause = "pricetype='FIXED' or pricetype='VARIABLE'";
            string pricetype = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
            new object[] { "pricetype", "pricetype", whereclause });
            
            string colname = e.Cell.Column.Key;
            if (colname == "priceindex" || colname == "fee_priceindex")
            {
                tradeGrid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, false);
                if (!Expr.IsNullOrEmpty(e.Cell.Value))
                {
                    if (pricetype.Contains("VARIABLE"))
                    {
                        if (e.Cell.Row.Cells["pricetype"].Value == null || string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) ||
                        (e.Cell.Row.Cells["pricetype"].Value != null && !string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) && e.Cell.Row.Cells["pricetype"].Value.ToString().Equals("FIXED", StringComparison.OrdinalIgnoreCase)))
                            e.Cell.Row.Cells["pricetype"].Value = "VARIABLE";
                    }
                }
                else
                {
                    if (pricetype.Contains("FIXED"))
                    {
                        if (e.Cell.Row.Cells["pricetype"].Value == null || string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) ||
                        (e.Cell.Row.Cells["pricetype"].Value != null && !string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) && e.Cell.Row.Cells["pricetype"].Value.ToString().Equals("VARIABLE", StringComparison.OrdinalIgnoreCase)))
                            e.Cell.Row.Cells["pricetype"].Value = "FIXED";
                    }
                }
                tradeGrid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, true);
            }
            else if (colname == "pricediff" || colname == "fee_pricediff")
            {
                tradeGrid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, false);
                if (!Expr.IsNullOrEmpty(e.Cell.Value))
                {
                    if (Expr.IsNullOrEmpty(tradeGrid.ActiveRow.Cells["fee_priceindex"].Value))
                    {
                        if (pricetype.Contains("FIXED"))
                        {
                            if (e.Cell.Row.Cells["pricetype"].Value == null || string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) ||
                            (e.Cell.Row.Cells["pricetype"].Value != null && !string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) && e.Cell.Row.Cells["pricetype"].Value.ToString().Equals("VARIABLE", StringComparison.OrdinalIgnoreCase)))
                            {
                                e.Cell.Row.Cells["pricetype"].Value = "FIXED";
                            }
                        }
                    }
                    else
                    {
                        if (e.Cell.Row.Cells["pricetype"].Value == null || string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) ||
                        (e.Cell.Row.Cells["pricetype"].Value != null && !string.IsNullOrEmpty(e.Cell.Row.Cells["pricetype"].Value.ToString()) && e.Cell.Row.Cells["pricetype"].Value.ToString().Equals("FIXED", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (pricetype.Contains("VARIABLE")) e.Cell.Row.Cells["pricetype"].Value = "VARIABLE";
                        }
                    }
                }
                tradeGrid.EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, true);
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeExercise_AfterCellUpdate_Multi_1
        Validate if tradeexercise.begtime and endtime are inside the range
        Targets: tradeexercise/begtime,tradeexercise/endtime */
        public UiEventResult AfterCellUpdate_multi_15(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            DataRow tradedetailRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            if (tradedetailRow == null) return new UiEventResult(EventStatus.Continue);
            if (tradedetailRow.IsNull("begtime") || tradedetailRow.IsNull("endtime")) return new UiEventResult(EventStatus.Continue);
            
            DateTime _begtime = Convert.ToDateTime(tradedetailRow["begtime"]);
            DateTime _endtime = Convert.ToDateTime(tradedetailRow["endtime"]);
            //ViewGrid tradeexerciseGrid = (ViewGrid)sender;
            //DateTime exerciseBegtime = tradeexerciseGrid.ActiveRow.Cells["begtime"].Value != DBNull.Value? Convert.ToDateTime(tradeexerciseGrid.ActiveRow.Cells["begtime"].Value) : DateTime.MinValue;
            DateTime exerciseBegtime = e.Cell.Row.Cells["begtime"].Value != DBNull.Value? Convert.ToDateTime(e.Cell.Row.Cells["begtime"].Value) : DateTime.MinValue;
            DateTime exerciseEndtime = e.Cell.Row.Cells["endtime"].Value != DBNull.Value ? Convert.ToDateTime(e.Cell.Row.Cells["endtime"].Value) : DateTime.MinValue;
            
            string colname = e.Cell.Column.Key;
            if (colname == "begtime")
            {
                if (exerciseBegtime.CompareTo(_endtime) > 0 || exerciseBegtime.CompareTo(_begtime) < 0)
                {
                    MessageBox.Show("Exercise begtime is not in the range.", "Validation error!");
                    e.Cell.Row.Cells["begtime"].Value = e.Cell.Row.Cells["begtime"].OriginalValue;
                }
            }
            else if (colname == "endtime")
            {
                if (exerciseEndtime.CompareTo(_begtime) < 0 || exerciseEndtime.CompareTo(_endtime) > 0)
                {
                    MessageBox.Show("Exercise endtime is not in the range.", "Validation error!");
                    e.Cell.Row.Cells["endtime"].Value = e.Cell.Row.Cells["endtime"].OriginalValue;
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* GasDayTimeEntry_Trade
        Gas Day Time Entry - Trade
        Targets: trade/begtimenormal,trade/endtimenormal,trade/begtime,trade/endtime */
        public UiEventResult AfterCellUpdate_multi_100(object sender, CellEventArgs e)
        {
            //Prevent looping between redundant field updates.
            if (!e.Cell.IsActiveCell) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Value.Equals(System.DBNull.Value)) return new UiEventResult(EventStatus.Continue);
            
            //Capture invoking data.
            string sInvoker = e.Cell.Column.ToString();
            DateTime dtNewtime = e.Cell.GetValue<DateTime>();
            string sInvtable = e.Cell.Band.ToString();
            
            //Pass to update method.
            string result = UpdateNormalTime(sInvtable, sInvoker, dtNewtime, e);
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* GasDayTimeEntry_Tradedetail
        Gas Day Time Entry - Trade detail
        Targets: tradedetail/begtimenormal,tradedetail/endtimenormal,tradedetail/begtime,tradedetail/endtime */
        public UiEventResult AfterCellUpdate_multi_101(object sender, CellEventArgs e)
        {
            //Prevent looping between redundant field updates.
            if (!e.Cell.IsActiveCell) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Value.Equals(System.DBNull.Value)) return new UiEventResult(EventStatus.Continue);
            
            //Capture invoking data.
            string sInvoker = e.Cell.Column.ToString();
            DateTime dtNewtime = e.Cell.GetValue<DateTime>();
            string sInvtable = e.Cell.Band.ToString();
            
            //Pass to update method
            if (dtNewtime != DateTime.MinValue)
            {
                string result = UpdateNormalTime(sInvtable, sInvoker, dtNewtime, e);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* GasDayTimeEntry_Other
        Other columns that trigger gas day time entry
        Targets: trade/product,tradedetail/product,trade/pipeline,tradedetail/pipeline,trade/timeperiod */
        public UiEventResult AfterCellUpdate_multi_102(object sender, CellEventArgs e)
        {
            //Capture invoking data.
            string sInvoker = e.Cell.Column.ToString();
            string sInvtable = e.Cell.Band.ToString();
            
            DateTime dtNewtimebeg = e.Cell.Row.Cells["begtime"].GetValue<DateTime>();
            DateTime dtNewtimeend = e.Cell.Row.Cells["endtime"].GetValue<DateTime>();
            
            string resultbeg = string.Empty;
            string resultend = string.Empty;
            
            //Pass to update method if dates are valid
            if (dtNewtimebeg != DateTime.MinValue)
            {
                resultbeg = UpdateNormalTime(sInvtable, "begtime", dtNewtimebeg, e);
            }
            if (dtNewtimeend != DateTime.MinValue)
            {
                resultend = UpdateNormalTime(sInvtable, "endtime", dtNewtimeend, e);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* PriceDetail_Priceindex_AfterCellUpdate_1
        Refresh pricelevel */
        public UiEventResult AfterCellUpdate_pricedetail_priceindex_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string indexType = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex", e.Cell.Row.Cells["priceindex"].GetValue<string>(), "indextype" });
            if (indexType.Equals("FORWARD")) e.Cell.Row.Cells["pricelevel"].Value = "SETTLE";
            else if (indexType.Equals("POSTING")) e.Cell.Row.Cells["pricelevel"].Value = "POSTED";
            else e.Cell.Row.Cells["pricelevel"].Value = "AVG";
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* PriceDetail_Priceindex_AfterCellUpdate_2
        Refresh feemode */
        public UiEventResult AfterCellUpdate_pricedetail_priceindex_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (!string.IsNullOrEmpty(e.Cell.Row.Cells["priceindex"].GetValue<string>())) e.Cell.Row.Cells["feemode"].Value = "VARIABLE";
            else e.Cell.Row.Cells["feemode"].Value = "FIXED";
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Broker_AfterCellUpdate_1
        Refresh contract */
        public UiEventResult AfterCellUpdate_trade_broker_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["counterparty"].GetValue<string>() != null && e.Cell.Row.Cells["counterparty"].GetValue<string>().Length > 0
            && e.Cell.Row.Cells["contract"].GetValue<string>() != null) return new UiEventResult(EventStatus.Continue);
            
            string currentcontract = e.Cell.Row.Cells["contract"].GetValue<string>();
            string tradetype = e.Cell.Row.Cells["tradetype"].GetValue<string>();
            string broker = e.Cell.Row.Cells["broker"].GetValue<string>();
            string company = e.Cell.Row.Cells["company"].GetValue<string>();
            string positiontype = "", positionmode = "", product = "";
            if (_view.ViewGrids["tradedetail"].ActiveRow != null)
            {
                positiontype = _view.ViewGrids["tradedetail"].ActiveRow.Cells["positiontype"].GetValue<string>();
                positionmode = _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>();
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
            }
            else
            {
                positiontype = e.Cell.Row.Cells["positiontype"].GetValue<string>();
                positionmode = e.Cell.Row.Cells["positionmode"].GetValue<string>();
                product = e.Cell.Row.Cells["product"].GetValue<string>();
            }
            string contract = Soap.Invoke<string>(_view.Class.Url, "GetContract", new string[] { "currentContract", "tradetype", "counterparty", "company", "positiontype", "positionmode", "product", "begtime", "endtime" },
            new object[] {currentcontract, tradetype, broker, company, positiontype, positionmode, product, e.Cell.Row.Cells["begtime"].GetValue<DateTime>(), e.Cell.Row.Cells["endtime"].GetValue<DateTime>() });
            
            if (!string.IsNullOrEmpty(contract)) e.Cell.Row.Cells["contract"].Value = contract;
            else e.Cell.Row.Cells["contract"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Broker_AfterCellUpdate_2
        Default broker fee */
        public UiEventResult AfterCellUpdate_trade_broker_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            foreach (DataRow tradeDetailRow in _view.DataSource.Tables["tradedetail"].Rows)
            {
                int fee = 0;
                string position = tradeDetailRow.GetColumnValue<string>("position");
                
                DataRow[] drFees = _view.DataSource.Tables["feedetail"].Select("dbvalue = " + Expr.Value(position) + " AND feetype='Executing Fee'", "", DataViewRowState.CurrentRows);
                foreach (DataRow feeDetailRow in drFees) feeDetailRow.Delete();
                
                foreach (DataRow feeRow in _view.DataSource.Tables["feedetail"].Select("", "", DataViewRowState.CurrentRows))
                {
                    string feeNumber = feeRow.GetColumnValue<string>("fee");
                    if (!string.IsNullOrEmpty(feeNumber) && feeNumber.StartsWith("0"))
                    {
                        int tempFee = Convert.ToInt32(feeNumber);
                        if (tempFee > fee) fee = tempFee;
                    }
                }
                
                DataRow[] drBrokerFees = _view.DataSource.Tables["feedetail"].Select("dbvalue = " + Expr.Value(position),"", DataViewRowState.CurrentRows);
                DataSet newDataSet = Soap.Invoke<DataSet>(_view.Class.Url, "DefaultBrokerFee", new string[1] { "contract" }, new object[1] { tradeDetailRow.GetColumnValue<string>("contract") });
                foreach (DataRow drFee in newDataSet.Tables["fee"].Select("defvalue=" + Expr.Value(true), "", DataViewRowState.CurrentRows))
                {
                    DataRow toAdd = _view.DataSource.Tables["feedetail"].NewRow();
                    DsHelper.CopyRow(drFee, toAdd);
                    
                    toAdd["dbvalue"] = position;
                    toAdd["dbcolumn"] = "POSITION";
                    
                    bool addRow = true;
                    foreach (DataRow drBrokerfee in drBrokerFees)
                    {
                        Dictionary<string, string> diff = DsHelper.CompareRow(toAdd, drBrokerfee);
                        if (diff == null || diff.Count <= 0 || (diff.Count == 1 && diff.ContainsKey("fee")) || (diff.Count == 2 && diff.ContainsKey("fee") && diff.ContainsKey("feecontract")))
                        {
                            addRow = false;
                            break;
                        }
                    }
                    
                    if (addRow)
                    {
                        toAdd["fee"] = (++fee).ToString().PadLeft(3, '0');
                        toAdd["feecontract"] = tradeDetailRow.GetColumnValue<string>("contract");
                        _view.DataSource.Tables["feedetail"].Rows.Add(toAdd);
                    }
                }
            }
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Company_AfterCellUpdate_1
        Keep trade.company and position.company in sync */
        public UiEventResult AfterCellUpdate_trade_company_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string company = e.Cell.Row.Cells["company"].GetValue<string>();
            DataRow[] tradeDetailRows = _view.DataSource.Tables["tradedetail"].Select("trade = " + Expr.Value(e.Cell.Row.Cells["trade"].GetValue<string>()));
            foreach (var tradeDetailRow in tradeDetailRows) tradeDetailRow["company"] = company;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Contract_AfterCellUpdate_1
        Refresh contract prep */
        public UiEventResult AfterCellUpdate_trade_contract_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            e.Cell.Row.Cells["contractprep"].Value = Soap.Invoke<bool>(_view.ClassUrl, "GetContractPrep", new string[] { "contract", "company" }, new object[] { e.Cell.Row.Cells["contract"].GetValue<string>(), e.Cell.Row.Cells["company"].GetValue<string>() });
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Counterparty_AfterCellUpdate_1
        Refresh counterpartytrader */
        public UiEventResult AfterCellUpdate_trade_counterparty_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (string.IsNullOrEmpty(e.Cell.Row.Cells["counterpartytrader"].GetValue<string>())) return new UiEventResult(EventStatus.Continue);
            
            string whereclause = "counterparty=" + Expr.Value(e.Cell.Row.Cells["counterparty"].GetValue<string>()) + " and name=" + Expr.Value(e.Cell.Row.Cells["counterpartytrader"].GetValue<string>());
            string counterpartytrader = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
            new object[] { "contact", "name", whereclause });
            if (string.IsNullOrEmpty(counterpartytrader)) e.Cell.Row.Cells["counterpartytrader"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Currency_AfterCellUpdate
        Refresh payment terms after currency update */
        public UiEventResult AfterCellUpdate_trade_currency_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string paymentTerms = Soap.Invoke<string>(_view.Class.Url, "GetPaymentTerms", new string[] { "currentPaymentTerms", "currency", "contract", "product" },
            new object[] { e.Cell.Row.Cells["paymentterms"].GetValue<string>(),
            e.Cell.Row.Cells["currency"].GetValue<string>(),
            e.Cell.Row.Cells["contract"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>() });
            
            if (!string.IsNullOrEmpty(paymentTerms)) e.Cell.Row.Cells["paymentterms"].Value = paymentTerms;
            else e.Cell.Row.Cells["paymentterms"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_FeePriceindex_AfterCellUpdate_1
        Refresh pricelevel */
        public UiEventResult AfterCellUpdate_trade_fee_priceindex_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string indexType = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex", e.Cell.Row.Cells["fee_priceindex"].GetValue<string>(), "indextype" });
            if (indexType.Equals("FORWARD")) e.Cell.Row.Cells["fee_pricelevel"].Value = "SETTLE";
            else if (indexType.Equals("POSTING")) e.Cell.Row.Cells["fee_pricelevel"].Value = "POSTED";
            else e.Cell.Row.Cells["fee_pricelevel"].Value = "AVG";
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_ReferencePriceindex_AfterCellUpdate_5
        Refresh market area */
        public UiEventResult AfterCellUpdate_trade_reference_priceindex_5(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null || e.Cell.Row.Cells["product"].Value == DBNull.Value) return new UiEventResult(EventStatus.Continue);
            if (_view.ViewGrids["tradedetail"].ActiveRow == null || _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>() != "FINANCIAL") return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "RetrieveParametersMultipleKeys", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex,product", e.Cell.Row.Cells["reference_priceindex"].GetValue<string>() + "," + e.Cell.Row.Cells["product"].GetValue<string>().ToString(), "marketarea" });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Tradestatus_AfterCellUpdate_1
        Refresh status */
        public UiEventResult AfterCellUpdate_trade_tradestatus_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string status = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "tradestatus", "tradestatus", e.Cell.Row.Cells["tradestatus"].GetValue<string>(), "status" });
            if (!string.IsNullOrEmpty(status))
            {
                string orig_status = e.Cell.Row.Cells["status"].Value.ToString();
                if (status != orig_status)
                {
                    if (((ViewGrid)sender).Name.ToLower() == "trade")
                    {
                        DataRow[] drPositions = _view.DataSource.Tables["tradedetail"].Select("trade = " + Expr.Value(e.Cell.Row.GetCellValue<string>("trade")));
                        if (drPositions.Length == 0)
                        {
                            _view.RetrieveChilds(_view.ViewGrids["trade"], new List<DataRelation> { _view.DataSource.Relations["trade_tradedetail"] });
                            drPositions = ((ViewGrid)sender).GetBindRow(e.Cell.Row).GetChildRows("trade_tradedetail");
                        }
                        foreach (DataRow drPosition in drPositions)
                        {
                            DateTime begtime = Convert.ToDateTime(drPosition.GetColumnValue<DateTime?>("begtime"));
                            DateTime endtime = Convert.ToDateTime(drPosition.GetColumnValue<DateTime?>("endtime"));
                            string commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" }, new object[] { drPosition.GetColumnValue<string>("positionmode"), drPosition.GetColumnValue<string>("product") });
                            string message = Soap.Invoke<string>(_view.Class.Url, "GetQuantityStatusWithPosdetail",
                            new string[] { "position", "posdetail", "commodity", "begtime", "endtime", "positionBegtime", "positionEndtime", "colname", "evergreentermdate" },
                            new object[] { drPosition.GetColumnValue<string>("position"),
                            drPosition.GetColumnValue<string>("posdetail"),
                            commodity,
                            e.Cell.Row.GetCellValue<DateTime?>("begtime"),
                            e.Cell.Row.GetCellValue<DateTime?>("endtime"),
                            begtime,
                            endtime,
                            "status",
                            DateTime.MinValue });
                            
                            if (!string.IsNullOrEmpty(message))
                            {
                                string newmessage = message.Replace("status", "tradestatus");
                                ErrorDialog.Show("Error", newmessage);
                                string orig_tradestatus = e.Cell.Row.Cells["tradestatus"].OriginalValue.ToString();
                                _view.ViewGrids["trade"].EventManager.SetEnabled(GridEventIds.AfterCellUpdate, false);
                                e.Cell.Row.Cells["tradestatus"].Value = orig_tradestatus;
                                _view.ViewGrids["trade"].EventManager.SetEnabled(GridEventIds.AfterCellUpdate, true);
                                return new UiEventResult(EventStatus.Cancel);
                            }
                        }
                    }
                    e.Cell.Row.Cells["status"].Value = status;
                }
            }
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Tradetype_AfterCellUpdate_1
        Refresh positionmode */
        public UiEventResult AfterCellUpdate_trade_tradetype_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["tradetype"].Value == DBNull.Value) return new UiEventResult(EventStatus.Continue);
            
            string positionMode = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "tradetype", "tradetype", e.Cell.Row.Cells["tradetype"].GetValue<string>(), "positionmode" });
            if (!string.IsNullOrEmpty(positionMode)) e.Cell.Row.Cells["positionmode"].Value = positionMode;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Tradetype_AfterCellUpdate_3
        Refresh option position */
        public UiEventResult AfterCellUpdate_trade_tradetype_3(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["tradetype"].Value == DBNull.Value) return new UiEventResult(EventStatus.Continue);
            
            string rtnValue = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "tradetype", "tradetype", e.Cell.Row.Cells["tradetype"].GetValue<string>(), "optionposition" });
            if(rtnValue.ToLower() == "true") e.Cell.Row.Cells["optionposition"].Value = true;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Tradetype_AfterCellUpdate_4
        Refresh swap position type */
        public UiEventResult AfterCellUpdate_trade_tradetype_4(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["tradetype"].Value == DBNull.Value) return new UiEventResult(EventStatus.Continue);
            
            string rtnValue = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[4] { "table", "key", "keyValue", "column" }, new object[4] { "tradetype", "tradetype", e.Cell.Row.Cells["tradetype"].GetValue<string>(), "swapposition" });
            
            if (rtnValue.ToLower() == "true") e.Cell.Row.Cells["positiontype"].Value = "SWAP";
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeDetail_Company_AfterCellUpdate_1
        Keep position.company and trade.company in sync */
        public UiEventResult AfterCellUpdate_tradedetail_company_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            _view.ViewGrids["trade"].ActiveRow.Cells["company"].Value = e.Cell.Row.Cells["company"].GetValue<string>();
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Contract_AfterCellUpdate_1
        Refresh payment terms */
        public UiEventResult AfterCellUpdate_tradedetail_contract_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string paymentTerms = Soap.Invoke<string>(_view.Class.Url, "GetPaymentTerms", new string[] { "currentPaymentTerms", "currency", "contract", "product" },
            new object[] { e.Cell.Row.Cells["paymentterms"].GetValue<string>(),
            _view.ViewGrids["trade"].ActiveRow.Cells["currency"].Value,
            e.Cell.Row.Cells["contract"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>() });
            
            if (!string.IsNullOrEmpty(paymentTerms)) e.Cell.Row.Cells["paymentterms"].Value = paymentTerms;
            else e.Cell.Row.Cells["paymentterms"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Contract_AfterCellUpdate_2
        Default broker fee */
        public UiEventResult AfterCellUpdate_tradedetail_contract_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            DataSet newDataset = Soap.Invoke<DataSet>(_view.Class.Url, "DefaultBrokerFee", new string[] { "contract" }, new object[] { e.Cell.Row.Cells["contract"].GetValue<string>() });
            if (newDataset.Tables["fee"].Rows.Count > 0)
            {
                int fee = 0;
                string position = e.Cell.Row.Cells["position"].GetValue<string>();
                DataRow[] drFees = _view.DataSource.Tables["feedetail"].Select("dbvalue = " + Expr.Value(position) + " AND feetype='Executing Fee'", "", DataViewRowState.CurrentRows);
                foreach (DataRow feeDetailRow in drFees) feeDetailRow.Delete();
                
                DataRow[] drBrokerFees = _view.DataSource.Tables["feedetail"].Select("dbvalue = " + Expr.Value(position), "", DataViewRowState.CurrentRows);
                foreach (DataRow drFee in newDataset.Tables["fee"].Select("defvalue=" + Expr.Value(true), "", DataViewRowState.CurrentRows))
                {
                    fee++;
                    DataRow toAdd = _view.DataSource.Tables["feedetail"].NewRow();
                    DsHelper.CopyRow(drFee, toAdd);
                    
                    bool addRow = true;
                    foreach (DataRow drBrokerfee in drBrokerFees)
                    {
                        Dictionary<string, string> diff = DsHelper.CompareRow(toAdd, drBrokerfee);
                        if (diff == null || diff.Count <= 0 || (diff.Count == 1 && diff.ContainsKey("fee")) || (diff.Count == 2 && diff.ContainsKey("fee") && diff.ContainsKey("feecontract")))
                        {
                            addRow = false;
                            break;
                        }
                    }
                    
                    if (!drFee.IsNull("feesettlement"))
                    {
                        foreach (DataRow drFeeSettlemt in newDataset.Tables["feesettlement"].Select("feesettlement=" + drFee["feesettlement"].ToString(), "", DataViewRowState.CurrentRows))
                        {
                            DsHelper.CopyRow(drFeeSettlemt, toAdd);
                        }
                    }
                    if (!drFee.IsNull("feetimeperiod"))
                    {
                        foreach (DataRow drFeeTimeperiod in newDataset.Tables["feetimeperiod"].Select("feetimeperiod=" + drFee["feetimeperiod"].ToString(), "", DataViewRowState.CurrentRows))
                        {
                            DsHelper.CopyRow(drFeeTimeperiod, toAdd);
                        }
                    }
                    if (!drFee.IsNull("feeproduct"))
                    {
                        foreach (DataRow drFeeProduct in newDataset.Tables["feeproduct"].Select("feeproduct=" + drFee["feeproduct"].ToString(), "", DataViewRowState.CurrentRows))
                        {
                            DsHelper.CopyRow(drFeeProduct, toAdd);
                        }
                    }
                    if (!drFee.IsNull("feetier"))
                    {
                        foreach (DataRow drFeeTier in newDataset.Tables["feetier"].Select("feetier=" + drFee["feetier"].ToString(), "", DataViewRowState.CurrentRows))
                        {
                            DsHelper.CopyRow(drFeeTier, toAdd);
                        }
                    }
                    
                    if (addRow)
                    {
                        toAdd["dbvalue"] = position;
                        toAdd["dbcolumn"] = "POSITION";
                        toAdd["fee"] = "00" + fee.ToString();
                        toAdd["feecontract"] = e.Cell.Row.Cells["contract"].GetValue<string>();
                        _view.DataSource.Tables["feedetail"].Rows.Add(toAdd);
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Controlarea_AfterCellUpdate_1
        Refresh point */
        public UiEventResult AfterCellUpdate_tradedetail_controlarea_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string point = Soap.Invoke<string>(_view.Class.Url, "GetPoint", new string[] { "currentPoint", "positionmode", "product", "controlarea", "pipeline" }, new object[] { e.Cell.Row.Cells["point"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>() });
            if (!string.IsNullOrEmpty(point)) e.Cell.Row.Cells["point"].Value = point;
            else e.Cell.Row.Cells["point"].Value = DBNull.Value;
            e.Cell.Row.Cells["recpoint"].Value = DBNull.Value;
            e.Cell.Row.Cells["delpoint"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Controlarea_AfterCellUpdate_2
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_controlarea_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["point"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Counterparty_AfterCellUpdate_1
        Refresh contract */
        public UiEventResult AfterCellUpdate_tradedetail_counterparty_1(object sender, CellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string contract = Soap.Invoke<string>(_view.Class.Url, "GetContract", new string[] { "currentContract", "tradetype", "counterparty", "company", "positiontype", "positionmode", "product", "begtime", "endtime" },
            new object[] { e.Cell.Row.Cells["contract"].GetValue<string>(), _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(),
            e.Cell.Row.Cells["counterparty"].GetValue<string>(), e.Cell.Row.Cells["company"].GetValue<string>(), e.Cell.Row.Cells["positiontype"].GetValue<string>(),
            e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(),
            e.Cell.Row.Cells["begtime"].GetValue<DateTime>(), e.Cell.Row.Cells["endtime"].GetValue<DateTime>() });
            
            //DZL 21-05-2021: Include the restrictions for contract status.
            
            if (!string.IsNullOrEmpty(contract))
            {
                DataSet ds = new DataSet();
                
                SqlHelper.RetrieveData(ds, new[] { "contract" }, new[] { "SELECT * FROM contract WHERE contract = '" + contract + "'" });
                
                DataRow contractRow = ds.Tables["contract"].Rows[0];
                
                if (contractRow.Field<string>("contractstatus") != "TERMINATED" && contractRow.Field<string>("contractstatus") != "PENDING")
                {
                    
                    e.Cell.Row.Cells["contract"].Value = contract;
                }
                else
                {
                    e.Cell.Row.Cells["contract"].Value = DBNull.Value;
                }
            }
            else
            {
                e.Cell.Row.Cells["contract"].Value = DBNull.Value;
            }
            _view.ViewGrids["trade"].ActiveRow.Cells["contractprep"].Value = Soap.Invoke<bool>(_view.ClassUrl, "GetContractPrep", new string[] { "contract", "company" }, new object[] { _view.ViewGrids["trade"].ActiveRow.Cells["contract"].GetValue<string>(), _view.ViewGrids["trade"].ActiveRow.Cells["company"].GetValue<string>() });
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeDetail_cst_strategyid_AfterCellUpdate
        Update cst_strategydescription, tradebook, cst_strategybook when the cst_strategyid is modified */
        public UiEventResult AfterCellUpdate_tradedetail_cst_strategyid_2(object sender, CellEventArgs e)
        {
            string strategyId = e.Cell.Text.ToString();
            
            if (string.IsNullOrEmpty(strategyId))
            {
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_strategydescription"].SetValue(DBNull.Value, false);
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["tradebook"].SetValue(DBNull.Value, false);
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_strategybook"].SetValue(DBNull.Value, false);
            }
            else
            {
                DataSet ds = new DataSet();
                
                SqlHelper.RetrieveData(ds, new[] { "cst_strategymanagement" }, new[] { "SELECT * FROM cst_strategymanagement WHERE strategyid = '" + strategyId + "'" });
                
                DataRow cst_strategymanagementRow = ds.Tables["cst_strategymanagement"].Rows[0];
                
                if (ds == null || ds.Tables["cst_strategymanagement"] == null || ds.Tables["cst_strategymanagement"].Rows.Count == 0)
                    return new UiEventResult(EventStatus.Continue);
                
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_strategydescription"].SetValue(cst_strategymanagementRow["description"].ToString(), false);
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["tradebook"].SetValue(cst_strategymanagementRow["tradebook"].ToString(), false);
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_strategybook"].SetValue(cst_strategymanagementRow["strategybook"].ToString(), false);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeDetail_Delpoint_AfterCellUpdate_1
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_delpoint_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["delpoint"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Destination_AfterCellUpdate_1
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_destination_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (string.IsNullOrEmpty(Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "tradetype", "tradetype", _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(), "positionclass" }))) return new UiEventResult(EventStatus.Continue);
            if (string.IsNullOrEmpty(e.Cell.Row.Cells["origin"].GetValue<string>())) return new UiEventResult(EventStatus.Continue);
            if (string.IsNullOrEmpty(e.Cell.Row.Cells["destination"].GetValue<string>())) return new UiEventResult(EventStatus.Continue);
            if (!string.IsNullOrEmpty(e.Cell.Row.Cells["location"].GetValue<string>())) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["destination"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["point"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            else e.Cell.Row.Cells["marketarea"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Exchange_AfterCellUpdate_1
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_exchange_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["point"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Exchbroker_AfterCellUpdate_1
        Refresh exchange broker contract */
        public UiEventResult AfterCellUpdate_tradedetail_exchbroker_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string exchbrokerContract = Soap.Invoke<string>(_view.Class.Url, "GetContract", new string[] { "currentContract", "tradetype", "counterparty", "company", "positiontype", "positionmode", "product", "begtime", "endtime" },
            new object[] { e.Cell.Row.Cells["exchbrokercontract"].GetValue<string>(), _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(),
            e.Cell.Row.Cells["exchbroker"].GetValue<string>(), e.Cell.Row.Cells["company"].GetValue<string>(), e.Cell.Row.Cells["positiontype"].GetValue<string>(),
            e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(),
            e.Cell.Row.Cells["begtime"].GetValue<DateTime>(), e.Cell.Row.Cells["endtime"].GetValue<DateTime>() });
            
            if (!string.IsNullOrEmpty(exchbrokerContract)) e.Cell.Row.Cells["exchbrokercontract"].Value = exchbrokerContract;
            else e.Cell.Row.Cells["exchbrokercontract"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Exchbroker_AfterCellUpdate_2
        Refresh broker account */
        public UiEventResult AfterCellUpdate_tradedetail_exchbroker_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string brokerAccount = Soap.Invoke<string>(_view.Class.Url, "GetBrokerAccount", new string[] { "currentBrokerAccount", "exchbroker", "product", "exchange" }, new object[] { e.Cell.Row.Cells["brokeraccount"].GetValue<string>(), e.Cell.Row.Cells["exchbroker"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>() });
            if (!string.IsNullOrEmpty(brokerAccount)) e.Cell.Row.Cells["brokeraccount"].Value = brokerAccount;
            else e.Cell.Row.Cells["brokeraccount"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Exchbrokercontract_AfterCellUpdate_1
        Default broker fee */
        public UiEventResult AfterCellUpdate_tradedetail_exchbrokercontract_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            DataSet newDataSet = Soap.Invoke<DataSet>(_view.Class.Url, "DefaultBrokerFee", new string[] { "contract" }, new object[] { e.Cell.Row.Cells["exchbrokercontract"].GetValue<string>() });
            if (newDataSet.Tables["fee"].Rows.Count > 0)
            {
                int fee = 0;
                string position = e.Cell.Row.Cells["position"].GetValue<string>();
                DataRow[] drFees = _view.DataSource.Tables["feedetail"].Select("dbvalue = " + Expr.Value(position) + " AND feetype='Clearing Fee'", "", DataViewRowState.CurrentRows);
                foreach (DataRow feeDetailRow in drFees) feeDetailRow.Delete();
                
                foreach (DataRow feeRow in _view.DataSource.Tables["feedetail"].Select("", "", DataViewRowState.CurrentRows))
                {
                    string feeNumber = feeRow["fee"].ToString();
                    if (!string.IsNullOrEmpty(feeNumber) && feeNumber.StartsWith("0"))
                    {
                        int tempFee = Convert.ToInt32(feeNumber);
                        if (tempFee > fee) fee = tempFee;
                    }
                }
                
                DataRow[] drBrokerFees = _view.DataSource.Tables["feedetail"].Select("dbvalue = " + Expr.Value(position), "", DataViewRowState.CurrentRows);
                foreach (DataRow drFee in newDataSet.Tables["fee"].Select("defvalue=" + Expr.Value(true), "", DataViewRowState.CurrentRows))
                {
                    DataRow toAdd = _view.DataSource.Tables["feedetail"].NewRow();
                    DsHelper.CopyRow(drFee, toAdd);
                    toAdd["dbvalue"] = position;
                    toAdd["dbcolumn"] = "POSITION";
                    
                    bool addRow = true;
                    foreach (DataRow drBrokerfee in drBrokerFees)
                    {
                        Dictionary<string, string> diff = DsHelper.CompareRow(toAdd, drBrokerfee);
                        if (diff == null || diff.Count <= 0 || (diff.Count == 1 && diff.ContainsKey("fee")) || (diff.Count == 2 && diff.ContainsKey("fee") && diff.ContainsKey("feecontract")))
                        {
                            addRow = false;
                            break;
                        }
                    }
                    
                    if (addRow)
                    {
                        toAdd["fee"] = (++fee).ToString().PadLeft(3, '0');
                        toAdd["fee"] = e.Cell.Row.Cells["exchbrokercontract"].GetValue<string>();
                        _view.DataSource.Tables["feedetail"].Rows.Add(toAdd);
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Exchbrokercontract_AfterCellUpdate_2
        Refresh payment terms */
        public UiEventResult AfterCellUpdate_tradedetail_exchbrokercontract_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string paymentTerms = Soap.Invoke<string>(_view.Class.Url, "GetPaymentTerms", new string[] { "currentPaymentTerms", "currency", "contract", "product" },
            new object[] { e.Cell.Row.Cells["paymentterms"].GetValue<string>(), _view.ViewGrids["trade"].ActiveRow.Cells["currency"].Value,
            e.Cell.Row.Cells["exchbrokercontract"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>() });
            if (!string.IsNullOrEmpty(paymentTerms)) e.Cell.Row.Cells["paymentterms"].Value = paymentTerms;
            else e.Cell.Row.Cells["paymentterms"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_FeePriceindex_AfterCellUpdate_1
        Refresh pricelevel */
        public UiEventResult AfterCellUpdate_tradedetail_fee_priceindex_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string indexType = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex", e.Cell.Row.Cells["fee_priceindex"].GetValue<string>(), "indextype" });
            if (indexType.Equals("FORWARD")) e.Cell.Row.Cells["pricelevel"].Value = "SETTLE";
            else if (indexType.Equals("POSTING")) e.Cell.Row.Cells["pricelevel"].Value = "POSTED";
            else e.Cell.Row.Cells["pricelevel"].Value = "AVG";
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_FeePriceindex_AfterCellUpdate_2
        Refresh currency */
        public UiEventResult AfterCellUpdate_tradedetail_fee_priceindex_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string currency = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex", e.Cell.Row.Cells["fee_priceindex"].GetValue<string>(), "currency" });
            if (!string.IsNullOrEmpty(currency))
            {
                DataRow[] drPriceMatch = _view.DataSource.Tables["pricedetail"].Select("dbcolumn = 'POSITION' AND dbvalue = " + Expr.Value(e.Cell.Row.Cells["position"].GetValue<string>()));
                foreach (DataRow drPrice in drPriceMatch) drPrice["currency"] = currency;
                _view.ViewGrids["trade"].ActiveRow.Cells["currency"].Value = currency;
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_FeePriceindex_AfterCellUpdate_3
        Refresh unit */
        public UiEventResult AfterCellUpdate_tradedetail_fee_priceindex_3(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string unit = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex", e.Cell.Row.Cells["fee_priceindex"].GetValue<string>(), "unit" });
            if (!string.IsNullOrEmpty(unit))
            {
                ViewGrid priceGrid = _view.ViewGrids["pricedetail"];
                if (priceGrid == null) new UiEventResult(EventStatus.Continue);
                string position = e.Cell.Row.Cells["position"].GetValue<string>();
                foreach (UltraGridRow row in _view.ViewGrids["pricedetail"].Rows)
                {
                    if (row.Cells["dbvalue"].Value.ToString() == position)
                        row.Cells["unit"].Value = unit;
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_FeePriceindex_AfterCellUpdate_4
        Refresh holiday calendar */
        public UiEventResult AfterCellUpdate_tradedetail_fee_priceindex_4(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string holidayCalendar = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex", e.Cell.Row.Cells["fee_priceindex"].GetValue<string>(), "holidaycalendar" });
            if (!string.IsNullOrEmpty(holidayCalendar)) _view.ViewGrids["trade"].ActiveRow.Cells["holidaycalendar"].Value = holidayCalendar;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_FeePriceindex_AfterCellUpdate_5
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_fee_priceindex_5(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (e.Cell.Row.Cells["product"].Value == DBNull.Value) return new UiEventResult(EventStatus.Continue);
            if (_view.ViewGrids["tradedetail"].ActiveRow == null || _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>() != "FINANCIAL") return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "RetrieveParametersMultipleKeys", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex,product", e.Cell.Row.Cells["fee_priceindex"].GetValue<string>() + "," + e.Cell.Row.Cells["product"].GetValue<string>(), "marketarea" });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Location_AfterCellUpdate_1
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_location_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["point"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            else e.Cell.Row.Cells["marketarea"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Paymentterms_AfterCellUpdate
        Update prepaid field if paymentterms is "PREPAID" */
        public UiEventResult AfterCellUpdate_tradedetail_paymentterms_1(object sender, CellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            List<String> listPrepaid = new List<string>();
            DataSet ds = new DataSet();
            string paymentterms = e.Cell.Text.ToString();
            SqlHelper.RetrieveData(ds, new[] { "cst_prepaidpaymentterms" }, new[] { "SELECT * FROM cst_prepaidpaymentterms" });
            foreach (DataRow prepaidrows in ds.Tables["cst_prepaidpaymentterms"].Rows)
            {
                listPrepaid.Add(prepaidrows["paymentterms"].ToString());
            }
            
            
            if (listPrepaid.Contains(paymentterms))
            {
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_prepaid"].SetValue(true, false);
            }
            else
            {
                _view.ViewGrids["tradedetail"].ActiveRow.Cells["cst_prepaid"].SetValue(false, false);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeDetail_Pipeline_AfterCellUpdate_1
        Refresh point */
        public UiEventResult AfterCellUpdate_tradedetail_pipeline_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string point = Soap.Invoke<string>(_view.Class.Url, "GetPoint", new string[] { "positionmode", "product", "controlarea", "pipeline" }, new object[] { e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>() });
            if (!string.IsNullOrEmpty(point)) e.Cell.Row.Cells["point"].Value = point;
            else e.Cell.Row.Cells["point"].Value = DBNull.Value;
            e.Cell.Row.Cells["recpoint"].Value = DBNull.Value;
            e.Cell.Row.Cells["delpoint"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Pipeline_AfterCellUpdate_2
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_pipeline_2(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["point"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            else e.Cell.Row.Cells["marketarea"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Pipeline_AfterCellUpdate_3
        Refresh timezone */
        public UiEventResult AfterCellUpdate_tradedetail_pipeline_3(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (string.IsNullOrEmpty(e.Cell.Row.Cells["pipeline"].GetValue<string>())) return new UiEventResult(EventStatus.Continue);
            
            string whereclause = "pipeline=" + Expr.Value(e.Cell.Row.Cells["pipeline"].Value);
            string timezone = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
            new object[] { "pipeline", "timezone", whereclause });
            
            if (!string.IsNullOrEmpty(timezone)) e.Cell.Row.Cells["timezone"].Value = timezone;
            else e.Cell.Row.Cells["timezone"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Point_AfterCellUpdate_1
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_point_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["point"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            else e.Cell.Row.Cells["marketarea"].Value = DBNull.Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Product_AfterCellUpdate_5
        Default product quality */
        public UiEventResult AfterCellUpdate_tradedetail_product_5(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string position = e.Cell.Row.Cells["position"].GetValue<string>();
            string product = e.Cell.Row.Cells["product"].GetValue<string>();
            DataRow[] positionqualityRows = ((ViewGrid)sender).GetBindRow(e.Cell.Row).GetChildRows("tradedetail_positionquality");
            if (positionqualityRows.Length <= 0)
            {
                _view.RetrieveChilds(_view.ViewGrids["tradedetail"], new List<DataRelation> { _view.DataSource.Relations["tradedetail_positionquality"] });
                positionqualityRows = ((ViewGrid)sender).GetBindRow(e.Cell.Row).GetChildRows("tradedetail_positionquality");
            }
            foreach (DataRow positionQualityRow in positionqualityRows) positionQualityRow.Delete();
            
            DataTable newTable = Soap.Invoke<DataTable>(_view.Class.Url, "DefaultQuality", new string[] { "position", "product" }, new object[] { position, product });
            if (newTable.Rows.Count > 0)
            {
                foreach (DataRow newRow in newTable.Select())
                {
                    DataRow toAdd = _view.DataSource.Tables["positionquality"].NewRow();
                    DsHelper.CopyRow(newRow, toAdd);
                    toAdd["position"] = position;
                    toAdd["creationname"] = DBNull.Value;
                    toAdd["creationdate"] = DBNull.Value;
                    toAdd["revisionname"] = DBNull.Value;
                    toAdd["revisiondate"] = DBNull.Value;
                    _view.DataSource.Tables["positionquality"].Rows.Add(toAdd);
                }
                _view.ViewGrids["positionquality"].BindData();
            }
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Product_AfterCellUpdate_6
        Refresh unit */
        public UiEventResult AfterCellUpdate_tradedetail_product_6(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string position = e.Cell.Row.Cells["position"].GetValue<string>();
            string unit = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "product", "product", e.Cell.Row.Cells["product"].GetValue<string>(), "unit" });
            if (!string.IsNullOrEmpty(unit))
            {
                e.Cell.Row.Cells["unit"].Value = unit;
                e.Cell.Row.Cells["massunit"].Value = DBNull.Value;
                e.Cell.Row.Cells["volumeunit"].Value = DBNull.Value;
                
                foreach (UltraGridRow row in _view.ViewGrids["pricedetail"].Rows)
                {
                    if (row.Cells["dbvalue"].Value.ToString() == position)
                        row.Cells["unit"].Value = unit;
                }
                foreach (UltraGridRow row in _view.ViewGrids["feedetail"].Rows)
                {
                    if (row.Cells["dbvalue"].Value.ToString() == position)
                        row.Cells["unit"].Value = unit;
                }
                foreach (UltraGridRow row in _view.ViewGrids["positionconstraint"].Rows)
                {
                    if (row.Cells["position"].Value.ToString() == position)
                        row.Cells["unit"].Value = unit;
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Product_AfterCellUpdate_7
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_product_7(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea",
            new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" },
            new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(),
            e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(),
            e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(),
            e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(),
            e.Cell.Row.Cells["point"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            else
            {
                if (e.Cell.Row.Cells["product"].Value == DBNull.Value) return new UiEventResult(EventStatus.Continue);
                marketarea = Soap.Invoke<string>(_view.Class.Url, "RetrieveParametersMultipleKeys", new string[] { "table", "key", "keyValue", "column" }, new object[] { "priceindex", "priceindex,product", e.Cell.Row.Cells["fee_priceindex"].GetValue<string>() + "," + e.Cell.Row.Cells["product"].GetValue<string>(), "marketarea" });
                if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Property_AfterCellUpdate_1
        Refresh pipeline/point/location/product */
        public UiEventResult AfterCellUpdate_tradedetail_property_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            DataSet dataSet = Soap.Invoke<DataSet>(_view.Class.Url, "GetPropertyInfo", new string[] { "property", "commodityclass" }, new object[] { e.Cell.Row.Cells["property"].GetValue<string>(), commodityclass });
            if (dataSet.Tables.Contains("propgathering") && dataSet.Tables["propgathering"].Rows.Count == 1)
            {
                if (commodityclass == "GAS")
                {
                    string pipeline = dataSet.Tables["propgathering"].Rows[0].GetColumnValue<string>("pipeline");
                    if (!string.IsNullOrEmpty(pipeline)) e.Cell.Row.Cells["pipeline"].Value = pipeline;
                    else e.Cell.Row.Cells["pipeline"].Value = DBNull.Value;
                    
                    string point = dataSet.Tables["propgathering"].Rows[0].GetColumnValue<string>("recpoint");
                    if (!string.IsNullOrEmpty(point)) e.Cell.Row.Cells["point"].Value = point;
                    else e.Cell.Row.Cells["point"].Value = DBNull.Value;
                    
                    e.Cell.Row.Cells["location"].Value = DBNull.Value;
                }
                else if (commodityclass == "LIQUID")
                {
                    string location = dataSet.Tables["propgathering"].Rows[0].GetColumnValue<string>("location");
                    if (!string.IsNullOrEmpty(location)) e.Cell.Row.Cells["location"].Value = location;
                    else e.Cell.Row.Cells["location"].Value = DBNull.Value;
                    
                    e.Cell.Row.Cells["pipeline"].Value = DBNull.Value;
                    e.Cell.Row.Cells["point"].Value = DBNull.Value;
                }
                else if (commodityclass.Length == 0)
                {
                    string product = dataSet.Tables["propgathering"].Rows[0].GetColumnValue<string>("product");
                    if (!string.IsNullOrEmpty(product)) e.Cell.Row.Cells["product"].Value = product;
                    else e.Cell.Row.Cells["product"].Value = DBNull.Value;
                    
                    string location = dataSet.Tables["propgathering"].Rows[0].GetColumnValue<string>("location");
                    if (!string.IsNullOrEmpty(location)) e.Cell.Row.Cells["location"].Value = location;
                    else e.Cell.Row.Cells["location"].Value = DBNull.Value;
                    
                    string pipeline = dataSet.Tables["propgathering"].Rows[0].GetColumnValue<string>("pipeline");
                    if (!string.IsNullOrEmpty(pipeline)) e.Cell.Row.Cells["pipeline"].Value = pipeline;
                    else e.Cell.Row.Cells["pipeline"].Value = DBNull.Value;
                    
                    string point = dataSet.Tables["propgathering"].Rows[0].GetColumnValue<string>("recpoint");
                    if (!string.IsNullOrEmpty(point)) e.Cell.Row.Cells["point"].Value = point;
                    else e.Cell.Row.Cells["point"].Value = DBNull.Value;
                }
            }
            else
            {
                e.Cell.Row.Cells["product"].Value = DBNull.Value;
                e.Cell.Row.Cells["location"].Value = DBNull.Value;
                e.Cell.Row.Cells["pipeline"].Value = DBNull.Value;
                e.Cell.Row.Cells["point"].Value = DBNull.Value;
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Recpoint_AfterCellUpdate_1
        Refresh market area */
        public UiEventResult AfterCellUpdate_tradedetail_recpoint_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string marketarea = Soap.Invoke<string>(_view.Class.Url, "GetMarketArea", new string[] { "currentMarketArea", "positionmode", "product", "controlarea", "pipeline", "location", "exchange", "source", "point" }, new object[] { e.Cell.Row.Cells["marketarea"].GetValue<string>(), e.Cell.Row.Cells["positionmode"].GetValue<string>(), e.Cell.Row.Cells["product"].GetValue<string>(), e.Cell.Row.Cells["controlarea"].GetValue<string>(), e.Cell.Row.Cells["pipeline"].GetValue<string>(), e.Cell.Row.Cells["location"].GetValue<string>(), e.Cell.Row.Cells["exchange"].GetValue<string>(), e.Cell.Row.Cells["source"].GetValue<string>(), e.Cell.Row.Cells["recpoint"].GetValue<string>() });
            if (!string.IsNullOrEmpty(marketarea)) e.Cell.Row.Cells["marketarea"].Value = marketarea;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_PositionType_AfterCellUpdate_1
        PositionType After Cell Update - DEV - Change PAY status in Price Detail when swaptype is modified where positiontype is SWAP */
        public UiEventResult AfterCellUpdate_tradedetail_swaptype_1(object sender, CellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string positiontype = e.Cell.Row.Cells["positiontype"].GetValue<string>();
            
            if (positiontype == "SWAP")
            {
                
                ViewGrid priceGrid = _view.ViewGrids["pricedetail"];
                if (priceGrid == null) new UiEventResult(EventStatus.Continue);
                string swaptype = e.Cell.Row.Cells["swaptype"].GetValue<string>();
                
                string position = e.Cell.Row.Cells["position"].GetValue<string>();
                foreach (UltraGridRow row in _view.ViewGrids["pricedetail"].Rows)
                {
                    if (row.Cells["dbvalue"].Value.ToString() == position)
                    {
                        if (swaptype == "BUY")
                        {
                            if (row.Cells["feemode"].Value.ToString() == "FIXED")
                            {
                                row.Cells["paystatus"].Value = "PAY";
                            }
                            else if (row.Cells["feemode"].Value.ToString() == "VARIABLE")
                            {
                                row.Cells["paystatus"].Value = "REC";
                                
                            }
                            
                            
                        }
                        else if (swaptype == "SELL")
                        {
                            if (row.Cells["feemode"].Value.ToString() == "FIXED")
                            {
                                row.Cells["paystatus"].Value = "REC";
                            }
                            else if (row.Cells["feemode"].Value.ToString() == "VARIABLE")
                            {
                                row.Cells["paystatus"].Value = "PAY";
                                
                            }
                        }
                    }
                }
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* EFP_Trade_AfterRetrieveData
        After Retrieve Data */
        public UiEventResult AfterRetrieveData_2(object sender, RetrieveDataEventArgs e)
        {
            
            UltraGridRow tradeRow = _view.ViewGrids["trade"].ActiveRow;
            if (tradeRow == null) return new UiEventResult(EventStatus.Continue);
            if (tradeRow.IsDataRow == false) return new UiEventResult(EventStatus.Continue);
            if (tradeRow.Cells["tradetype"].Value == DBNull.Value || !tradeRow.Cells["tradetype"].Value.ToString().Contains("EFP")) return new UiEventResult(EventStatus.Continue);
            
            if (e.TableNames.Count > 0 && (e.TableNames[0] == "tradedetail" || e.TableNames[0] == "pricedetail"))
            {
                DataSet ds = _view.DataSource;
                string trade = tradeRow.Cells["trade"].Value.ToString();
                DataRow[] tradeMatch = ds.Tables["tradedetail"].Select("trade = " + Expr.Value(trade));
                ViewGrid tradegrid = _view.ViewGrids["trade"];
                DataRow tradedatarow = tradegrid.GetBindRow(tradeRow);
                
                if (tradeMatch.Length > 0)
                {
                    _view.ViewGrids["trade"].EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, false);
                    
                    DataRow[] tradeDetailRows = ds.Tables["tradedetail"].Select("trade = " + Expr.Value(trade) + " and positionmode='PHYSICAL'", "position");
                    
                    if (tradeDetailRows.Length > 0 && tradedatarow.Table.Columns.Contains("efp_positiontype")) tradeRow.Cells["efp_positiontype"].Value = tradeDetailRows[0]["positiontype"].ToString();
                    
                    if (tradeDetailRows.Length == 1)
                    {
                        DataRow tradeDetailRow = tradeDetailRows[0];
                        
                        if (tradeDetailRow.IsNull("positiontype") == false && tradedatarow.Table.Columns.Contains("efpphys_positiontype")) tradeRow.Cells["efpphys_positiontype"].Value = tradeDetailRow["positiontype"].ToString();
                        if (tradeDetailRow.IsNull("product") == false && tradedatarow.Table.Columns.Contains("efpphys_product")) tradeRow.Cells["efpphys_product"].Value = tradeDetailRow["product"].ToString();
                        if (tradeDetailRow.IsNull("unit") == false && tradedatarow.Table.Columns.Contains("efpphys_unit")) tradeRow.Cells["efpphys_unit"].Value = tradeDetailRow["unit"].ToString();
                        if (tradeDetailRow.IsNull("quantity") == false && tradedatarow.Table.Columns.Contains("efpphys_quantity")) tradeRow.Cells["efpphys_quantity"].Value = Convert.ToDecimal(tradeDetailRow["quantity"]);
                        if (tradeDetailRow.IsNull("begtime") == false && tradedatarow.Table.Columns.Contains("efpphys_begtime")) tradeRow.Cells["efpphys_begtime"].Value = Convert.ToDateTime(tradeDetailRow["begtime"]);
                        if (tradeDetailRow.IsNull("endtime") == false && tradedatarow.Table.Columns.Contains("efpphys_endtime")) tradeRow.Cells["efpphys_endtime"].Value = Convert.ToDateTime(tradeDetailRow["endtime"]); //tradeDetailRow["endtime"];
                        if (tradeDetailRow.IsNull("timeunit") == false && tradedatarow.Table.Columns.Contains("efpphys_timeunit")) tradeRow.Cells["efpphys_timeunit"].Value = tradeDetailRow["timeunit"].ToString();
                        if (tradeDetailRow.IsNull("carrier") == false && tradedatarow.Table.Columns.Contains("efpphys_carrier")) tradeRow.Cells["efpphys_carrier"].Value = tradeDetailRow["carrier"].ToString();
                        
                        if (tradeDetailRow.IsNull("cycle") == false && tradedatarow.Table.Columns.Contains("efpphys_cycle")) tradeRow.Cells["efpphys_cycle"].Value = tradeDetailRow["cycle"].ToString();
                        if (tradeDetailRow.IsNull("location") == false && tradedatarow.Table.Columns.Contains("efpphys_location")) tradeRow.Cells["efpphys_location"].Value = tradeDetailRow["location"].ToString();
                        if (tradeDetailRow.IsNull("delmethod") == false && tradedatarow.Table.Columns.Contains("efpphys_delmethod")) tradeRow.Cells["efpphys_delmethod"].Value = tradeDetailRow["delmethod"].ToString();
                        if (tradeDetailRow.IsNull("incoterms") == false && tradedatarow.Table.Columns.Contains("efpphys_incoterms")) tradeRow.Cells["efpphys_incoterms"].Value = tradeDetailRow["incoterms"].ToString();
                        if (tradeDetailRow.IsNull("paymentterms") == false && tradedatarow.Table.Columns.Contains("efpphys_paymentterms")) tradeRow.Cells["efpphys_paymentterms"].Value = tradeDetailRow["paymentterms"].ToString();
                        
                        if (tradeDetailRow.IsNull("fee_priceindex") == false && tradedatarow.Table.Columns.Contains("efpphys_priceindex")) tradeRow.Cells["efpphys_priceindex"].Value = tradeDetailRow["fee_priceindex"].ToString();
                        if (tradeDetailRow.IsNull("fee_pricediff") == false && tradedatarow.Table.Columns.Contains("efpphys_pricediff")) tradeRow.Cells["efpphys_pricediff"].Value = Convert.ToDecimal(tradeDetailRow["fee_pricediff"]);
                    }
                    
                    tradeDetailRows = ds.Tables["tradedetail"].Select("trade = " + Expr.Value(trade) + " and positionmode='FINANCIAL'", "position");
                    
                    if (tradeDetailRows.Length == 1)
                    {
                        DataRow tradeDetailRow = tradeDetailRows[0];
                        if (tradeDetailRow.IsNull("positiontype") == false && tradedatarow.Table.Columns.Contains("efpfin_positiontype")) tradeRow.Cells["efpfin_positiontype"].Value = tradeDetailRow["positiontype"].ToString();
                        if (tradeDetailRow.IsNull("product") == false && tradedatarow.Table.Columns.Contains("efpfin_product")) tradeRow.Cells["efpfin_product"].Value = tradeDetailRow["product"].ToString();
                        
                        if (tradeDetailRow.IsNull("unit") == false && tradedatarow.Table.Columns.Contains("efpfin_unit")) tradeRow.Cells["efpfin_unit"].Value = tradeDetailRow["unit"].ToString();
                        if (tradeDetailRow.IsNull("quantity") == false && tradedatarow.Table.Columns.Contains("efpfin_quantity")) tradeRow.Cells["efpfin_quantity"].Value = Convert.ToDecimal(tradeDetailRow["quantity"]);
                        if (tradeDetailRow.IsNull("begtime") == false && tradedatarow.Table.Columns.Contains("efpfin_begtime")) tradeRow.Cells["efpfin_begtime"].Value = Convert.ToDateTime(tradeDetailRow["begtime"]);
                        if (tradeDetailRow.IsNull("endtime") == false && tradedatarow.Table.Columns.Contains("efpfin_endtime")) tradeRow.Cells["efpfin_endtime"].Value = Convert.ToDateTime(tradeDetailRow["endtime"]); //tradeDetailRow["endtime"];
                        if (tradeDetailRow.IsNull("timeunit") == false && tradedatarow.Table.Columns.Contains("efpfin_timeunit")) tradeRow.Cells["efpfin_timeunit"].Value = tradeDetailRow["timeunit"].ToString();
                        if (tradeDetailRow.IsNull("contractquantity") == false && tradedatarow.Table.Columns.Contains("efpfin_contractquantity")) tradeRow.Cells["efpfin_contractquantity"].Value = Convert.ToDecimal(tradeDetailRow["contractquantity"]);
                        
                        if (tradeDetailRow.IsNull("fee_priceindex") == false && tradedatarow.Table.Columns.Contains("efpfin_priceindex")) tradeRow.Cells["efpfin_priceindex"].Value = tradeDetailRow["fee_priceindex"].ToString();
                        if (tradeDetailRow.IsNull("fee_pricediff") == false && tradedatarow.Table.Columns.Contains("efpfin_pricediff")) tradeRow.Cells["efpfin_pricediff"].Value = Convert.ToDecimal(tradeDetailRow["fee_pricediff"]);
                        
                        if (tradeDetailRow.IsNull("fee_postprice") == false && tradedatarow.Table.Columns.Contains("efpfin_postprice")) tradeRow.Cells["efpfin_postprice"].Value = tradeDetailRow["fee_postprice"].ToString();
                        if (tradeDetailRow.IsNull("fee_postdate") == false && tradedatarow.Table.Columns.Contains("efpfin_postdate")) tradeRow.Cells["efpfin_postdate"].Value = Convert.ToDateTime(tradeDetailRow["fee_postdate"]);
                        
                    }
                    
                    _view.ViewGrids["trade"].EventManager.SetEnabled(Infragistics.Win.UltraWinGrid.GridEventIds.AfterCellUpdate, true);
                    
                }
                
                _view.ViewGrids["trade"].GetBindRow(tradeRow).AcceptChanges();
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Send Email - Internal Trade - AfterUpdateData
        Send Email - Internal Trade - Send an email when an internal trade is created */
        public UiEventResult AfterUpdateData_2(object sender, EventArgs e)
        {
            if (_view.DataSource.Tables["trade"].Columns.IndexOf("internaltradecreated") == -1)
                return new UiEventResult(EventStatus.Continue);
            
            DataRow[] addedRows = _view.DataSource.Tables["trade"].Select("internaltradecreated = 'true'");
            
            string emailListCC = Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "InternalTradeEmail", "EmailListCC" });
            
            DataSet useridDS = new DataSet();
            SqlHelper.RetrieveData(useridDS, new[] { "userid" }, new[] { "SELECT * FROM userid WHERE status = 'ACTIVE' and cst_userarea is not null" });
            
            for (int i = 0; i < addedRows.Length; i++ )
            {
                List<string> emailList = new List<string>();
                DataRow tradeRow = addedRows[i];
                string body = string.Empty;
                
                if (tradeRow["internaltrade"] != DBNull.Value)
                {
                    //Get email for the all users with the same area of the trader of the trade
                    DataRow userIdRow = useridDS.Tables["userid"].AsEnumerable().Where(x => x.Field<string>("name").Equals(tradeRow["trader"].ToString())).FirstOrDefault();
                    
                    if (userIdRow == null)
                        continue;
                    
                    string area = userIdRow["cst_userarea"].ToString();
                    
                    emailList = useridDS.Tables["userid"].AsEnumerable().Where(x => x.Field<string>("cst_userarea").Equals(area)).Select(x => x.Field<string>("email")).ToList();
                }
                
                if(emailList.Count > 0)
                {
                    DataRow tradedetailRow = _view.DataSource.Tables["tradedetail"].AsEnumerable().Where(x => x.Field<string>("trade").Equals(tradeRow["trade"].ToString())).FirstOrDefault();
                    DataRow pricedetailRow = _view.DataSource.Tables["feedetail"].AsEnumerable().Where(x => x.Field<string>("dbvalue").Equals(tradedetailRow["position"].ToString()) && x.Field<string>("dbcolumn").Equals("POSITION")).FirstOrDefault();
                    if (pricedetailRow == null)
                        pricedetailRow = _view.DataSource.Tables["pricedetail"].AsEnumerable().Where(x => x.Field<string>("dbvalue").Equals(tradedetailRow["position"].ToString()) && x.Field<string>("dbcolumn").Equals("POSITION")).FirstOrDefault();
                    
                    body = "<HTML> <BODY><th><br> Internal trade " + tradeRow["trade"].ToString() + " has been created. Please, assign a strategy. <br><br></th>";
                    body = body + "<table border=1> <th> Trade </th> \n" +
                    "<th> Trade Date </th> \n" +
                    "<th> Trader </th> \n" +
                    "<th> BegTime </th> \n" +
                    "<th> EndTime </th> \n" +
                    "<th> Quantity </th> \n" +
                    "<th> Unit </th> \n" +
                    "<th> Time Unit </th> \n" +
                    "<th> Index Factor </th> \n" +
                    "<th> Price Index </th> \n" +
                    "<th> Price </th> \n" +
                    "<th> Currency </th> \n" +
                    "<th> Internal Trade </th> \n" +
                    "<th> Internal Trade Strategy </th>";
                    
                    body = body + "<tr> <td> " + tradeRow["trade"].ToString() + " </td> \n" +
                    "<td> " + tradedetailRow["cst_tradedate"].ToString("dd/MM/yyyy") + " </td> \n" +
                    "<td> " + tradeRow["trader"].ToString() + " </td> \n" +
                    "<td> " + tradedetailRow["begtime"].ToString("dd/MM/yyyy") + " </td> \n" +
                    "<td> " + tradedetailRow["endtime"].ToString("dd/MM/yyyy") + " </td> \n" +
                    "<td> " + tradedetailRow["quantity"].ToString() + " </td> \n" +
                    "<td> " + tradedetailRow["unit"].ToString() + " </td> \n" +
                    "<td> " + tradedetailRow["timeunit"].ToString() + " </td> \n" +
                    "<td> " + pricedetailRow["indexfactor"].ToString() + " </td> \n" +
                    "<td> " + pricedetailRow["priceindex"].ToString() + " </td> \n" +
                    "<td> " + pricedetailRow["pricediff"].ToString() + " </td> \n" +
                    "<td> " + pricedetailRow["currency"].ToString() + " </td> \n" +
                    "<td> " + tradeRow["internaltrade"].ToString() + " </td> \n" +
                    "<td> " + tradedetailRow["cst_strategydescription"].ToString() + " </td> </tr> ";
                    
                    body = body + "</table> </BODY> </HTML>";
                    
                    //Send Mail
                    string from = "horizon_alerts@repsol.com";
                    string to = emailList.Aggregate((buffer, next) => buffer + "," + next);
                    //string cc = string.Empty;
                    string cc = emailListCC;
                    string subject = "Horizon Alert: New internal trade " + tradeRow["trade"].ToString() + " has been created";
                    //string body = "Internal trade " + tradeRow["trade"].ToString() + " has been created. Please, assign a strategy.";
                    bool htmlFormat = true;
                    
                    Email email = new Email();
                    email.Send(from, to, cc, subject, body, htmlFormat);
                }
            }
            
            _view.DataSource.Tables["trade"].Columns.Remove("internaltradecreated");
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Certificatetype_BeforeCellActivate
        Constraint Certificatetype */
        public UiEventResult BeforeCellActivate_certificatetype_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string product = "";
            if (_view.ViewGrids["tradedetail"].ActiveRow != null)
            {
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
            }
            else
            {
                DataRow row = grid.GetBindRow(e.Cell.Row);
                if (row.Table.Columns.Contains("product"))
                    product = !row.IsNull("product")? row["product"].ToString(): "";
            }
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", product, "commodityclass" });
            string newConstraint = "";
            DataRow tradedetailRow = null;
            
            if (_view.ViewGrids["tradedetail"].ActiveRow != null)
                tradedetailRow = _view.ViewGrids["tradedetail"].GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            else
            {
                DataSet ds = _view.DataSource;
                string trade = _view.ViewGrids["trade"].ActiveRow.Cells["trade"].GetValue<string>();
                DataRow[] tradeMatch = ds.Tables["tradedetail"].Select("trade = " + Expr.Value(trade));
                if (tradeMatch.Length > 0) tradedetailRow = tradeMatch[0];
            }
            
            bool internaltrade = _view.ViewGrids["trade"].ActiveRow.Cells["internal"].GetValue<bool>();
            
            string positionmode = "";
            bool option = false;
            if (tradedetailRow != null)
            {
                if (!tradedetailRow.IsNull("positionmode")) positionmode = tradedetailRow["positionmode"].ToString();
                if (!tradedetailRow.IsNull("optionposition")) option = Convert.ToBoolean(tradedetailRow["optionposition"]);
            }
            if (commodityclass == "EMISSION")
            {
                // newConstraint = "PURCHASED,SOLD";
                newConstraint = "GRANTED,PURCHASED,EXPIRED,SURRENDERED,SOLD";
                if ((positionmode == "PHYSICAL" && option) || internaltrade) newConstraint = "PURCHASED,SOLD";
            }
            else if (commodityclass == "RENEWABLE")
            {
                newConstraint = "PURCHASED,SOLD,EXPIRED";
                if ((positionmode == "PHYSICAL" && option) ||internaltrade) newConstraint = "PURCHASED,SOLD";
            }
            grid.SetColumnStyle(e.Cell.Column, newConstraint);
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Feedetail_Feemethod_BeforeCellActivate_1
        Feemethod constraint */
        public UiEventResult BeforeCellActivate_feedetail_feemethod_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "MONTHPRORATE,FIXEDPRORATE,FIXED,HOUR,DAY,WEEK,MONTH,QUARTER,YEAR,FULLTURN,HALFTURN,PERCENT,DELVOLUME,RECVOLUME,LOSSVOLUME,CONTRACTVOLUME,TIERVOLUME,TIERDIFF,PEAK,UNITDAY,UNITMONTH,MINTOLERANCE,MAXTOLERANCE,UNIT,INCREMENTAL,DECREMENTAL,DELIVERYHOUR,SCHEDULEVOLUME,STORAGE,STORAGEUSE,STORAGEEXCESS,WORLDSCALE,INTEREST,REGRADE,RAILCAR,TRUCK";
            
            grid.SetColumnStyle(e.Cell.Column, dbconstraint);
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Feedetail_Feemode_BeforeCellActivate_1
        Feemode constraint */
        public UiEventResult BeforeCellActivate_feedetail_feemode_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "FIXED,VARIABLE";
            
            grid.SetColumnStyle(e.Cell.Column, dbconstraint);
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Gasplantcontract_BeforeCellActive_1
        Gasplantcontract Constraint */
        public UiEventResult BeforeCellActivate_gasplantcontract_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "";
            dbconstraint = "Select distinct gasplantcontract from position, ngposition, processcontractpoint" +
            " where position.position = ngposition.position and ngposition.position = processcontractpoint.position" +
            " and position.company = " + Expr.Value(e.Cell.Row.GetCellValue<string>("company")) +
            " and position.counterparty=" + Expr.Value(e.Cell.Row.GetCellValue<string>("counterparty"));
            if (e.Cell.Row.Cells["pipeline"].Value != DBNull.Value) dbconstraint += " and processcontractpoint.pipeline=" + Expr.Value(e.Cell.Row.GetCellValue<string>("pipeline"));
            if (e.Cell.Row.Cells["point"].Value != DBNull.Value) dbconstraint += " and processcontractpoint.point=" + Expr.Value(e.Cell.Row.GetCellValue<string>("point"));
            
            grid.SetColumnStyle(e.Cell.Column, dbconstraint);
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Marketarea_BeforeCellActivate_1
        Constraint */
        public UiEventResult BeforeCellActivate_marketarea_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (grid.Name != "trade" && grid.Name != "tradedetail") return new UiEventResult(EventStatus.Continue);
            
            string trader = "";
            if (grid.Name == "trade")
            {
                trader = e.Cell.Row.GetCellValue<string>("trader");
            }
            else
            {
                DataRow[] tradeRows = ((ViewGrid)sender).GetBindRow(e.Cell.Row).GetParentRows("trade_tradedetail");
                if (tradeRows.Length > 0 && !tradeRows[0].IsNull("trader")) trader = tradeRows[0]["trader"].ToString();
            }
            
            string dbconstraint = "", marketarea= "";
            if (trader != null && trader.Length > 0) marketarea = Soap.Invoke<string>(_view.Class.Url, "CheckTraderConstraint", new string[] { "table", "key", "keyValue", "column" }, new object[] { "traderconstraint", "trader,dbcolumn", trader + ",marketarea", "dbvalue" });
            if (marketarea.Length > 0)
            {
                dbconstraint = "select dbvalue from traderconstraint where trader='" + trader + "' and dbcolumn='marketarea'";
            }
            else
            {
                dbconstraint = "Select location from location where status = 'ACTIVE' AND market = 1";
                string commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" }, new object[] { e.Cell.Row.GetCellValue<string>("positionmode"), e.Cell.Row.GetCellValue<string>("product") });
                if (commodity != "POWER" && commodity != "NG")
                {
                    string producttype = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "product", "product", e.Cell.Row.Cells["product"].GetValue<string>(), "producttype" });
                    dbconstraint += " and (producttype is null OR producttype = " + Expr.Value(producttype) + ")";
                }
            }
            
            string newConstraint = CombineWithTraderConstraint("marketarea", dbconstraint);
            grid.SetColumnStyle(e.Cell.Column, newConstraint);
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* BeforeCellActive_Pricelevel
        Pricelevel constraint
        Targets: trade/fee_pricelevel,tradedetail/fee_pricelevel,tradedetail/pricelevel,pricedetail/pricelevel,feedetail/pricelevel */
        public UiEventResult BeforeCellActivate_multi_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string priceindex = "";
            if (grid.Name.ToLower() == "trade")
                priceindex = e.Cell.Row.Cells["fee_priceindex"].GetValue<string>();
            else if (grid.Name.ToLower() == "tradedetail")
                priceindex = e.Cell.Row.Cells["fee_priceindex"].GetValue<string>();
            else if (grid.Name.ToLower() == "pricedetail")
                priceindex = e.Cell.Row.Cells["priceindex"].GetValue<string>();
            else if (grid.Name.ToLower() == "feedetail")
                priceindex = e.Cell.Row.Cells["priceindex"].GetValue<string>();
            
            string dbconstraint = "";
            string indextype = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "priceindex", "priceindex", priceindex, "indextype" });
            
            if (indextype.Equals("FORWARD")) dbconstraint = "SETTLE,PROMPT,DELIV,PROMPT CURRENT,PROMPT SETTLE,FORWARD";
            else if (indextype.Equals("POSTING")) dbconstraint = "POSTED,";
            else if (indextype.Equals("COMPOSITE") || indextype.Equals("CURRENT") || indextype.Equals("MONTHLY") || indextype.Equals("HOURLY")
                || indextype.Equals("HALFHOURLY") || indextype.Equals("QUARTERLY") || indextype.Equals("QTRHOUR")) dbconstraint = "AVG,HIGH,LOW";
            //else if (!string.IsNullOrEmpty(e.Cell.Row.Cells["priceindex"].GetValue<string>())) dbconstraint = "AVG,HIGH,LOW";
            else if (!string.IsNullOrEmpty(priceindex)) dbconstraint = "AVG,HIGH,LOW";
            else dbconstraint = "HIGH,LOW,AVG,PROMPT,POSTED,DELIV,SETTLE,PROMPT CURRENT,PROMPT SETTLE,FORWARD";
            
            grid.SetCellStyle(e.Cell, dbconstraint);
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* EFP_BeforeDropDown
        EFP Before Drop
        Targets: positiontype,efp_positiontype,fee_priceindex,product,timeunit,unit,carrier,cycle,delmethod,incoterms,location,paymentterms,efpfin_positiontype,efpphys_positiontype,efpphys_delmethod,efpphys_carrier,efpphys_location,efpphys_product,efpfin_product,efpphys_timeunit,efpfin_timeunit,efpphys_unit,efpfin_unit,efpphys_incoterms,efpphys_paymentterms,efpphys_spec,efpphys_priceindex,efpfin_priceindex,efpphys_cycle,efpphys_spec */
        public UiEventResult BeforeCellActivate_multi_2(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (_view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].Value != DBNull.Value && !_view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>().Contains("EFP")) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint="";
            string colname = e.Cell.Column.Key;
            
            if (colname.Contains("positiontype"))
                dbconstraint = "BUY,SELL,SWAP";
            else if (colname.Contains("positiontype"))
                dbconstraint = "BUY,SELL,SWAP";
            else if (colname.Contains("priceindex"))
                dbconstraint = "select priceindex, description from priceindex where status='ACTIVE'";
            else if (colname.Contains("product"))
                dbconstraint = "select product,description from product";
            else if (colname.Contains("timeunit"))
                dbconstraint = "QTRHOUR,HALFHOUR,HOUR,DAY,WEEK,MONTH,QUARTER,YEAR,TOTAL,SEASON";
            else if (colname.Contains("unit"))
                dbconstraint = "select unit,description from unit";
            else if (colname.Contains("carrier"))
                dbconstraint = "select carrier,description from carrier where status='ACTIVE'";
            else if (colname.Contains("cycle"))
            {
                dbconstraint = "select cycle from carriercycle";
                string where_clause = "";
                if (grid.Name == "trade")
                {
                    if (e.Cell.Row.Cells["efpphys_carrier"].Value != DBNull.Value)
                        where_clause = " carrier=" + SqlHelper.SqlValue(e.Cell.Row.Cells["efpphys_carrier"].Value);
                    if (e.Cell.Row.Cells["efpphys_begtime"].Value != DBNull.Value)
                    {
                        if (where_clause.Length > 0) where_clause += " and ";
                        where_clause += " endtime >" + SqlHelper.SqlValue(e.Cell.Row.Cells["efpphys_begtime"].Value);
                    }
                    if (e.Cell.Row.Cells["efpphys_endtime"].Value != DBNull.Value)
                    {
                        if (where_clause.Length > 0) where_clause += " and ";
                        where_clause += " begtime <" + SqlHelper.SqlValue(e.Cell.Row.Cells["efpphys_endtime"].Value);
                    }
                    if (where_clause.Length > 0) dbconstraint = dbconstraint + " where " + where_clause;
                }
                else if (grid.Name == "tradedetail")
                {
                    if (e.Cell.Row.Cells["carrier"].Value != DBNull.Value)
                        where_clause = " carrier=" + SqlHelper.SqlValue(e.Cell.Row.Cells["carrier"].Value);
                    if (e.Cell.Row.Cells["begtime"].Value != DBNull.Value)
                    {
                        if (where_clause.Length > 0) where_clause += " and ";
                        where_clause += " endtime >" + SqlHelper.SqlValue(e.Cell.Row.Cells["begtime"].Value);
                    }
                    if (e.Cell.Row.Cells["endtime"].Value != DBNull.Value)
                    {
                        if (where_clause.Length > 0) where_clause += " and ";
                        where_clause += " begtime <" + SqlHelper.SqlValue(e.Cell.Row.Cells["endtime"].Value);
                    }
                    if (where_clause.Length > 0) dbconstraint = dbconstraint + " where " + where_clause;
                }
            }
            else if (colname.Contains("delmethod"))
                dbconstraint = "select delmethod,description from delmethod";
            else if (colname.Contains("incoterms"))
                dbconstraint = "select incoterms,description from incoterms";
            else if (colname.Contains("location"))
                dbconstraint = "select location,description from location where status='ACTIVE'";
            else if (colname.Contains("paymentterms"))
                dbconstraint = "select paymentterms,description from paymentterms";
            
            grid.SetCellStyle(e.Cell, dbconstraint);
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Pricedetail_Feemethod_BeforeCellActive_1
        Feemethod constraint */
        public UiEventResult BeforeCellActivate_pricedetail_feemethod_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "COMMODITY PRICE";
            
            grid.SetColumnStyle(e.Cell.Column, dbconstraint);
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Product_BeforeCellActivate_1
        Constraint */
        public UiEventResult BeforeCellActivate_product_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (grid.Name != "trade" && grid.Name != "tradedetail") return new UiEventResult(EventStatus.Continue);
            
            string trader = "", tradetype = string.Empty;
            if (grid.Name == "trade")
            {
                trader = e.Cell.Row.GetCellValue<string>("trader");
                if (e.Cell.Row.Cells["tradetype"].Value != DBNull.Value)
                    tradetype = e.Cell.Row.GetCellValue<string>("tradetype");
            }
            else
            {
                DataRow[] tradeRows = ((ViewGrid)sender).GetBindRow(e.Cell.Row).GetParentRows("trade_tradedetail");
                if (tradeRows.Length > 0 && !tradeRows[0].IsNull("trader")) trader = tradeRows[0]["trader"].ToString();
            }
            
            string dbconstraint = "", product ="";
            
            if (trader != null && trader.Length > 0) product = Soap.Invoke<string>(_view.Class.Url, "CheckTraderConstraint", new string[] { "table", "key", "keyValue", "column" }, new object[] { "traderconstraint", "trader,dbcolumn", trader + ",product", "dbvalue" });
            if (product.Length < 0) product = Soap.Invoke<string>(_view.Class.Url, "CheckTraderConstraint", new string[] { "table", "key", "keyValue", "column" }, new object[] { "traderconstraint", "trader,dbcolumn", trader + ",producttype", "dbvalue" });
            
            if (product.Length > 0)
            {
                dbconstraint = "select product from product where product in (select dbvalue from traderconstraint where trader=" + Expr.Value(trader) + " and dbcolumn='product')";
                dbconstraint += " OR producttype in (select dbvalue from traderconstraint where trader=" + Expr.Value(trader) + " and dbcolumn='producttype')";
            }
            else
            {
                string trade = e.Cell.Row.GetCellValue<string>("trade");
                string positionmode = e.Cell.Row.GetCellValue<string>("positionmode");
                string commodityclass = "";
                if (_view.ViewGrids["tradedetail"].ActiveRow == null)
                {
                    commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                    new object[] { "tradetype", "tradetype", _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(), "commodityclass" });
                }
                else
                {
                    //Request 342124
                    if (tradetype != string.Empty)
                    {
                        commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                        new object[] { "tradetype", "tradetype", _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(), "commodityclass" });
                        if (string.IsNullOrEmpty(commodityclass)) commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
                    }
                    else
                    {
                        commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                        new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
                    }
                }
                string property = "";
                DataRow row = grid.GetBindRow(e.Cell.Row);
                if (commodityclass == "LIQUID")
                {
                    if (row.Table.Columns.Contains("phys_property")) property = row["phys_property"].ToString();
                    if (row.Table.Columns.Contains("property")) property = row["property"].ToString();
                    if (string.IsNullOrEmpty(property))
                    {
                        string sql = "SELECT DISTINCT product FROM product WHERE status = 'ACTIVE'";
                        //if (!string.IsNullOrEmpty(commodityclass)) sql += " AND commodityclass = " + SqlHelper.SqlValue(commodityclass);
                        sql = CombineWithTraderConstraint("product", sql);
                        if (!string.IsNullOrEmpty(tradetype)) sql += (sql.ToUpper().Contains("WHERE") ? " AND " : " WHERE ") + "((select count(1) from cst_tradetypeproduct where tradetype = " + SqlHelper.SqlValue(tradetype) + ") = 0 OR product in (select product from cst_tradetypeproduct where tradetype = " + SqlHelper.SqlValue(tradetype) + "))";
                        grid.SetColumnStyle(e.Cell.Column, sql);
                        return new UiEventResult(EventStatus.Continue);
                    }
                    string location = row["location"].ToString();
                    dbconstraint = "SELECT DISTINCT product FROM propgathering WHERE status = 'ACTIVE' AND property = " + Expr.Value(property);
                    if (!string.IsNullOrEmpty(location)) dbconstraint += " AND location = " + SqlHelper.SqlValue(location);
                }
                else if (commodityclass == "GAS")
                {
                    if (row.Table.Columns.Contains("ng_property")) property = row["ng_property"].ToString();
                    if (row.Table.Columns.Contains("property")) property = row["property"].ToString();
                    if (string.IsNullOrEmpty(property))
                    {
                        string sql = "SELECT DISTINCT product FROM product WHERE status = 'ACTIVE'";
                        //if (!string.IsNullOrEmpty(commodityclass)) sql += " AND commodityclass = " + SqlHelper.SqlValue(commodityclass);
                        sql = CombineWithTraderConstraint("product", sql);
                        if (!string.IsNullOrEmpty(tradetype)) sql += (sql.ToUpper().Contains("WHERE") ? " AND " : " WHERE ") + "((select count(1) from cst_tradetypeproduct where tradetype = " + SqlHelper.SqlValue(tradetype) + ") = 0 OR product in (select product from cst_tradetypeproduct where tradetype = " + SqlHelper.SqlValue(tradetype) + "))";
                        grid.SetColumnStyle(e.Cell.Column, sql);
                        return new UiEventResult(EventStatus.Continue);
                    }
                    string pipeline = row["pipeline"].ToString();
                    string point = "";
                    if (row.Table.Columns.Contains("ng_point")) point = row["ng_point"].ToString();
                    else if (row.Table.Columns.Contains("point")) point = row["point"].ToString();
                    string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
                    dbconstraint = "SELECT DISTINCT product FROM propgathering WHERE status = 'ACTIVE' AND property = " + Expr.Value(property);
                    if (!string.IsNullOrEmpty(point)) dbconstraint += " AND recpoint = " + SqlHelper.SqlValue(point); if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
                    if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND pipeline = " + SqlHelper.SqlValue(pipeline);
                }
                else
                {
                    dbconstraint = "SELECT DISTINCT product FROM product WHERE status = 'ACTIVE'";
                    //if (!string.IsNullOrEmpty(commodityclass)) dbconstraint += "AND commodityclass = " + SqlHelper.SqlValue(commodityclass);
                }
            }
            
            dbconstraint = CombineWithTraderConstraint("product", dbconstraint);
            if (!string.IsNullOrEmpty(tradetype)) dbconstraint += (dbconstraint.ToUpper().Contains("WHERE") ? " AND " : " WHERE ") + "((select count(1) from cst_tradetypeproduct where tradetype = " + SqlHelper.SqlValue(tradetype) + ") = 0 OR product in (select product from cst_tradetypeproduct where tradetype = " + SqlHelper.SqlValue(tradetype) + "))";
            grid.SetColumnStyle(e.Cell.Column, dbconstraint);
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Tiertimeunit_BeforeCellActive_1
        Tiertimeunit constraint */
        public UiEventResult BeforeCellActivate_tiertimeunit_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string listvalue = "QTRHOUR,HALFHOUR,HOUR,DAY,MONTH,TOTAL";
            
            grid.SetCellStyle(e.Cell, listvalue);
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Timeunit_BeforeCellActive_1
        Timeunit constraint */
        public UiEventResult BeforeCellActivate_timeunit_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (grid.Name.ToLower() != "trade" && grid.Name.ToLower() != "tradedetail" && grid.Name.ToLower() != "positionconstraint") return new UiEventResult(EventStatus.Continue);
            
            string product = string.Empty, positionmode = string.Empty, tradetype= string.Empty;
            bool timeseries = false;
            if (_view.ViewGrids["tradedetail"].ActiveRow != null)
            {
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
                positionmode = _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>();
            }
            else
            {
                DataRow row = grid.GetBindRow(e.Cell.Row);
                if (row.Table.Columns.Contains("product"))
                    product = !row.IsNull("product")? row["product"].ToString(): "";
                if (row.Table.Columns.Contains("positionmode"))
                    positionmode = !row.IsNull("positionmode") ? row["positionmode"].ToString() : "";
            }
            
            if (_view.ViewGrids["tradedetail"].ActiveRow != null) timeseries = _view.ViewGrids["tradedetail"].ActiveRow.Cells["timeseries"].GetValue<bool>();
            if (_view.ViewGrids["trade"].ActiveRow != null && timeseries == false)
            {
                tradetype = _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>();
                timeseries = Soap.Invoke<bool>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                new object[] { "tradetype", "tradetype", tradetype, "timeseries" });
            }
            
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", product, "positionclass" });
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", product, "commodityclass" });
            
            string listvalue = "QTRHOUR,HALFHOUR,HOUR,DAY,WEEK,MONTH,QUARTER,YEAR,SEASON,TOTAL";
            if (timeseries)
            {
                listvalue = "HOUR,";
                if (commodityclass == "POWER")
                {
                    listvalue = "QTRHOUR,HALFHOUR,HOUR";
                }
            }
            else
            {
                if (positionmode == "PHYSICAL")
                {
                    if (positionclass == "COMMODITY" && commodityclass == "GAS")
                    {
                        if (grid.Name.ToLower() == "positionconstraint")
                            listvalue = "HOUR,DAY";
                        else
                        listvalue = "HOUR,DAY,MONTH";
                    }
                    else if (positionclass != "COMMODITY" && commodityclass == "GAS")
                    {
                        listvalue = "HOUR,DAY,MONTH,YEAR";
                    }
                    else if (commodityclass == "EMISSION" || commodityclass == "RENEWABLE")
                    {
                        listvalue = "MONTH,TOTAL";
                    }
                    else if (commodityclass == "LIQUID" || commodityclass == "SOLID")
                    {
                        listvalue = "DAY,MONTH,TOTAL";
                    }
                    else if (commodityclass == "POWER")
                    {
                        listvalue = "QTRHOUR,HALFHOUR,HOUR,DAY,MONTH";
                    }
                }
                else if (positionmode == "FINANCIAL")
                {
                    if (positionclass == "COMMODITY" && commodityclass == "GAS")
                    {
                        if (grid.Name.ToLower() == "positionconstraint")
                            listvalue = "HOUR,DAY";
                        else
                        listvalue = "HOUR,DAY,MONTH,QUARTER";
                    }
                    else if (positionclass != "COMMODITY" && commodityclass == "GAS")
                    {
                        listvalue = "HOUR,DAY,MONTH,YEAR,QUARTER";
                    }
                    else if (commodityclass == "EMISSION" || commodityclass == "RENEWABLE")
                    {
                        listvalue = "MONTH,";
                    }
                    else if (commodityclass == "LIQUID" || commodityclass == "SOLID")
                    {
                        listvalue = "DAY,MONTH,QUARTER";
                    }
                    else if (commodityclass == "POWER")
                    {
                        listvalue = "QTRHOUR,HALFHOUR,HOUR,DAY,MONTH,QUARTER";
                    }
                }
            }
            
            grid.SetCellStyle(e.Cell, listvalue);
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Tradetype_BeforeCellActive_1
        Constraint */
        public UiEventResult BeforeCellActivate_tradetype_1(object sender, CancelableCellEventArgs e)
        {
            
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (grid.Name != "trade") return new UiEventResult(EventStatus.Continue);
            
            string trader = e.Cell.Row.GetCellValue<string>("trader");
            
            string dbconstraint = "", newConstraint = "", tradetype = "";
            if (trader != null && trader.Length > 0) tradetype = Soap.Invoke<string>(_view.Class.Url, "CheckTraderConstraint", new string[] { "table", "key", "keyValue", "column" }, new object[] { "traderconstraint", "trader,dbcolumn", trader + ",tradetype", "dbvalue" });
            if (tradetype.Length > 0)
            {
                dbconstraint = "select dbvalue from traderconstraint where trader='" + trader + "' and dbcolumn='tradetype'";
                newConstraint = CombineWithTraderConstraint("marketarea", dbconstraint);
            }
            else
            {
                newConstraint = "select tradetype from tradetype where status = 'ACTIVE'";
            }
            grid.SetColumnStyle(e.Cell.Column, newConstraint);
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Multi_BeforeCellUpdate_1
        Validate quantitystatus
        Targets: trade/company,trade/timeperiod,trade/begtime,trade/endtime,tradedetail/begtime,tradedetail/block,tradedetail/company,tradedetail/controlarea,tradedetail/counterparty,tradedetail/endtime,tradedetail/evergreentermdate,tradedetail/gencontrolarea,tradedetail/genpct,tradedetail/genpoint,tradedetail/location,tradedetail/pipeline,tradedetail/point,tradedetail/positiontype,tradedetail/product,tradedetail/quantity,tradedetail/timeunit,tradedetail/unit,tradedetail/registry,tradedetail/startcertificatenumber,tradedetail/endcertificatenumber,tradedetail/blocktotal,tradedetail/source,tradedetail/genunit,tradedetail/duedate,tradedetail/measure,tradedetail/accttype,tradedetail/optionstatus,tradedetail/certificatetype,tradedetail/accountname,tradedetail/accountnumber,tradedetail/registryconfirmation,tradedetail/shipperaccount,tradedetail/heatvalue,trade/timezone */
        public UiEventResult BeforeCellUpdate_multi_1(object sender, BeforeCellUpdateEventArgs e)
        {
            
            if (e.Cell.Row.IsFilterRow) return new UiEventResult(EventStatus.Continue);
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row).RowState == DataRowState.Added) return new UiEventResult(EventStatus.Continue);
            
            string colname = e.Cell.Column.Key;
            DateTime newbegtime = DateTime.MinValue; DateTime newendtime = DateTime.MinValue;
            DateTime begtime = DateTime.MinValue; DateTime endtime = DateTime.MinValue; DateTime evergreentermdate = DateTime.MinValue;
            
            if (((ViewGrid)sender).Name.ToLower() == "trade")
            {
                if (colname == "begtime" && e.NewValue != null && e.NewValue != DBNull.Value) newbegtime = Convert.ToDateTime(e.NewValue);
                
                if (colname == "endtime" && e.NewValue != null && e.NewValue != DBNull.Value) newendtime = Convert.ToDateTime(e.NewValue);
                Dictionary<string, string> commodityDict = new Dictionary<string, string>();
                DataRow[] drPositions = _view.DataSource.Tables["tradedetail"].Select("trade = " + Expr.Value(e.Cell.Row.GetCellValue<string>("trade")));
                foreach (DataRow drPosition in drPositions)
                {
                    if (newbegtime == DateTime.MinValue)
                        begtime = Convert.ToDateTime(drPosition.GetColumnValue<DateTime?>("begtime"));
                    else
                    begtime = newbegtime;
                    
                    if (newendtime == DateTime.MinValue)
                        endtime = Convert.ToDateTime(drPosition.GetColumnValue<DateTime?>("endtime"));
                    else
                    endtime = newendtime;
                    
                    if (colname == "timeperiod" && e.NewValue != DBNull.Value)
                    {
                        TimeRange tr = TimePeriod.GetTimeRange(e.NewValue.ToString());
                        colname = "begtime";
                        if (tr.EndTime <= endtime) colname = "endtime";
                        begtime = tr.BegTime;
                        endtime = tr.EndTime;
                    }
                    //string commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" }, new object[] { drPosition.GetColumnValue<string>("positionmode"), drPosition.GetColumnValue<string>("product") });
                    string commodity = string.Empty;
                    string positionmode = drPosition.GetColumnValue<string>("positionmode");
                    string product = drPosition.GetColumnValue<string>("product");
                    string key = positionmode + product;
                    if (commodityDict.ContainsKey(key))
                        commodity = commodityDict[key];
                    else
                    {
                        commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" }, new object[] { positionmode, product});
                        commodityDict.Add(key, commodity);
                    }
                    
                    string message = Soap.Invoke<string>(_view.Class.Url, "GetQuantityStatusWithPosdetail",
                    new string[] { "position", "posdetail", "commodity", "begtime", "endtime", "positionBegtime", "positionEndtime", "colname", "evergreentermdate"},
                    new object[] { drPosition.GetColumnValue<string>("position"),
                    drPosition.GetColumnValue<string>("posdetail"),
                    commodity,
                    e.Cell.Row.GetCellValue<DateTime?>("begtime"),
                    e.Cell.Row.GetCellValue<DateTime?>("endtime"),
                    begtime,
                    endtime,
                    colname,
                    DateTime.MinValue
                    });
                    
                    if (!string.IsNullOrEmpty(message))
                    {
                        ErrorDialog.Show("Error", message);
                        e.Cancel = true;
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
            }
            else
            {
                if (colname == "begtime" && e.NewValue != null && e.NewValue != DBNull.Value) newbegtime = Convert.ToDateTime(e.NewValue);
                
                if (colname == "endtime" && e.NewValue != null && e.NewValue != DBNull.Value) newendtime = Convert.ToDateTime(e.NewValue);
                if (colname == "evergreentermdate" && e.NewValue != null && e.NewValue != DBNull.Value)
                    evergreentermdate = Convert.ToDateTime(e.NewValue);
                if (newbegtime == DateTime.MinValue)
                    begtime = Convert.ToDateTime(e.Cell.Row.GetCellValue<DateTime?>("begtime"));
                else
                begtime = newbegtime;
                
                if (newendtime == DateTime.MinValue)
                    endtime = Convert.ToDateTime(e.Cell.Row.GetCellValue<DateTime?>("endtime"));
                else
                endtime = newendtime;
                
                string commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" }, new object[] { e.Cell.Row.GetCellValue<string>("positionmode"), e.Cell.Row.GetCellValue<string>("product") });
                string message = Soap.Invoke<string>(_view.Class.Url, "GetQuantityStatusWithPosdetail",
                new string[] { "position", "posdetail", "commodity", "begtime", "endtime", "positionBegtime", "positionEndtime", "colname", "evergreentermdate" },
                new object[] { e.Cell.Row.GetCellValue<string>("position"),
                e.Cell.Row.GetCellValue<string>("posdetail"),
                commodity,
                _view.ViewGrids["trade"].ActiveRow.GetCellValue<DateTime?>("begtime"),
                _view.ViewGrids["trade"].ActiveRow.GetCellValue<DateTime?>("endtime"),
                begtime,
                endtime,
                colname,
                evergreentermdate
                });
                
                if (!string.IsNullOrEmpty(message))
                {
                    ErrorDialog.Show("Error", message);
                    e.Cancel = true;
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Validate quantitystatus - quantity detail pane
        Validate quantitystatus
        Targets: totalquantity,he1,he2,he3,he4,he5,he6,he7,he8,he9,he10,he11,he12,he13,he14,he15,he16,he17,he18,he19,he20,he21,he22,he23,he24,he25,he26,he27,he28,he29,he30,he,31,he32,he33,he34,he35,he36,he37,he38,he39,he40,he41,he42,he43,he44,he45,he46,he47,he48,he49,he50,he51,he52,he53,he54,he55,he56,he57,he58,he59,he60,he61,he62,he63,he64,he65,he66,he67,he68,he69,he70,he71,he72,he73,he74,he75,he76,he77,he78,he79,he80,he81,he82,he83,he84,he85,he86,he87,he88,he89,he90,he91,he92,he93,he94,he95,he96,he97,he98,he99,he100 */
        public UiEventResult BeforeCellUpdate_multi_8(object sender, BeforeCellUpdateEventArgs e)
        {
            
            if (e.Cell.Row.IsFilterRow) return new UiEventResult(EventStatus.Continue);
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row).RowState == DataRowState.Added) return new UiEventResult(EventStatus.Continue);
            
            string colname = e.Cell.Column.Key;
            DateTime begtime = Convert.ToDateTime(e.Cell.Row.GetCellValue<DateTime?>("begtime"));
            DateTime endtime = Convert.ToDateTime(e.Cell.Row.GetCellValue<DateTime?>("endtime"));
            
            string commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" },
            new object[] { _view.ViewGrids["tradedetail"].ActiveRow.GetCellValue<string>("positionmode"), _view.ViewGrids["tradedetail"].ActiveRow.GetCellValue<string>("product") });
            
            string message = Soap.Invoke<string>(_view.Class.Url, "GetQuantityStatusWithPosdetail",
            new string[] { "position", "posdetail", "commodity", "begtime", "endtime", "positionBegtime", "positionEndtime", "colname", "evergreentermdate" },
            new object[] { e.Cell.Row.GetCellValue<string>("position"),
            e.Cell.Row.GetCellValue<string>("posdetail"),
            commodity,
            _view.ViewGrids["trade"].ActiveRow.GetCellValue<DateTime?>("begtime"),
            _view.ViewGrids["trade"].ActiveRow.GetCellValue<DateTime?>("endtime"),
            begtime,
            endtime,
            "quantity",
            DateTime.MinValue });
            
            if (!string.IsNullOrEmpty(message))
            {
                ErrorDialog.Show("Error", message);
                e.Cancel = true;
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* GasDayTimeEntry_Trade_Before
        Gas Day Time Entry - Trade
        Targets: trade/begtimenormal,trade/endtimenormal,tradedetail/begtimenormal,tradedetail/endtimenormal */
        public UiEventResult BeforeCellUpdate_multi_100(object sender, BeforeCellUpdateEventArgs e)
        {
            //Allow beg/endtimenormal columns to be set to NULL.
            if (!e.Cell.IsActiveCell) return new UiEventResult(EventStatus.Continue);
            if (e.NewValue.Equals(System.DBNull.Value)) return new UiEventResult(EventStatus.Continue);
            
            //Capture invoking column name and value
            string sInvoker = e.Cell.Column.ToString();
            string sInvtable = e.Cell.Band.ToString();
            string sProductT = string.Empty;
            string sPipelineT = string.Empty;
            
            #region Capture / Cancel
            sProductT = e.Cell.Row.GetCellValue<string>("product");
            sPipelineT = e.Cell.Row.GetCellValue<string>("pipeline");
            #endregion
            
            //Prevent begtimenormal overlapping endtimenormal
            #region Timerange Check
            if (sInvoker.Contains("normal"))
            {
                if (sInvoker.Contains("begtime") && !e.Cell.Row.GetCellValue<DateTime>("endtimenormal").Equals(DateTime.MinValue))
                {
                    if ((DateTime)e.NewValue >= e.Cell.Row.GetCellValue<DateTime>("endtimenormal"))
                    {
                        MessageBox.Show("Begtime must be before Endtime.", "Error");
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
                if (sInvoker.Contains("endtime") && !e.Cell.Row.GetCellValue<DateTime>("begtimenormal").Equals(DateTime.MinValue))
                {
                    if (e.Cell.Row.GetCellValue<DateTime>("begtimenormal") >= (DateTime)e.NewValue)
                    {
                        MessageBox.Show("Begtime must be before Endtime.", "Error");
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
            }
            #endregion
            
            //Only NG trades allow begtimenormal/endtimenormal to be updated on trade level.
            #region NG Check
            List<int> lMarkethours = new List<int>();
            int iHourP = 0;
            if (sInvoker.Contains("normal") && sInvtable == "trade")
            {
                //If trade level product/pipeline is empty, check positions for legitimate values.
                if ((sProductT == string.Empty || sProductT == null) || (sPipelineT == string.Empty || sPipelineT == null))
                {
                    if (_view.ViewGrids["tradedetail"].Rows.Count > 1)
                    {
                        foreach (UltraGridRow gRow in _view.ViewGrids["tradedetail"].Rows)
                        {
                            string sTmpprod = gRow.Cells["product"].GetValue<string>();
                            string sTmppip = gRow.Cells["pipeline"].GetValue<string>();
                            if ((sTmpprod != String.Empty && sTmpprod != null) && (sTmppip != String.Empty && sTmppip != null))
                            {
                                try
                                {
                                    bool bTmpgasDay = Soap.Invoke<bool>("Utility_WebWS.asmx", "GetProductGasDay", new string[] { "product" }, new object[] { sTmpprod });
                                    if (bTmpgasDay)
                                    {
                                        //Check that each position has a pipeline with the same markethour.
                                        string hour = Soap.Invoke<string>("Utility_WebWS.asmx", "GetPipelineMarketDayHour", new string[] { "pipeline" }, new object[] { sTmppip });
                                        iHourP = hour.Contains(".") ? Convert.ToInt16(hour.Substring(0, hour.IndexOf('.', 0))) : Convert.ToInt16(hour);
                                        if (lMarkethours.Count > 0)
                                        {
                                            if (lMarkethours[lMarkethours.Count - 1] != iHourP)
                                            {
                                                MessageBox.Show("'Normal' time update only possible on trade level if all Positions are on Pipelines which use the same Market Hour.\nPlease update on Position level.", "Error");
                                                return new UiEventResult(EventStatus.Cancel);
                                            }
                                        }
                                        lMarkethours.Add(iHourP);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Please select Gas Day product for all positions.", "Error");
                                        return new UiEventResult(EventStatus.Cancel);
                                    }
                                }
                                catch (Exception)
                                {
                                    return new UiEventResult(EventStatus.Cancel);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please select NG product and pipeline for all positions.", "Error");
                                return new UiEventResult(EventStatus.Cancel);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select NG product and pipeline for all positions.", "Error");
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
            }
            #endregion
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Trade_Internal_BeforeCellUpdate_1
        Validate internal trade */
        public UiEventResult BeforeCellUpdate_trade_internal_1(object sender, BeforeCellUpdateEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row).RowState != DataRowState.Added)
            {
                if ((bool)e.NewValue)
                    ErrorDialog.Show("Error", "You cannot convert a normal trade to an internal trade.");
                else
                ErrorDialog.Show("Error", "You cannot convert an internal trade to a nomal trade.");
                
                e.Cancel = true;
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Status_Verification
        Trade_Status_Verification */
        public UiEventResult BeforeCellUpdate_trade_status_1(object sender, BeforeCellUpdateEventArgs e)
        {
            
            if (e.Cell.Row.IsFilterRow) return new UiEventResult(EventStatus.Continue);
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row).RowState == DataRowState.Added) return new UiEventResult(EventStatus.Continue);
            
            if (e.Cell.Row.Cells["status"].GetValue<string>() != null)
            {
                string product = string.Empty;
                if (_view.ViewGrids["tradedetail"].ActiveRow == null)
                    product = e.Cell.Row.Cells["product"].GetValue<string>();
                else
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
                string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                new object[] { "product", "product", product, "commodityclass" });
                string quantitytable = "";
                if (commodityclass == "POWER")
                    quantitytable = "powerquantity";
                else if (commodityclass == "GAS")
                    quantitytable = "ngquantity";
                else if (commodityclass == "SOLID" || commodityclass == "LIQUID")
                    quantitytable = "physicalquantity";
                else if (commodityclass == "EMISSION" || commodityclass == "RENEWABLE")
                    quantitytable = "emissionquantity";
                else
                return new UiEventResult(EventStatus.Continue);
                
                string whereclause = "quantitystatus = 'ACTUAL' and position.position="+quantitytable+".position and position.trade ='" + e.Cell.Row.Cells["trade"].GetValue<string>()+"'";
                string quantitystatus = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
                new object[] { "position,"+ quantitytable, "quantitystatus", whereclause });
                if (!string.IsNullOrEmpty(quantitystatus) && quantitystatus.Contains("ACTUAL"))
                {
                    ErrorDialog.Show("Error", "Trade status cannot be changed on a trade that has already been actualized.");
                    e.Cancel = true;
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_Tradetype_BeforeCellUpdate_1
        Validate tradetype cannot be changed if trade has been updated */
        public UiEventResult BeforeCellUpdate_trade_tradetype_1(object sender, BeforeCellUpdateEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row).RowState != DataRowState.Added &&
            e.Cell.Row.Cells["trade"].GetValue<string>() != null)
            {
                ErrorDialog.Show("Error", "Trade type cannot be changed on a trade that has already been saved.");
                e.Cancel = true;
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_Paymentterms_AfterCellUpdate_1
        Update prepaid field if paymentterms is "PREPAID" */
        public UiEventResult BeforeCellUpdate_tradedetail_paymentterms_1(object sender, BeforeCellUpdateEventArgs e)
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeDetail_Positionmode_BeforeCellUpdate_1
        Validate positionmode cannot be changed if trade has been updated */
        public UiEventResult BeforeCellUpdate_tradedetail_positionmode_1(object sender, BeforeCellUpdateEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row).RowState != DataRowState.Added &&
            e.Cell.Row.Cells["trade"].GetValue<string>() != null)
            {
                ErrorDialog.Show("Error", "Position mode cannot be changed on a trade that has already been saved.");
                e.Cancel = true;
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Contract_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_contract_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string positionmode = "", product = "";
            if (_view.ViewGrids["tradedetail"].ActiveRow != null)
            {
                positionmode = _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>();
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
            }
            else
            {
                DataRow row = grid.GetBindRow(cell.Row);
                if (row.Table.Columns.Contains("product"))
                    product = !row.IsNull("product")? row["product"].ToString(): "";
                if (row.Table.Columns.Contains("positionmode"))
                    positionmode = !row.IsNull("positionmode")? row["positionmode"].ToString() : "";
            }
            
            string dbconstraint = GetContractConstraint(cell.Row.Cells["begtime"].GetValue<DateTime>(), cell.Row.Cells["endtime"].GetValue<DateTime>(),
            _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(), cell.Row.Cells["positiontype"].GetValue<string>(),
            positionmode, product, cell.Row.Cells["company"].GetValue<string>(), cell.Row.Cells["counterparty"].GetValue<string>());
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
        }
        /* Counterparty_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_counterparty_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string dbconstraint = "";
            if (_view.ViewGrids["trade"].ActiveRow.Cells["internal"].GetValue<bool>())
            {
                dbconstraint = "Select (company) col1, (company) col2 From company";
                dbconstraint += " Where company in (select company from company)";
                dbconstraint += " order by company";
            }
            else
            {
                dbconstraint = "Select (counterparty) col1, (name) col2 from counterparty where counterparty.status = 'ACTIVE'";
                if (DbModel.GetDbTable("creditlimit") != null)
                {
                    if (DbModel.GetDbColumn("creditlimit", "counterparty") != null)
                        dbconstraint += " and counterparty not in (select distinct counterparty from creditlimit where creditstatus='RESTRICTED' and company = " + SqlHelper.SqlValue(cell.Row.Cells["company"].Value) +")" ;
                    else if (DbModel.GetDbColumn("creditlimit", "creditparty") != null)
                        dbconstraint += " and counterparty not in (select distinct creditparty from creditlimit where creditstatus='RESTRICTED' and company = " + SqlHelper.SqlValue(cell.Row.Cells["company"].Value)  +")" ;
                }
                
                dbconstraint += " order by counterparty";
            }
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Counterparty - BeforeDropDown 2
        Counterparty - EVE - Add constraint for approvals */
        public UiEventResult BeforeDropDown_counterparty_2(BaseGrid grid, UltraGridCell cell)
        {
            return new UiEventResult(EventStatus.Continue, "collaboration in (select collaboration collaboration from approval where approval = 1 and dbtable = 'counterparty' and approvaltype = 'COMPLIANCE APPROVED') and collaboration in (select collaboration collaboration from approval where approval = 1 and dbtable = 'counterparty' and approvaltype = 'CREDIT APPROVED')");
        }
        /* Cycle_BeforeDropDown_1
        Cycle Constraint */
        public UiEventResult BeforeDropDown_cycle_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "select cycle from carriercycle";
            string where_clause = "";
            
            if (cell.Row.Cells["carrier"].Value != DBNull.Value)
                where_clause = " carrier=" + SqlHelper.SqlValue(cell.Row.Cells["carrier"].Value);
            if (cell.Row.Cells["begtime"].Value != DBNull.Value)
            {
                if (where_clause.Length > 0) where_clause += " and ";
                where_clause += " endtime >" + SqlHelper.SqlValue(cell.Row.Cells["begtime"].Value);
            }
            if (cell.Row.Cells["endtime"].Value != DBNull.Value)
            {
                if (where_clause.Length > 0) where_clause += " and ";
                where_clause += " begtime <" + SqlHelper.SqlValue(cell.Row.Cells["endtime"].Value);
            }
            if (where_clause.Length > 0) dbconstraint = dbconstraint + " where " + where_clause;
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Delpoint_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_delpoint_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string property = cell.Row.Cells["property"].GetValue<string>();
            if (string.IsNullOrEmpty(property)) return new UiEventResult(EventStatus.Continue);
            
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
            string product = cell.Row.Cells["product"].GetValue<string>();
            string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
            string trade = cell.Row.Cells["trade"].GetValue<string>();
            string dbconstraint = "Select distinct delpoint From propgathering WHERE property = " + SqlHelper.SqlValue(property);
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND product = " + SqlHelper.SqlValue(product);
            if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
            if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND pipeline = " + SqlHelper.SqlValue(pipeline);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Exchbrokercontract_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_exchbrokercontract_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string product = string.Empty, positionmode = string.Empty;
            if (_view.ViewGrids["tradedetail"].ActiveRow != null)
            {
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
                positionmode = _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>();
            }
            else
            {
                DataRow row = grid.GetBindRow(cell.Row);
                if (row.Table.Columns.Contains("product"))
                    product = !row.IsNull("product")? row["product"].ToString(): "";
                if (row.Table.Columns.Contains("positionmode"))
                    positionmode = !row.IsNull("positionmode") ? row["positionmode"].ToString() : "";
            }
            
            string dbconstraint = GetContractConstraint(cell.Row.Cells["begtime"].GetValue<DateTime>(), cell.Row.Cells["endtime"].GetValue<DateTime>(),
            _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(), cell.Row.Cells["positiontype"].GetValue<string>(),
            positionmode, product, cell.Row.Cells["company"].GetValue<string>(), cell.Row.Cells["exchbroker"].GetValue<string>());
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* FeeDetail_FeeContract_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_feedetail_feecontract_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            DateTime begtime = grid.GetBindRow(cell.Row).IsNull("begtime") ? _view.ViewGrids["tradedetail"].ActiveRow.Cells["begtime"].GetValue<DateTime>() : cell.Row.Cells["begtime"].GetValue<DateTime>();
            DateTime endtime = grid.GetBindRow(cell.Row).IsNull("endtime") ? _view.ViewGrids["tradedetail"].ActiveRow.Cells["endtime"].GetValue<DateTime>() : cell.Row.Cells["endtime"].GetValue<DateTime>();
            string dbconstraint = GetContractConstraint(begtime, endtime, _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(),
            _view.ViewGrids["tradedetail"].ActiveRow.Cells["positiontype"].GetValue<string>(), _view.ViewGrids["tradedetail"].ActiveRow.Cells["positionmode"].GetValue<string>(),
            _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), cell.Row.Cells["company"].GetValue<string>(), cell.Row.Cells["counterparty"].GetValue<string>());
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Loadshape_BeforeDropDown_1
        Loadshape constraint */
        public UiEventResult BeforeDropDown_loadshape_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "select loadshape, description from loadshape where status = 'ACTIVE'";
            string commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" }, new object[] { cell.Row.Cells["positionmode"].GetValue<string>(), cell.Row.Cells["product"].GetValue<string>() });
            
            string whereclause = "unit=" + Expr.Value(cell.Row.Cells["unit"].GetValue<string>());
            string unittype = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" }, new object[] { "unit", "unittype", whereclause });
            
            if (commodity == "POWER")
            {
                dbconstraint += " and type in ('ENERGY','ENERGY RATE','FACTOR')";
            }
            else if (commodity == "NG")
            {
                if (unittype == "ENERGY") dbconstraint += " and type in ('ENERGY','FACTOR')";
                else if (unittype == "VOLUME") dbconstraint += " and type in ('VOLUME','FACTOR')";
            }
            else if (commodity == "EMISSION")
            {
                dbconstraint += " and commodityclass = 'EMISSION'";
            }
            else if (commodity == "RENEWABLE")
            {
                dbconstraint += " and commodityclass = 'RENEWABLE'";
            }
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Location_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_location_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null)
                return new UiEventResult(EventStatus.Continue);
            
            DataRow row = grid.GetBindRow(cell.Row);
            //Request 341671
            string product = string.Empty, carrier = string.Empty;
            if (row.Table.TableName.ToLower() == "optiondetail")
            {
                if (row.Table.Columns.Contains("product"))
                    product = !row.IsNull("product")? row["product"].ToString(): "";
                if (row.Table.Columns.Contains("carrier"))
                    carrier = !row.IsNull("carrier")? row["carrier"].ToString() : "";
            }
            else
            {
                if (_view.ViewGrids["tradedetail"].ActiveRow != null)
                    product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
                else
                {
                    if (row.Table.Columns.Contains("product"))
                        product = !row.IsNull("product")? row["product"].ToString(): "";
                }
            }
            string[] arg_name = new string[4] { "table", "key", "keyValue", "column" };
            object[] arg_value = new object[4] { "product", "product", product, "commodityclass" };
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", arg_name, arg_value);
            if (commodityclass != "LIQUID")
            {
                string sql = "SELECT DISTINCT location, description FROM location WHERE status = 'ACTIVE'";
                if (row.Table.TableName.ToLower() == "optiondetail")
                {
                    if (commodityclass == "POWER")
                    {
                        if (carrier.Length > 0)
                            sql = "SELECT DISTINCT point, description FROM powerpoint WHERE controlarea = " + Expr.Value(carrier) + " and status = 'ACTIVE'";
                        else
                        sql = "SELECT DISTINCT point, description FROM powerpoint WHERE status = 'ACTIVE'";
                    }
                    else if (commodityclass == "GAS")
                    {
                        if (carrier.Length > 0)
                            sql = "SELECT DISTINCT point, description FROM ngpoint WHERE pipeline = " + Expr.Value(carrier) + " and status = 'ACTIVE'";
                        else
                        sql = "SELECT DISTINCT point, description FROM ngpoint WHERE status = 'ACTIVE'";
                    }
                }
                
                return new UiEventResult(EventStatus.Continue, sql);
            }
            
            string property = "";
            if (row.Table.Columns.Contains("phys_property")) property = row["phys_property"].ToString();
            if (row.Table.Columns.Contains("property")) property = row["property"].ToString();
            if (string.IsNullOrEmpty(property))
                return new UiEventResult(EventStatus.Continue, "SELECT DISTINCT location, description FROM location WHERE status = 'ACTIVE'");
            
            string dbconstraint = "SELECT DISTINCT propgathering.location, location.description FROM propgathering,location WHERE location.status = 'ACTIVE' AND propgathering.status = 'ACTIVE' AND propgathering.location = location.location AND property = " + SqlHelper.SqlValue(property);
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND product = " + SqlHelper.SqlValue(product);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* NgDelpoint_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_ng_delpoint_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string property = cell.Row.Cells["ng_property"].GetValue<string>();
            if (string.IsNullOrEmpty(property)) return new UiEventResult(EventStatus.Continue);
            string product = cell.Row.Cells["product"].GetValue<string>();
            string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
            string dbconstraint = "Select distinct delpoint From propgathering WHERE poperty = " + SqlHelper.SqlValue(property);
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND product = " + SqlHelper.SqlValue(product); if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
            if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND pipeline = " + SqlHelper.SqlValue(pipeline);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* NgPoint_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_ng_point_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string property = cell.Row.Cells["ng_property"].GetValue<string>();
            if (string.IsNullOrEmpty(property)) return new UiEventResult(EventStatus.Continue);
            
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", cell.Row.Cells["product"].GetValue<string>(), "positionclass" });
            
            string product = cell.Row.Cells["product"].GetValue<string>();
            string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
            string dbconstraint = "SELECT DISTINCT recpoint,ngpoint.description FROM propgathering,ngpoint WHERE propgathering.recpoint = ngpoint.point AND propgathering.status = 'ACTIVE' AND property = " + SqlHelper.SqlValue(property);
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND product = " + SqlHelper.SqlValue(product); if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
            if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND propgathering.pipeline = " + SqlHelper.SqlValue(pipeline);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* NgProperty_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_ng_property_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", cell.Row.Cells["product"].GetValue<string>(), "commodityclass" });
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", cell.Row.Cells["product"].GetValue<string>(), "positionclass" });
            string dbconstraint = "SELECT DISTINCT propgathering.property, property.description FROM propgathering,product,property WHERE propgathering.status = 'ACTIVE' AND product.status = 'ACTIVE' AND propgathering.product = product.product AND propgathering.property = property.property";
            
            if (commodityclass == "GAS") dbconstraint += " AND product.commodityclass = 'GAS'";
            else dbconstraint += " AND 1=0";
            string product = cell.Row.Cells["product"].GetValue<string>();
            string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
            string point = cell.Row.Cells["ng_point"].GetValue<string>();
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND propgathering.product = " + SqlHelper.SqlValue(product);
            if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null AND";
            if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND pipeline = " + SqlHelper.SqlValue(pipeline);
            if (!string.IsNullOrEmpty(point)) dbconstraint += " AND recpoint = " + SqlHelper.SqlValue(point);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* NgRecpoint_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_ng_recpoint_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string property = cell.Row.Cells["ng_property"].GetValue<string>();
            if (string.IsNullOrEmpty(property)) return new UiEventResult(EventStatus.Continue);
            string product = cell.Row.Cells["product"].GetValue<string>();
            string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", cell.Row.Cells["product"].GetValue<string>(), "positionclass" });
            string dbconstraint = "Select distinct recpoint From propgathering WHERE property = " + SqlHelper.SqlValue(property);
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND product = " + SqlHelper.SqlValue(product);
            if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
            if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND pipeline = " + SqlHelper.SqlValue(pipeline);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Paymentterms_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_paymentterms_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            if (grid.GetBindRow(cell.Row).Table.Columns.Contains("contract"))
            {
                string dbconstraint = string.Empty;
                if (!string.IsNullOrEmpty(cell.Row.Cells["exchange"].GetValue<string>()) && !string.IsNullOrEmpty(cell.Row.Cells["exchbroker"].GetValue<string>()))
                    dbconstraint = "Select distinct paymentterms from contractacctg where contract = " + Expr.Value(cell.Row.Cells["exchbrokercontract"].GetValue<string>());
                else
                dbconstraint = "Select distinct paymentterms from contractacctg where contract = " + Expr.Value(cell.Row.Cells["contract"].GetValue<string>());
                
                if (!string.IsNullOrEmpty(cell.Row.Cells["product"].GetValue<string>()))
                {
                    dbconstraint += " and ( product = " + Expr.Value(cell.Row.Cells["product"].GetValue<string>());
                    dbconstraint += " or product is null  ) ";
                }
                
                if (_view.ViewGrids["trade"].ActiveRow != null && !string.IsNullOrEmpty(_view.ViewGrids["trade"].ActiveRow.Cells["currency"].GetValue<string>()))
                    dbconstraint += " and currency=" + Expr.Value(_view.ViewGrids["trade"].ActiveRow.Cells["currency"].GetValue<string>());
                
                return new UiEventResult(EventStatus.Continue, dbconstraint);
            }
            else if (grid.GetBindRow(cell.Row).Table.Columns.Contains("feecontract"))
            {
                //return new UiEventResult(EventStatus.Continue, "Select distinct paymentterms from contractacctg where contract = " + Expr.Value(cell.Row.Cells["feecontract"].GetValue<string>()));
                
                string dbconstraint = "Select distinct paymentterms from contractacctg where contract = " + Expr.Value(cell.Row.Cells["feecontract"].GetValue<string>());
                if (!string.IsNullOrEmpty(cell.Row.Cells["product"].GetValue<string>()))
                {
                    dbconstraint += " and ( product = " + Expr.Value(cell.Row.Cells["product"].GetValue<string>());
                    dbconstraint += " or product is null  ) ";
                }
                if (!string.IsNullOrEmpty(cell.Row.Cells["currency"].GetValue<string>()))
                {
                    dbconstraint += " and ( currency = " + Expr.Value(cell.Row.Cells["currency"].GetValue<string>());
                    dbconstraint += " or currency is null  ) ";
                }
                return new UiEventResult(EventStatus.Continue, dbconstraint);
            }
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* PhysProperty_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_phys_property_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", cell.Row.Cells["product"].GetValue<string>(), "commodityclass" });
            string dbconstraint = "SELECT DISTINCT propgathering.property, property.description FROM propgathering,product,property WHERE propgathering.status = 'ACTIVE' AND product.status = 'ACTIVE' AND propgathering.product = product.product AND propgathering.property = property.property";
            
            if (commodityclass == "LIQUID") dbconstraint += " AND product.commodityclass = 'LIQUID'";
            else dbconstraint += " AND 1=0";
            string product = cell.Row.Cells["product"].GetValue<string>();
            string location = cell.Row.Cells["location"].GetValue<string>();
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND propgathering.product = " + SqlHelper.SqlValue(product);
            if (!string.IsNullOrEmpty(location)) dbconstraint += " AND location = " + SqlHelper.SqlValue(location);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Pile_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_pile_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "SELECT DISTINCT pile FROM pile";
            string location = cell.Row.Cells["location"].GetValue<string>();
            if (!string.IsNullOrEmpty(location)) dbconstraint += " WHERE location = " + Expr.Value(location);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Pipeline_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_pipeline_1(BaseGrid grid, UltraGridCell cell)
        {
            
            DataRow row = grid.GetBindRow(cell.Row);
            if (row.Equals(null)) return new UiEventResult(EventStatus.Continue);
            
            string property = "";
            if (row.Table.Columns.Contains("ng_property")) property = row["ng_property"].ToString();
            if (string.IsNullOrEmpty(property) && row.Table.Columns.Contains("property")) property = row["property"].ToString();
            string product = cell.Row.Cells["product"].GetValue<string>();
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", product, "positionclass" });
            if (string.IsNullOrEmpty(property))
            {
                string sql = "SELECT DISTINCT pipeline, description FROM pipeline WHERE status = 'ACTIVE'";
                //if (positionclass == "COMMODITY") 325927
                //{
                    //    if (!string.IsNullOrEmpty(product))
                        //        sql += " AND product = " + SqlHelper.SqlValue(product);
                //}
                return new UiEventResult(EventStatus.Continue, sql);
            }
            else
            {
                string point = "";
                if (row.Table.Columns.Contains("ng_point")) point = row["ng_point"].ToString();
                if (row.Table.Columns.Contains("point")) point = row["point"].ToString();
                string dbconstraint = "SELECT DISTINCT propgathering.pipeline, pipeline.description FROM pipeline,propgathering WHERE pipeline.status = 'ACTIVE' AND propgathering.status = 'ACTIVE' AND propgathering.pipeline = pipeline.pipeline AND propgathering.property = " + SqlHelper.SqlValue(property);
                if (!string.IsNullOrEmpty(product)) dbconstraint += " AND propgathering.product = " + SqlHelper.SqlValue(product);
                if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
                if (!string.IsNullOrEmpty(point)) dbconstraint += " AND recpoint = " + SqlHelper.SqlValue(point);
                return new UiEventResult(EventStatus.Continue, dbconstraint);
            }
            
        }
        /* Point_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_point_1(BaseGrid grid, UltraGridCell cell)
        {
            
            DataRow row = grid.GetBindRow(cell.Row);
            if (row.Equals(null)) return new UiEventResult(EventStatus.Continue);
            
            string property = "";
            string dbconstraint = "";
            string product = cell.Row.Cells["product"].GetValue<string>();
            string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            if (commodityclass == "GAS")
            {
                if (row.Table.Columns.Contains("property")) property = cell.Row.Cells["property"].GetValue<string>();
                if (string.IsNullOrEmpty(property))
                {
                    string sql = "SELECT DISTINCT point, description FROM ngpoint WHERE status = 'ACTIVE'";
                    if (!string.IsNullOrEmpty(pipeline)) sql += " AND pipeline = " + Expr.Value(pipeline);
                    return new UiEventResult(EventStatus.Continue, sql);
                }
                string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
                dbconstraint = "SELECT DISTINCT recpoint, ngpoint.description FROM propgathering,ngpoint WHERE propgathering.recpoint = ngpoint.point AND propgathering.status = 'ACTIVE' AND ngpoint.status = 'ACTIVE' AND property = " + Expr.Value(property);
                if (!string.IsNullOrEmpty(product)) dbconstraint += " AND product = " + SqlHelper.SqlValue(product); if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
                if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND propgathering.pipeline = " + SqlHelper.SqlValue(pipeline);
            }
            
            if (!string.IsNullOrEmpty(dbconstraint)) return new UiEventResult(EventStatus.Continue, dbconstraint);
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Program_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_program_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "tradetype", "tradetype", _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(), "commodityclass" });
            if (string.IsNullOrEmpty(commodityclass)) commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            string dbconstraint = "select distinct program from emissionprogram where programtype = " + Expr.Value(commodityclass);
            return new UiEventResult(EventStatus.Continue, dbconstraint);
        }
        /* Property_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_property_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "";
            string product = cell.Row.Cells["product"].GetValue<string>();
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            if (commodityclass == "LIQUID")
            {
                string location = cell.Row.Cells["location"].GetValue<string>();
                dbconstraint = "SELECT DISTINCT propgathering.property, property.description FROM propgathering,product,property WHERE propgathering.status = 'ACTIVE' AND product.status = 'ACTIVE' AND propgathering.product = product.product AND propgathering.property = property.property";
                dbconstraint += " AND product.commodityclass = 'LIQUID'";
                if (!string.IsNullOrEmpty(product)) dbconstraint += " AND propgathering.product = " + SqlHelper.SqlValue(product);
                if (!string.IsNullOrEmpty(location)) dbconstraint += " AND location = " + SqlHelper.SqlValue(location);
            }
            if (commodityclass == "GAS")
            {
                string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
                string point = cell.Row.Cells["point"].GetValue<string>();
                string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
                dbconstraint = "SELECT DISTINCT propgathering.property, property.description FROM propgathering,product,property WHERE propgathering.status = 'ACTIVE' AND product.status = 'ACTIVE' AND propgathering.product = product.product AND propgathering.property = property.property";
                dbconstraint += " AND product.commodityclass = 'GAS'";
                if (!string.IsNullOrEmpty(product)) dbconstraint += " AND propgathering.product = " + SqlHelper.SqlValue(product);
                if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
                if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND pipeline = " + SqlHelper.SqlValue(pipeline);
                if (!string.IsNullOrEmpty(point)) dbconstraint += " AND recpoint = " + SqlHelper.SqlValue(point);
            }
            
            if (!string.IsNullOrEmpty(dbconstraint)) return new UiEventResult(EventStatus.Continue, dbconstraint);
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Recpoint_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_recpoint_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string property = cell.Row.Cells["property"].GetValue<string>();
            if (string.IsNullOrEmpty(property)) return new UiEventResult(EventStatus.Continue);
            
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
            string product = cell.Row.Cells["product"].GetValue<string>();
            string pipeline = cell.Row.Cells["pipeline"].GetValue<string>();
            string trade = cell.Row.Cells["trade"].GetValue<string>();
            string dbconstraint = "Select distinct recpoint From propgathering WHERE property = " + SqlHelper.SqlValue(property);
            if (!string.IsNullOrEmpty(product)) dbconstraint += " AND product = " + SqlHelper.SqlValue(product);
            if (positionclass == "PROCESS") dbconstraint += " AND gasplant is not null";
            if (!string.IsNullOrEmpty(pipeline)) dbconstraint += " AND pipeline = " + SqlHelper.SqlValue(pipeline);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Registry_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_registry_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            DataRow row = grid.GetBindRow(cell.Row);
            string product = string.Empty;
            if (_view.ViewGrids["tradedetail"].ActiveRow != null)
            {
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
            }
            else
            {
                if (row.Table.Columns.Contains("product"))
                    product = !row.IsNull("product") ? row["product"].ToString() : "";
            }
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", product, "commodityclass" });
            string dbconstraint = "select distinct registry from emissionregistry where registrytype = " + Expr.Value(commodityclass);
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Source_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_source_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string dbconstraint = "select distinct location from location where locationtype = 'Generation' and operator =  " + Expr.Value(_view.ViewGrids["trade"].ActiveRow.Cells["company"].GetValue<string>());
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Trade_Companyeic_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_trade_companyeic_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (_view.ViewGrids["tradedetail"].ActiveRow == null) return new UiEventResult(EventStatus.Continue);
            
            string company = _view.ViewGrids["tradedetail"].ActiveRow.Cells["company"].GetValue<string>();
            string controlarea = _view.ViewGrids["tradedetail"].ActiveRow.Cells["controlarea"].GetValue<string>();
            DateTime begtime = _view.ViewGrids["tradedetail"].ActiveRow.Cells["begtime"].GetValue<DateTime>();
            DateTime endtime = _view.ViewGrids["tradedetail"].ActiveRow.Cells["endtime"].GetValue<DateTime>();
            
            if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(controlarea))
                return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "";
            dbconstraint = "SELECT eiccode,tsstage,begtime,endtime FROM powerbalancegroupeic WHERE counterparty= " + SqlHelper.SqlValue(company) +
            " and controlarea = " + SqlHelper.SqlValue(controlarea) +
            " and begtime <" + SqlHelper.SqlDate(endtime) + " and endtime >" + SqlHelper.SqlDate(begtime);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Trade_Counterpartyeic_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_trade_counterpartyeic_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (_view.ViewGrids["tradedetail"].ActiveRow == null) return new UiEventResult(EventStatus.Continue);
            
            string counterparty = _view.ViewGrids["tradedetail"].ActiveRow.Cells["counterparty"].GetValue<string>();
            string controlarea = _view.ViewGrids["tradedetail"].ActiveRow.Cells["controlarea"].GetValue<string>();
            DateTime begtime = _view.ViewGrids["tradedetail"].ActiveRow.Cells["begtime"].GetValue<DateTime>();
            DateTime endtime = _view.ViewGrids["tradedetail"].ActiveRow.Cells["endtime"].GetValue<DateTime>();
            
            if (string.IsNullOrEmpty(counterparty) || string.IsNullOrEmpty(controlarea))
                return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "";
            dbconstraint = "SELECT eiccode,tsstage,begtime,endtime FROM powerbalancegroupeic WHERE counterparty= " + SqlHelper.SqlValue(counterparty) +
            " and controlarea = " + SqlHelper.SqlValue(controlarea) +
            " and begtime <" + SqlHelper.SqlDate(endtime) + " and endtime >" + SqlHelper.SqlDate(begtime);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Trade_Timezone_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_trade_timezone_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "";
            string block = cell.Row.Cells["block"].GetValue<string>();
            if (!string.IsNullOrEmpty(block))
            {
                dbconstraint = "SELECT blocktimezone.timezone timezone FROM blocktimezone, timezone " +
                " WHERE blocktimezone.timezone = timezone.timezone and blocktimezone.status = 'ACTIVE' AND blocktimezone.block = " + Expr.Value(block);
            }
            else return new UiEventResult(EventStatus.Continue);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* TradeDetail_Companyeic_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_tradedetail_companyeic_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string company = cell.Row.Cells["company"].GetValue<string>();
            string controlarea = cell.Row.Cells["controlarea"].GetValue<string>();
            DateTime begtime = cell.Row.Cells["begtime"].GetValue<DateTime>();
            DateTime endtime = cell.Row.Cells["endtime"].GetValue<DateTime>();
            
            if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(controlarea))
                return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "";
            dbconstraint = "SELECT eiccode,tsstage,begtime,endtime FROM powerbalancegroupeic WHERE counterparty= " + SqlHelper.SqlValue(company) +
            " and controlarea = " + SqlHelper.SqlValue(controlarea) +
            " and begtime <" + SqlHelper.SqlDate(endtime) + " and endtime >" + SqlHelper.SqlDate(begtime);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* TradeDetail_Counterpartyeic_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_tradedetail_counterpartyeic_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string counterparty = cell.Row.Cells["counterparty"].GetValue<string>();
            string controlarea = cell.Row.Cells["controlarea"].GetValue<string>();
            DateTime begtime = cell.Row.Cells["begtime"].GetValue<DateTime>();
            DateTime endtime = cell.Row.Cells["endtime"].GetValue<DateTime>();
            
            if (string.IsNullOrEmpty(counterparty) || string.IsNullOrEmpty(controlarea))
                return new UiEventResult(EventStatus.Continue);
            
            string dbconstraint = "";
            dbconstraint = "SELECT eiccode,tsstage,begtime,endtime FROM powerbalancegroupeic WHERE counterparty= " + SqlHelper.SqlValue(counterparty) +
            " and controlarea = " + SqlHelper.SqlValue(controlarea) +
            " and begtime <" + SqlHelper.SqlDate(endtime) + " and endtime >" + SqlHelper.SqlDate(begtime);
            
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Unit_BeforeDropDown_1
        Unit constraint */
        public UiEventResult BeforeDropDown_unit_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string product = "";
            if (grid.Name.ToLower() == "trade" || grid.Name.ToLower() == "tradedetail")
            {
                if (string.IsNullOrEmpty(cell.Row.Cells["product"].GetValue<string>())) return new UiEventResult(EventStatus.Continue);
                product = cell.Row.Cells["product"].GetValue<string>();
            }
            else
            {
                if (string.IsNullOrEmpty(_view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>())) return new UiEventResult(EventStatus.Continue);
                product = _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>();
            }
            
            string dbconstraint = "select unit, description from unit where unit in (select unit from unitproduct where product=" + Expr.Value(product)+")";
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Vintageyear_BeforeDropDown_1
        Constraint */
        public UiEventResult BeforeDropDown_vintageyear_1(BaseGrid grid, UltraGridCell cell)
        {
            
            if (grid.GetBindRow(cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "tradetype", "tradetype", _view.ViewGrids["trade"].ActiveRow.Cells["tradetype"].GetValue<string>(), "commodityclass" });
            if (string.IsNullOrEmpty(commodityclass)) commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            string dbconstraint = "select distinct vintageyear from emissionvintageyear where status = 'ACTIVE' and vintageyeartype = " + Expr.Value(commodityclass);
            return new UiEventResult(EventStatus.Continue, dbconstraint);
            
        }
        /* Trade_BeforeRowsDeleted_1
        Validate trade control */
        public UiEventResult BeforeRowsDeleted_trade_1(object sender, BeforeRowsDeletedEventArgs e)
        {
            
            List<DataRow> tradeDetailRowsToDelete = new List<DataRow>();
            for (int i = 0; i < e.Rows.Length; i++) tradeDetailRowsToDelete.Add(((ViewGrid)sender).GetBindRow(e.Rows[i]));
            foreach (DataRow tradeDetailRow in tradeDetailRowsToDelete)
            {
                if (tradeDetailRow.RowState == DataRowState.Added) continue;
                bool valid = Soap.Invoke<bool>(_view.Class.Url, "GetTradeControl", new string[] { "company" }, new object[] { tradeDetailRow.GetColumnValue<string>("company") });
                if (!valid)
                {
                    ErrorDialog.Show("Error", "This trade cannot be deleted since it is set not to be deleted in Trade Parameter window and Trade Controls Pane.");
                    e.Cancel = true;
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade_BeforeRowsDeleted_2
        Validate position delete */
        public UiEventResult BeforeRowsDeleted_trade_2(object sender, BeforeRowsDeletedEventArgs e)
        {
            
            List<DataRow> tradeRowsToDelete = new List<DataRow>();
            for (int i = 0; i < e.Rows.Length; i++) tradeRowsToDelete.Add(((ViewGrid)sender).GetBindRow(e.Rows[i]));
            foreach (DataRow tradeRow in tradeRowsToDelete)
            {
                if (tradeRow.RowState == DataRowState.Added) continue;
                DataRow[] drTradeDetailRowsToDelete = _view.DataSource.Tables["tradedetail"].Select("trade = " + Expr.Value(tradeRow.GetColumnValue<string>("trade")));
                foreach (DataRow tradeDetailRow in drTradeDetailRowsToDelete)
                {
                    if (tradeDetailRow.RowState == DataRowState.Added) continue;
                    string commodity = Soap.Invoke<string>(_view.Class.Url, "GetCommodity", new string[] { "positionmode", "product" }, new object[] { tradeDetailRow.GetColumnValue<string>("positionmode"), tradeDetailRow.GetColumnValue<string>("product") });
                    string message = Soap.Invoke<string>(_view.Class.Url, "GetQuantityStatusWithPosdetail", new string[] { "position", "posdetail", "commodity", "begtime", "endtime", "positionBegtime", "positionEndtime", "colname", "evergreentermdate" }, new object[] { tradeDetailRow.GetColumnValue<string>("position"), tradeDetailRow.GetColumnValue<string>("posdetail"), commodity, tradeRow.GetColumnValue<DateTime>("begtime"), tradeRow.GetColumnValue<DateTime>("endtime"), tradeDetailRow.GetColumnValue<DateTime>("begtime"), tradeDetailRow.GetColumnValue<DateTime>("endtime"), "company", DateTime.MinValue });
                    if (!string.IsNullOrEmpty(message))
                    {
                        ErrorDialog.Show("Error", "This row cannot be deleted because it has been scheduled, nominated or actualized.");
                        e.Cancel = true;
                        return new UiEventResult(EventStatus.Cancel);
                    }
                    
                    string[] tablesToCheck = new string[] { "findetail", "netdetail", "valuationdetail", "strategyposition", "hedgeposition" };
                    foreach (string table in tablesToCheck)
                    {
                        bool msg = Soap.Invoke<bool>(_view.Class.Url, "DeleteValidatePosition", new string[] { "position", "posdetail", "tablename" }, new object[] { tradeDetailRow.GetColumnValue<string>("position"), tradeDetailRow.GetColumnValue<string>("posdetail"), table });
                        if (msg)
                        {
                            ErrorDialog.Show("Error", "This position cannot be deleted since it has rows in " + table + ".");
                            e.Cancel = true;
                            return new UiEventResult(EventStatus.Cancel);
                        }
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeDetail_BeforeRowsDeleted_1
        Validate tradedetail delete */
        public UiEventResult BeforeRowsDeleted_tradedetail_1(object sender, BeforeRowsDeletedEventArgs e)
        {
            
            string[] tableList = new string[] { "findetail", "valuationdetail", "netdetail", "hedgeposition", "strategyposition" };
            List<DataRow> tradeDetailRowsToDelete = new List<DataRow>();
            for (int i = 0; i < e.Rows.Length; i++) tradeDetailRowsToDelete.Add(((ViewGrid)sender).GetBindRow(e.Rows[i]));
            foreach (DataRow tradeDetailRow in tradeDetailRowsToDelete)
            {
                if (tradeDetailRow.RowState == DataRowState.Added) continue;
                foreach (string tablename in tableList)
                {
                    if (DbModel.GetDbTable(tablename) == null) continue;
                    string position = tradeDetailRow.GetColumnValue<string>("position");
                    string posdetail = tradeDetailRow.GetColumnValue<string>("posdetail");
                    //DataRow[] positionRows = _view.DataSource.Tables["tradedetail"].Select("position = " + Expr.Value(position),"",DataViewRowState.CurrentRows);
                    DataRow[] positionRows = _view.DataSource.Tables["tradedetail"].Select("position = " + Expr.Value(position) + " and posdetail = " + Expr.Value(posdetail), "", DataViewRowState.CurrentRows);
                    if (positionRows.Length > 1) return new UiEventResult(EventStatus.Continue);
                    bool msg = Soap.Invoke<bool>(_view.Class.Url, "DeleteValidatePosition", new string[] { "position", "posdetail", "tablename" }, new object[] { tradeDetailRow.GetColumnValue<string>("position"), tradeDetailRow.GetColumnValue<string>("posdetail"), tablename });
                    if (msg)
                    {
                        ErrorDialog.Show("Error", "This position cannot be deleted since it has rows in " + tablename + ".");
                        e.Cancel = true;
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeExercise_BeforeRowsDeleted_1
        Validate exercise position delete */
        public UiEventResult BeforeRowsDeleted_tradeexercise_1(object sender, BeforeRowsDeletedEventArgs e)
        {
            
            string[] tableList = new string[] { "findetail", "valuationdetail", "netdetail", "hedgeposition", "strategyposition" };
            List<DataRow> tradeExerciseRowsToDelete = new List<DataRow>();
            for (int i = 0; i < e.Rows.Length; i++) tradeExerciseRowsToDelete.Add(((ViewGrid)sender).GetBindRow(e.Rows[i]));
            foreach (DataRow tradeExerciseRow in tradeExerciseRowsToDelete)
            {
                if (tradeExerciseRow.RowState == DataRowState.Added) continue;
                foreach (string tablename in tableList)
                {
                    if (DbModel.GetDbTable(tablename) == null) continue;
                    bool msg = Soap.Invoke<bool>(_view.Class.Url, "DeleteValidatePosition", new string[] { "position", "posdetail", "tablename" }, new object[] { tradeExerciseRow.GetColumnValue<string>("exerciseposition"), "", tablename });
                    if (msg)
                    {
                        ErrorDialog.Show("Error", "This position cannot be deleted since it has rows in " + tablename + ".");
                        e.Cancel = true;
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* BeforeUpdateData_1
        Force new position trigger */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            ArrayList positionColumnAL = new ArrayList();
            foreach (DataColumn positionColumn in _view.DataSource.Tables["tradedetail"].Columns)
            {
                string columnName = positionColumn.ColumnName;
                if (DsHelper.SystemColumns.Contains(columnName) || DsHelper.CollaborationColumns.Contains(columnName)
                    || columnName == "RowNumber" || positionColumn.IsProxy() || columnName == "position" || columnName == "collaboration") continue;
                
                if (DbModel.GetDbColumn("position", columnName) != null && !positionColumnAL.Contains(columnName)) positionColumnAL.Add(columnName);
            }
            ArrayList productAL = new ArrayList();
            DataRow[] modifiedTradedetailRows = _view.DataSource.Tables["tradedetail"].Select("", "", DataViewRowState.ModifiedCurrent);
            foreach (DataRow tradedetailRow in modifiedTradedetailRows)
            {
                DateTime begtime = tradedetailRow.GetColumnValue<DateTime>("begtime");
                DateTime endtime = tradedetailRow.GetColumnValue<DateTime>("endtime");
                string commodityclass = string.Empty;
                if (!productAL.Contains(tradedetailRow.GetColumnValue<string>("product")))
                {
                    commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
                    new object[] { "product", "product", tradedetailRow.GetColumnValue<string>("product"), "commodityclass" });
                    productAL.Add(tradedetailRow.GetColumnValue<string>("product"));
                }
                if (commodityclass == "EMISSION" || commodityclass == "RENEWABLE") continue;
                string message = this.GetQuantityStatus(tradedetailRow.GetColumnValue<string>("position"), commodityclass, begtime, endtime, begtime, endtime, "", "");
                if (message.Length > 0) continue;
                bool positionModified = false;
                
                ArrayList checkColumnAL = new ArrayList();
                checkColumnAL.Add("tradebook");
                checkColumnAL.Add("contract");
                
                foreach (DataColumn column in tradedetailRow.Table.Columns)
                {
                    if (tradedetailRow.IsColumnModified(column) && positionColumnAL.Contains(column.ColumnName))
                    {
                        if (column.ColumnName == "expirationdate" && tradedetailRow[column] == DBNull.Value) continue;
                        else if (checkColumnAL.Contains(column.ColumnName) && tradedetailRow[column] != DBNull.Value)
                        {
                            DataRow[] Rows = _view.DataSource.Tables["tradedetail"].Select("position = " + Expr.Value(tradedetailRow.GetColumnValue<string>("position")));
                            DataRow[] modifiedRows = _view.DataSource.Tables["tradedetail"].Select("position = " + Expr.Value(tradedetailRow.GetColumnValue<string>("position")) + " AND " + column.ColumnName + "=" + Expr.Value(tradedetailRow.GetColumnValue<string>(column.ColumnName)), "", DataViewRowState.ModifiedCurrent);
                            if (Rows.Length == modifiedRows.Length) continue;
                        }
                        
                        positionModified = true;
                        break;
                    }
                }
                if (!positionModified) continue;
                DataRow[] tradedetailRows = _view.DataSource.Tables["tradedetail"].Select("position = " + Expr.Value(tradedetailRow.GetColumnValue<string>("position")));
                if (tradedetailRows.Length == 1) continue;
                
                bool b_newRow = false;
                string modifiedColumname = "";
                foreach (DataRow dr in tradedetailRows)
                {
                    if (tradedetailRow["posdetail"].Equals(dr["posdetail"])) continue;
                    foreach (string columnname in positionColumnAL)
                    {
                        if (tradedetailRow[columnname].Equals(dr[columnname])) continue;
                        modifiedColumname = columnname;
                        b_newRow = true;
                        break;
                    }
                    
                    if (b_newRow) break;
                }
                
                if (b_newRow && _view.DataSource.Tables["tradedetail"].Select("position = " + Expr.Value(tradedetailRow.GetColumnValue<string>("position")) + " AND posdetail <> " + Expr.Value(tradedetailRow.GetColumnValue<string>("posdetail")), "", DataViewRowState.ModifiedCurrent).Length > 0)
                {
                    UltraGridRow gridRow = _view.ViewGrids["tradedetail"].FindGridRow(tradedetailRow);
                    if (gridRow == null) continue;
                    
                    string className = this._view.ClassName;
                    bool allowDelete = this._view.GetClassMethodPermission("Delete" + className);
                    if (allowDelete == false)
                    {
                        MessageBox.Show("The user does not have deletion permission.", "Update Failed!");
                        return new UiEventResult(EventStatus.Cancel);
                    }
                    
                    gridRow.Activate();
                    string position = tradedetailRow["position"].ToString();
                    bool deleteError = Soap.Invoke<bool>(_view.Class.Url, "DeleteValidatePosition", new string[] { "position", "posdetail", "tablename" }, new object[] { position, "", "valuationdetail" });
                    if (deleteError)
                    {
                        MessageBox.Show("The valuation detail has been created for the position: " + position + ". \n All positions: " + position + " need to have same '" + modifiedColumname + "' value.", "Update Failed!");
                        return new UiEventResult(EventStatus.Cancel);
                    }
                    _view.ViewGrids["tradedetail"].DuplicateRow();
                    gridRow.Delete();
                }
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Validate emission certificate type
        Validate emission certificate type for internal trade */
        public UiEventResult BeforeUpdateData_2(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (_view.DataSource.Tables["tradedetail"].Columns.Contains("certificatetype") == false) return new UiEventResult(EventStatus.Continue);
            DataRow[] newTradedetailRows = _view.DataSource.Tables["tradedetail"].Select("certificatetype <> 'PURCHASED' and certificatetype <> 'SOLD'", "", DataViewRowState.Added | DataViewRowState.ModifiedCurrent);
            foreach (DataRow tradedetailRow in newTradedetailRows)
            {
                DataRow tradeRow = tradedetailRow.GetParentRow("trade_tradedetail");
                if (tradeRow != null && Convert.ToBoolean(tradeRow["internal"]))
                {
                    ErrorDialog.Show("Validation", "Only PURCHASED/SOLD certificate type allowed for emission internal trade. Trade:" + tradeRow["trade"].ToString());
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Send Email - Internal Trade - BeforeUpdateData
        Send Email - Internal Trade - Send an email when an internal trade is created */
        public UiEventResult BeforeUpdateData_4(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<DataRow> addedRowsList = new List<DataRow>();
            
            addedRowsList.AddRange(_view.DataSource.Tables["trade"].Select("", "", DataViewRowState.Added).Where(x => x.Field<bool>("internal") == true));
            
            if(addedRowsList.Count == 0)
                return new UiEventResult(EventStatus.Continue);
            
            if (_view.DataSource.Tables["trade"].Columns.IndexOf("internaltradecreated") == -1)
                _view.DataSource.Tables["trade"].Columns.Add("internaltradecreated");
            
            foreach (DataRow row in addedRowsList)
                row["internaltradecreated"] = true;
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Update Globaleris Status
        Update Globaleris Status -DEV- Update cst_globalerisstatus field to PENDING after a change */
        public UiEventResult BeforeUpdateData_10(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<string> paneNameList = new List<string>();
            List<DataRow> modifiedRowsList = new List<DataRow>();
            
            DataSet ds = _view.DataSource;
            List<DataColumn> columnsModifiedList = new List<DataColumn>();
            List<string> globalerisColumns = new List<string>();
            globalerisColumns.Add("counterparty");
            globalerisColumns.Add("positiontype");
            globalerisColumns.Add("begtime");
            globalerisColumns.Add("endtime");
            globalerisColumns.Add("location");
            globalerisColumns.Add("pipeline");
            globalerisColumns.Add("point");
            globalerisColumns.Add("quantity");
            globalerisColumns.Add("unit");
            globalerisColumns.Add("cst_deliverybegtime");
            globalerisColumns.Add("cst_deliveryendtime");
            globalerisColumns.Add("energyunit");
            globalerisColumns.Add("energy");
            
            //Panes of Trade Execution View
            paneNameList.Add("tradedetail");
            
            
            //Get modified Rows
            foreach (string table in paneNameList)
            {
                modifiedRowsList.AddRange(_view.DataSource.Tables[table].Select("", "", DataViewRowState.ModifiedCurrent));
                
                // Add the Trade row of the any modified row.
                foreach (DataRow row in modifiedRowsList)
                {
                    // Modified columns
                    columnsModifiedList = new List<DataColumn>(row.GetChangedColumns());
                    
                    switch (table)
                    {
                        case "tradedetail":
                        foreach (DataColumn column in columnsModifiedList)
                        {
                            if (globalerisColumns.Contains(column.ColumnName))
                            {
                                DataSet positionDS = new DataSet();
                                SqlHelper.RetrieveData(positionDS, new[] { "position" }, new[] { "SELECT * FROM position WHERE position IN (" + row["position"].ToString() + ")" });
                                
                                foreach (DataRow posRow in positionDS.Tables["position"].Rows)
                                {
                                    posRow["cst_globalerisstatus"] = "PENDING";
                                }
                                
                                
                                SqlHelper.UpdateData(positionDS);
                                
                            }
                        }
                        break;
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* UpdateTradeCNType
        UpdateCNType -DEV- Update CN Type for new/updated Trades */
        public UiEventResult BeforeUpdateData_12(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<DataRow> modifiedAddedRowsList = new List<DataRow>();
            //Add cntype column
            if (!_view.DataSource.Tables["trade"].Columns.Contains("cst_cntype"))
            {
                _view.DataSource.Tables["trade"].Columns.Add("cst_cntype");
            }
            
            
            
            // Get modified Rows
            modifiedAddedRowsList.AddRange(_view.DataSource.Tables["trade"].Select("", "", DataViewRowState.Added | DataViewRowState.ModifiedCurrent));
            
            //Add the Trade row of the any modified row.
            foreach (DataRow row in modifiedAddedRowsList)
            {
                DataRow[] counterparty = _view.DataSource.Tables["tradedetail"].Select("trade=" + row["trade"] , "counterparty");
                
                string counterparties = "'" + String.Join("','", counterparty.AsEnumerable().Select(x => x.Field<string>("counterparty")).ToArray()) + "'";
                
                DataSet counterpartyDS = new DataSet();
                //   SqlHelper.RetrieveData(counterpartyDS, new[] { "counterparty" }, new[] { "SELECT distinct cst_cntype FROM counterparty WHERE counterparty IN (select counterparty from position where trade='" + row["trade"].ToString() + "')" });
                SqlHelper.RetrieveData(counterpartyDS, new[] { "counterparty" }, new[] { "SELECT distinct cst_cntype FROM counterparty WHERE counterparty IN (" + counterparties + ")" });
                
                if (counterpartyDS.Tables[0].Rows.Count == 1)
                {
                    string cntype = counterpartyDS.Tables[0].Rows[0]["cst_cntype"].ToString();
                    if (!string.IsNullOrEmpty(cntype))
                    {
                        row["cst_cntype"] = cntype;
                    }
                    else if (row["tradetype"].ToString() == "PHYSICAL SWAP")
                    {
                        row["cst_cntype"] = "EMISSION PARTY";
                    }
                    else
                    {
                        
                        
                        if (row["positiontype"].ToString() == "BUY")
                        {
                            row["cst_cntype"] = "RECEPTION PARTY";
                            
                        }
                        else if (row["positiontype"].ToString() == "SELL")
                        {
                            row["cst_cntype"] = "EMISSION PARTY";
                            
                        }
                        else
                        {
                            row.SetField("cst_cntype",DBNull.Value);
                            
                        }
                    }
                }
                else
                {
                    row.SetField("cst_cntype", DBNull.Value);
                }
                
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* UpdatePositionConfirmationUser
        UpdateConfirmationUser -DEV- Update Confirmation user for each trade type */
        public UiEventResult BeforeUpdateData_13(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<DataRow> modifiedAddedRowsList = new List<DataRow>();
            
            // Get modified Rows
            modifiedAddedRowsList.AddRange(_view.DataSource.Tables["tradedetail"].Select("", "", DataViewRowState.Added ));
            
            foreach (DataRow drTradeDetail in modifiedAddedRowsList)
            {
                DataRow trade = _view.DataSource.Tables["trade"].Select("trade=" + drTradeDetail["trade"], "tradetype").FirstOrDefault();
                
                if (trade != null)
                {
                    string tradetype = trade.Field<string>("tradetype");
                    
                    DataSet confirmationusercnDS = new DataSet();
                    SqlHelper.RetrieveData(confirmationusercnDS, new[] { "cst_confirmationusercn" }, new[] { "SELECT name FROM cst_confirmationusercn WHERE tradetype = '" + tradetype + "'" });
                    
                    if (confirmationusercnDS.Tables[0].Rows.Count == 1)
                    {
                        drTradeDetail["cst_confirmationuser"] = confirmationusercnDS.Tables[0].Rows[0].Field<string>("name");
                    }
                    else if (confirmationusercnDS.Tables[0].Rows.Count >= 1)
                    {
                        MessageBox.Show("More than one user is assigned for Trade Type '" + tradetype + "'. The value for Confirmation User field will be blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        
                        drTradeDetail["cst_confirmationuser"] = DBNull.Value;
                    }
                    else
                    {
                        drTradeDetail["cst_confirmationuser"] = DBNull.Value;
                        
                    }
                }
                
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Trade Update Heatvalue Hvunit
        TradeExecution Before Update Data - DEV - Update Heatvalue and hvunit fields */
        public UiEventResult BeforeUpdateData_14(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataRow[] newModifiedTradedetailRows = _view.DataSource.Tables["tradedetail"].Select("", "", DataViewRowState.Added | DataViewRowState.ModifiedCurrent);
            
            foreach (DataRow tradedetailRow in newModifiedTradedetailRows)
            {
                DataSet ds = new DataSet();
                
                SqlHelper.RetrieveData(ds, new[] { "product" }, new[] { "SELECT * FROM product WHERE product = '" + tradedetailRow.Field<string>("product") + "' and calcenergy=1" });
                
                
                if (ds.Tables["product"].Rows.Count > 0)
                {
                    DataRow productRow = ds.Tables["product"].Rows[0];
                    if (productRow["heatvalue"].ToString() != "0")
                    {
                        tradedetailRow["heatvalue"] = productRow["heatvalue"].ToString();
                        tradedetailRow["hvunit"] = productRow["hvunit"].ToString();
                    }
                    
                }
                
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Combine Constraint With Trader Constraint
        Class Method */
        public string CombineWithTraderConstraint(string column,string constraint)
        {
            
            string newConstraint = Soap.Invoke<string>(_view.Class.Url, "GetConstraintWithTraderConstraint", new string[] { "trader", "column", "query" }, new object[] { AppManager.UserName, column, constraint });
            
            if (string.IsNullOrEmpty(newConstraint)) newConstraint = constraint + " AND 1=0";
            
            return newConstraint;
            
        }
        /* Get Contract Constraint
        Class Method */
        public string GetContractConstraint(DateTime begtime, DateTime endtime, string tradetype, string positiontype, string positionmode, string product, string company, string counterparty)
        {
            /******************************************************************************
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.1        18/09/2020  SAI              Contract Constraint
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            
            string dbconstraint = "";
            DateTime minValue = new DateTime(1900, 1, 1, 0, 0, 0);
            DateTime maxValue = new DateTime(2099, 1, 1, 0, 0, 0);
            if (begtime == null || (begtime != null && begtime == DateTime.MinValue)) begtime = minValue;
            if (endtime == null || (endtime != null && endtime == DateTime.MinValue)) endtime = maxValue;
            if (!string.IsNullOrEmpty(company) && !string.IsNullOrEmpty(counterparty))
            {
                dbconstraint = "Select contract, description From contract Where";
                if (positiontype == "BUY")
                {
                    dbconstraint += " ((contract.extendevergreentermdate = 0";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(company);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + ")";
                    dbconstraint += " and (partytype <> 'SELLER'))";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(counterparty);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + ")";
                    dbconstraint += " and (partytype <> 'BUYER'))";
                    dbconstraint += " ) or (contract.extendevergreentermdate = 1";
                    dbconstraint += " and (contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(company);
                    dbconstraint += " and (partytype <> 'SELLER'))";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(counterparty);
                    dbconstraint += " and (partytype <> 'BUYER'))";
                    dbconstraint += ")))";
                }
                else if (positiontype == "SELL")
                {
                    dbconstraint += " ((contract.extendevergreentermdate = 0";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(company);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + ")";
                    dbconstraint += " and (partytype <> 'BUYER'))";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(counterparty);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + ")";
                    dbconstraint += " and (partytype <> 'SELLER'))";
                    dbconstraint += " ) or (contract.extendevergreentermdate = 1";
                    dbconstraint += " and (contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(company);
                    dbconstraint += " and (partytype <> 'BUYER'))";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(counterparty);
                    dbconstraint += " and (partytype <> 'SELLER'))";
                    dbconstraint += ")))";
                }
                else
                {
                    dbconstraint += " ((contract.extendevergreentermdate = 0";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(company);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + "))";
                    dbconstraint += " and contract in (select contract from contractparty where counterparty = " + SqlHelper.SqlValue(counterparty);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + "))";
                    dbconstraint += " ) or (contract.extendevergreentermdate = 1";
                    dbconstraint += " and (contract in ( select contract from contractparty where counterparty = " + SqlHelper.SqlValue(company);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + "))";
                    dbconstraint += " and contract in ( select contract from contractparty where counterparty = " + SqlHelper.SqlValue(counterparty);
                    dbconstraint += " and (contractparty.endtime is null or contractparty.endtime > " + SqlHelper.SqlDate(begtime) + ")";
                    dbconstraint += " and (contractparty.begtime is null or contractparty.begtime < " + SqlHelper.SqlDate(endtime) + "))";
                    dbconstraint += ")))";
                }
            }
            
            if (dbconstraint.Length <= 0)
            {
                //SAI 18/09/2020 Added  and contract.contractstatus != 'TERMINATED'
                //DZL 21/05/2021 Added  and contract.contractstatus != 'PENDING'
                dbconstraint = "Select contract, description From contract Where contract.status = 'ACTIVE' and contract.contractstatus != 'TERMINATED' AND contract.contractstatus != 'PENDING'  ";
            }
            else
            {
                //SAI 18/09/2020 Added  and contract.contractstatus != 'TERMINATED'
                //DZL 21/05/2021 Added  and contract.contractstatus != 'PENDING'
                
                dbconstraint += " and contract.status = 'ACTIVE' and contract.contractstatus != 'TERMINATED' AND contract.contractstatus != 'PENDING'";
            }
            
            if (begtime != minValue) dbconstraint += " and effdate <= " + SqlHelper.SqlDate(begtime) + " ";
            if (endtime != maxValue) dbconstraint += " and (termdate is null or termdate >= " + SqlHelper.SqlDate(endtime) + ")";
            
            if (!string.IsNullOrEmpty(tradetype) && tradetype.Length > 0)
            {
                dbconstraint += " and (contract in (select contract from contractposition where tradetype = " + SqlHelper.SqlValue(tradetype) + ")";
                dbconstraint += " or contract not in (select contract from contractposition where contractposition.contract = contract.contract))";
            }
            if (!string.IsNullOrEmpty(positionmode) && positionmode.Length > 0)
            {
                dbconstraint += " and (contract in (select contract from contractposition where positionmode = " + SqlHelper.SqlValue(positionmode) + ")";
                dbconstraint += " or contract not in (select contract from contractposition where contractposition.contract = contract.contract))";
            }
            if (!string.IsNullOrEmpty(product) && product.Length > 0)
            {
                dbconstraint += " and (contract in (select contract from contractposition where producttype in (select producttype from product where product = " + SqlHelper.SqlValue(product) + "))";
                dbconstraint += " or contract not in (select contract from contractposition where contractposition.contract = contract.contract))";
            }
            
            return dbconstraint;
        }
        /* Get Quantity Status
        Class Method */
        public string GetQuantityStatus(string position, string commodity, DateTime orig_begtime, DateTime orig_endtime, DateTime begtime, DateTime endtime, string colname, string component)
        {
            
            string message = "", tablename = "";
            if (commodity == "GAS") commodity = "NG";
            
            if (commodity == "NG") tablename = "ngposition";
            else if (commodity == "PHYSICAL") tablename = "physicalposition";
            else if (commodity == "POWER") tablename = "powerposition";
            if (tablename.Length > 0)
            {
                string[] arg_name = new string[8] { "position", "commodity", "orig_begtime", "orig_endtime", "begtime", "endtime", "colname", "component" };
                object[] arg_value = new object[8] { position, commodity, orig_begtime, orig_endtime, begtime, endtime, colname, component };
                
                message = Soap.Invoke<string>(_view.Class.Url, "GetQuantityStatus", arg_name, arg_value);
                
            }
            return message;
            
        }
        /* GasDayTimeEntry_UpdateTime
        Main update method */
        public string UpdateNormalTime(string table, string column, DateTime value, CellEventArgs e)
        {
            //Update respective date field based on invoking column.
            
            //Declare variables
            string sInvoker = column;
            string sInvtable = table;
            
            string sPipelineT = string.Empty;
            string sProductT = string.Empty;
            bool bGasDayT = false;
            
            string sPipelineP = string.Empty;
            string sProductP = string.Empty;
            bool bGasDayP = false;
            
            int iHourT = 0;
            int iHourP = 0;
            
            //Capture invoking column name and value
            DateTime dtNewtime = value;
            
            //Updated column respective to invoking column
            string col = string.Empty;
            switch (sInvoker)
            {
                case "begtime": col = "begtimenormal"; break;
                case "endtime": col = "endtimenormal"; break;
                case "begtimenormal": col = "begtime"; break;
                case "endtimenormal": col = "endtime"; break;
                default: /*col = col; */return string.Empty;
            }
            
            #region Update
            #region Trade Level Check
            //If request comes from trade level, update trade level columns.
            if (sInvtable == "trade")
            {
                //Capture Product/Pipeline. Cancel if null.
                sProductT = e.Cell.Row.GetCellValue<string>("product");
                sPipelineT = e.Cell.Row.GetCellValue<string>("pipeline");
                
                //Update beg/end times.
                //If Trade level product/pipeline are available, perform update.
                if (sProductT != string.Empty && sProductT != null && sPipelineT != string.Empty && sPipelineT != null)
                {
                    #region Capture Gasday/Markethour
                    //Check if product is gas day enforced
                    try
                    {
                        bGasDayT = Soap.Invoke<bool>("Utility_WebWS.asmx", "GetProductGasDay", new string[] { "product" }, new object[] { sProductT });
                    }
                    catch (Exception)
                    {
                        return "GETPRODUCTGASDAYERROR";
                    }
                    
                    //If product is gas day enforced, capture market hour.
                    if (bGasDayT)
                    {
                        //Capture market day hour shift
                        try
                        {
                            //Convert both 00 and 00.00 market hour format.
                            string hour = Soap.Invoke<string>("Utility_WebWS.asmx", "GetPipelineMarketDayHour", new string[] { "pipeline" }, new object[] { sPipelineT });
                            iHourT = hour.Contains(".") ? Convert.ToInt16(hour.Substring(0, hour.IndexOf('.', 0))) : Convert.ToInt16(hour);
                        }
                        catch (Exception ex)
                        {
                            ErrorDialog.Show("GETPIPELINEMARKETDAYHOURERROR",ex.Message + " :: " + iHourT);
                            return "GETPIPELINEMARKETDAYHOURERROR";
                        }
                    }
                    else
                    {
                        return "NOTGASDAY";
                    }
                    #endregion
                    
                    //Perform Update on relevant field.
                    if (sInvoker.Contains("normal"))
                    {
                        e.Cell.Row.Cells[col].Value = dtNewtime.AddHours(iHourT * -1);
                    }
                    else
                    {
                        e.Cell.Row.Cells[col].Value = dtNewtime.AddHours(iHourT);
                    }
                }
                else
                {
                    //If any required values are null, column value shoud be null.
                    _view.ViewGrids["trade"].ActiveRow.Cells[col].Value = System.DBNull.Value;
                }
                
                #endregion
                
                //Update all relevant position rows.
                #region Position Level Check
                
                List<int> lMarkethours = new List<int>();
                
                //Determine which column should be updated.
                
                //Capture min/max beg/end times from position level.
                DateTime dtMinbegtime = DateTime.MaxValue;
                DateTime dtMaxendtime = DateTime.MinValue;
                foreach (UltraGridRow gRow in _view.ViewGrids["tradedetail"].Rows)
                {
                    if (gRow.Cells["begtimenormal"].GetValue<DateTime>() < dtMinbegtime && !gRow.Cells["begtimenormal"].Value.Equals(System.DBNull.Value))
                    {
                        dtMinbegtime = gRow.Cells["begtimenormal"].GetValue<DateTime>();
                    }
                    if (dtMaxendtime < gRow.Cells["endtimenormal"].GetValue<DateTime>() && !gRow.Cells["endtimenormal"].Value.Equals(System.DBNull.Value))
                    {
                        dtMaxendtime = gRow.Cells["endtimenormal"].GetValue<DateTime>();
                    }
                }
                
                //If request comes from trade level, update all required positions.
                foreach (UltraGridRow gRow in _view.ViewGrids["tradedetail"].Rows)
                {
                    //If pipeline and product exist, capture data and update as necessary.
                    sProductP = gRow.Cells["product"].GetValue<string>();
                    sPipelineP = gRow.Cells["pipeline"].GetValue<string>();
                    
                    //If position has same pipeline/product as trade level, update.
                    //If pipeline or product is empty, continue to next position.
                    if (string.IsNullOrEmpty(sProductP) || string.IsNullOrEmpty(sPipelineP))
                    {
                        gRow.Cells[col].Value = System.DBNull.Value;
                        continue;
                    }
                    else
                    {
                        try
                        {
                            bGasDayP = Soap.Invoke<bool>("Utility_WebWS.asmx", "GetProductGasDay", new string[] { "product" }, new object[] { sProductP });
                            if (bGasDayP)
                            {
                                try
                                {
                                    //Convert both 00 and 00.00 market hour format.
                                    string hour = Soap.Invoke<string>("Utility_WebWS.asmx", "GetPipelineMarketDayHour", new string[] { "pipeline" }, new object[] { sPipelineP });
                                    iHourP = hour.Contains(".") ? Convert.ToInt16(hour.Substring(0, hour.IndexOf('.', 0))) : Convert.ToInt16(hour);
                                }
                                catch (Exception)
                                {
                                    return "GETPRODUCTGASDAYERROR";
                                }
                            }
                            else
                            {
                                //If product is not gas day, set normal column to null.
                                if (col.Contains("normal"))
                                {
                                    e.Cell.Row.Cells[col].Value = System.DBNull.Value;
                                }
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            return "GETPRODUCTGASDAYERROR";
                        }
                        //beg/endtime = (beg/endtimenormal - market hours) and Vice Versa.
                        //If invoking column come from trade beg/end time, update both position columns. // PROBLEM
                        if (!col.Contains("normal") && (gRow.Cells[col + "normal"].GetValue<DateTime>() == (col.Contains("beg") ? dtMinbegtime : dtMaxendtime) || gRow.Cells[col].Value.Equals(DBNull.Value)))
                        {
                            gRow.Cells[col].Value = dtNewtime.AddHours(iHourP * -1);
                            gRow.Cells[col + "normal"].Value = dtNewtime;
                        }
                        else if (col.Contains("normal") && (gRow.Cells[col].GetValue<DateTime>() == (col.Contains("beg") ? dtMinbegtime : dtMaxendtime) || gRow.Cells[col].Value.Equals(DBNull.Value)))
                        {
                            gRow.Cells[col].Value = dtNewtime.AddHours(iHourP);
                        }
                        
                    }
                }
            }
            
            //If request comes from position level, only update that position. Also check trade level time range.
            if (sInvtable == "tradedetail")
            {
                //If request comes from trade level, update all possible positions.
                
                //If pipeline and product exist, capture data and update as necessary.
                sProductP = e.Cell.Row.Cells["product"].GetValue<string>();
                sPipelineP = e.Cell.Row.Cells["pipeline"].GetValue<string>();
                
                //If pipeline or product is empty, continue to next position.
                if (((sProductP == String.Empty || sProductP == null) || (sPipelineP == String.Empty || sPipelineP == null)) && col.Contains("normal"))
                {
                    e.Cell.Row.Cells[col].Value = System.DBNull.Value;
                    _view.ViewGrids["trade"].ActiveRow.Cells[col].Value = System.DBNull.Value;
                    return "NOUPDATE";
                }
                else
                {
                    try
                    {
                        bGasDayP = Soap.Invoke<bool>("Utility_WebWS.asmx", "GetProductGasDay", new string[] { "product" }, new object[] { sProductP });
                        if (bGasDayP)
                        {
                            try
                            {
                                string hour = Soap.Invoke<string>("Utility_WebWS.asmx", "GetPipelineMarketDayHour", new string[] { "pipeline" }, new object[] { sPipelineP });
                                iHourP = hour.Contains(".") ? Convert.ToInt16(hour.Substring(0, hour.IndexOf('.', 0))) : Convert.ToInt16(hour);
                            }
                            catch (Exception)
                            {
                                return "GETPRODUCTGASDAYERROR";
                            }
                        }
                        else
                        {
                            //If product is not gas day, set normal column to null.
                            if (col.Contains("normal"))
                            {
                                e.Cell.Row.Cells[col].Value = System.DBNull.Value;
                            }
                            return "NOTGASDAY";
                        }
                    }
                    catch (Exception)
                    {
                        return "GETPRODUCTGASDAYERROR";
                    }
                    //beg/endtime = (beg/endtimenormal - market hours) and Vice Versa. //PROBLEM
                    e.Cell.Row.Cells[col].Value = col.Contains("normal") ? dtNewtime.AddHours(iHourP) : dtNewtime.AddHours(iHourP * -1);
                }
                
                //Update trade level normal beg/end times to match position level min/max normal beg/end times.
                DateTime dtMinbegtime = DateTime.MaxValue;
                DateTime dtMaxendtime = DateTime.MinValue;
                foreach (UltraGridRow gRow in _view.ViewGrids["tradedetail"].Rows)
                {
                    if (gRow.Cells["begtimenormal"].GetValue<DateTime>() < dtMinbegtime && !gRow.Cells["begtimenormal"].Value.Equals(System.DBNull.Value))
                    {
                        dtMinbegtime = gRow.Cells["begtimenormal"].GetValue<DateTime>();
                    }
                    if (dtMaxendtime < gRow.Cells["endtimenormal"].GetValue<DateTime>() && !gRow.Cells["endtimenormal"].Value.Equals(System.DBNull.Value))
                    {
                        dtMaxendtime = gRow.Cells["endtimenormal"].GetValue<DateTime>();
                    }
                }
                _view.ViewGrids["trade"].ActiveRow.Cells["begtimenormal"].Value = dtMinbegtime;
                _view.ViewGrids["trade"].ActiveRow.Cells["endtimenormal"].Value = dtMaxendtime;
            }
            #endregion
            #endregion
            
            return "OK";
        }
        /* DuplicateRowEvent_Trade_3
        Set cst_tradedate field for duplicated rows */
        public UiEventResult DuplicateRowEvent_trade_3(object sender, NewRowEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.NewRow).Equals(null)) return new UiEventResult(EventStatus.Continue);
            
            
            foreach (UltraGridRow _row in _view.ViewGrids["tradedetail"].Rows)
            {
                _row.Cells["cst_tradedate"].Value = DateTime.Now;
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Validate duplicated wellhead position
        Validate duplicated NG wellhead position */
        public UiEventResult DuplicateRowEvent_tradedetail_1(object sender, NewRowEventArgs e)
        {
            
            DataRow tradeDetailRow = ((ViewGrid)sender).GetBindRow(_view.ViewGrids["tradedetail"].ActiveRow);
            if (tradeDetailRow == null) return new UiEventResult(EventStatus.Continue);
            if (tradeDetailRow.IsNull("product") || tradeDetailRow.IsNull("property")) return new UiEventResult(EventStatus.Continue);
            if (tradeDetailRow.IsNull("trade") || tradeDetailRow["trade"].ToString().Length < 4) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters",
            new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", e.NewRow.Cells["product"].Value.ToString(), "commodityclass" });
            if (commodityclass != "GAS") return new UiEventResult(EventStatus.Continue);
            
            string whereclause = "quantitystatus not in ('TRADE', 'FORECAST') and position.position=ngquantity.position and position.trade='" + e.NewRow.Cells["trade"].GetValue<string>() + "'";
            whereclause += " and begtime<" + SqlHelper.SqlDate(e.NewRow.Cells["endtime"].GetValue<DateTime>());
            whereclause += " and endtime>" + SqlHelper.SqlDate(e.NewRow.Cells["begtime"].GetValue<DateTime>());
            string quantitystatus = Soap.Invoke<string>(_view.Class.Url, "ValidateSQL", new string[] { "table", "column", "whereclause" },
            new object[] { "position,ngquantity", "quantitystatus", whereclause });
            
            if (quantitystatus.Length > 0)
            {
                MessageBox.Show("There is a nominated record for the time period.", "New row error!");
                e.NewRow.Delete();
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* DuplicateRowEvent_Tradedetail_2
        Set cst_tradedate field for duplicated rows */
        public UiEventResult DuplicateRowEvent_tradedetail_2(object sender, NewRowEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.NewRow).Equals(null)) return new UiEventResult(EventStatus.Continue);
            
            string sql = string.Format("SELECT CAST(TT.cst_defaultreadytoinvoice AS INT) AS IntValue FROM trade T INNER JOIN Position P ON P.Trade = T.trade INNER JOIN tradetype TT ON TT.tradetype = T.tradetype WHERE P.trade = {0} ", e.NewRow.Cells["trade"].Value);
            int defaultreadytoinvoice = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
            
            if (defaultreadytoinvoice == 0)
            {
                e.NewRow.Cells["cst_readytoinvoice"].Value = false;
            }
            else
            {
                e.NewRow.Cells["cst_readytoinvoice"].Value = true;
            }
            
            e.NewRow.Cells["cst_tradedate"].Value = DateTime.Now;
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* NewRowEvent_Positionconstraint_1
        Default timeunit from position to new position constraint row */
        public UiEventResult NewRowEvent_positionconstraint_1(object sender, NewRowEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.NewRow).Equals(null)) return new UiEventResult(EventStatus.Continue);
            
            string commodityclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "commodityclass" });
            string positionclass = Soap.Invoke<string>(_view.Class.Url, "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" },
            new object[] { "product", "product", _view.ViewGrids["tradedetail"].ActiveRow.Cells["product"].GetValue<string>(), "positionclass" });
            
            string timeunit = "", unit="";
            DataRow[] tradeDetailRows = ((ViewGrid)sender).GetBindRow(e.NewRow).GetParentRows("tradedetail_positionconstraint");
            foreach (DataRow tradeDetailRow in tradeDetailRows)
            {
                if (!string.IsNullOrEmpty(tradeDetailRow["timeunit"].ToString())) timeunit = tradeDetailRow["timeunit"].ToString();
                if (!string.IsNullOrEmpty(tradeDetailRow["unit"].ToString())) unit = tradeDetailRow["unit"].ToString();
            }
            
            if (!string.IsNullOrEmpty(unit)) e.NewRow.Cells["unit"].Value = unit;
            if (!string.IsNullOrEmpty(timeunit))
            {
                e.NewRow.Cells["timeunit"].Value = timeunit;
                if (commodityclass == "GAS" && positionclass == "COMMODITY" && timeunit != "HOUR" && timeunit != "DAY") e.NewRow.Cells["timeunit"].Value = DBNull.Value;
            }
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* NewRowEvent_Tradedetail_1
        Set cst_tradedate field for new rows */
        public UiEventResult NewRowEvent_tradedetail_1(object sender, NewRowEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.NewRow).Equals(null)) return new UiEventResult(EventStatus.Continue);
            
            e.NewRow.Cells["cst_tradedate"].Value = DateTime.Now;
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* NewRowEvent_TradeForwardCurve_1
        Default trade forward curve begtime/endtime to trade begtime/endtime */
        public UiEventResult NewRowEvent_tradeforwardcurve_1(object sender, NewRowEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.NewRow).Equals(null)) return new UiEventResult(EventStatus.Continue);
            
            e.NewRow.Cells["begtime"].Value = _view.ViewGrids["trade"].ActiveRow.Cells["begtime"].Value;
            e.NewRow.Cells["endtime"].Value = _view.ViewGrids["trade"].ActiveRow.Cells["endtime"].Value;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* Trade Execution - Create Document Per Position
        Trade Execution - EVE - Toolclick button logic for create a document per position */
        public UiEventResult ToolClick_Create_Document_Per_Position_After_1()
        {
            ViewGrid tradeDetailViewGrid = _view.ViewGrids["tradedetail"];
            ViewGrid documentViewGrid = _view.ViewGrids["collaboration_document"];
            
            if (tradeDetailViewGrid.ActiveRow == null)
            {
                FormHelper.ShowMessage("Info", "A position row needs to be selected to create. To create a new document it is necessary to select the associated position.");
            }
            else if (tradeDetailViewGrid.ActiveRow.GetDataRow() != null && tradeDetailViewGrid.ActiveRow.GetDataRow().RowState == DataRowState.Added)
            {
                FormHelper.ShowMessage("Info", "Please update the new position first, then create its document.");
            }
            else if (documentViewGrid.ActiveRow == null)
            {
                FormHelper.ShowMessage("Info", "A document row needs to be selected to create.  To create a new document append a new row, choose a document format and click \"Create Document Per Position\" button.");
            }
            else
            {
                documentViewGrid.ActiveRow.Update();
                
                if (string.IsNullOrEmpty(documentViewGrid.ActiveRow.Cells["documentformat"].Value.ToString()))
                {
                    FormHelper.ShowMessage("Info", "A document format is required");
                }
                else if (string.IsNullOrEmpty(documentViewGrid.ActiveRow.Cells["collaboration"].Value.ToString()))
                {
                    FormHelper.ShowMessage("Info", "The collaboration column is null and is required");
                }
                else if (string.IsNullOrEmpty(documentViewGrid.ActiveRow.Cells["dbtable"].Value.ToString()))
                {
                    FormHelper.ShowMessage("Info", "The dbtable column is null and is required");
                }
                else
                {
                    string documentFormat = documentViewGrid.ActiveRow.Cells["documentformat"].Value.ToString();
                    
                    string position = tradeDetailViewGrid.ActiveRow.Cells["position"].Value.ToString();
                    
                    string contentDocumentBase64 = Soap.Invoke<string>("cst_UtilsWS.asmx", "GetRRSSReportByDocumentFormat", new string[] { "documentFormat", "keyParameters", "valueParameters" }, new object[] { documentFormat, new string[] { "position" }, new string[] { position } });
                    
                    if (!string.IsNullOrEmpty(contentDocumentBase64))
                    {
                        string collaboration = documentViewGrid.ActiveRow.Cells["collaboration"].Value.ToString();
                        
                        documentViewGrid.ActiveRow.Selected = false;
                        
                        DocResults document = (DocResults)AllegroSerializer.DeserializeXmlNode(Soap.Invoke("cst_UtilsWS.asmx", "StoreDocumentIntoCollaboration", new string[] { "collaboration", "dbTable", "documentFormat", "description", "fileName", "documentData" }, new object[] { collaboration, "trade", documentFormat, position, position, contentDocumentBase64 }), typeof(DocResults));
                        
                        if (document != null)
                        {
                            _view.SetDocumentRow(string.IsNullOrEmpty(documentViewGrid.ActiveRow.Cells["filename"].Value.ToString()) || documentViewGrid.ActiveRow.GetDataRow().RowState == DataRowState.Added ? documentViewGrid.ActiveRow : documentViewGrid.AppendRow(), document);
                        }
                    }
                    else
                    {
                        FormHelper.ShowMessage("Info", "It has not been possible to generate the document");
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

