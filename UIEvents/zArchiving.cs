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
    public class zArchivingCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Archiving Process - ztimerange field
        Archiving Process - ztimerange field - DEV - Select a correct value of ztimerange field */
        public UiEventResult AfterCellUpdate_zarchivingtables_zprocesstype_1(object sender, CellEventArgs e)
        {
            string processtype = e.Cell.Text.ToString();
            
            if (processtype.Equals("Archive"))
                _view.ViewGrids["zarchivingtables"].ActiveRow.Cells["ztimerange"].SetValue("13", false);
            
            if (processtype.Equals("Delete"))
                _view.ViewGrids["zarchivingtables"].ActiveRow.Cells["ztimerange"].SetValue("3", false);
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Archiving Process - Before Update Data
        Archiving Process - Before Update Data - DEV - Updates the Archiving Tables History pane in the Archiving Parameters view */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<DataRow> modifiedRowsList = new List<DataRow>();
            modifiedRowsList.AddRange(_view.DataSource.Tables["zarchivingtables"].Select("", "", DataViewRowState.ModifiedCurrent));
            
            if(modifiedRowsList.Count == 0)
                return new UiEventResult(EventStatus.Continue);
            
            DataSet zarchivingDataSet = new DataSet();
            SqlHelper.RetrieveData(zarchivingDataSet, new string[] { "zarchivingtableshistory" }, new string[] { "SELECT * FROM zarchivingtableshistory WHERE 1 = 0" });
            
            foreach (DataRow row in modifiedRowsList)
            {
                List<DataColumn> columnsList = new List<DataColumn>(row.GetChangedColumns());
                
                if (columnsList.Count == 0)
                    continue;
                
                foreach (DataColumn dc in columnsList)
                {
                    DataRow historyRow = zarchivingDataSet.Tables["zarchivingtableshistory"].NewRow();
                    historyRow["surrogate"] = -1;
                    historyRow["zsurrogatearchivingtables"] = row["surrogate"].ToString();
                    historyRow["ztabletype"] = row["ztabletype"].ToString();
                    historyRow["ztablename"] = row["ztablename"].ToString();
                    historyRow["zcolumnmodified"] = dc.ColumnName;
                    historyRow["zoriginalvalue"] = row[dc.ColumnName, DataRowVersion.Original].ToString();
                    historyRow["zmodifiedvalue"] = row[dc.ColumnName].ToString();
                    historyRow["zauditname"] = AppManager.UserName.ToString();
                    historyRow["zauditdate"] = DateTime.Now;
                    zarchivingDataSet.Tables["zarchivingtableshistory"].Rows.Add(historyRow);
                }
            }
            
            SqlHelper.UpdateData(zarchivingDataSet);
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Archiving Process - DuplicatezArchivingtables
        Archiving Process - DEV - Clean zarchivingtableshistory when is duplicated */
        public UiEventResult DuplicateRowEvent_zarchivingtables_1(object sender, NewRowEventArgs e)
        {
            for (int i = _view.ViewGrids["zarchivingtableshistory"].Rows.Count; i >= 1; i--)
            {
                UltraGridRow dRow = _view.ViewGrids["zarchivingtableshistory"].Rows[i - 1];
                dRow.Delete();
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Archiving Process - Init View
        Archiving Process - DEV - Init View */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

