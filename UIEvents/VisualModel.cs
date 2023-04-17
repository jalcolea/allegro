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
    public class VisualModelCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* CS-AssemblyStoreBinaryHelper-AfterCellUpdate
        CS-AssemblyStoreBinaryHelper-AfterCellUpdate */
        public UiEventResult AfterCellUpdate_classevent_code_1(object sender, CellEventArgs e)
        {
            if (e.Cell.Value == null || e.Cell.Value.ToString() != "[BinaryData Update Pending]")
                return new UiEventResult(EventStatus.Continue);
            
            if (_view.DataSource.Tables["classevent_header"].Columns.Contains("csbinarydata") == false)
                _view.DataSource.Tables["classevent_header"].Columns.Add("csbinarydata", typeof(string));
            
            DataRow[] asrows = _view.DataSource.Tables["classevent_header"].Select("action = 'AssemblyStore'", "", DataViewRowState.Added | DataViewRowState.ModifiedCurrent);
            foreach (DataRow row in asrows)
            {
                if (_view.DataSource.Tables["classevent_header"].ExtendedProperties.ContainsKey(row["name"]))
                    row["csbinarydata"] = _view.DataSource.Tables["classevent_header"].ExtendedProperties[row["name"]];
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

