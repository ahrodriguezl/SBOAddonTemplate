using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Avantis.Login;
using SAPbouiCOM.Framework;

namespace Avantis.ORM
{
    public enum BoCreateTableResult
    {
        boctr_Created,
        boctr_Migrated,
        boctr_Error
    }

    public class SBOTable
    {
        public Type MappedType { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public SAPbobsCOM.BoUTBTableType Type { get; private set; }
        public BoTableSource TableSource { get; set; }

        public int UDOObjType { get; private set; }
        public SAPbobsCOM.BoYesNoEnum ManageSeries { get; set; }
        public SAPbobsCOM.BoYesNoEnum CanDelete { get; set; }
        public SAPbobsCOM.BoYesNoEnum CanClose { get; set; }
        public SAPbobsCOM.BoYesNoEnum CanCancel { get; set; }
        public SAPbobsCOM.BoYesNoEnum CanFind { get; set; }
        public SAPbobsCOM.BoYesNoEnum CanYearTransfer { get; set; }
        public SAPbobsCOM.BoYesNoEnum CanApprove { get; set; }
        public SAPbobsCOM.BoYesNoEnum CanLog { get; set; }

        public SBOTable[] Childs { get; private set; }
        public SBOIndex[] Indexes { get; private set; }
        public SBOField[] Fields { get; private set; }

        private Dictionary<string, List<string>> _indexes { get; set; }

        public SBOTable(Type type)
        {
            MappedType = type;

            var tableAttr = (TableNameAttribute)Orm.GetCustomAttribute<TableNameAttribute>(type);
            var descAttr = (DescriptionAttribute)Orm.GetCustomAttribute<DescriptionAttribute>(type);
            var typeAttr = (TableTypeAttribute)Orm.GetCustomAttribute<TableTypeAttribute>(type);
            bool tableAttrIsValid = (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Name));
            var indexListAttr = Orm.GetAllCustomAttributes<IndexAttribute>(type);

            this.Name = tableAttrIsValid ? tableAttr.Name : type.Name;
            this.Description = (descAttr != null && !string.IsNullOrEmpty(descAttr.Text)) ? descAttr.Text : MappedType.Name;
            this.Type = (typeAttr == null) ? SAPbobsCOM.BoUTBTableType.bott_NoObject : typeAttr.Value;
            this.TableSource = tableAttrIsValid ? tableAttr.Sourced : BoTableSource.btt_UDT;

            var udoObtTypeAttr = (ObjectTypeAttribute)Orm.GetCustomAttribute<ObjectTypeAttribute>(type);
            this.UDOObjType = -1;

            if (udoObtTypeAttr != null)
            {
                this.UDOObjType = (int)udoObtTypeAttr.Value;

                var mngSeriesAttr = (ManageSeriesAttribute)Orm.GetCustomAttribute<ManageSeriesAttribute>(type);
                var canDeleteAttr = (CanDeleteAttribute)Orm.GetCustomAttribute<CanDeleteAttribute>(type);
                var canCloseAttr = (CanCloseAttribute)Orm.GetCustomAttribute<CanCloseAttribute>(type);
                var canCancelAttr = (CanCancelAttribute)Orm.GetCustomAttribute<CanCancelAttribute>(type);
                var canFindAttr = (CanFindAttribute)Orm.GetCustomAttribute<CanFindAttribute>(type);
                var canYearTransfAttr = (CanYearTransferAttribute)Orm.GetCustomAttribute<CanYearTransferAttribute>(type);
                var canApproveAttr = (CanApproveAttribute)Orm.GetCustomAttribute<CanApproveAttribute>(type);
                var canLogAttr = (CanLogAttribute)Orm.GetCustomAttribute<CanLogAttribute>(type);

                if (mngSeriesAttr != null)
                    this.ManageSeries = mngSeriesAttr.Value;

                if (canDeleteAttr != null)
                    this.CanDelete = canDeleteAttr.Value;

                if (canCloseAttr != null)
                    this.CanClose = canCloseAttr.Value;

                if (canCancelAttr != null)
                    this.CanCancel = canCancelAttr.Value;

                if (canFindAttr != null)
                    this.CanFind = canFindAttr.Value;

                if (canYearTransfAttr != null)
                    this.CanYearTransfer = canYearTransfAttr.Value;

                if (canApproveAttr != null)
                    this.CanApprove = canApproveAttr.Value;

                if (canLogAttr != null)
                    this.CanLog = canLogAttr.Value;
            }

            var baseType = type;

            var props = new List<PropertyInfo>();
            var propNames = new HashSet<string>();

            while (baseType != typeof(object))
            {
                var typeInfo = baseType.GetTypeInfo();
                var newProps = (
                    from p in typeInfo.DeclaredProperties
                    where
                        !propNames.Contains(p.Name) &&
                        p.CanRead && p.CanWrite &&
                        (p.GetMethod != null) && (p.SetMethod != null) &&
                        (p.GetMethod.IsPublic && p.SetMethod.IsPublic) &&
                        (!p.GetMethod.IsStatic) && (!p.SetMethod.IsStatic)
                    select p).ToList();

                foreach (var p in newProps)
                {
                    propNames.Add(p.Name);
                }

                props.AddRange(newProps);

                baseType = typeInfo.BaseType;
            }

            var fields = new List<SBOField>();
            var childs = new List<SBOTable>();
            _indexes = new Dictionary<string, List<string>>();

            foreach (var p in props)
            {
                bool ignore = p.IsDefined(typeof(IgnoreAttribute), true);
                bool isChildTable = p.IsDefined(typeof(ChildTableAttribute), true);

                if (isChildTable)
                {
                    var tb = new SBOTable(p.PropertyType);
                    childs.Add(tb);
                }

                if (!ignore && !isChildTable)
                {
                    SBOField field = null;
                    var useIndex = p.IsDefined(typeof(UsedIndexAttribute), true);
                    var isForeignKey = p.IsDefined(typeof(ForeignKeyAttribute), true);

                    if (isForeignKey)
                    {
                        var frgKeyAttr = (ForeignKeyAttribute)Orm.GetCustomAttribute<ForeignKeyAttribute>(p);
                        field = frgKeyAttr.Field;
                    }
                    else
                    {
                        field = new SBOField(p, this.TableSource);
                    }

                    fields.Add(field);

                    if (useIndex)
                        GetIndexMapping(field);
                }
            }

            this.Fields = fields.ToArray();
            this.Childs = childs.ToArray();

            var indexes = new List<SBOIndex>();

            foreach (var index in indexListAttr)
            {
                List<string> map = new List<string>();

                if (!_indexes.TryGetValue(index.Name, out map))
                    throw new Exception(string.Format("El ínidice '{0}' no existe en la tabla {1}.", index.Name, this.Name));

                indexes.Add(new SBOIndex(index.Name, map.ToArray(), index.IsUnique));
            }

            this.Indexes = indexes.ToArray();
        }

