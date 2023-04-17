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
    public class ElementLogCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* ElementLog_BeforeRowActivate_Element_UI_1
        Element Log - Do not allow insert or delete. */
        public UiEventResult BeforeRowActivate_element_123(object sender, RowEventArgs e)
        {
            // Do not allow row insert or delete
            _view.ViewGrids["element"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["element"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementLog_BeforeRowActivate_Log_UI_1
        Element Log - Do not allow insert or delete. */
        public UiEventResult BeforeRowActivate_log_123(object sender, RowEventArgs e)
        {
            // Do not allow row insert or delete
            _view.ViewGrids["log"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["log"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

