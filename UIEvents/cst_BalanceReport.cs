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
using GemBox.Spreadsheet;

namespace Allegro.ClassEvents
{
    public class cst_BalanceReportCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* BalanceReport  - InitView
        Balance Report  - EVE -  InitView */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* BalanceReport  - Export
        Balance Report - EVE - Export Operations for Balance report */
        public UiEventResult ToolClick_Export_Operations_After_1()
        {
            try
            {
                ExcelFile excelFile = new ExcelFile();
                excelFile.Worksheets.Add("Operaciones Spain Compra");
                
                ExcelWorksheet excelWorksheet = excelFile.Worksheets["Operaciones Spain Compra"];
                
                DataSet ds = new DataSet();
                
                SqlHelper.RetrieveData(ds, new[] { "cstview_balanceReport" }, new[] { "select IdOperacion IdOperacion, Pos_type Pos_type, Trade_type Trade_type, Contraparte Contraparte, Fecha_Inicio_Carga Fecha_Inicio_Carga, Fecha_Fin_Carga Fecha_Fin_Carga, Fecha_Entrega_Inicio Fecha_Entrega_Inicio, Fecha_entrega_Fin Fecha_entrega_Fin, Fecha_cambio_Ini Fecha_cambio_Ini, Fecha_Cambio_Fin Fecha_Cambio_Fin, Posicion Posicion, Terminal_Carga Terminal_Carga, Terminal_Descarga Terminal_Descarga, Planta Planta, Unidad_Origen Unidad_Origen, Volumen_Total Volumen_Total, Volumen_dia Volumen_dia, Vessel Vessel from cstview_balanceReport inner join cst_extensionparameter ep on ep.extension = 'Balance Report' and ep.configkey = 'Date Filter' where Pos_type = 'BUY' and Trade_Status NOT IN ('DELETED','CLOSED','NEW') and (Trade_type NOT IN ('FINANCIAL SWAP BILATERAL','SERVICES','BUNKERING-THIRD PARTY LNG','VIRTUAL TRADES','VIRTUAL FINANCIAL SWAP','CO2 CREDIT PURCHASE','FINANCIAL SWAP EXCHANGE')  and (Terminal_Descarga <> 'CANAPORT LNG' OR Terminal_Descarga IS NULL) and (Terminal_Carga <> 'CANAPORT LNG' OR Terminal_Carga IS NULL) and (Planta <> 'CANAPORT LNG' OR Planta IS NULL)) and Fecha_entrega_Fin >= ep.value" });
                
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Columns.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                    {
                        excelWorksheet.Rows[0].Cells[i].Value = ds.Tables[0].Columns[i].ColumnName;
                        
                        for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                        {
                            excelWorksheet.Rows[j + 1].Cells[i].Value = ds.Tables[0].Rows[j][i];
                        }
                    }
                }
                
                excelFile.Worksheets.Add("Operaciones Spain Venta");
                
                ExcelWorksheet excelWorksheet2 = excelFile.Worksheets["Operaciones Spain Venta"];
                
                DataSet ds2 = new DataSet();
                
                SqlHelper.RetrieveData(ds2, new[] { "cstview_balanceReport" }, new[] { "select IdOperacion IdOperacion, Pos_type Pos_type, Trade_type Trade_type, Contraparte Contraparte, Fecha_Inicio_Carga Fecha_Inicio_Carga, Fecha_Fin_Carga Fecha_Fin_Carga, Fecha_Entrega_Inicio Fecha_Entrega_Inicio, Fecha_entrega_Fin Fecha_entrega_Fin, Fecha_cambio_Ini Fecha_cambio_Ini, Fecha_Cambio_Fin Fecha_Cambio_Fin, Posicion Posicion, Terminal_Carga Terminal_Carga, Terminal_Descarga Terminal_Descarga, Planta Planta, Unidad_Origen Unidad_Origen, Volumen_Total Volumen_Total, Volumen_dia Volumen_dia, Vessel Vessel from cstview_balanceReport inner join cst_extensionparameter ep on ep.extension = 'Balance Report' and ep.configkey = 'Date Filter' where Pos_type = 'SELL' and Trade_Status NOT IN ('DELETED','CLOSED','NEW') and (Trade_type NOT IN ('FINANCIAL SWAP BILATERAL','SERVICES','BUNKERING-THIRD PARTY LNG','VIRTUAL TRADES','VIRTUAL FINANCIAL SWAP','CO2 CREDIT PURCHASE','FINANCIAL SWAP EXCHANGE')  and (Terminal_Descarga <> 'CANAPORT LNG' OR Terminal_Descarga IS NULL) and (Terminal_Carga <> 'CANAPORT LNG' OR Terminal_Carga IS NULL) and (Planta <> 'CANAPORT LNG' OR Planta IS NULL)) and Fecha_entrega_Fin >= ep.value" });
                
                if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Columns.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds2.Tables[0].Columns.Count; i++)
                    {
                        excelWorksheet2.Rows[0].Cells[i].Value = ds2.Tables[0].Columns[i].ColumnName;
                        
                        for (int j = 0; j < ds2.Tables[0].Rows.Count; j++)
                        {
                            excelWorksheet2.Rows[j + 1].Cells[i].Value = ds2.Tables[0].Rows[j][i];
                        }
                    }
                }
                
                excelFile.Worksheets.Add("Operaciones ESK Truck");
                
                ExcelWorksheet excelWorksheet3 = excelFile.Worksheets["Operaciones ESK Truck"];
                
                DataSet ds3 = new DataSet();
                
                SqlHelper.RetrieveData(ds3, new[] { "cstview_balanceReport" }, new[] { "select IdOperacion IdOperacion, Pos_type Pos_type, Trade_type Trade_type, Contraparte Contraparte, Fecha_Inicio_Carga Fecha_Inicio_Carga, Fecha_Fin_Carga Fecha_Fin_Carga, Fecha_Entrega_Inicio Fecha_Entrega_Inicio, Fecha_entrega_Fin Fecha_entrega_Fin, Fecha_cambio_Ini Fecha_cambio_Ini, Fecha_Cambio_Fin Fecha_Cambio_Fin, Posicion Posicion, Terminal_Carga Terminal_Carga, Terminal_Descarga Terminal_Descarga, Planta Planta, Unidad_Origen Unidad_Origen, Volumen_Total Volumen_Total, Volumen_dia Volumen_dia, Vessel Vessel from cstview_balanceReport where Pos_type = 'LOAD' and Trade_type = 'CISTENA BF'" });
                
                if (ds3 != null && ds3.Tables.Count > 0 && ds3.Tables[0].Columns.Count > 0 && ds3.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds3.Tables[0].Columns.Count; i++)
                    {
                        excelWorksheet3.Rows[0].Cells[i].Value = ds3.Tables[0].Columns[i].ColumnName;
                        
                        for (int j = 0; j < ds3.Tables[0].Rows.Count; j++)
                        {
                            excelWorksheet3.Rows[j + 1].Cells[i].Value = ds3.Tables[0].Rows[j][i];
                        }
                    }
                }
                
                excelFile.SaveXlsx(@"c:\temp\BalanceReport.xlsx");
                ShowMessage("Balance Report", "The file has been created successfully");
            }
            catch
            {
                ShowMessage("Balance Report", "Error in Balance export. Please, contact your administrator.");
                
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

