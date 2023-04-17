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
    public class SysAdminCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Utils - Crypt Custom Config Keys
        Utils - EVE - Allows to encrypt Custom Config Keys */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_view.DataSource.Tables.Contains("cst_configkey"))
            {
                Allegro.Core.Encryption.Crypt crypt = null;
                
                foreach (DataRow row in _view.DataSource.Tables["cst_configkey"].AsEnumerable().Where(dr => dr.RowState == DataRowState.Added || dr.RowState == DataRowState.Modified))
                {
                    if (row.Field<bool>("crypt"))
                    {
                        if (crypt == null)
                        {
                            crypt = new Allegro.Core.Encryption.Crypt();
                        }
                        
                        row["value"] = crypt.Encrypt(row.Field<string>("value"));
                    }
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

