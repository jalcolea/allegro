/******************************************************************************
NAME:  TradeImporter

PURPOSE/DESCRIPTION/NOTES:    Trade Importer

REVISIONS:
Ver        Date        Author           Description
---------  ----------  ---------------  ------------------------------------
1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
---------  ----------  ---------------  ------------------------------------
*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace Allegro.ClassEvents
{
    #region Excel
    public interface IExcelLibWrapper
    {
        /// <summary>
        /// Creates a wrapper for Excel lib
        /// </summary>
        /// <param name="filename">Filename to load</param>
        /// <returns>Loaded Excel</returns>
        IExcelFile Load(FileStream filename);
    }
    
    public class ExternalSourceException : Exception
    {
        public ExternalSourceException(string message) : base(message) { }
    }
    
    /// <summary>
    /// Excel file wrapper
    /// </summary>
    public interface IExcelFile
    {
        /// <summary>
        /// Available worksheets
        /// </summary>
        IList<IExcelWorkSheet> WorkSheets { get; }
        
        /// <summary>
        /// Adds works sheet to Excel file
        /// </summary>
        /// <param name="worksheetName">Worksheet name to add</param>
        void AddEmptyWorkSheet(string worksheetName);
    }
    
    public interface IExcelWorkSheet
    {
        /// <summary>
        /// Worksheet name
        /// </summary>
        string Name { get; }
        
        int FirstIndexColumn { get; }
        int FirstIndexRow { get; }
        int LastIndexColumn { get; }
        int LastIndexRow { get; }
        
        /// <summary>
        /// Gets stored value in a cell
        /// </summary>
        /// <param name="column">Excel column</param>
        /// <param name="row">Excel row</param>
        /// <returns>Stored value in cell</returns>
        object GetCellValue(string column, int row);
        
        /// <summary>
        /// Gets stored value in a cell using indexes
        /// </summary>
        /// <param name="colIndex">Column index</param>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Stored value</returns>
        object GetCellValue(int colIndex, int rowIndex);
        
        /// <summary>
        /// Sets value in a cell
        /// </summary>
        /// <param name="column">Column to store</param>
        /// <param name="row">Row to store</param>
        /// <param name="value">Stored Value</param>
        void SetCellValue(string column, int row, object value);
    }
    
    public interface IExternalFile
    {
        /// <summary>
        /// Map external file to dataset type
        /// </summary>
        /// <param name="filename">File name to import</param>
        /// <param name="configuration">Mapping configuration</param>
        /// <returns>Resulting dataset</returns>
        DataSet MapFile(FileStream filename, ExternalDataSourceView configuration);
    }
    
    public class ExternalExcelFile : IExternalFile
    {
        private readonly IExcelLibWrapper _wrapper;
        
        public ExternalExcelFile(IExcelLibWrapper wrapper)
        {
            _wrapper = wrapper;
        }
        
        //        public DataSet MapFile(string filename, ExternalDataSourceView configuration)
            public DataSet MapFile(FileStream filename, ExternalDataSourceView configuration)
        {
            var dsData = configuration.DataSetSchema.Clone();
            IExcelFile excel = _wrapper.Load(filename);
            
            foreach (ExternalDataSourcePane paneConf in configuration.PaneConfiguration)
            {
                IExcelWorkSheet worksheet = excel.WorkSheets.FirstOrDefault(ws => ws.Name == paneConf.ExternalDataTable);
                
                if(worksheet == null)
                {
                    throw new ExternalSourceException("The Excel Sheet doesn't contain a tab that matchs with any one configured in Navigation/Trades/Parameters/Trade Importer - External Source Mapping/Datasource Pane/externaldatatable.");
                }
                
                int startIndexRow = worksheet.FirstIndexRow;
                var headerIndex = new Dictionary<int, string>();
                
                if (paneConf.HasHeader)
                {
                    for (int i = worksheet.FirstIndexColumn; i <= worksheet.LastIndexColumn; i++)
                    {
                        if (worksheet.GetCellValue(i, 0)!=null)
                        {
                            headerIndex.Add(i, paneConf.GetAllegroMapping(worksheet.GetCellValue(i, 0).ToString()));
                        }
                    }
                    
                    startIndexRow++;
                }
                else
                {
                    int index = 0;
                    for (int i = worksheet.FirstIndexColumn; i <= worksheet.LastIndexColumn; i++)
                    {
                        headerIndex.Add(i, index.ToString());
                        index++;
                    }
                }
                
                for (int j = startIndexRow; j <= worksheet.LastIndexRow; j++)
                {
                    DataRow row = dsData.Tables[paneConf.AllegroDataTable].NewRow();
                    
                    for (int i = worksheet.FirstIndexColumn; i <= worksheet.LastIndexColumn; i++)
                    {
                        if (paneConf.HasHeader)
                        {
                            object value = worksheet.GetCellValue(i, j);
                            if (value != null)
                            {
                                SetValue(row, row.Table.Columns[headerIndex[i]], value);
                            }
                        }
                        else
                        {
                            object value = worksheet.GetCellValue(i, j);
                            if (value != null)
                            {
                                SetValue(row, row.Table.Columns[int.Parse(headerIndex[i])], value);
                            }
                        }
                    }
                    
                    dsData.Tables[paneConf.AllegroDataTable].Rows.Add(row);
                }
            }
            
            return dsData;
        }
        
        private void SetValue(DataRow row, DataColumn col, object value)
        {
            
            if(col.DataType == typeof(DateTime))
            {
                if(value is double)
                {
                    row[col] = DateTime.FromOADate((double)value);
                }
                else
                {
                    row[col] = value;
                }
            }
            //DZL 30-11-2021: Check Decimal Datatype
            else if(col.DataType == typeof(decimal))
            {
                if (value is string)
                {
                    row[col] = 0;
                    
                }
                else
                {
                    row[col] = value;
                    
                }
            }
            else if (col.DataType == typeof(bool))
            {
                if (value == "")
                {
                    row[col] = 0;
                    
                }
                else
                {
                    row[col] = value;
                    
                }
            }
            else
            {
                row[col] = value;
            }
        }
    }
    #endregion
    #region External datasource
    /// <summary>
    /// Db constants
    /// </summary>
    internal static class ExternalDataSourceDb
    {
        /// <summary>
        /// External datasource mapping db model constants
        /// </summary>
        internal static class ExternalDataSourceMapping
        {
            public const string TableName = "cst_externaldatasourcemapping";
            public const string Attribute = "attribute";
            public const string MappingId = "mappingid";
            public const string ViewColumn = "viewcolumn";
            public const string ViewPane = "viewpane";
            public const string ViewName = "viewname";
        }
        
        /// <summary>
        /// External datasource mapping db model constants for pane
        /// </summary>
        internal static class ExternalDataSourcePane
        {
            public const string TableName = "cst_externaldatasourcepane";
            public const string ViewPane = "viewpane";
            public const string ViewName = "viewname";
            public const string AllegroDataTable = "allegrodatatable";
            public const string ExternalDataTable = "externaldatatable";
            public const string HasHeader = "hasheader";
            public const string Separator = "separator";
            public const string StartingCell = "startingcell";
        }
        
        /// <summary>
        /// External datasource view configuration
        /// </summary>
        internal static class ExternalDataSourceView
        {
            public const string TableName = "cst_externaldatasourceview";
            public const string DataSetName = "datasetname";
            public const string Mapping = "mapping";
            public const string ViewName = "viewname";
            public const string Culture = "culture";
        }
    }
    
    /// <summary>
    /// Mapping types
    /// </summary>
    public enum MappingType
    {
        Excel,
        XML,
        CSV
    }
    
    /// <summary>
    /// Mapping used to realate Allegro fields with external mapping
    /// </summary>
    [Serializable]
    public class ExternalDataSourceMapping : IXmlSerializable
    {
        /// <summary>
        /// Mapping attribute, used just in external sources that supports attributes
        /// </summary>
        public string Attribute { get; set; }
        
        /// <summary>
        /// External mapping identification
        /// </summary>
        public string MappingId { get; set; }
        
        /// <summary>
        /// Allegro view column
        /// </summary>
        public string ViewColumn { get; set; }
        
        /// <summary>
        /// Parent pane
        /// </summary>
        public ExternalDataSourcePane Parent { get; private set; }
        
        /// <summary>
        /// Creates external datasource mapping configuration. This constructor should be used for
        /// serialization purposes because it is created completely empty and may contain inconsistencies.
        /// </summary>
        /// <param name="parent">Parent pane</param>
        internal ExternalDataSourceMapping(ExternalDataSourcePane parent)
        {
            Parent = parent;
        }
        
        /// <summary>
        /// Creates external datasource mapping configuration
        /// </summary>
        /// <param name="drConf">Datarow with configuration</param>
        internal ExternalDataSourceMapping(ExternalDataSourcePane parent, DataRow drConf)
        {
            Parent = parent;
            
            if (!drConf.IsNull(ExternalDataSourceDb.ExternalDataSourceMapping.Attribute))
            {
                Attribute = drConf.Field<string>(ExternalDataSourceDb.ExternalDataSourceMapping.Attribute);
            }
            
            MappingId = drConf.Field<string>(ExternalDataSourceDb.ExternalDataSourceMapping.MappingId);
            ViewColumn = drConf.Field<string>(ExternalDataSourceDb.ExternalDataSourceMapping.ViewColumn);
        }
        
        public XmlSchema GetSchema()
        {
            return null;
        }
        
        public void ReadXml(XmlReader reader)
        {
            Attribute = SanitizeString(reader.GetAttribute("attributte"));
            MappingId = SanitizeString(reader.GetAttribute("mappingid"));
            ViewColumn = SanitizeString(reader.GetAttribute("viewcolumn"));
        }
        
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("externaldatasourcemapping");
            writer.WriteAttributeString("attribute", Attribute);
            writer.WriteAttributeString("mappingid", MappingId);
            writer.WriteAttributeString("viewcolumn", ViewColumn);
            writer.WriteEndElement();
        }
        
        /// <summary>
        /// Sanitizes string value to avoid nulls
        /// </summary>
        /// <param name="value">String to sanitize</param>
        /// <returns>Sanitized string</returns>
        private string SanitizeString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            else
            {
                return value;
            }
        }
    }
    
    /// <summary>
    /// External datasource pane configruation
    /// </summary>
    [Serializable]
    public class ExternalDataSourcePane : IXmlSerializable
    {
        /// <summary>
        /// Mapping from this pane
        /// </summary>
        private readonly List<ExternalDataSourceMapping> _mappings = new List<ExternalDataSourceMapping>();
        
        /// <summary>
        /// Allegro pane datatable
        /// </summary>
        public string AllegroDataTable { get; set; }
        
        /// <summary>
        /// External datatable used by external system
        /// </summary>
        public string ExternalDataTable { get; set; }
        
        /// <summary>
        /// Tells when external datasource table contains header
        /// </summary>
        public bool HasHeader { get; set; }
        
        /// <summary>
        /// Used separator when is supported by file type
        /// </summary>
        public string Separator { get; set; }
        
        /// <summary>
        /// Starting cell to start to read
        /// </summary>
        public string StartingCell { get; set; }
        
        /// <summary>
        /// View pane name
        /// </summary>
        public string ViewPane { get; set; }
        
        /// <summary>
        /// Pane mappings
        /// </summary>
        public IEnumerable<ExternalDataSourceMapping> Mappings { get { return _mappings; } }
        
        /// <summary>
        /// View configuration parent
        /// </summary>
        public ExternalDataSourceView Parent { get; private set; }
        
        /// <summary>
        /// Creates an empty pane. This constructor is suitable for serialization.
        /// Otherwise it could conntain inconsistencies
        /// </summary>
        /// <param name="parent">Parent view</param>
        internal ExternalDataSourcePane(ExternalDataSourceView parent)
        {
            Parent = parent;
        }
        
        /// <summary>
        /// Creates a external pane configuration
        /// </summary>
        /// <param name="drConfigPane">Pane conifguration</param>
        /// <param name="mappings">Enumeration of mappings</param>
        internal ExternalDataSourcePane(ExternalDataSourceView parent, DataRow drConfigPane, IEnumerable<DataRow> mappings)
        {
            Parent = parent;
            
            if (!drConfigPane.IsNull(ExternalDataSourceDb.ExternalDataSourcePane.ExternalDataTable))
            {
                ExternalDataTable = drConfigPane.Field<string>(ExternalDataSourceDb.ExternalDataSourcePane.ExternalDataTable);
            }
            
            if (!drConfigPane.IsNull(ExternalDataSourceDb.ExternalDataSourcePane.StartingCell))
            {
                StartingCell = drConfigPane.Field<string>(ExternalDataSourceDb.ExternalDataSourcePane.StartingCell);
            }
            
            AllegroDataTable = drConfigPane.Field<string>(ExternalDataSourceDb.ExternalDataSourcePane.AllegroDataTable);
            HasHeader = drConfigPane.Field<bool>(ExternalDataSourceDb.ExternalDataSourcePane.HasHeader);
            Separator = drConfigPane.Field<string>(ExternalDataSourceDb.ExternalDataSourcePane.Separator);
            ViewPane = drConfigPane.Field<string>(ExternalDataSourceDb.ExternalDataSourcePane.ViewPane);
            
            foreach (DataRow drConf in mappings)
            {
                _mappings.Add(new ExternalDataSourceMapping(this, drConf));
            }
        }
        
        /// <summary>
        /// Gets Allegro mapping from external source
        /// </summary>
        /// <param name="column">External mapping column</param>
        /// <returns>Allegro column</returns>
        public string GetAllegroMapping(string column)
        {
            foreach (var mapping in _mappings)
            {
                if (mapping.MappingId == column)
                {
                    return mapping.ViewColumn;
                }
            }
            
            throw new KeyNotFoundException("Mapping column not found.");
        }
        
        public XmlSchema GetSchema()
        {
            return null;
        }
        
        public void ReadXml(XmlReader reader)
        {
            AllegroDataTable = reader.GetAttribute("allegrodatatable");
            ExternalDataTable = reader.GetAttribute("externaldatatable");
            HasHeader = reader.GetAttribute("hasheader") == "True";
            Separator = reader.GetAttribute("separator");
            StartingCell = reader.GetAttribute("startingcell");
            ViewPane = reader.GetAttribute("viewpane");
            
            reader.Read();
            
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "externaldatasourcemapping")
            {
                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "externaldatasourcemapping")
                {
                    var mapping = new ExternalDataSourceMapping(this);
                    mapping.ReadXml(reader);
                    _mappings.Add(mapping);
                    reader.Read();
                }
            }
        }
        
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("externaldatasourcepane");
            writer.WriteAttributeString("allegrodatatable", AllegroDataTable);
            writer.WriteAttributeString("externaldatatable", ExternalDataTable);
            writer.WriteAttributeString("hasheader", HasHeader.ToString());
            writer.WriteAttributeString("startingcell", StartingCell);
            writer.WriteAttributeString("viewpane", ViewPane);
            
            foreach (ExternalDataSourceMapping mapping in Mappings)
            {
                mapping.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
    }
    
    /// <summary>
    /// External datasource configuration
    /// </summary>
    [Serializable]
    public class ExternalDataSourceView : IXmlSerializable
    {
        /// <summary>
        /// External pane mapping conigurations
        /// </summary>
        private readonly List<ExternalDataSourcePane> _panes = new List<ExternalDataSourcePane>();
        
        /// <summary>
        /// Data set name
        /// </summary>
        public string DataSetName { get; set; }
        
        /// <summary>
        /// Target schema dataset used to hold data
        /// </summary>
        public DataSet DataSetSchema { get; set; }
        
        /// <summary>
        /// Mapping type used
        /// </summary>
        public MappingType Mapping { get; set; }
        
        /// <summary>
        /// Viewname that is going to hold data
        /// </summary>
        public string ViewName { get; set; }
        
        /// <summary>
        /// Format
        /// </summary>
        public string Format { get; set; }
        
        /// <summary>
        /// Types format configuration
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }
        
        /// <summary>
        /// Pane configuration
        /// </summary>
        public ICollection<ExternalDataSourcePane> PaneConfiguration { get { return _panes; } }
        
        public XmlSchema GetSchema()
        {
            return null;
        }
        
        public void ReadXml(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "ExternalDataSourceView")
            {
                reader.Read();
                
                if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "externaldatasource")
                {
                    DataSetName = reader.GetAttribute("name");
                    ViewName = reader.GetAttribute("viewname");
                    Format = reader.GetAttribute("format");
                    FormatProvider = System.Globalization.CultureInfo.GetCultureInfo(Format);
                    reader.Read();
                    
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "schema")
                    {
                        reader.ReadStartElement();
                        DataSetSchema = new DataSet();
                        DataSetSchema.ReadXmlSchema(reader);
                        reader.ReadEndElement();
                    }
                    
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "externaldatasourcepane")
                    {
                        while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "externaldatasourcepane")
                        {
                            var pane = new ExternalDataSourcePane(this);
                            pane.ReadXml(reader);
                            _panes.Add(pane);
                            reader.Read();
                        }
                    }
                }
            }
        }
        
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ExternalDataSourceView");
            writer.WriteStartElement("externaldatasource");
            writer.WriteAttributeString("name", DataSetName);
            writer.WriteAttributeString("viewname", ViewName);
            writer.WriteAttributeString("format", Format);
            writer.WriteStartElement("schema");
            DataSetSchema.WriteXmlSchema(writer);
            writer.WriteEndElement();
            foreach(ExternalDataSourcePane pane in PaneConfiguration)
            {
                pane.WriteXml(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
    #endregion
}