        public SBOField FindColumnWithPropertyName(string propertyName)
        {
            var exact = Fields.FirstOrDefault(c => c.PropertyName == propertyName);
            return exact;
        }

        public SBOField FindColumn(string columnName)
        {
            var exact = Fields.FirstOrDefault(c => c.AliasID.ToLower() == columnName.ToLower());
            return exact;
        }

        private void GetIndexMapping(SBOField field)
        {
            lock (_indexes)
            {
                List<string> map;
                foreach (var index in field.Indexes)
                {
                    if (!this._indexes.TryGetValue(index, out map))
                    {
                        map = new List<string>();
                        this._indexes.Add(index, map);
                    }

                    if (!this._indexes[index].Contains(field.AliasID))
                        map.Add(field.AliasID);

                    this._indexes[index] = map;
                }
            }
        }

        public class SBOIndex
        {
            public string Name { get; private set; }
            public string[] Fields { get; private set; }
            public bool IsUnique { get; private set; }

            public SBOIndex(string Name, string[] Fields, bool IsUnique = false)
            {
                this.Name = Name;
                this.Fields = Fields;
                this.IsUnique = IsUnique;
            }
        }

        public class SBOField
        {
            public string PropertyName { get { return PropertyInfo.Name; } }
            public Type PropertyType { get; private set; }
            public PropertyInfo PropertyInfo { get; private set; }

            public string AliasID { get; private set; }
            public string Description { get; private set; }
            public SAPbobsCOM.BoFieldTypes Type { get; private set; }
            public SAPbobsCOM.BoFldSubTypes SubType { get; private set; }
            public int EditSize { get; private set; }

            public string DefaultValue { get; private set; }
            public Dictionary<string, string> ValidValues { get; private set; }

            public string[] Indexes { get; private set; }

            public SAPbobsCOM.BoYesNoEnum Mandatory { get; private set; }
            public bool CanFind { get; private set; }

            public string LinkedTable { get; private set; }
            public string LinkedUDO { get; private set; }
            public int LinkedSystemObject { get; private set; }

            public SBOField(PropertyInfo propertyInfo, BoTableSource sourced)
            {
                this.PropertyInfo = propertyInfo;
                this.PropertyType = propertyInfo.PropertyType;

                var aliasAttr = propertyInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(AliasIDAttribute));
                var descriptionAttr = propertyInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(DescriptionAttribute));
                var typeAttr = (FieldTypeAttribute)Orm.GetCustomAttribute<FieldTypeAttribute>(propertyInfo);
                var subTypeAttr = (FieldSubTypeAttribute)Orm.GetCustomAttribute<FieldSubTypeAttribute>(propertyInfo);
                var editSizeAttr = (EditSizeAttribute)Orm.GetCustomAttribute<EditSizeAttribute>(propertyInfo);
                var mandatoryAttr = (MandatoryAttribute)Orm.GetCustomAttribute<MandatoryAttribute>(propertyInfo);
                var canFindAttr = (FindFieldAttribute)Orm.GetCustomAttribute<FindFieldAttribute>(propertyInfo);
                var linkedTableAttr = (LinkedTableAttribute)Orm.GetCustomAttribute<LinkedTableAttribute>(propertyInfo);
                var linkedUdoAttr = (LinkedUDOAttribute)Orm.GetCustomAttribute<LinkedUDOAttribute>(propertyInfo);
                var linkedSysObjAttr = (LinkedSystemObjectAttribute)Orm.GetCustomAttribute<LinkedSystemObjectAttribute>(propertyInfo);
                var indexListAttr = Orm.GetAllCustomAttributes<UsedIndexAttribute>(propertyInfo);
                var validValuesAttrs = Orm.GetAllCustomAttributes<ValidValueAttribute>(propertyInfo);
                var defaultValueAttr = (DefaultValueAttribute)Orm.GetCustomAttribute<DefaultValueAttribute>(propertyInfo);

                if (linkedTableAttr != null)
                    this.LinkedTable = linkedTableAttr.Name;

