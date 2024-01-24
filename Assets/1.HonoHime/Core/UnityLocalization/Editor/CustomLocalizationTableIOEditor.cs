using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Tables;
using Sirenix.OdinInspector;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

using UnityEditor.Localization;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System.Text.RegularExpressions;
using System;

public class CustomLocalizationTableIOEditor : OdinEditorWindow
{
    public static CustomLocalizationTableIOEditor ins;

    [BoxGroup("Import&Export 設置")]
    public UnityEngine.Object collectionFolder;
    [BoxGroup("Import&Export 設置")]
    [ReadOnly]
    public string collectionFolderPath = "Assets/3.ProfileData/Localization/Collection";
    [BoxGroup("Import&Export 設置")]
    public UnityEngine.Object assetFolder;
    [BoxGroup("Import&Export 設置")]
    public UnityEngine.Object excelAsset;
    [BoxGroup("Import&Export 設置")]
    [ReadOnly]
    public string filePath = "Assets/3.ProfileData/Localization/Excel/HH_UnityLocalizationAsset.xls";
    [BoxGroup("Import&Export 設置")]
    public bool overwriteCreateExcel = true;

    [BoxGroup("Import&Export 設置")]
    public List<StringTableCollection> collections = new List<StringTableCollection>();

    public void Init()
    {
        excelAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object));
        assetFolder = AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(filePath), typeof(UnityEngine.Object));
        collectionFolder = AssetDatabase.LoadAssetAtPath(collectionFolderPath, typeof(UnityEngine.Object));
    }

    #region EditorWindow

    [MenuItem("Tools/CustomLocalizationTableIOEditor")]
    public static void Open()
    {
        if(CustomLocalizationTableIOEditor.ins== null)
        {
            CustomLocalizationTableIOEditor.ins = new CustomLocalizationTableIOEditor();
            CustomLocalizationTableIOEditor.ins.Init();
        }

        CustomLocalizationTableIOEditor.ins.GetAllCollections();
        CustomLocalizationTableIOEditor.ins.Show();
    }

    #endregion

    #region Public

    [BoxGroup("Method")]
    [Button]
    public void CreateAndExportAll()
    {
        GetAllCollections();
        CreateExcelData();

        for (int i = 0; i < collections.Count; i++)
        {
            Export(collections[i]);
        }
    }

    public void Export(StringTableCollection collection)
    {
        if (excelAsset == null)
        {
            Debug.Log("Please Create ExcelAsset First.");
        }

        string existingFilePath = AssetDatabase.GetAssetPath(excelAsset);

        // 讀取現有的 Excel 檔案
        using (FileStream fs = new FileStream(existingFilePath, FileMode.Open, FileAccess.ReadWrite))
        {
            string extension = Path.GetExtension(filePath);
            IWorkbook workbook = null;
            if (extension == ".xlsx")
                workbook = new XSSFWorkbook(fs);
            else if (extension == ".xls")
                workbook = new HSSFWorkbook(fs);

            ISheet sheet = workbook.GetSheet(collection.TableCollectionName);
            sheet = sheet ?? workbook.CreateSheet(collection.TableCollectionName);

            //Clear Data
            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row != null)
                {
                    for (int colIndex = 0; colIndex < row.LastCellNum; colIndex++)
                    {
                        ICell cell = row.GetCell(colIndex);
                        if (cell != null)
                        {
                            cell.SetCellValue(string.Empty);
                        }
                    }
                }
            }

            // Create headers
            IRow headerRow = sheet.GetRow(0);
            headerRow = headerRow ?? sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Key");
            headerRow.CreateCell(1).SetCellValue("Key Id");

            int headerRowElement = 2;
            foreach (var table in collection.StringTables)
            {
                headerRow.CreateCell(headerRowElement).SetCellValue(table.LocaleIdentifier.Code);
                headerRowElement++;
            }


            // Create data rows
            int dataRowCount = 1; //目標Key的Row

            foreach (var row in collection.GetRowEnumerator())
            {
                // Key and Key Id
                IRow dataRow = sheet.GetRow(dataRowCount);
                dataRow = dataRow ?? sheet.CreateRow(dataRowCount);

                dataRow.CreateCell(0).SetCellValue(row.KeyEntry.Key);
                dataRow.CreateCell(1).SetCellValue(row.KeyEntry.Id);

                int dataRowElementCount = 2; //該Key各個Localization 的colume
                                             //每個 Localization
                foreach (var tableEntry in row.TableEntries)
                {
                    dataRow.CreateCell(dataRowElementCount).SetCellValue(tableEntry == null ? string.Empty : tableEntry.Value);
                    dataRowElementCount++;
                }

                dataRowCount++;
            }

            // 寫回修改後的內容到檔案
            using (FileStream fsWrite = new FileStream(existingFilePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fsWrite);
            }

        }

        AssetDatabase.Refresh();
        Debug.Log("Export StringTable : " + collection.TableCollectionName);
    }

    [BoxGroup("Method")]
    [Button]
    public void Import()
    {
        Debug.Log("Import");
        
        if (excelAsset == null)
        {
            excelAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object));
            if (excelAsset == null)
            {
                Debug.Log("ExcelAsset is null.");
                return;
            }
        }

        string existingFilePath = AssetDatabase.GetAssetPath(excelAsset);
        List<string> excelCollectionName = new List<string>();

        // 讀取 Excel 內的檔案
        using (FileStream fs = new FileStream(existingFilePath, FileMode.Open, FileAccess.ReadWrite))
        {
            string extension = Path.GetExtension(filePath);
            IWorkbook workbook = null;
            if (extension == ".xlsx")
                workbook = new XSSFWorkbook(fs);
            else if (extension == ".xls")
                workbook = new HSSFWorkbook(fs);

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                //判斷有沒有目標sheet
                ISheet targetSht = workbook.GetSheetAt(i);
                var stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(targetSht.SheetName);
                if (stringTableCollection == null)
                {
                    stringTableCollection = CreateStringTableCollection(targetSht.SheetName);
                    Debug.Log("No collection named :" + targetSht.SheetName + " create one. " + stringTableCollection.TableCollectionName);
                }

                //保留Collection
                excelCollectionName.Add(stringTableCollection.TableCollectionName);

                //抓取head，判斷當前有的語系
                List<string> localeList = new List<string>();
                IRow headRow = targetSht.GetRow(0); //head
                                                    //cellIndex = 0 是key
                                                    //cellIndex = 1 是key id
                for (int cellIndex = 2; cellIndex < headRow.LastCellNum; cellIndex++)
                {
                    ICell cell = headRow.GetCell(cellIndex);
                    if (cell != null)
                    {
                        localeList.Add(cell.ToString());
                    }
                }

                //從Excel更新每個Entry
                List<string> entryKeyList = new List<string>();
                for (int rowIndex = 1; rowIndex <= targetSht.LastRowNum; rowIndex++)
                {
                    IRow dataRow = targetSht.GetRow(rowIndex);
                    if (dataRow == null) continue;

                    ICell keyCell = dataRow.GetCell(0); //KEY
                    var cellString = keyCell.ToString();

                    if (keyCell == null) continue; //如果KEY是空白就跳過
                    if (string.IsNullOrEmpty(keyCell.ToString())) continue;
                    entryKeyList.Add(keyCell.ToString());

                    ICell keyIDCell = dataRow.GetCell(1); //KEY ID
                    long keyID = -1;
                    //如果KEY ID是空白就新增一個
                    if (keyIDCell == null || string.IsNullOrWhiteSpace(keyIDCell.ToString()))
                    {
                        keyID = stringTableCollection.SharedData.KeyGenerator.GetNextKey();
                        keyIDCell = dataRow.CreateCell(1);
                        keyIDCell.SetCellValue(keyID.ToString());

                        //Debug.Log("Add new ID :" + keyID);
                    }
                    else
                    {
                        Debug.Log("Use Exist ID :" + keyIDCell);
                        keyID = (long)Convert.ToDouble(keyIDCell.ToString());
                    }

                    for (int k = 0; k < localeList.Count; k++)
                    {
                        SetEntry(stringTableCollection, keyCell.ToString(), keyID, localeList[k], dataRow.GetCell(k + 2).ToString());
                    }
                }

                //移除Excel中不存在的Entry
                foreach (var row in stringTableCollection.GetRowEnumerator())
                {
                    string keyToRemove = row.KeyEntry.Key;

                    if (!entryKeyList.Contains(keyToRemove))
                    {
                        RemoveEntry(stringTableCollection, keyToRemove);
                    }
                }


                // 寫回修改後的內容到檔案
                using (FileStream fsWrite = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fsWrite);
                }

                Debug.Log(stringTableCollection.TableCollectionName + " Import complete.");
                EditorUtility.SetDirty(stringTableCollection);
            }
        }

        // 移除 Excel沒有的 Collection
        List<string> collectionToDelect = new List<string>();
        foreach (var stringTableCollection in LocalizationEditorSettings.GetStringTableCollections())
        {
            if (!excelCollectionName.Contains(stringTableCollection.TableCollectionName))
                collectionToDelect.Add(stringTableCollection.TableCollectionName);
        }

        for (int i = 0; i < collectionToDelect.Count; i++)
        {
            DeleteStringTableCollection(collectionToDelect[i]);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Localization assets saved successfully.");
    }


    #endregion

    #region Private

    private void GetAllCollections()
    {
        collections.Clear();
        foreach (StringTableCollection collection in LocalizationEditorSettings.GetStringTableCollections())
        {
            collections.Add(collection);
        }
    }

    private void CreateExcelData()
    {
        //如果已有指定Excel，就直接操作該檔案
        if (excelAsset != null)
        {
            filePath = AssetDatabase.GetAssetPath(excelAsset);
            Debug.Log("Excel file exist: " + filePath);
            return;
        }


        //沒有就新增一個
        string assetFolderPath = AssetDatabase.GetAssetPath(assetFolder);
        if (!string.IsNullOrEmpty(assetFolderPath))
        {
            string targetFilePath = assetFolderPath + "/HH_UnityLocalizationAsset.xls";
            filePath = targetFilePath;

            if (!overwriteCreateExcel)
            {
                int fileNumber = 1;

                while (File.Exists(filePath))
                {
                    string exten_str = Path.GetExtension(targetFilePath);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(targetFilePath);

                    string pattern = @"\((\d+)\)$";
                    Match match = Regex.Match(fileNameWithoutExtension, pattern);

                    if (match.Success)
                    {
                        int existingNumber = int.Parse(match.Groups[1].Value);
                        fileNameWithoutExtension = fileNameWithoutExtension.Replace(match.Value, "") + "(" + (existingNumber + 1) + ")";
                    }
                    else
                    {
                        fileNameWithoutExtension += "(" + fileNumber + ")";
                    }

                    filePath = assetFolderPath + "/" + fileNameWithoutExtension + exten_str;
                    fileNumber++;
                }
            }

            Debug.Log("Localization File Path: " + filePath);
        }


        IWorkbook workbook = null;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        string extension = Path.GetExtension(filePath);
        if (extension == ".xlsx")
            workbook = new XSSFWorkbook();
        else if (extension == ".xls")
            workbook = new HSSFWorkbook();
#else
        workbook = new HSSFWorkbook(); // XLS format (Excel 2003 and earlier)
#endif

        // Save the workbook to a file
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(fs);
        }

        AssetDatabase.Refresh();

        excelAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object));
        EditorUtility.SetDirty(this);
    }



    [Button]
    private StringTableCollection CreateStringTableCollection(string collectionName)
    {
        // Create a new StringTableCollection
        StringTableCollection newCollection = LocalizationEditorSettings.CreateStringTableCollection(collectionName, collectionFolderPath);

        // Save changes
        EditorUtility.SetDirty(newCollection);

        return newCollection;
    }

    [Button]
    private void DeleteStringTableCollection(string collectionName)
    {
        StringTableCollection collectionToDelete = LocalizationEditorSettings.GetStringTableCollection(collectionName);


        if (collectionToDelete == null)
        {
            Debug.LogError("StringTableCollection to delete is not assigned!");
            return;
        }

        // Delete each StringTable asset and SharedTableData asset associated with the StringTableCollection
        foreach (var table in collectionToDelete.StringTables)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(table));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(table.SharedData));
        }

        // Delete the StringTableCollection asset
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(collectionToDelete));

        // Refresh the AssetDatabase to reflect the changes
        AssetDatabase.Refresh();

        Debug.Log("DeleteStringTableCollection : " + collectionName);
    }



    private void AddEntry(StringTableCollection collection, string _key, long _keyID)
    {



        var newKey = collection.SharedData.AddKey(_key, _keyID);

        foreach (var table in collection.StringTables)
        {
            var stringTable = collection.GetTable(table.LocaleIdentifier) as StringTable;
            stringTable.AddEntry(newKey.Id, string.Empty);

            EditorUtility.SetDirty(stringTable);
        }

        EditorUtility.SetDirty(collection.SharedData);
        EditorUtility.SetDirty(collection);
    }

    private void RemoveEntry(StringTableCollection collection, string _key)
    {
        foreach (var table in collection.StringTables)
        {
            var stringTable = collection.GetTable(table.LocaleIdentifier) as StringTable;
            stringTable.RemoveEntry(_key);

            EditorUtility.SetDirty(stringTable);
        }

        collection.SharedData.RemoveKey(_key);

        // Save changes
        EditorUtility.SetDirty(collection);
        EditorUtility.SetDirty(collection.SharedData); // Mark SharedTableData of the collection as dirty

    }


    private void SetEntry(StringTableCollection collection, string _key, long _keyID, string _localeIdentifier, string value)
    {
        var stringTable = collection.GetTable(_localeIdentifier) as StringTable;

        //必須先以Key搜尋，
        StringTableEntry stringTableEntry = stringTable.GetEntry(_key);

        //找不到key
        if (stringTableEntry == null)
        {
            stringTableEntry = stringTable.GetEntry(_keyID);

            //判斷是不是key改名字
            if (stringTableEntry == null)
            {
                //也沒有改名字，那就新增
                AddEntry(collection, _key, _keyID);
                stringTableEntry = stringTable.GetEntry(_key);
            }
            else
            {
                stringTableEntry.Key = _key;
            }
        }

        stringTableEntry.Value = value;

        EditorUtility.SetDirty(stringTable);
        EditorUtility.SetDirty(collection);
    }

    


    //[MenuItem("CONTEXT/StringTableCollection/Print CSV")]
    //public static void CreateCSV(MenuCommand command)
    private void CreateCSVSample(StringTableCollection collection)
    {
        //var collection = command.context as StringTableCollection;

        StringBuilder sb = new StringBuilder();

        // Header
        sb.Append("Key,");
        foreach (var table in collection.StringTables)
        {
            //語系
            Debug.Log("LocaleIdentifier : " + table.LocaleIdentifier);
            sb.Append(table.LocaleIdentifier);
            sb.Append(",");
        }
        sb.Append("\n");

        // Add each row
        foreach (var row in collection.GetRowEnumerator())
        {
            // Key 
            Debug.Log("KeyEntry.Key : " + row.KeyEntry.Key);
            sb.Append(row.KeyEntry.Key);
            sb.Append(",");

            //每個 Localization
            foreach (var tableEntry in row.TableEntries)
            {
                // The table entry will be null if no entry exists for this key
                Debug.Log("tableEntry : " + tableEntry == null ? string.Empty : tableEntry.Value);
                sb.Append(tableEntry == null ? string.Empty : tableEntry.Value);
                sb.Append(",");
            }
            sb.Append("\n");
        }

        // Print the contents. You could save it to a file here.
        Debug.Log(sb.ToString());
    }

    #endregion
}
