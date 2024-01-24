using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.IO;
using UnityEngine.Networking;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

namespace Utage.Extension.Custom {
    [InitializeOnLoad]
    public class GoogleExcelDownloader : OdinEditorWindow
    {
        const string DIALOGUE_EXCEL_PATH = "Assets/Utage/HomuraHime/HomuraHime.xls";
        const string DIALOGUE_CSV_PATH = "Assets/Utage/HomuraHime/CsvTemp";
        const string DIALOGUE_URL = "https://script.google.com/macros/s/AKfycbw6Hc8hJ4FZbqs_cKCyapsiqmkPmin2kNDfYGH9n2RgGXsIzJT_5N1pFhhp8KwBVXhE7w/exec";
        static readonly string[] DIALOGUE_IGNORE_TAG = { "總表" };

        const string LOCALIZE_EXCEL_PATH = "Assets/3.ProfileData/Localization/Excel/HH_UnityLocalizationAsset.xls";
        const string LOCALIZE_CSV_PATH = "Assets/3.ProfileData/Localization/Excel/CsvTemp";
        const string LOCALIZE_URL = "https://script.google.com/macros/s/AKfycbwCmQhKPN_PkW2dbaMhqFhqxSYL3kOtU0GB7eD6aMoTKatn9I6GzR0d-7IT6DlwMJIhzg/exec";
        static readonly string[] LOCALIZE_IGNORE_TAG = { };


        public static List<string> SHEETS;

        public enum Type
        {
            Dialogue,
            Localize
        }
        [BoxGroup("Import&Export 設置")]
        [OnValueChanged("InfoInit")]
        [SerializeField] Type type = Type.Dialogue;

        [BoxGroup("Import&Export 設置")]
        [OnValueChanged("InfoInit")]
        [SerializeField] Object m_TargetExcel = null;
        private bool targetNull => m_TargetExcel == null;
        [BoxGroup("Import&Export 設置")]
        [HideIf("targetNull")] [SerializeField] string m_ExcelPath;
        [BoxGroup("Import&Export 設置")]
        [HideIf("targetNull")] [SerializeField] string m_CsvPath;
        [BoxGroup("Import&Export 設置")]
        [HideIf("targetNull")] [SerializeField] string m_URL;
        [BoxGroup("Import&Export 設置")]
        [HideIf("targetNull")] [SerializeField] string[] m_ignoreTag;
        [BoxGroup("Import&Export 設置")]
        [HideIf("targetNull")] [SerializeField] string[] m_SheetNames;
        static GoogleExcelDownloader() 
        {
            EditorApplication.delayCall += CheckForUpdate;
            EditorApplication.quitting += OnQuitting;
        }

        private static void OnQuitting()
        {
            if (EditorPrefs.HasKey("GASUpdateCheck"))
                EditorPrefs.SetBool("GASUpdateCheck", false);
        }

        private static void CheckForUpdate()
        {
            if (EditorPrefs.HasKey("GASUpdateCheck") && EditorPrefs.GetBool("GASUpdateCheck"))
                return;
            
            bool shouldUpdate = EditorUtility.DisplayDialog("GAS更新提醒", "是否要更新Google上相關excel?", "是", "否");
            if (shouldUpdate)
                OpenWindow();
            EditorPrefs.SetBool("GASUpdateCheck", true);
        }

        [MenuItem("Tools/Google App/ExcelDownloader")]
        private static void OpenWindow()
        {
            GetWindow<GoogleExcelDownloader>().Show();
        }
        protected override void Initialize()
        {
            base.Initialize();
            InfoInit();
        }
        private void InfoInit()
        {
            if (type == Type.Dialogue)
            {
                m_ExcelPath = DIALOGUE_EXCEL_PATH;
                m_CsvPath = DIALOGUE_CSV_PATH;
                m_URL = DIALOGUE_URL;
                m_ignoreTag = DIALOGUE_IGNORE_TAG;
            }
            else
            {
                m_ExcelPath = LOCALIZE_EXCEL_PATH;
                m_CsvPath = LOCALIZE_CSV_PATH;
                m_URL = LOCALIZE_URL;
                m_ignoreTag = LOCALIZE_IGNORE_TAG;
            }
            m_TargetExcel = AssetDatabase.LoadAssetAtPath(m_ExcelPath, typeof(UnityEngine.Object));

            if (m_TargetExcel != null)
            {
                GetAndCreateFilePath();
                GetSheetNames();
                SHEETS = m_SheetNames.ToList();
                Target = SHEETS.FirstOrDefault().ToString();
            }
        }

