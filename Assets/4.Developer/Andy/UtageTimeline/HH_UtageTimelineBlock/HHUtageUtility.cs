using System.IO;
using System.Collections;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using UnityEngine;

public class HHUtageUtility
{
    public static LanguageManager.Language CurrentLanguage = LanguageManager.Language.EN;

    public const string DEFAULT_PATH = "Assets/Utage/HomuraHime/HomuraHime.xls";
    public const string EXTXLS = ".xls";
    public const string EXTXLSX = ".xlsx";
    public const int TEXTCOLUMENUM = 7;
    public const int ARG1_COLUMENUM = 1;
    public const int LINE_COLUMENUM_EN = 11, LINE_COLUMENUM_CH = 12, LINE_COLUMENUM_JP = 13;
    public const int GUIDCOLUMENUM = 14;

    public static IWorkbook ReadBook(string _path)
    {
        string ext = Path.GetExtension(_path);
        using (FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite))
        {
            if (ext == EXTXLS)
            {
                return new HSSFWorkbook(fs);
            }
            else if (ext == EXTXLSX)
            {
                return new XSSFWorkbook(fs);
            }
            else
            {
                Debug.LogError(_path + " is not excel file");
                return null;
            }
        }
    }
    public static ISheet GetSheet(string _sheetName, string _path = DEFAULT_PATH)
    {
        IWorkbook book = ReadBook(_path);
        if (book == null)
            return null;

        ISheet targetSht = book.GetSheet(_sheetName);
        return targetSht;
    }
    public static string GetLineGUID(string _senario, IRow _row)
    {
        int guidCellNum = 0;
        var sheet = GetSenarioSheet(_senario);
        for (int i = 0; i < sheet.GetRow(0).RowNum; i++)
        {
            var stringValue = sheet.GetRow(0).GetCell(i).StringCellValue;
            if (stringValue == "guid" || stringValue == "GUID")
            {
                guidCellNum = i;
                break;
            }
        }
        return _row.GetCell(guidCellNum).StringCellValue;
    }
    public static ISheet GetSenarioSheet(string _senario)
    {
        IWorkbook book = ReadBook(DEFAULT_PATH);
        int shtsNum = book.NumberOfSheets;
        for (int i = 0; i < shtsNum; i++)
        {
            var sheet = book.GetSheetAt(i);
            for (int j = sheet.FirstRowNum + 1; j <= sheet.LastRowNum; ++j)
            {
                string str = "";
                IRow row = sheet.GetRow(j);
                if (!TryGetFirstCellStringInRow(sheet, row, ref str))
                {
                    continue;
                }
                else
                {
                    if (str[0] == '*' && str.Contains(_senario))
                        return sheet;
                }
            }
        }
        return null;
    }
    public static bool TryFindSenario(string _senario, ref List<IRow> _rows)
    {
        _rows = new List<IRow>();
        IWorkbook book = ReadBook(DEFAULT_PATH);
        int shtsNum = book.NumberOfSheets;
        bool pass = false;
        bool found = false;
        for (int i = 0; i < shtsNum; i++)
        {
            var sheet = book.GetSheetAt(i);
            for (int j = sheet.FirstRowNum +1 ; j <= sheet.LastRowNum; ++j)
            {
                string str = "";
                IRow row = sheet.GetRow(j);
                if (!TryGetFirstCellStringInRow(sheet, row, ref str))
                {
                    continue;
                }
                else
                {
                    if (str[0] == '*' && str.Contains(_senario))
                    {
                        pass = true;
                        found = true;
                    }
                    else if (str == "EndScenario")
                    {
                        found = false;
                    }

                    if (found)
                    {
                        Debug.Log(row.GetCell(row.FirstCellNum).StringCellValue);
                        _rows.Add(row);
                    }
                }
            }
        }
        return pass;
    }
    private static bool TryGetFirstCellStringInRow(ISheet _sheet, IRow _row, ref string _str)
    {
        if (_row == null)
        {
            _str = "";
            return false;
        }
        ICell cell = null;
        for (int i = 0; i < _row.LastCellNum; i++)
        {
            cell = _row.GetCell(i);
            if (cell == null)
                continue;
            if (!string.IsNullOrEmpty(cell.StringCellValue))
            {
                _str = cell.StringCellValue;
                break;
            }
        }
        if (cell == null)
        {
            _str = "";
            return false;
        }
        _str = cell.StringCellValue;
        if (_str == "")
            return false;
        return true;
    }
}
