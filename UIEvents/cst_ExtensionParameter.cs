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
    public class cst_ExtensionParameterCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Data View to Excel - Before Update Data
        Custom Extensions Parameters. Don't allow to introduce the same Parameter-Path-Excel Name-Tab in different rows in the pane DataView Excel Parameters. */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /******************************************************************************
            NAME:  Data Views to Excel Export
            
            PURPOSE/DESCRIPTION/NOTES:    Data Views to Excel Export
            
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.0        25/08/2020  SAI              Data Views to Excel Export
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            if (_view.DataSource.Tables["cst_excelparameter"].HasChanges())
            {
                foreach(DataRow row in _view.DataSource.Tables["cst_excelparameter"].GetChanges().Rows)
                {
                    //Check if modified/added column share de same path, name, tab and execution parameter
                    if(row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                    {
                        if (_view.DataSource.Tables["cst_excelparameter"].AsEnumerable().Where(x =>
                        x.Field<string>("excelname") == row["excelname"].ToString() &&
                        x.Field<string>("folder") == row["folder"].ToString() &&
                        x.Field<string>("tabname") == row["tabname"].ToString() &&
                        x.Field<string>("executionparameter") == row["executionparameter"].ToString()
                            ).ToList().Count > 1)
                        {
                            ShowMessage("ExcelParameter", "There is already a row with the same execution parameter, folder, excelname and tabname");
                            return new UiEventResult(EventStatus.Cancel);
                        }
                    }
                    
                }
                
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

