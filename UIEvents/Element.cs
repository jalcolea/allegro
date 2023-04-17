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
    public class ElementCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Element_AfterCellUpdate_exportfolder_UI_1
        Elements Manager - no spaces */
        public UiEventResult AfterCellUpdate_exportfolder_123(object sender, CellEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string name = e.Cell.Value.ToString();
            if (name.Contains(" "))
            {
                MessageBox.Show("Folder path cannot contain spaces!", "Warning");
                e.Cell.SetValue(string.Empty, false);
                return new UiEventResult(EventStatus.Cancel);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_AfterRetrieveData_UI_1
        Elements Manager - Show the decrypted key. */
        public UiEventResult AfterRetrieveData_123(object sender, RetrieveDataEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            if (_view.DataSource.Tables["em_license"].Rows.Count > 0)
            {
                string licenseKey = _view.DataSource.Tables["em_license"].Rows[0].GetColumnValue<string>("licensekey");
                if (string.IsNullOrEmpty(licenseKey))
                    return new UiEventResult(EventStatus.Continue);
                
                string[] elements = Soap.Invoke<string[]>("ElementWebWS.asmx", "GetLicensedElements", new string[] { "licenseKey", }, new object[] { licenseKey });
                DateTime expiredate = Soap.Invoke<DateTime>("ElementWebWS.asmx", "GetLicenseExpireDate", new string[] { "licenseKey", }, new object[] { licenseKey });
                // Add the Elements to the view.
                
                for (int i = 0; i < elements.Length; i++)
                {
                    DataRow newRow = _view.DataSource.Tables["elements"].NewRow();
                    newRow.SetColumnValue("element", elements[i]);
                    _view.DataSource.Tables["elements"].Rows.Add(newRow);
                }
                
                // Add the Expire date to the view.
                _view.DataSource.Tables["em_license"].Rows[0].SetColumnValue("expiredate", expiredate);
                
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_AfterUpdateData_UI_1
        Elements Manager - refresh the parameter view. */
        public UiEventResult AfterUpdateData_123(object sender, EventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            _view.RetrieveData();
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_BeforeReowActivate_About_UI_1
        Elements Manager - Do not allow insert or delete. */
        public UiEventResult BeforeRowActivate_about_123(object sender, RowEventArgs e)
        {
            // Do not allow row insert or delete
            _view.ViewGrids["about"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["about"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_BeforeRowActivate_configuration_UI_1
        Elements Parameters - do not allow insert or delete. */
        public UiEventResult BeforeRowActivate_configuration_123(object sender, RowEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            // Do not allow row insert or delete
            _view.ViewGrids["configuration"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["configuration"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_BeforeRowActivate_license_UI_1
        Elements Parameters - do not allow insert or delete. */
        public UiEventResult BeforeRowActivate_license_123(object sender, RowEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            // Do not allow row insert or delete
            _view.ViewGrids["license"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["license"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_BeforeRowActivate_parameters_UI_1
        Elements Parameters - do not allow insert or delete. */
        public UiEventResult BeforeRowActivate_parameters_123(object sender, RowEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            // Do not allow row insert or delete
            _view.ViewGrids["parameters"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["parameters"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_BeforeUpdateData_UI_1
        Elements Parameters - before update validate */
        public UiEventResult BeforeUpdateData_123(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            // Only one row allowed for Configuration.
            if (_view.ViewGrids["configuration"].Rows.Count != 1 || _view.ViewGrids["configuration"].PaneType != "GRID")
            {
                MessageBox.Show("Configuration can only have one export folder path.", "Warning");
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Element_InitView_UI_1
        Elements Manager - class variables. */
        public UiEventResult InitView_123()
        {
            // Do not allow row insert or delete.
            
            // Configuration
            _view.ViewGrids["configuration"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["configuration"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            
            // Parameters
            _view.ViewGrids["parameters"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["parameters"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            
            // License
            _view.ViewGrids["license"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["license"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            
            // Elements
            _view.ViewGrids["elements"].DisplayLayout.Bands[0].Override.AllowAddNew = AllowAddNew.No;
            _view.ViewGrids["elements"].DisplayLayout.Bands[0].Override.AllowDelete = DefaultableBoolean.False;
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