        [HideIf("targetNull")]
        [FoldoutGroup("Test")]
        [ListToPopup(typeof(GoogleExcelDownloader), "SHEETS")]
        [SerializeField] string Target;

        [HideIf("targetNull")]
        [FoldoutGroup("Test")]
        [Sirenix.OdinInspector.Button]
        public void Download_Sheet()
        {
            EditorCoroutineUtility.StartCoroutine(DownlCertionCsv_Coroutine(Target), this);
        }
        [FoldoutGroup("Test")]
        [Sirenix.OdinInspector.Button]
        public void Download_All()
        {
            EditorCoroutineUtility.StartCoroutine(DownloadAllCsv_Coroutine(), this);
        }
        [HideIf("targetNull")]
        [FoldoutGroup("Test")]
        [Sirenix.OdinInspector.Button]
        public void CheckUpdate_Sheet()
        {
            EditorCoroutineUtility.StartCoroutine(CheckUpdateInfo_Sheet(Target), this);

        }
        [HideIf("targetNull")]
        [FoldoutGroup("Test")]
        [Sirenix.OdinInspector.Button]
        public void Import_Sheet()
        {
            Debug.Log("LoadCsvToExcel: " + Target);
            string extension = Path.GetExtension(m_ExcelPath);
            IWorkbook book = null;
            if (extension == ".xlsx")
                book = new XSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));
            else if (extension == ".xls")
                book = new HSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));

            var files = Directory.EnumerateFiles(m_CsvPath + "/", "*.csv");
            for (int i = 0; i < files.Count(); i++)
            {
                string file = files.ElementAtOrDefault(i);
                string sheetName = file.Split('/').Last().Replace(".csv", "");
                if (sheetName != Target)
                    continue;

                ISheet sheet = book.GetSheet(sheetName);
                Debug.Log(Target);
                for (int j = 0; j < book.NumberOfNames; j++)
                {
                    Debug.Log(book.GetSheetAt(j).SheetName);
                }

                ReadCsvStringContentToSheet(file, ref sheet);
            }
            using (FileStream fs = new FileStream(m_ExcelPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
                book.Close();
            }
            AssetDatabase.Refresh();
        }
        private string[] LocalCsvNames()
        {
            List<string> result = new List<string>();
            var files = Directory.EnumerateFiles(m_CsvPath + "/", "*.csv");
            for (int i = 0; i < files.Count(); i++)
            {
                string file = files.ElementAtOrDefault(i);
                string sheetName = file.Split('/').Last().Replace(".csv", "");
                result.Add(sheetName);
            }
            return result.ToArray();
        }
        public class ArrayDifferences<T>
        {
            public List<T> InAOnly { get; set; }
            public List<T> InBOnly { get; set; }
        }

        public static ArrayDifferences<T> GetArrayDifferences<T>(T[] arrayA, T[] arrayB)
        {
            ArrayDifferences<T> result = new ArrayDifferences<T>
            {
                InAOnly = new List<T>(),
                InBOnly = new List<T>()
            };

            HashSet<T> setB = new HashSet<T>(arrayB);

            foreach (T element in arrayA)
            {
                if (!setB.Contains(element))
                    result.InBOnly.Add(element);
            }
            HashSet<T> setA = new HashSet<T>(arrayA);

            foreach (var element in arrayB)
            {
                if (!setA.Contains(element))
                    result.InAOnly.Add(element);
            }

            return result;
        }
        [HideIf("targetNull")]
        [BoxGroup("Method")]
        [Sirenix.OdinInspector.Button]
        private void CheckUpdate()
        {
            EditorCoroutineUtility.StartCoroutine(CheckUpdateInfo(), this);
        }
        [DisableIf("hideNeedUpdate")]
        [BoxGroup("Method")]
        [Sirenix.OdinInspector.Button]
        private void DownloadModified()
        {
            EditorCoroutineUtility.StartCoroutine(DownloadModifiedCsv_Coroutine(sheetNeedUpdate.ToArray()), this);
        }
        [DisableIf("hideNeedUpdate")]
        [BoxGroup("Method")]
        [Sirenix.OdinInspector.Button]
        private void ImportModified()
        {
            string extension = Path.GetExtension(m_ExcelPath);
            IWorkbook book = null;
            if (extension == ".xlsx")
                book = new XSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));
            else if (extension == ".xls")
                book = new HSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));

            var files = Directory.EnumerateFiles(m_CsvPath + "/", "*.csv");
            for (int shtIndex = 0; shtIndex < sheetNeedUpdate.Count; shtIndex++)
            {
                for (int i = 0; i < files.Count(); i++)
                {
                    string file = files.ElementAtOrDefault(i);
                    string sheetName = file.Split('/').Last().Replace(".csv", "");
                    if (sheetName != sheetNeedUpdate[shtIndex])
                        continue;

                    ISheet sheet = book.GetSheet(sheetName);
                    if (sheet == null)
                        sheet = book.CreateSheet(sheetName);
                    Debug.Log(sheetName);

                    ReadCsvStringContentToSheet(file, ref sheet);
                }
                using (FileStream fs = new FileStream(m_ExcelPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    book.Write(fs);
                    book.Close();
                }
            }
           
            AssetDatabase.Refresh();
            if (type == Type.Localize)
            {
                if (CustomLocalizationTableIOEditor.ins == null)
                {
                    Debug.LogError("CustomLocalizationTableIOEditor.ins == null");
                }
                CustomLocalizationTableIOEditor.ins.Import();
            }
        }

        private bool hideNeedUpdate => (targetNull & sheetNeedUpdate != null & sheetNeedUpdate.Count > 0);
        [HideIf("hideNeedUpdate")]
        [BoxGroup("Method")]
        [SerializeField] List<string> sheetNeedUpdate;
        [HideIf("hideNeedUpdate")]
        [BoxGroup("Method")]
        [SerializeField] List<string> sheetNeedRemove;
        public IEnumerator CheckUpdateInfo()
        {
            int pageCount = 0;
            string[] sheetNames = null;
            sheetNeedUpdate = new List<string>();
            sheetNeedRemove = new List<string>();

            EditorUtility.DisplayProgressBar("檢查update", "初始化頁數資訊", 0.5f);
            WWWForm form = new WWWForm();
            form.AddField("method4", "getAllSheetNames");
            using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
            {
                yield return www.SendWebRequest();
                EditorUtility.ClearProgressBar();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    System.Func<string, bool> condition = s => !IsIgnoreSheet(s);
                    sheetNames = www.downloadHandler.text.Split('@');
                    var localSheets = LocalCsvNames();
                    var diffInfo = GetArrayDifferences<string>(localSheets, sheetNames.Where(condition).ToArray());
                    
                    //需要被移除的sheet
                    sheetNeedRemove.AddRange(diffInfo.InBOnly);
                    //需要新增的sheet
                    sheetNeedUpdate.AddRange(diffInfo.InAOnly);
                }
            }

            EditorUtility.DisplayProgressBar("檢查update", "初始化頁數資訊", 0.5f);
            form = new WWWForm();
            form.AddField("page", 1);
            form.AddField("searchName", "MinTest");
            form.AddField("method1", "getSheetsCount");
            using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
            {
                yield return www.SendWebRequest();
                EditorUtility.ClearProgressBar();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    pageCount = int.Parse(www.downloadHandler.text);
                    Debug.Log("count: " + pageCount);
                }
            }
            int index = 0;
            string sheetName = "";
            for (index = 0; index < pageCount; index++)
            {
                if (IsIgnoreSheet(sheetNames[index]))
                    continue;
                
                form = new WWWForm();
                form.AddField("page", index);
                form.AddField("searchName", sheetNames[index]);
                form.AddField("sheetContent", ReadCsvContent(sheetNames[index]));
                form.AddField("method4", "isSheetChange");
                var sheetDescription = (index == 0) ? "" : $"/已完成上一份{sheetName}";
                EditorUtility.DisplayProgressBar("檢查update", $"逐一檢查page{index}/{pageCount}" + sheetDescription, index / pageCount); ;
                using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
                {
                    yield return www.SendWebRequest();
                    EditorUtility.ClearProgressBar();
                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        string result = www.downloadHandler.text;
                        sheetName = result.Split('/').FirstOrDefault();
                        if (IsIgnoreSheet(sheetName))
                            continue;
                        if (result.Split('/').LastOrDefault() == "true")
                            sheetNeedUpdate.Add(sheetName);
                        Debug.Log(result);
                    }
                }
            }
            //DeleteSheet(sheetNeedRemove.ToArray());
        }

        private string ReadCsvContent(string _fileName)
        {
            string result = "";
            var files = Directory.EnumerateFiles(m_CsvPath + "/", "*.csv");
            for (int i = 0; i < files.Count(); i++)
            {
                string file = files.ElementAtOrDefault(i);
                string sheetName = file.Split('/').Last().Replace(".csv", "");

                if (sheetName == _fileName)
                    return File.ReadAllText(file);
            }
            return result;
        }
        public IEnumerator CheckUpdateInfo_Sheet(string _sheetName)
        {
            var files = Directory.EnumerateFiles(m_CsvPath + "/", "*.csv");
            string content = "";
            for (int i = 0; i < files.Count(); i++)
            {
                string file = files.ElementAtOrDefault(i);
                string sheetName = file.Split('/').Last().Replace(".csv", "");

                if (sheetName == _sheetName)
                {
                    content = File.ReadAllText(file);
                    break;
                }
            }
            if (!string.IsNullOrEmpty(content))
            {
                WWWForm form = new WWWForm();
                form.AddField("searchName", _sheetName);
                form.AddField("sheetContent", content);
                form.AddField("method4", "isSheetChange");
                EditorUtility.DisplayProgressBar("檢查update", $"逐一檢查page {_sheetName}", 0.5f);
                using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
                {
                    yield return www.SendWebRequest();
                    EditorUtility.ClearProgressBar();
                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        string result = www.downloadHandler.text;
                        Debug.Log($"{_sheetName} changed result: " + result);
                    }
                }
            }
        }
        private void GetSheetNames()
        {
            string extension = Path.GetExtension(m_ExcelPath);
            IWorkbook book = null;
            List<string> sheetNames = new List<string>();
            if (extension == ".xlsx")
                book = new XSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));
            else if(extension == ".xls")
                book = new HSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));

            int sheetCount = book.NumberOfSheets;
            for (int i = 0; i < sheetCount; i++)
            {
                ISheet sheet = book.GetSheetAt(i);
                sheetNames.Add(sheet.SheetName);
            }

            m_SheetNames = sheetNames.ToArray();
        }
        private void GetAndCreateFilePath()
        {
            if (m_TargetExcel == null)
            {
                Debug.LogError("Target excel is empty. Can't get asset path");
                return;
            }
            m_ExcelPath = AssetDatabase.GetAssetPath(m_TargetExcel);
            string fileName = m_TargetExcel.name + ".xls";
            string pathTemp = m_ExcelPath.Remove(m_ExcelPath.Length - fileName.Length - 1) ;

            if (!Directory.Exists(pathTemp + "/CsvTemp"))
            {
                Debug.Log("'CsvTemp' folder not exist");
                string guid = AssetDatabase.CreateFolder(pathTemp, "CsvTemp");
                m_CsvPath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"Create folder [CsvTemp] at {m_CsvPath}.");
            }
            else
            {
                Debug.Log("'CsvTemp' folder exist");
                m_CsvPath = pathTemp + "/CsvTemp";
            }
        }
        [FoldoutGroup("Test")]
        [Sirenix.OdinInspector.Button]
        public void LoadAllCsvToExcel()
        {
            Debug.Log("LoadAllCsvToExcel: " + Utage.ExcelParser.IsExcelFile(m_ExcelPath));
            
            IWorkbook book = new HSSFWorkbook();
            var files = Directory.EnumerateFiles(m_CsvPath + "/", "*.csv");
            for (int i = 0; i < files.Count(); i++)
            {
                string file = files.ElementAtOrDefault(i);
                string sheetName = file.Split('/').Last().Replace(".csv", "");
                ISheet sheet = book.CreateSheet(sheetName);
                ReadCsvStringContentToSheet(file, ref sheet);
            }
            using (FileStream fs = new FileStream(m_ExcelPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
                book.Close();
            }
            AssetDatabase.Refresh();
        }

        IEnumerator DownloadAllCsv_Coroutine()
        {
            int sheetsCount = 0;

            WWWForm form = new WWWForm();
            form.AddField("method1", "getSheetsCount");
            using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
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

                using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
                {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        sheetName = www.downloadHandler.text;
                    }
                }
                if (IsIgnoreSheet(sheetName))
                {
                    Debug.Log("ignore: " + sheetName);
                    continue;
                }
                Debug.Log("sheetName: " + sheetName);

                form = new WWWForm();
                form.AddField("page", i);
                form.AddField("method3", "readSheet");
                using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
                {
                    yield return www.SendWebRequest();
                    EditorUtility.DisplayProgressBar("下載csv中", "轉換寫入csv中...", i / sheetsCount);

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        ImportGoogleCSV(m_CsvPath + "/" + sheetName, www.downloadHandler.text);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }

        private bool IsIgnoreSheet(string sheetName)
        {
            bool ignore = false;
            for (int j = 0; j < m_ignoreTag.Length; j++)
            {
                if (sheetName.Contains(m_ignoreTag[j]))
                {
                    ignore = true;
                    break;
                }
            }

            return ignore;
        }

        IEnumerator DownloadModifiedCsv_Coroutine(string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                EditorUtility.DisplayProgressBar("下載Sheet", $"逐一下載{i}", (float)i/ (float)names.Length);
                yield return DownlCertionCsv_Coroutine(names[i]);
            }
            EditorUtility.ClearProgressBar();
        }
        IEnumerator DownlCertionCsv_Coroutine(string _name)
        {
            string sheetName = "";
            WWWForm form = new WWWForm();
            form.AddField("searchName", _name);
            form.AddField("method2", "getSheetName");

            using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
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
            using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
            {
                yield return www.SendWebRequest();
                EditorUtility.DisplayProgressBar("下載csv", "轉換寫入csv中...", 1 / 1);

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    ImportGoogleCSV(m_CsvPath + "/" + sheetName, www.downloadHandler.text);
                }
            }
            EditorUtility.ClearProgressBar();
        }
        private void ImportGoogleCSV(string _path, string _text)
        {
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
        private void ReadCsvStringContentToSheet(string _path, ref ISheet _sheet)
        {
            for (int i = _sheet.FirstRowNum; i <= _sheet.LastRowNum; i++)
            {
                IRow row = _sheet.GetRow(i);
                if (row != null) _sheet.RemoveRow(row);
            }

            string txt = File.ReadAllText(_path);
            List<List<string>> data = JsonConvert.DeserializeObject<List<List<string>>>(txt);
            for (int i = 0; i < data.Count; i++)
            {
                IRow iRow = _sheet.GetRow(i);
                if (iRow == null)
                    iRow = _sheet.CreateRow(i);

                for (int j = 0; j < data[i].Count; j++)
                {
                    ICell iCell = iRow.GetCell(j);
                    if (iCell == null)
                        iCell = iRow.CreateCell(j);
                    
                    iCell.SetCellValue(data[i][j]);
                }
            }
        }
        [HideIf("hideNeedUpdate")]
        [BoxGroup("Method")]
        [Sirenix.OdinInspector.Button]
        public void DeleteNeedRemove()
        {
            string extension = Path.GetExtension(m_ExcelPath);
            IWorkbook book = null;
            if (extension == ".xlsx")
                book = new XSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));
            else if (extension == ".xls")
                book = new HSSFWorkbook(new FileStream(m_ExcelPath, FileMode.Open, FileAccess.Read));
            List<ISheet> sheets = new List<ISheet>();
            for (int i = 0; i < sheetNeedRemove.Count; i++)
            {
                ISheet sht = book.GetSheet(sheetNeedRemove[i]);
                if (sht == null)
                    continue;
                sheets.Add(sht);
            }

            IWorkbook newBook = CopyWorkbookExceptSheet(book, sheets.ToArray());

            book.Close();

            using (FileStream newFs = new FileStream(m_ExcelPath, FileMode.Create, FileAccess.Write))
            {
                newBook.Write(newFs);
                newFs.Close();
            }
            for (int i = 0; i < sheetNeedRemove.Count; i++)
            {
                File.Delete(m_CsvPath + "/" + sheetNeedRemove[i] + ".csv");
            }
        }
        private IWorkbook CopyWorkbookExceptSheet(IWorkbook sourceBook, ISheet[] sheets)
        {
            IWorkbook newBook;
            if (sourceBook is XSSFWorkbook)
                newBook = new XSSFWorkbook();
            else
                newBook = new HSSFWorkbook();
            var toDeleteNames = sheets.Select(s => s.SheetName).ToArray();

            for (int i = 0; i < sourceBook.NumberOfSheets; i++)
            {
                string sheetName = sourceBook.GetSheetName(i);
                if (!toDeleteNames.Contains(sheetName))
                {
                    ISheet sourceSheet = sourceBook.GetSheetAt(i);
                    ISheet newSheet = newBook.CreateSheet(sheetName);

                    // 複製行
                    for (int j = 0; j <= sourceSheet.LastRowNum; j++)
                    {
                        IRow sourceRow = sourceSheet.GetRow(j);
                        IRow newRow = newSheet.CreateRow(j);

                        // 複製單元格
                        if (sourceRow != null)
                        {
                            foreach (var cell in sourceRow.Cells)
                            {
                                ICell newCell = newRow.CreateCell(cell.ColumnIndex, cell.CellType);

                                switch (cell.CellType)
                                {
                                    case CellType.Numeric:
                                        newCell.SetCellValue(cell.NumericCellValue);
                                        break;
                                    case CellType.String:
                                        newCell.SetCellValue(cell.StringCellValue);
                                        break;
                                    case CellType.Formula:
                                        newCell.SetCellFormula(cell.CellFormula);
                                        break;
                                    // 其他類型的 Cell 需要根據實際需求處理
                                    // ...

                                    default:
                                        newCell.SetCellValue(cell.StringCellValue);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return newBook;
        }
    }       
}
    //private ExcelWorksheet GetExcelSheetFromBook(string _sheetName, ref ExcelWorksheets _sheets)
    //{
    //    var selectSheet = from sheet in _sheets
    //                      where sheet.Name == _sheetName
    //                      select sheet;
    //    ExcelWorksheet worksheet = selectSheet.FirstOrDefault();
    //    if (worksheet == null)
    //    {
    //        Debug.Log("Create new sheet" + _sheetName);
    //        worksheet = _sheets.Add(_sheetName);
    //    }
    //    else
    //    {
    //        Debug.Log("Already has sheet " + _sheetName);
    //    }

    //    return worksheet;
    //}
    //private void ReadCsvStringContentToSheet(string _path, ref ExcelWorksheet _sheet)
    //{
    //    string txt = File.ReadAllText(_path);
    //    string[] row = txt.Split('@');
    //    for (int i = 0; i < row.Length; i++)
    //    {
    //        string[] column = row[i].Split('^');
    //        for (int j = 0; j < column.Length; j++)
    //        {
    //            int r = i + 1;
    //            int c = j + 1;
    //            _sheet.Cells[r, c].Value = column[j];
    //        }
    //    }
    //    Debug.Log($"Load csv completed. Load elements total count : {_sheet.Cells.Count()}");
    //}