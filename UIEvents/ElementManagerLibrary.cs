#define HORIZON
// FILE: ElementManagerLibrary.cs
// MODULE: Elements Manager / Client-Side Forms and Functions
// AUTHOR: David Beckham
// DATE: 2015-11-03

#region References

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
//[09.01.2018 j.buffet] Support of Horizon
#if HORIZON
using Allegro.UI.Forms;
using Allegro.UI;
#endif
#endregion

namespace Allegro.ClassEvents
{
    public class ElementManagerLibrary
    {
        #region Private Variables
        
        private static ElementManagerLibrary _instance;
        private ViewForm _parentView;
        private StatusStrip _dynamicStatusStrip;
        private ToolStripProgressBar _toolStripProgressBar;
        private ToolStripStatusLabel _toolStripStatusLabel;
        // Lock synchronization object
        private static readonly object SyncLock = new object();
        // List of all the elements currently getting imported
        private List<string> _elementList;
        
        #endregion
        
        public ElementManagerLibrary(ViewForm view)
        {
            CheckStatusStrip(view);
        }
        
        /// <summary>
        /// Checks the status strip.
        /// If it isn't already added to the parent view, add the control.
        /// Otherwise get the object of existing control.
        /// </summary>
        /// <param name="view">The parent view.</param>
        private void CheckStatusStrip(ViewForm view)
        {
            _parentView = view;
            
            // Don't add the status strip again if it is already added in the parent view.
            if (_parentView.Controls.ContainsKey("dynamicStatusStrip"))
            {
                if (_elementList == null)
                {
                    _elementList = new List<string>();
                }
                // Get the objects of the status strip and it's child items.
                if (_dynamicStatusStrip == null)
                {
                    _dynamicStatusStrip = (StatusStrip)_parentView.Controls["dynamicStatusStrip"];
                    if (_dynamicStatusStrip != null)
                    {
                        _toolStripProgressBar = (ToolStripProgressBar)_dynamicStatusStrip.Items["toolStripProgressBar"];
                        _toolStripStatusLabel = (ToolStripStatusLabel)_dynamicStatusStrip.Items["toolStripStatusLabel"];
                    }
                    else
                    {
                        _dynamicStatusStrip = new StatusStrip { Name = "dynamicStatusStrip" };
                        _toolStripProgressBar = new ToolStripProgressBar { Name = "toolStripProgressBar" };
                        _toolStripStatusLabel = new ToolStripStatusLabel { Name = "toolStripStatusLabel" };
                        _dynamicStatusStrip.Items.AddRange(new ToolStripItem[] { _toolStripStatusLabel, _toolStripProgressBar });
                        _parentView.Controls.Add(_dynamicStatusStrip);
                    }
                }
            }
            else
            {
                // Add the status strip to the parent view.
                _elementList = new List<string>();
                _dynamicStatusStrip = new StatusStrip { Name = "dynamicStatusStrip" };
                _toolStripProgressBar = new ToolStripProgressBar { Name = "toolStripProgressBar" };
                _toolStripStatusLabel = new ToolStripStatusLabel { Name = "toolStripStatusLabel" };
                _dynamicStatusStrip.Items.AddRange(new ToolStripItem[] { _toolStripStatusLabel, _toolStripProgressBar });
                _parentView.Controls.Add(_dynamicStatusStrip);
            }
        }
        
        /// <summary>
        /// Gets the instance of this class.
        /// </summary>
        /// <param name="view">The parent view.</param>
        /// <returns>Instance of this class.</returns>
        public static ElementManagerLibrary GetInstance(ViewForm view)
        {
            // Double-check locking to make sure only one instance of this class.
            if (_instance == null)
            {
                lock (SyncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new ElementManagerLibrary(view);
                    }
                }
            }
            
            return _instance;
        }
        