                if (linkedUdoAttr != null)
                {
                    this.EditSize = editSizeAttr != null ? editSizeAttr.Value : 8;
                    this.LinkedUDO = linkedUdoAttr.Name;
                }

                this.LinkedSystemObject = -1;

                if (linkedSysObjAttr != null)
                    this.LinkedSystemObject = (int)linkedSysObjAttr.Value;

                this.AliasID = (aliasAttr != null && aliasAttr.ConstructorArguments.Count > 0) ? aliasAttr.ConstructorArguments[0].Value.ToString() : propertyInfo.Name;
                this.Description = (descriptionAttr != null && descriptionAttr.ConstructorArguments.Count > 0) ? descriptionAttr.ConstructorArguments[0].Value.ToString() : propertyInfo.Name;

                if (linkedTableAttr == null && linkedUdoAttr == null && linkedSysObjAttr == null)
                {
                    this.Type = typeAttr != null ? typeAttr.Value : Orm.GetFieldType(this.PropertyType);
                    this.SubType = subTypeAttr != null ? subTypeAttr.SubType : SAPbobsCOM.BoFldSubTypes.st_None;
                    this.EditSize = editSizeAttr != null ? editSizeAttr.Value : 8;
                    this.Mandatory = mandatoryAttr != null ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
                    this.CanFind = canFindAttr != null ? canFindAttr.Value : false;
                }

                this.DefaultValue = defaultValueAttr != null ? defaultValueAttr.Code : string.Empty;
                ValidValues = new Dictionary<string, string>();

                foreach (var attr in validValuesAttrs)
                {
                    string value = string.Empty;

                    if (!ValidValues.TryGetValue(attr.Code, out value))
                        ValidValues.Add(attr.Code, attr.Name);
                }

                var indexes = new List<string>();

                foreach (var index in indexListAttr)
                {
                    if (!indexes.Contains(index.Name))
                        indexes.Add(index.Name);
                }

