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
using System.Text.RegularExpressions;
using System.Linq;
using  System.Text;
using System.Xml;
using System.Threading.Tasks;

namespace Allegro.ClassEvents
{
    public class ElementManagerCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* ElementManager_AfterCellUpdate_classevent_classevent_UI_1
        Elements Manager - Class Event view - auto populate component. */
        public UiEventResult AfterCellUpdate_classevent_classevent_123(object sender, CellEventArgs e)
        {
            //[09.01.2018 j.buffet] Support of Horizon
            bool HORIZON = true;
            if (!HORIZON)
            {
                // Name                 Date            Comment
                //  ===============================================================================
                // Kerry Siu          xxx  01/06/2015     Original Code.
                //
                
                if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null)
                {
                    return new UiEventResult(EventStatus.Continue);
                }
                
                var classevent = e.Cell.Value.ToString();
                
                var dsComponent = new DataSet();
                var sqlComponent =
                "select component.component, component.version from classevent left join class on class.class = classevent.class left join component on component.component = class.component where classevent.name = " +
                SqlHelper.SqlValue(classevent);
                SqlHelper.RetrieveData(dsComponent, new[] { "dtComponent" }, new[] { sqlComponent });
                if (dsComponent.Tables.Contains("dtComponent") && dsComponent.Tables["dtComponent"].Rows.Count > 0)
                {
                    e.Cell.Row.Cells["component"].Value =
                    dsComponent.Tables["dtComponent"].Rows[0].GetColumnValue<string>("component");
                    e.Cell.Row.Cells["version"].Value =
                    dsComponent.Tables["dtComponent"].Rows[0].GetColumnValue<string>("version");
                }
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_AfterCellUpdate_name_Server_1
        Elements Manager - no spaces. */
        public UiEventResult AfterCellUpdate_name_123(object sender, CellEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            
            var name = e.Cell.Value.ToString();
            if (!name.Contains(" "))
            {
                return new UiEventResult(EventStatus.Continue);
            }
            //MessageBox.Show("Element name cannot contain spaces!", "Warning");
            ShowMessage("WARNING", "Element name cannot contain spaces!");
            e.Cell.SetValue(string.Empty, false);
            return new UiEventResult(EventStatus.Cancel);
        }
        /* ElementManager_AfterCellUpdate_Version_UI_1
        Elements Manager - Validate version format */
        public UiEventResult AfterCellUpdate_version_123(object sender, CellEventArgs e)
        {
            if (!Regex.IsMatch(e.Cell.Value.ToString(), @"^(\d+\.){1}(\d+)$"))
            {
                ShowMessage("Element Version", "Please add version in the correct format e.g. 2.7");
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_BeforeRowActivate_element_UI_1
        Elements Manager - Disable multi row select. */
        public UiEventResult BeforeRowActivate_element_123(object sender, RowEventArgs e)
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            // Do not allow multi row selection
            _view.ViewGrids["element"].DisplayLayout.Override.SelectTypeRow = SelectType.SingleAutoDrag;
            
            //Version cannot be changed for the licensed elements.
            if (e.Row.GetCellValue<bool>("license"))
            {
                e.Row.Cells["version"].Activation = Activation.NoEdit;
            }
            // only allow one to be selected
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_BeforeRowsDeleted_Element_UI_1
        Element Manager - Warn the user before deleting the element entry. */
        public UiEventResult BeforeRowsDeleted_element_123(object sender, BeforeRowsDeletedEventArgs e)
        {
            // Warn the user of the deletion.
            var result = MessageBox.Show("Please note that the components of this element will not be deleted from Allegro.\nUse Delete Element functionality to delete all the components from Allegro.\n\nDo you want to proceed?", "Delete Element Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(result == DialogResult.No)
            {
                return new UiEventResult(EventStatus.Cancel);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_BeforeUpdateData_UI_1
        Elements Manager - Update the element deployment status. */
        public UiEventResult BeforeUpdateData_123(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataRow[] newRows = _view.DataSource.Tables["em_element"].Select("status = 'IN PROGRESS'", string.Empty, DataViewRowState.Added);
            var element = string.Empty;
            foreach (DataRow row in newRows)
            {
                row.SetColumnValue("status", "SUCCESS");
                element = row.GetColumnValue<string>("name");
            }
            
            UltraGridRow activeRow = _view.ViewGrids["element"].Rows.Where(row => row.GetCellValue<string>("name").Equals(element)).FirstOrDefault();
            if (activeRow != null)
            {
                _view.ViewGrids["element"].ActiveRow = activeRow;
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_DuplicateRowEvent_Element_UI_1
        Elements Manager - Reset the Export Number, Version and Deployment Status. */
        public UiEventResult DuplicateRowEvent_element_123(object sender, NewRowEventArgs e)
        {
            if (e.NewRow.HasCell("exportnumber"))
            {
                e.NewRow.Cells["exportnumber"].SetValue("0", false);
            }
            if (e.NewRow.HasCell("version"))
            {
                e.NewRow.Cells["version"].SetValue("1.0", false);
            }
            if (e.NewRow.HasCell("status"))
            {
                e.NewRow.Cells["status"].SetValue("IN PROGRESS", false);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_ToolClick_Delete_Element_Before_UI_1
        Elements Manager - Delete extension button. */
        public UiEventResult ToolClick_Delete_Element_Before_123()
        {
            if (_view.DataSource.HasChanges())
            {
                ShowMessage("Element Delete Warning", "There are unsaved changes.\nPlease save or discard the changes before proceeding.");
                return new UiEventResult(EventStatus.Continue);
            }
            if (_view.ViewGrids["element"].Selected.Rows.Count != 1)
            {
                ShowMessage("Element Delete Warning", "Please select one element to delete at a time.");
                return new UiEventResult(EventStatus.Continue);
            }
            //var element = _view.ViewGrids["element"].Selected.Rows.GetEnumerator().Current.GetCellValue<string>("name");
            var elementList = string.Empty;
            foreach (var myRow in _view.ViewGrids["element"].Selected.Rows)
            {
                elementList += myRow.Cells["name"].Value + ",";
            }
            elementList = elementList.TrimEnd(',');
            
            // This will be always one element
            var message = Soap.Invoke<string>("ElementWebWS.asmx", "ConfirmDeleteElement", new string[] { "elementName", }, new object[] { elementList });
            if (message.Equals("ServerConfigKeyNotFound"))
            {
                System.Windows.Forms.MessageBox.Show("Server Config Key is not found. Please contact Allegro System Administrator.", "Element Delete Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                return new UiEventResult(EventStatus.Continue);
            }
            else if (message.Equals("DeleteElementNotAllowed"))
            {
                System.Windows.Forms.MessageBox.Show("You are not allowed to delete the element as this functionlity is disabled for you.", "Element Delete Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                return new UiEventResult(EventStatus.Continue);
            }
            else if (string.IsNullOrEmpty(message))
            {
                message = string.Format("This will permanently delete all the Data Model, Visual Model, Security and Message Event changes included in this element from Allegro database and might require View reset and DbSync.{0}{1}Are you sure you want to delete element {2}?", Environment.NewLine, Environment.NewLine, elementList);
            }
            else
            {
                message = string.Format("The following data will be permanently deleted from Allegro:{0}{1}{2}This will permanently delete all the Data Model, Visual Model, Security and Message Event changes included in this element from Allegro database and might require View reset and DbSync.{3}{4}Are you sure you want to delete element {5}?", Environment.NewLine, message, Environment.NewLine, Environment.NewLine, Environment.NewLine, elementList);
            }
            
            System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show(message, "Element Delete Warning", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
            
            if(response == DialogResult.No)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            
            string[] argName = { "elementList" };
            object[] argValue = { elementList };
            
            var surrogate = Soap.Invoke<string>("ElementWebWS.asmx", "StartScheduledTasksDeleteElement", argName, argValue);
            
            var elementManager = ElementManagerLibrary.GetInstance(_view);
            
            string[] parameterNames = { "eventvalue", "surrogate" };
            object[] parameterValues = { "Element Delete", surrogate };
            
            elementManager.CheckDeleteElementQueueStatus(parameterNames, parameterValues);
            
            _view.RetrieveData(_view.GetSelectCriteria(true, true).DbCriteria);
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_ToolClick_Export_Element_Before_UI_1
        Elements Manager - Export extension button. */
        public UiEventResult ToolClick_Export_Element_Before_123()
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015     Original Code.
            //
            
            if (_view.DataSource.HasChanges())
            {
                ShowMessage("Element Export Warning", "There are unsaved changes.\nPlease save or discard the changes before proceeding.");
                return new UiEventResult(EventStatus.Continue);
            }
            
            var elementList = string.Empty;
            if (_view.ViewGrids["element"].Selected.Rows.Count != 1)
            {
                ShowMessage("Element Export Warning", "Please select one element to export at a time.");
                return new UiEventResult(EventStatus.Continue);
            }
            
            var licenseFlag = _view.ViewGrids["element"].Selected.Rows[0].GetCellValue<bool>("license");
            if (licenseFlag)
            {
                ShowMessage("Element Export Warning", "Cannot export licensed Element.");
                return new UiEventResult(EventStatus.Continue);
            }
            
            foreach (var myRow in _view.ViewGrids["element"].Selected.Rows)
            {
                elementList += "'" + myRow.Cells["name"].Value + "',";
            }
            elementList = elementList.TrimEnd(',');
            
            string[] argName = { "elementList" };
            object[] argValue = { elementList };
            
            Soap.Invoke("ElementWebWS.asmx", "StartScheduledTasksExportElement", argName, argValue);
            return new UiEventResult(EventStatus.Continue);
        }
        /* ElementManager_ToolClick_Import_Element_Before_UI_1
        Elements Manager - Import extension button. */
        public UiEventResult ToolClick_Import_Element_Before_123()
        {
            // Name                 Date            Comment
            // ===============================================================================
            // Kerry Siu            01/06/2015      Original Code.
            // Apeksha Kulkarni     04/02/2016      Added new progress bar to show the progress while importing element.
            
            // Create an instance of the open file dialog box.
            // Set filter options and filter index.
            
            if (_view.DataSource.HasChanges())
            {
                ShowMessage("Element Import Warning", "There are unsaved changes.\nPlease save or discard the changes before proceeding.");
                return new UiEventResult(EventStatus.Continue);
            }
            
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = "Import Extensions | configuration.xml",
                FilterIndex = 1,
                Multiselect = false,
                RestoreDirectory = true
                };
                
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return new UiEventResult(EventStatus.Continue);
                }
                
                var filelocation = openFileDialog1.FileName;
                if (filelocation.Length > 260)
                {
                    ShowMessage("WARNING", "File path is too long!");
                    return new UiEventResult(EventStatus.Continue);
                }
                var sFolder = System.IO.Path.GetDirectoryName(filelocation);
                var argName = new[] { "sFolder" };
                var argValue = new object[] { sFolder };
                
                //Invoke Import Method
                var surrogate = Soap.Invoke<string>("ElementWebWS.asmx", "StartScheduledTasksImportElement", argName, argValue);
                
                var elementManager = ElementManagerLibrary.GetInstance(_view);
                if (sFolder != null)
                {
                    elementManager.MonitorGridQueue("Element Import", _view, surrogate, new System.IO.DirectoryInfo(sFolder).Name);
                }
                
                return new UiEventResult(EventStatus.Continue);
            }
            
            
        }
    }
    
