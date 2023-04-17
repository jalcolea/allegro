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
    public class DatabaseArchiveCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* DbArchive_BeforeCellUpdate_DataTables_UI_1
        Data Archiving - Lock the default tables to be archived. */
        public UiEventResult BeforeCellUpdate_tablename_911(object sender, BeforeCellUpdateEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            
            if(e.NewValue.Equals("valuationsummary"))
            {
                ShowMessage("Data Archiving", "'valuationsummary' table is archived along with 'valuationdetail' table. So it doesn't need to be added separately.");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            var defaultTablesList = new List<string> { "correlation", "dbaudit", "message", "pricevalue", "valuationdetail", "document" };
            
            var currentTableList = new List<string>();
            foreach(var row in ((ViewGrid)sender).Rows)
            {
                currentTableList.Add(row.Cells["tablename"].Value.ToString());
            }
            
            // The newly added / updated table name exists in the default table list and current table list then cancel the insert/update.
            if (defaultTablesList.Intersect(currentTableList).Any(table => table.Contains(e.NewValue.ToString())))
            {
                var message =
                string.Format(
                "'{0}' is already in the archive data tables list.{1}Please choose a data table which is not in the data tables list.{2}{3}WARNING: Please DO NOT archive any complex data tables as it can cause issues in Allegro Database.",
                e.NewValue, Environment.NewLine, Environment.NewLine, Environment.NewLine);
                ShowMessage("Data Archiving", message);
                
                return new UiEventResult(EventStatus.Cancel);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* DbArchive_BeforeRowActivate_Configuration_UI_1
        Data Archiving - Do not allow insert or delete for the configuration pane. */
        public UiEventResult BeforeRowActivate_Configuration_911(object sender, RowEventArgs e)
        {
            _view.ViewGrids["Configuration"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["Configuration"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            return new UiEventResult(EventStatus.Continue);
        }
        /* DbArchive_BeforeRowsDeleted_DataTables_UI_1
        Data Archiving - Do not allow to delete the recommended data tables to be archived. */
        public UiEventResult BeforeRowsDeleted_Data_Tables_911(object sender, BeforeRowsDeletedEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Rows[0]) == null)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            var defaultTablesList = new List<string> { "correlation", "dbaudit", "message", "pricevalue", "valuationdetail", "document" };
            
            var selectedTables = new List<string>();
            foreach (var row in e.Rows)
            {
                selectedTables.Add(row.Cells["tablename"].Value.ToString());
            }
            
            if (defaultTablesList.Intersect(selectedTables).Count() > 0)
            {
                ShowMessage("Data Archiving", "Selected table/s is a recommended table to be archived and cannot be deleted from the list.");
                return new UiEventResult(EventStatus.Cancel);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

