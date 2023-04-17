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
    public class NetForwardCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* CustomUpdateOpenInventory
        Class event to update Open Inventory monthly based on select criteria timerande */
        public UiEventResult ToolClick_Update_Open_Inventory_After_1()
        {
            SelectCriteria criteria = _view.GetSelectCriteria(true, true);
            string valuationmode = "";
            if (_view.DtParameter.Rows.Count > 0)
            {
                if (!_view.DtParameter.Rows[0].IsNull("valuationmode"))
                {
                    valuationmode = _view.DtParameter.Rows[0]["valuationmode"].ToString();
                }
            }
            
            string[] _string = new string[2] { "criteria", "valuationmode" };
            object[] _object = new object[2] { criteria, valuationmode };
            Soap.Invoke("extendNetForwardWS.asmx", "UpdateOpenInventory", _string, _object);
            UiEventResult result = new UiEventResult(EventStatus.Continue);
            return result;
        }
        
        
    }
}