        /// <summary>
        /// Shows the status bar.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="view">The parent view.</param>
        private void ShowStatusBar(string elementName, ViewForm view)
        {
            if (_dynamicStatusStrip == null || _dynamicStatusStrip.Items.Count == 0)
            {
                CheckStatusStrip(view);
            }
            
            // Add the element which is currently being imported to the list of elements.
            _elementList.Add(elementName);
            
            _dynamicStatusStrip.Visible = true;
            _dynamicStatusStrip.Name = "DynamicStatusStrip";
            _dynamicStatusStrip.BackColor = Color.Transparent;
            _dynamicStatusStrip.ForeColor = Color.Black;
            _dynamicStatusStrip.Dock = DockStyle.Bottom;
            
            _toolStripProgressBar.Size = new Size(150, 15);
            _toolStripProgressBar.Style = ProgressBarStyle.Marquee;
            _toolStripProgressBar.Step = 50;
            _toolStripProgressBar.Minimum = 0;
            _toolStripProgressBar.Maximum = 100;
            
            _toolStripStatusLabel.Spring = true;
            _toolStripStatusLabel.Text = GetElementsString();
            _toolStripStatusLabel.TextAlign = ContentAlignment.MiddleRight;
            
            _dynamicStatusStrip.Visible = true;
            Application.EnableVisualStyles();
            _dynamicStatusStrip.ResumeLayout(false);
            _dynamicStatusStrip.PerformLayout();
        }
        
        /// <summary>
        /// Gets the elements string.
        /// </summary>
        /// <returns>String of all the elements in the list.</returns>
        private string GetElementsString()
        {
            var elements = "Importing element ";
            if (_elementList.Count > 0)
            {
                elements = _elementList.Aggregate(elements, (current, element) => current + (element + ", "));
                elements = elements.TrimEnd(' ', ',') + "...";
            }
            return elements;
        }
        
        /// <summary>
        /// Monitors the grid queue to check if the import is completed.
        /// </summary>
        /// <param name="eventValue">The event value, e.g Import Element.</param>
        /// <param name="view">The parent view.</param>
        /// <param name="surrogate">The surrogate.</param>
        /// <param name="elementName">Name of the element.</param>
        public async void MonitorGridQueue(string eventValue, ViewForm view, string surrogate, string elementName)
        {
            // Make the status strip visible
            ShowStatusBar(elementName, view);
            
            string[] parameterNames = { "eventvalue", "surrogate" };
            object[] parameterValues = { eventValue, surrogate };
            
            var isCompleted = false;
            try
            {
                while (!isCompleted)
                {
                    isCompleted = await GetQueueStatusAsync(parameterNames, parameterValues, elementName);
                }
            }
            catch (Exception ex)
            {
                LogInfo(string.Format("Error during {0} for {1}. Error: {2}", eventValue, elementName, ex.Message + ex.StackTrace), "MonitorGridQueue");
            }
            
            if (_elementList.Count == 0)
            {
                _dynamicStatusStrip.Visible = false;
            }
        }
        
        /// <summary>
        /// Gets the queue status asynchronous.
        /// </summary>
        /// <param name="parameterNames">The parameter names.</param>
        /// <param name="parameterValues">The parameter values.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>Boolean indicating the task is completed.</returns>
        private async Task<bool> GetQueueStatusAsync(string[] parameterNames, object[] parameterValues, string elementName)
        {
            var count = 1;
            while (count > 0)
            {
                count = Soap.Invoke<int>("ElementWebWS.asmx", "GetQueueStatus", parameterNames, parameterValues);
                if (count == 0)
                {
                    _elementList.Remove(elementName);
                    _toolStripStatusLabel.Text = GetElementsString();
                    break;
                }
                await Task.Delay(1000);
            }
            return true;
        }
        
