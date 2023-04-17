using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Allegro.ClassEvents
{
    public class cst_HubExcelWorkbook : cst_IHubExcelWorkbook
    {
        private readonly Infragistics.Documents.Excel.Workbook book;
        
        public cst_HubExcelWorkbook(Infragistics.Documents.Excel.Workbook bookExt)
        {
            book = bookExt;
        }
        
        public int WorksheetsCount()
        {
            return book.Worksheets.Count;
        }
        
        public cst_IHubExcelWorkSheet GetWorkSheet(string key)
        {
            return new cst_HubExcelWorksheet(book.Worksheets[key]);
        }
        
        public cst_IHubExcelWorkSheet GetWorkSheet(int key)
        {
            return new cst_HubExcelWorksheet(book.Worksheets[key]);
        }
    }
    
    public class cst_HubExcelWorksheet : cst_IHubExcelWorkSheet
    {
        private readonly Infragistics.Documents.Excel.Worksheet wsheet;
        
        public cst_HubExcelWorksheet(Infragistics.Documents.Excel.Worksheet wsheetExt)
        {
            wsheet = wsheetExt;
        }
        
        public int RowsCount()
        {
            return wsheet.Rows.Count();
        }
        
        public cst_IHubExcelRow GetRow(int key)
        {
            return new HubExcelRow(wsheet.Rows[key]);
        }
    }
    
    public class HubExcelRow : cst_IHubExcelRow
    {
        private readonly Infragistics.Documents.Excel.WorksheetRow wsRow;
        
        public HubExcelRow(Infragistics.Documents.Excel.WorksheetRow wsRowExt)
        {
            wsRow = wsRowExt;
        }
        
        public int CellsCount()
        {
            return wsRow.Cells.Count();
        }
        
        public cst_IHubExcelCell GetCell(int key)
        {
            return new cst_HubExcelCell(wsRow.Cells[key]);
        }
    }
    
    public class cst_HubExcelCell : cst_IHubExcelCell
    {
        private readonly object value;
        private readonly string TextValue;
        
        public cst_HubExcelCell(Infragistics.Documents.Excel.WorksheetCell wsRowCell)
        {
            value = wsRowCell.Value;
            TextValue = wsRowCell.GetText();
        }
        
        public object GetValue()
        {
            DateTime date;
            
            if ((TextValue.Contains("/") || TextValue.Contains("-")) && DateTime.TryParse(TextValue, out date))
            {
                return date;
            }
            else
            {
                return value;
            }
        }
    }
    
    public class cst_HubExcelManager : cst_IHubExcelManager
    {
        private Infragistics.Documents.Excel.Workbook wbook;
        
        public cst_HubExcelManager(Stream stream)
        {
            wbook = Infragistics.Documents.Excel.Workbook.Load(stream);
        }
        
        public cst_HubExcelManager(string path)
        {
            wbook = Infragistics.Documents.Excel.Workbook.Load(path);
        }
        
        public cst_IHubExcelWorkbook LoadWorkbook()
        {
            return new cst_HubExcelWorkbook(wbook);
        }
    }
}
