using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Collections;
using System;

namespace Utage.Extension.Custom {
    public class DialogExcelDownloader : MonoBehaviour {
        [SerializeField] string URL;
        [SerializeField] string excelPath;
        [SerializeField] string cvsPath;

        private bool downloadBtn;
        private bool loadBtn;

        [Sirenix.OdinInspector.Button("更疭﹚Csv")]
        private void DownloadCertainCsvFunc(string _name)
        {
            StartCoroutine(DownlCertionCsv(_name));
        }
        [Sirenix.OdinInspector.Button("更疭﹚Sheet")]
        private void LoadCertainFileFromCsv(string _sheetName)
        {
            string sourceDir = "Assets/Demo/CsvTemps/" + _sheetName + ".csv";
            try
            {
                if (!File.Exists(sourceDir))
                {
                    Debug.Log($"{sourceDir} don't contain file {_sheetName}");
                    return;
                }

                using (ExcelPackage sourcepackage = new ExcelPackage(new FileInfo(excelPath)))
                {
                    //眖sheetsい眔疭﹚sheet
                    ExcelWorksheets sheets = sourcepackage.Workbook.Worksheets;
                    ExcelWorksheet worksheet = GetExcelSheetFromBook(_sheetName, ref sheets);

                    //弄fileいゅず甧だ皌﹚sheet
                    ReadCsvStringContentToSheet(sourceDir, ref worksheet);
                    sourcepackage.Save();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        [Sirenix.OdinInspector.Button("更┮ΤCsv")]
        private void DownloadAllCsvFunc()
        {
            StartCoroutine(DownloadAllCsv());
        }

        [Sirenix.OdinInspector.Button("更戈Excel")]
        private void LoadAllFileFromCsv()
        {
            string sourceDirectory = "Assets/Demo/CsvTemps/";
            int fileCount = 0;
            try
            {
                var files = Directory.EnumerateFiles(sourceDirectory, "*.csv");
                foreach (var file in files)
                {
                    Debug.Log(file);
                    using (ExcelPackage sourcepackage = new ExcelPackage(new FileInfo(excelPath)))
                    {
                        string sheetName = file.Split('/').Last().Replace(".csv", "");

                        //眖sheetsい眔疭﹚sheet
                        ExcelWorksheets sheets = sourcepackage.Workbook.Worksheets;
                        ExcelWorksheet worksheet = GetExcelSheetFromBook(sheetName, ref sheets);

                        //弄fileいゅず甧だ皌﹚sheet
                        ReadCsvStringContentToSheet(file, ref worksheet);
                        sourcepackage.Save();
                    }
                    fileCount++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            AssetDatabase.Refresh();
        }
        private ExcelWorksheet GetExcelSheetFromBook(string _sheetName, ref ExcelWorksheets _sheets)
        {
            var selectSheet = from sheet in _sheets
                              where sheet.Name == _sheetName
                              select sheet;
            ExcelWorksheet worksheet = selectSheet.FirstOrDefault();
            if (worksheet == null)
            {
                Debug.Log("Create new sheet" + _sheetName);
                worksheet = _sheets.Add(_sheetName);
            }
            else
            {
                Debug.Log("Already has sheet " + _sheetName);
            }

            return worksheet;
        }
        private void ReadCsvStringContentToSheet(string _path, ref ExcelWorksheet _sheet)
        {
            string txt = File.ReadAllText(_path);
            string[] row = txt.Split('@');
            for (int i = 0; i < row.Length; i++)
            {
                string[] column = row[i].Split('^');
                for (int j = 0; j < column.Length; j++)
                {
                    int r = i + 1;
                    int c = j + 1;
                    _sheet.Cells[r, c].Value = column[j];
                }
            }
            Debug.Log($"Load csv completed. Load elements total count : {_sheet.Cells.Count()}");
        }

        
        public IEnumerator DownlCertionCsv(string _name)
        {
            string sheetName = "";
            WWWForm form = new WWWForm();
            form.AddField("searchName", _name);
            form.AddField("method2", "getSheetName");

            using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    sheetName = www.downloadHandler.text;
                    Debug.Log("sheetName: " + sheetName);
                }
            }

            form = new WWWForm();
            form.AddField("searchName", _name);
            form.AddField("method3", "readSheet");
            using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
            {
                yield return www.SendWebRequest();
                EditorUtility.DisplayProgressBar("更csv", "锣传糶csvい...", 1 / 1);

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    ImportGoogleCSV("Assets/Demo/CsvTemps/" + sheetName, www.downloadHandler.text);
                }
            }
            EditorUtility.ClearProgressBar();
        }
        public IEnumerator DownloadAllCsv()
        {
            int sheetsCount = 0;

            WWWForm form = new WWWForm();
            form.AddField("method1", "getSheetsCount");
            using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    int.TryParse(www.downloadHandler.text, out sheetsCount);
                    Debug.Log("getSheetsCount: " + sheetsCount);
                }
            }

            for (int i = 0; i < sheetsCount; i++)
            {
                string sheetName = "";
                form = new WWWForm();
                form.AddField("page", i);
                form.AddField("method2", "getSheetName");

                using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
                {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        sheetName = www.downloadHandler.text;
                        Debug.Log("sheetName: " + sheetName);
                    }
                }

                form = new WWWForm();
                form.AddField("page", i);
                form.AddField("method3", "readSheet");
                using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
                {
                    yield return www.SendWebRequest();
                    EditorUtility.DisplayProgressBar("更csvい", "锣传糶csvい...", i / sheetsCount);

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        ImportGoogleCSV("Assets/Demo/CsvTemps/" + sheetName, www.downloadHandler.text);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
        private void ImportGoogleCSV(string _path, string _text)
        {
            Debug.Log(_text);
            if (!File.Exists(_path + ".csv"))
            {
                var stream = File.CreateText(_path + ".csv");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                stream.Close();
            }

            File.WriteAllText(_path + ".csv", _text);
            AssetDatabase.Refresh();
        }
    }
}