        /// <summary>
        /// Gets the queue status asynchronous.
        /// </summary>
        /// <param name="parameterNames">The parameter names.</param>
        /// <param name="parameterValues">The parameter values.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>Boolean indicating the task is completed.</returns>
        public void CheckDeleteElementQueueStatus(string[] parameterNames, object[] parameterValues)
        {
            var count = 1;
            while (count > 0)
            {
                count = Soap.Invoke<int>("ElementWebWS.asmx", "GetQueueStatus", parameterNames, parameterValues);
                if (count == 0)
                {
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        
        #region Web Services
        
        /// Invokes the specified server method synchronously.
        /// <param name="className">The name of class to which the method belongs to.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">The parameters for the method.</param>
        public static void CallServerMethod(string className, string methodName, Dictionary<string, object> parameters = null)
        {
            // Prepare the parameters
            if (parameters == null)
                parameters = new Dictionary<string, object>();
            
            var parameterNames = new string[parameters.Count];
            var parameterValues = new object[parameters.Count];
            
            parameters.Keys.CopyTo(parameterNames, 0);
            parameters.Values.CopyTo(parameterValues, 0);
            
            // Invoke the method synchronously
            Soap.Invoke(className + "WS.asmx", methodName, parameterNames, parameterValues);
        }
        
        /// Invokes the specified server method synchronously and returns its result.
        /// <typeparam name="T">The return type of the method to invoke.</typeparam>
        /// <param name="className">The name of class to which the method belongs to.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">The parameters for the method.</param>
        /// <returns>The result of the invoked method.</returns>
        public static T CallServerMethod<T>(string className, string methodName, Dictionary<string, object> parameters = null)
        {
            // Prepare the parameters
            if (parameters == null)
                parameters = new Dictionary<string, object>();
            
            var parameterNames = new string[parameters.Count];
            var parameterValues = new object[parameters.Count];
            
            parameters.Keys.CopyTo(parameterNames, 0);
            parameters.Values.CopyTo(parameterValues, 0);
            
            // Invoke the method synchronously and return the result
            return Soap.Invoke<T>(className + "WS.asmx", methodName, parameterNames, parameterValues);
        }
        //// j.buffet
        /// Invokes the specified server method asynchronously.
        /// <param name="className">The name of class to which the method belongs to.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">The parameters for the method.</param>
        /// <param name="callback">The client-side callback to invoke once the server method has finished.</param>
        ////public static void StartServerMethod(string className, string methodName, Dictionary<string, object> parameters = null, AsyncCallback callback = null)
            ////{
            ////    // Prepare the parameters
            ////    if (parameters == null)
                ////        parameters = new Dictionary<string, object>();
            
            ////    var parameterNames = new string[parameters.Count];
            ////    var parameterValues = new object[parameters.Count];
            
            ////    parameters.Keys.CopyTo(parameterNames, 0);
            ////    parameters.Values.CopyTo(parameterValues, 0);
            
            ////    // Invoke the method asynchronously
            ////    Soap.AsyncInvoke(className + "WS.asmx", methodName, parameterNames, parameterValues, callback);
        ////}
        
        /// <summary>
        /// Writes the given information message to the log.
        /// </summary>
        /// <param name="message">The text of the message.</param>
        /// <param name="taskName">The name of the task.</param>
        /// <param name="taskID">The ID of the task.</param>
        /// <param name="subtaskName">The name of the subtask.</param>
        /// <param name="subtaskID">The ID of the subtask.</param>
        /// <param name="userName">The name of the user that caused this message.</param>
        public static void LogInfo(string message, string taskName, uint? taskID = null, string subtaskName = null, uint? subtaskID = null, string userName = null)
        {
            // Prepare the parameters
            var parameters = new Dictionary<string, object>();
            parameters.Add("type", "INFO");
            parameters.Add("message", message);
            parameters.Add("errorMessage", null);
            parameters.Add("taskName", taskName);
            parameters.Add("taskID", taskID);
            parameters.Add("subtaskName", subtaskName);
            parameters.Add("subtaskID", subtaskID);
            parameters.Add("userName", userName);
            
            // Forward the call
            CallServerMethod("ElementWeb", "LogMessage", parameters);
        }
        #endregion
    }
}
