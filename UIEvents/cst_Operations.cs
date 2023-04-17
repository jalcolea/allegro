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
    public class cst_OperationsCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Operations - InitView
        Operations - EVE - InitView */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* Operations - Export
        Operations - EVE - Export Operations for Balance report */
        public UiEventResult ToolClick_Export_Operations_After_1()
        {
            try
            {
                ExcelFile excelFile = new ExcelFile();
                excelFile.Worksheets.Add("Operaciones Spain Compra");
                
                ExcelWorksheet excelWorksheet = excelFile.Worksheets["Operaciones Spain Compra"];
                
                DataSet ds = new DataSet();
                
                SqlHelper.RetrieveData(ds, new[] { "cstview_operations" }, new[] { "select * from cstview_operations where Positiontype = 'BUY'" });
                
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
                
                SqlHelper.RetrieveData(ds2, new[] { "cstview_operations" }, new[] { "select * from cstview_operations where Positiontype = 'SELL'" });
                
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