                this.Indexes = indexes.ToArray();
            }
        }
    }

    public class SBODatabase
    {
        private SAPbobsCOM.Company oCompany;
        readonly static Dictionary<string, SBOTable> _mappings = new Dictionary<string, SBOTable>();

        public SBODatabase()
        {
            var oCompany = SBOClient.GetInstance().Company;
            this.oCompany = oCompany;
        }

        private SBOTable GetTableMapping(Type type)
        {
            SBOTable map;
            var key = type.FullName;
            lock (_mappings)
            {
                if (_mappings.TryGetValue(key, out map))
                {
                    map = new SBOTable(type);
                    _mappings[key] = map;
                }
                else
                {
                    map = new SBOTable(type);
                    _mappings.Add(key, map);
                }
            }
            return map;
        }

        private IndexInfo[] GetTableIndexes(string TableName)
        {
            var indexList = new List<IndexInfo>();

            string sQuery = SBODbHelper.GetTableIndexesSQL(TableName);

            SAPbobsCOM.Recordset oRecordset = null;

            oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRecordset.DoQuery(sQuery);
            oRecordset.MoveFirst();

            for (int i = 0; i < oRecordset.RecordCount; i++)
            {
                var index = new IndexInfo();

                index.ID = Convert.ToInt32(oRecordset.Fields.Item("KeyId").Value);
                index.KeyName = Convert.ToString(oRecordset.Fields.Item("KeyName").Value);
                index.TableName = Convert.ToString(oRecordset.Fields.Item("TableName").Value);
                index.IsUnique = Convert.ToString(oRecordset.Fields.Item("UniqueKey").Value) == "Y";

                indexList.Add(index);

                oRecordset.MoveNext();
            }

            Marshal.ReleaseComObject(oRecordset);
            GC.Collect();

            return indexList.ToArray();
        }



        private FieldInfo[] GetTableFields(string TableName)
        {
            List<FieldInfo> list = new List<FieldInfo>();

            var oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRecordset.DoQuery(SBODbHelper.GetTableFieldsSQL(TableName));
            oRecordset.MoveFirst();

            for (int i = 0; i < oRecordset.RecordCount; i++)
            {
                var field = new FieldInfo();

                string notNull = Convert.ToString(oRecordset.Fields.Item("FieldID").Value);
                string linkedTable = Convert.ToString(oRecordset.Fields.Item("RTable").Value);
                string linkedUDO = Convert.ToString(oRecordset.Fields.Item("RelUDO").Value);
                string linkedSysObj = Convert.ToString(oRecordset.Fields.Item("RelSO").Value);

                field.Table = Convert.ToString(oRecordset.Fields.Item("TableID").Value);
                field.ID = Convert.ToInt32(oRecordset.Fields.Item("FieldID").Value);
                field.AliasID = Convert.ToString(oRecordset.Fields.Item("AliasID").Value);
                field.Type = GetFieldType(Convert.ToString(oRecordset.Fields.Item("TypeID").Value));
                field.SubType = GetFieldSubType(Convert.ToString(oRecordset.Fields.Item("EditType").Value));
                field.EditSize = Convert.ToInt32(oRecordset.Fields.Item("EditSize").Value);
                field.Mandatory = notNull == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;

                if (!string.IsNullOrEmpty(linkedTable))
                    field.LinkedTable = linkedTable;

                if (!string.IsNullOrEmpty(linkedUDO))
                    field.LinkedUDO = linkedUDO;

                if (!string.IsNullOrEmpty(linkedSysObj))
                    field.LinkedSysObj = Convert.ToInt32(linkedSysObj);

                list.Add(field);

                oRecordset.MoveNext();
            }

            Marshal.ReleaseComObject(oRecordset);
            GC.Collect();

            return list.ToArray();
        }

        private void SetUserKey(string TableName, string KeyName, string[] Fields, bool IsUnique = false)
        {
            int lRetCode = 0;
            string sErrMsg = string.Empty;

            SAPbobsCOM.UserKeysMD oUserKeysMD;

            oUserKeysMD = (SAPbobsCOM.UserKeysMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserKeys);
            oUserKeysMD.KeyName = KeyName;
            oUserKeysMD.TableName = TableName;
            oUserKeysMD.Unique = IsUnique ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;

            bool first = true;
            foreach (var alias in Fields)
            {
                if (first) { first = false; }
                else { oUserKeysMD.Elements.Add(); }

                oUserKeysMD.Elements.ColumnAlias = alias;
            }

            lRetCode = oUserKeysMD.Add();

            Marshal.ReleaseComObject(oUserKeysMD);
            GC.Collect();

            if (lRetCode != 0)
            {
                oCompany.GetLastError(out lRetCode, out sErrMsg);
                throw new Exception(string.Format("Error {0}: {1}. Table[{2}] - Index[{3}] ", lRetCode, sErrMsg, TableName, KeyName));
            }
        }

        private int GetIndexID(string TableName, string KeyName)
        {
            string sQuery = SBODbHelper.GetIndexIdSQL(TableName, KeyName);

            var oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRecordset.DoQuery(sQuery);

            if (oRecordset.RecordCount != 1)
                return -1;

            int id = Convert.ToInt32(oRecordset.Fields.Item("KeyId").Value);

            Marshal.ReleaseComObject(oRecordset);
            GC.Collect();

            return id;
        }

        private int GetFieldID(string TableName, string AliasID)
        {
            string sQuery = SBODbHelper.GetFieldIdSQL(TableName, AliasID);

            SAPbobsCOM.Recordset oRecordset = null;

            oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRecordset.DoQuery(sQuery);

            if (oRecordset.RecordCount != 1)
                return -1;

            int id = Convert.ToInt32(oRecordset.Fields.Item("FieldID").Value);

            Marshal.ReleaseComObject(oRecordset);
            GC.Collect();

            return id;
        }

        private SAPbobsCOM.BoFieldTypes GetFieldType(string Value)
        {
            SAPbobsCOM.BoFieldTypes type;

            switch (Value)
            {
                case "N":
                    type = SAPbobsCOM.BoFieldTypes.db_Numeric;
                    break;
                case "M":
                    type = SAPbobsCOM.BoFieldTypes.db_Memo;
                    break;
                case "D":
                    type = SAPbobsCOM.BoFieldTypes.db_Date;
                    break;
                case "B":
                    type = SAPbobsCOM.BoFieldTypes.db_Float;
                    break;
                default:
                    type = SAPbobsCOM.BoFieldTypes.db_Alpha;
                    break;
            }

            return type;
        }

        private SAPbobsCOM.BoFldSubTypes GetFieldSubType(string Value)
        {
            SAPbobsCOM.BoFldSubTypes subType;

            switch (Value)
            {
                case "?":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Address;
                    break;
                case "#":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Phone;
                    break;
                case "T":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Time;
                    break;
                case "R":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Rate;
                    break;
                case "S":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Sum;
                    break;
                case "P":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Price;
                    break;
                case "Q":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Quantity;
                    break;
                case "%":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Percentage;
                    break;
                case "M":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Measurement;
                    break;
                case "B":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Link;
                    break;
                case "I":
                    subType = SAPbobsCOM.BoFldSubTypes.st_Image;
                    break;
                default:
                    subType = SAPbobsCOM.BoFldSubTypes.st_None;
                    break;
            }

            return subType;
        }

        private bool ExistTable(string TableName)
        {
            GC.Collect();
            SAPbobsCOM.UserTablesMD oUserTablesMD = null;
            bool boolIdent = false;
            oUserTablesMD = ((SAPbobsCOM.UserTablesMD)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)));
            boolIdent = oUserTablesMD.GetByKey(TableName);
            Marshal.ReleaseComObject(oUserTablesMD);
            GC.Collect();
            return (boolIdent);
        }

        public void DropTable<T>()
        {
            DropTable(typeof(T));
        }

        public void DropTable(Type Ty)
        {
            int lRetCode = 0;
            string sErrMsg = string.Empty;
            SBOTable tableMapped = GetTableMapping(Ty);

            string tableName = tableMapped.Name;

            if (tableMapped.TableSource == BoTableSource.btt_Native)
                throw new Exception(tableName + ": Una tabla nativa no puede ser eliminada.");

            if (tableMapped.TableSource == BoTableSource.btt_UDT)
            {
                if (!ExistTable(tableMapped.Name))
                    throw new Exception(tableName + ": La tabla especificada no existe.");

                if (tableMapped.UDOObjType != -1)
                    DropUDO(tableMapped.Name);

                SAPbobsCOM.UserTablesMD oUserTablesMD = null;
                oUserTablesMD = ((SAPbobsCOM.UserTablesMD)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)));

                if (oUserTablesMD.GetByKey(tableName))
                {
                    lRetCode = oUserTablesMD.Remove();

                    if (lRetCode != 0)
                    {
                        oCompany.GetLastError(out lRetCode, out sErrMsg);
                        throw new Exception(string.Format("Error {0}: {1}.", lRetCode, sErrMsg));
                    }
                    else
                    {
                        string message = string.Format("Tabla[{0}] - {1}: Tabla eliminada correctamente.", tableMapped.Name, tableMapped.Description);

                        Application.SBO_Application.StatusBar.SetText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    }
                }

                Marshal.ReleaseComObject(oUserTablesMD);
                GC.Collect();
            }

            if (tableMapped.TableSource == BoTableSource.btt_Native)
            {
                foreach (var field in tableMapped.Fields)
                {
                    var oUserFieldsMD = (SAPbobsCOM.UserFieldsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                    if (oUserFieldsMD.GetByKey(tableName, GetFieldID(tableMapped.Name, field.AliasID)))
                    {
                        lRetCode = oUserFieldsMD.Remove();

                        if (lRetCode != 0)
                        {
                            oCompany.GetLastError(out lRetCode, out sErrMsg);
                            throw new Exception(string.Format("Error {0}: {1}.", lRetCode, sErrMsg));
                        }
                    }

                    string message = string.Format("Tabla[{0}]: Campos de usuario eliminados correctamente.", tableMapped.Name);

                    Application.SBO_Application.StatusBar.SetText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    Marshal.ReleaseComObject(oUserFieldsMD);
                    GC.Collect();
                }
            }
        }

        public void CreateTable<T>()
        {
            CreateTable(typeof(T));
        }

        public BoCreateTableResult CreateTable(Type Ty)
        {
            int lRetCode = 0;
            string sErrMsg = string.Empty;
            var result = BoCreateTableResult.boctr_Migrated;

            SBOTable tableMapped = GetTableMapping(Ty);

            string tableName = tableMapped.TableSource == BoTableSource.btt_UDT ? "@" + tableMapped.Name : tableMapped.Name;

            if (tableMapped.TableSource == BoTableSource.btt_UDT)
            {
                if (!ExistTable(tableMapped.Name))
                    result = CreateTable(tableMapped.Name, tableMapped.Description, tableMapped.Type);
            }

            var currentFields = GetTableFields(tableName);
            var listToInsertFields = new List<SBOTable.SBOField>();
            var listToUpdateFields = new List<SBOTable.SBOField>();

            foreach (var mappedField in tableMapped.Fields)
            {
                var found = false;
                foreach (var currentField in currentFields)
                {
                    string aliasID = mappedField.AliasID;

                    found = (string.Compare(aliasID, currentField.AliasID, StringComparison.OrdinalIgnoreCase) == 0);
                    if (found)
                    {
                        listToUpdateFields.Add(mappedField);
                        break;
                    }
                }

                if (!found)
                {
                    listToInsertFields.Add(mappedField);
                }
            }

            SAPbobsCOM.UserFieldsMD oUserFieldsMD = null;

            foreach (var field in listToInsertFields)
            {
                oUserFieldsMD = (SAPbobsCOM.UserFieldsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                oUserFieldsMD.TableName = tableName;
                oUserFieldsMD.Name = field.AliasID;
                oUserFieldsMD.Description = field.Description;
                oUserFieldsMD.Type = field.Type;
                oUserFieldsMD.SubType = field.SubType;
                oUserFieldsMD.Mandatory = field.Mandatory;
                oUserFieldsMD.DefaultValue = field.DefaultValue;

                if (field.ValidValues.Count != 0)
                {
                    bool first = true;

                    var list = field.ValidValues.Keys.ToList();
                    list.Sort();

                    foreach (var element in list)
                    {
                        if (first != true)
                            oUserFieldsMD.ValidValues.Add();

                        oUserFieldsMD.ValidValues.Value = element;
                        oUserFieldsMD.ValidValues.Description = field.ValidValues[element];

                        if (first == true)
                            first = false;
                    }
                }

                if ((field.Type == SAPbobsCOM.BoFieldTypes.db_Numeric || field.Type == SAPbobsCOM.BoFieldTypes.db_Alpha) &&
                    field.SubType == SAPbobsCOM.BoFldSubTypes.st_None)
                    oUserFieldsMD.EditSize = field.EditSize;

                if (!string.IsNullOrEmpty(field.LinkedTable))
                    oUserFieldsMD.LinkedTable = field.LinkedTable;

                if (!string.IsNullOrEmpty(field.LinkedUDO))
                {
                    oUserFieldsMD.EditSize = field.EditSize;
                    oUserFieldsMD.LinkedUDO = field.LinkedUDO;
                }

                if (field.LinkedSystemObject != -1)
                    oUserFieldsMD.LinkedSystemObject = (SAPbobsCOM.BoObjectTypes)field.LinkedSystemObject;

                lRetCode = oUserFieldsMD.Add();

                Marshal.ReleaseComObject(oUserFieldsMD);
                GC.Collect();

                if (lRetCode != 0)
                {
                    oCompany.GetLastError(out lRetCode, out sErrMsg);
                    throw new Exception(string.Format("Error {0}: {1}. Field[{2}]", lRetCode, sErrMsg, field.AliasID));
                }
            }

            foreach (var field in listToUpdateFields)
            {
                string aliasID = field.AliasID;

                oUserFieldsMD = (SAPbobsCOM.UserFieldsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                oUserFieldsMD.GetByKey(tableName, GetFieldID(tableName, aliasID));
                oUserFieldsMD.Description = field.Description;
                oUserFieldsMD.DefaultValue = field.DefaultValue;

                bool first = true;

                var listValues = field.ValidValues.Keys.ToList();
                listValues.Sort();

                foreach (var value in listValues)
                {
                    bool exist = false;

                    var list = oUserFieldsMD.ValidValues;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list.SetCurrentLine(i);
                        if (list.Value == value)
                        {
                            exist = true;
                            list.Description = field.ValidValues[value];
                            break;
                        }
                    }

                    if (!exist)
                    {
                        if (list.Count != 0 && first == false)
                            list.Add();

                        list.Value = value;
                        list.Description = field.ValidValues[value];
                    }

                    first = false;
                }

                if ((field.Type == SAPbobsCOM.BoFieldTypes.db_Numeric || field.Type == SAPbobsCOM.BoFieldTypes.db_Alpha) &&
                    field.SubType == SAPbobsCOM.BoFldSubTypes.st_None)
                {
                    if (field.EditSize > oUserFieldsMD.EditSize)
                        oUserFieldsMD.EditSize = field.EditSize;
                }

                if (!string.IsNullOrEmpty(field.LinkedTable))
                    oUserFieldsMD.LinkedTable = field.LinkedTable;

                if (!string.IsNullOrEmpty(field.LinkedUDO))
                    oUserFieldsMD.LinkedUDO = field.LinkedUDO;

                if (field.LinkedSystemObject != -1)
                    oUserFieldsMD.LinkedSystemObject = (SAPbobsCOM.BoObjectTypes)field.LinkedSystemObject;

                lRetCode = oUserFieldsMD.Update();

                Marshal.ReleaseComObject(oUserFieldsMD);
                GC.Collect();

                if (lRetCode != 0)
                {
                    oCompany.GetLastError(out lRetCode, out sErrMsg);
                    throw new Exception(string.Format("Error {0}: {1}. Field[{2}]", lRetCode, sErrMsg, field.AliasID));
                }
            }

            var currentIndexes = GetTableIndexes(tableName);
            var listToInsertIndexes = new List<SBOTable.SBOIndex>();
            var listToUpdateIndexes = new List<SBOTable.SBOIndex>();

            foreach (var mappedIndex in tableMapped.Indexes)
            {
                var found = false;

                foreach (var currentIndex in currentIndexes)
                {
                    found = (string.Compare(mappedIndex.Name, currentIndex.KeyName, StringComparison.OrdinalIgnoreCase) == 0);
                    if (found)
                    {
                        listToUpdateIndexes.Add(mappedIndex);
                        break;
                    }
                }

                if (!found)
                {
                    listToInsertIndexes.Add(mappedIndex);
                }
            }

            foreach (var index in listToInsertIndexes)
            {
                this.SetUserKey(tableMapped.Name, index.Name, index.Fields, index.IsUnique);
            }

            foreach (var index in listToUpdateIndexes)
            {
                var oUserKeyMD = (SAPbobsCOM.UserKeysMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserKeys);
                bool exist = oUserKeyMD.GetByKey(tableName, GetIndexID(tableName, index.Name));

                if (exist)
                {
                    //lRetCode = oUserKeyMD.Remove();

                    Marshal.ReleaseComObject(oUserKeyMD);
                    GC.Collect();

                    //if (lRetCode != 0)
                    //{
                    //    oCompany.GetLastError(out lRetCode, out sErrMsg);
                    //    throw new Exception(string.Format("Error {0}: {1}. Tabla[{2}] - Índice[{3}] ", lRetCode, sErrMsg, tableMapped.Name, index.Name));
                    //}

                    //this.SetUserKey(tableMapped.Name, index.Name, index.Fields, index.IsUnique);
                }
            }

            string status = result == BoCreateTableResult.boctr_Created ? "creada" : "actualizada";
            string message = string.Format("Tabla[{0}]: Tabla {1} correctamente.", tableMapped.Name, status);

            Application.SBO_Application.StatusBar.SetText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

            if (tableMapped.UDOObjType != -1)
                CreateUDO(tableMapped);

            return result;
        }

        private BoCreateTableResult CreateTable(string TableName, string Description, SAPbobsCOM.BoUTBTableType TableType)
        {
            int lRetCode = 0;
            string sErrMsg = string.Empty;

            GC.Collect();

            SAPbobsCOM.UserTablesMD oUserTablesMD = null;
            oUserTablesMD = ((SAPbobsCOM.UserTablesMD)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)));

            if (TableName.Length > 19)
                throw new Exception(string.Format("Tabla[{0}] - {1}: El nombre de la tabla excede del límite permitido (Max. 19 caracteres).", TableName, Description));

            if (Description.Length > 30)
                throw new Exception(string.Format("Tabla[{0}] - {1}: La descripción de la tabla excede del límite permitido (Max. 30 caracteres).", TableName, Description));

            oUserTablesMD.TableName = TableName;
            oUserTablesMD.TableDescription = Description;
            oUserTablesMD.TableType = TableType;

            lRetCode = oUserTablesMD.Add();
            if (lRetCode != 0)
            {
                oCompany.GetLastError(out lRetCode, out sErrMsg);
                throw new Exception(string.Format("Error {0}: {1}.", lRetCode, sErrMsg));
            }

            Marshal.ReleaseComObject(oUserTablesMD);
            GC.Collect();

            return BoCreateTableResult.boctr_Created;
        }

        private bool ExistUDO(string code)
        {
            bool boolIdent = false;
            var oUserObjectMD = ((SAPbobsCOM.UserObjectsMD)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD)));

            boolIdent = oUserObjectMD.GetByKey(code);

            Marshal.ReleaseComObject(oUserObjectMD);
            GC.Collect();

            return (boolIdent);
        }

        private void DropUDO(string code)
        {
            int lRetCode = 0;
            string sErrMsg = string.Empty;

            var oUserObjectMD = (SAPbobsCOM.UserObjectsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);

            if (oUserObjectMD.GetByKey(code))
            {
                lRetCode = oUserObjectMD.Remove();

                if (lRetCode != 0)
                {
                    SBOClient.GetInstance().Company.GetLastError(out lRetCode, out sErrMsg);
                    throw new Exception(string.Format("Error {0}: {1}.", lRetCode, sErrMsg));
                }
            }

            Marshal.ReleaseComObject(oUserObjectMD);
            GC.Collect();
        }

        private void CreateUDO(SBOTable tableMapped)
        {
            int lRetCode = 0;
            string sErrMsg = string.Empty;
            bool first;

            var oUserObjectMD = (SAPbobsCOM.UserObjectsMD)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);

            if (!ExistUDO(tableMapped.Name))
            {
                oUserObjectMD.Code = tableMapped.Name;
                oUserObjectMD.Name = tableMapped.Description;
                oUserObjectMD.TableName = tableMapped.Name;
                oUserObjectMD.ObjectType = (SAPbobsCOM.BoUDOObjType)tableMapped.UDOObjType;

                oUserObjectMD.CanFind = tableMapped.CanFind;
                oUserObjectMD.CanDelete = tableMapped.CanDelete;
                oUserObjectMD.CanCancel = tableMapped.CanCancel;
                oUserObjectMD.CanYearTransfer = tableMapped.CanYearTransfer;
                oUserObjectMD.CanLog = tableMapped.CanLog;

                oUserObjectMD.CanClose = tableMapped.CanClose;
                oUserObjectMD.ManageSeries = tableMapped.ManageSeries;
                oUserObjectMD.CanApprove = tableMapped.CanApprove;

                first = true;
                if (tableMapped.CanFind == SAPbobsCOM.BoYesNoEnum.tYES)
                {
                    oUserObjectMD.FindColumns.ColumnAlias = "DocEntry";
                    oUserObjectMD.FindColumns.ColumnDescription = "No. Interno";

                    if (tableMapped.UDOObjType == 1)
                    {
                        oUserObjectMD.FindColumns.Add();
                        oUserObjectMD.FindColumns.ColumnAlias = "Code";
                        oUserObjectMD.FindColumns.ColumnDescription = "Código";

                        oUserObjectMD.FindColumns.Add();
                        oUserObjectMD.FindColumns.ColumnAlias = "Name";
                        oUserObjectMD.FindColumns.ColumnDescription = "Nombre";
                    }
                    if (tableMapped.UDOObjType == 3)
                    {
                        oUserObjectMD.FindColumns.Add();
                        oUserObjectMD.FindColumns.ColumnAlias = "DocNum";
                        oUserObjectMD.FindColumns.ColumnDescription = "No. de Documento";
                    }

                    foreach (var field in tableMapped.Fields)
                    {
                        if (field.CanFind)
                        {
                            oUserObjectMD.FindColumns.Add();
                            oUserObjectMD.FindColumns.ColumnAlias = "U_" + field.AliasID;
                            oUserObjectMD.FindColumns.ColumnDescription = field.Description;
                        }
                    }
                }

                first = true;
                foreach (var child in tableMapped.Childs)
                {
                    if (first) { first = false; }
                    else { oUserObjectMD.ChildTables.Add(); }

                    oUserObjectMD.ChildTables.TableName = child.Name;
                }

                lRetCode = oUserObjectMD.Add();
            }
            else
            {
                if (oUserObjectMD.GetByKey(tableMapped.Name))
                {
                    oUserObjectMD.Name = tableMapped.Description;
                    oUserObjectMD.CanFind = tableMapped.CanFind;
                    oUserObjectMD.CanDelete = tableMapped.CanDelete;
                    oUserObjectMD.CanCancel = tableMapped.CanCancel;
                    oUserObjectMD.CanYearTransfer = tableMapped.CanYearTransfer;
                    oUserObjectMD.CanLog = tableMapped.CanLog;

                    oUserObjectMD.CanClose = tableMapped.CanClose;
                    oUserObjectMD.ManageSeries = tableMapped.ManageSeries;
                    oUserObjectMD.CanApprove = tableMapped.CanApprove;

                    first = true;
                    bool found;

                    if (tableMapped.CanFind == SAPbobsCOM.BoYesNoEnum.tYES)
                    {
                        foreach (var field in tableMapped.Fields)
                        {
                            if (field.CanFind)
                            {
                                found = false;
                                var currentColumns = oUserObjectMD.FindColumns;

                                for (int i = 0; i < currentColumns.Count; i++)
                                {
                                    found = (string.Compare("U_" + field.AliasID, currentColumns.ColumnAlias, StringComparison.OrdinalIgnoreCase) == 0);
                                    if (found)
                                        break;
                                }

                                if (currentColumns.Count > 1)
                                    first = false;

                                if (!found)
                                {
                                    if (first) { first = false; }
                                    else { currentColumns.Add(); }

                                    currentColumns.ColumnAlias = "U_" + field.AliasID;
                                    currentColumns.ColumnDescription = field.Description;
                                }
                            }
                        }
                    }

                    first = false;
                    foreach (var child in tableMapped.Childs)
                    {
                        found = false;
                        var childTables = oUserObjectMD.ChildTables;

                        for (int i = 0; i < childTables.Count; i++)
                        {
                            childTables.SetCurrentLine(i);
                            found = (string.Compare(childTables.TableName, child.Name, StringComparison.OrdinalIgnoreCase) == 0);
                            if (found)
                                break;
                        }

                        if (!found)
                        {
                            childTables.Add();
                            childTables.TableName = child.Name;
                        }
                    }

                    lRetCode = oUserObjectMD.Update();
                }
            }

            Marshal.ReleaseComObject(oUserObjectMD);
            GC.Collect();

            if (lRetCode != 0)
            {
                oCompany.GetLastError(out lRetCode, out sErrMsg);
                string message = string.Format("Error {0}: {1}. Table[{2}]: El objeto especificado no puede ser creado o actualizado.", lRetCode, sErrMsg, tableMapped.Name);

                Application.SBO_Application.StatusBar.SetText(message, SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private class IndexInfo
        {
            public int ID { get; set; }
            public string TableName { get; set; }
            public string KeyName { get; set; }
            public bool IsUnique { get; set; }
        }

        private class FieldInfo
        {
            public int ID { get; set; }
            public string Table { get; set; }
            public string AliasID { get; set; }
            public string Description { get; set; }
            public SAPbobsCOM.BoFieldTypes Type { get; set; }
            public SAPbobsCOM.BoFldSubTypes SubType { get; set; }
            public int EditSize { get; set; }
            public SAPbobsCOM.BoYesNoEnum Mandatory { get; set; }
            public string LinkedTable { get; set; }
            public string LinkedUDO { get; set; }
            public int LinkedSysObj { get; set; }

            public FieldInfo()
            {
                this.LinkedSysObj = -1;
            }
        }
    }

    public static class Orm
    {
        public static string GetTableName(Type type)
        {
            var tableAttr = (TableNameAttribute)Orm.GetCustomAttribute<TableNameAttribute>(type);

            if (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Name))
                return tableAttr.Sourced == BoTableSource.btt_UDT ? "@" + tableAttr.Name : tableAttr.Name;

            return "@" + type.Name;
        }

        public static string GetFieldName(Type type)
        {
            return string.Empty;
        }

        public static object GetCustomAttribute<T>(Type type)
        {
            var tableAttr = type.GetCustomAttributes(true)
                .Where(x => x.GetType() == typeof(T))
                .FirstOrDefault();

            return tableAttr;
        }

        public static T[] GetAllCustomAttributes<T>(Type type)
        {
            var tableAttr = type.GetCustomAttributes(true)
                .Where(x => x.GetType() == typeof(T));

            return tableAttr.Cast<T>().ToArray();
        }

        public static T[] GetAllCustomAttributes<T>(PropertyInfo prop)
        {
            var tableAttr = prop.CustomAttributes
                    .Where(x => x.AttributeType == typeof(T))
                    .Select(x => (T)Orm.InflateAttribute(x));
            return tableAttr.Cast<T>().ToArray();
        }

        public static object GetCustomAttribute<T>(TypeInfo typeInfo)
        {
            var tableAttr = typeInfo.CustomAttributes
                                .Where(x => x.AttributeType == typeof(T))
                                .Select(x => (T)Orm.InflateAttribute(x))
                                .FirstOrDefault();
            return tableAttr;
        }

        public static object GetCustomAttribute<T>(PropertyInfo prop)
        {
            var tableAttr = prop.CustomAttributes
                                .Where(x => x.AttributeType == typeof(T))
                                .Select(x => (T)Orm.InflateAttribute(x))
                                .FirstOrDefault();
            return tableAttr;
        }

        public static SAPbobsCOM.BoFieldTypes GetFieldType(Type FieldType)
        {
            var clrType = FieldType;

            if (clrType == typeof(String) || clrType == typeof(StringBuilder) || clrType == typeof(Uri) || clrType == typeof(UriBuilder))
            {
                return SAPbobsCOM.BoFieldTypes.db_Alpha;
            }
            else if (clrType == typeof(Single) || clrType == typeof(Double) || clrType == typeof(Decimal))
            {
                return SAPbobsCOM.BoFieldTypes.db_Float;
            }
            else if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32) || clrType == typeof(UInt32) || clrType == typeof(Int64))
            {
                return SAPbobsCOM.BoFieldTypes.db_Numeric;
            }
            else if (clrType == typeof(DateTime))
            {
                return SAPbobsCOM.BoFieldTypes.db_Date;
            }
            else
            {
                throw new NotSupportedException("Don't know about " + clrType);
            }
        }

        public static FieldInfo GetField(TypeInfo t, string name)
        {
            var f = t.GetDeclaredField(name);
            if (f != null)
                return f;
            return GetField(t.BaseType.GetTypeInfo(), name);
        }

        public static PropertyInfo GetProperty(TypeInfo t, string name)
        {
            var f = t.GetDeclaredProperty(name);
            if (f != null)
                return f;
            return GetProperty(t.BaseType.GetTypeInfo(), name);
        }

        public static object InflateAttribute(CustomAttributeData x)
        {
            var atype = x.AttributeType;
            var typeInfo = atype.GetTypeInfo();
            var args = x.ConstructorArguments.Select(a => a.Value).ToArray();
            var r = Activator.CreateInstance(x.AttributeType, args);
            foreach (var arg in x.NamedArguments)
            {
                if (arg.IsField)
                {
                    GetField(typeInfo, arg.MemberName).SetValue(r, arg.TypedValue.Value);
                }
                else
                {
                    GetProperty(typeInfo, arg.MemberName).SetValue(r, arg.TypedValue.Value);
                }
            }
            return r;
        }
    }
}