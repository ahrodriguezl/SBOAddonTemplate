using System;

namespace Avantis.ORM
{
    public enum BoTableSource
    {
        btt_Native = 0,
        btt_UDT = 1
    }

    public enum BoFieldType
    {
        bft_Alphanumeric = 0,
        bft_Address = 1,
        bft_Phone = 2,
        bft_Memo = 3,
        bft_Numeric = 4,
        bft_Date = 5,
        bft_Time = 6,
        bft_Rate = 7,
        bft_Sum = 8,
        bft_Price = 9,
        bft_Quantity = 10,
        bft_Percentage = 11,
        bft_Measurements = 12,
        bft_Link = 13,
        bft_Image = 14
    }

    /// <summary>
    /// Nombre de la tabla (Máximo 19 caracteres).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public string Name { get; set; }
        public BoTableSource Sourced { get; set; }

        public TableNameAttribute(string name, BoTableSource sourced = BoTableSource.btt_UDT)
        {
            this.Name = name;
            this.Sourced = sourced;
        }
    }

    /// <summary>
    /// Descripción de tabla (Máximo 30 caracteres).
    /// Descripción de Campo (Máximo 100 caracteres).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class DescriptionAttribute : Attribute
    {
        public string Text { get; set; }

        public DescriptionAttribute(string text)
        {
            this.Text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableTypeAttribute : Attribute
    {
        public SAPbobsCOM.BoUTBTableType Value { get; private set; }

        public TableTypeAttribute(int value)
        {
            this.Value = (SAPbobsCOM.BoUTBTableType)value;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AliasIDAttribute : Attribute
    {
        public string Name { get; set; }

        public AliasIDAttribute(string name)
        {
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        private string _propertyName;

        public Type Type { get; private set; }
        public SBOTable Table { get; private set; }
        public SBOTable.SBOField Field { get { return this.Table.FindColumnWithPropertyName(_propertyName); } }

        public ForeignKeyAttribute(Type Ty, string PropertyName)
        {
            this.Type = Ty;
            this.Table = new SBOTable(Ty);
            this._propertyName = PropertyName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FieldTypeAttribute : Attribute
    {
        public SAPbobsCOM.BoFieldTypes Value { get; private set; }

        public FieldTypeAttribute()
        {
        }

        public FieldTypeAttribute(int value)
        {
            this.Value = (SAPbobsCOM.BoFieldTypes)value;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FieldSubTypeAttribute : Attribute
    {
        public SAPbobsCOM.BoFldSubTypes SubType { get; private set; }

        public FieldSubTypeAttribute()
        {
        }

        public FieldSubTypeAttribute(int SubType)
        {
            this.SubType = (SAPbobsCOM.BoFldSubTypes)SubType;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditSizeAttribute : Attribute
    {
        public int Value { get; set; }

        public EditSizeAttribute(int value)
        {
            this.Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ValidValueAttribute : Attribute
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public ValidValueAttribute(string code, string name)
        {
            this.Code = code;
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValueAttribute : Attribute
    {
        public string Code { get; set; }

        public DefaultValueAttribute(string code)
        {
            this.Code = code;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class MandatoryAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public bool IsUnique { get; set; }
        public string Name { get; set; }

        public IndexAttribute(string Name, bool IsUnique = false)
        {
            this.Name = Name;
            this.IsUnique = IsUnique;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UsedIndexAttribute : Attribute
    {
        public string Name { get; set; }

        public UsedIndexAttribute(string Name)
        {
            this.Name = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FindFieldAttribute : Attribute
    {
        public bool Value { get; set; }

        public FindFieldAttribute(bool value)
        {
            this.Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LinkedTableAttribute : Attribute
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public SBOTable Table { get; private set; }

        public LinkedTableAttribute(Type Ty)
        {
            this.Type = Ty;
            this.Table = new SBOTable(Ty);
            this.Name = Table.Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LinkedUDOAttribute : Attribute
    {
        public string Name { get; private set; }

        public LinkedUDOAttribute(Type Ty)
        {
            var table = new SBOTable(Ty);
            this.Name = table.Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LinkedSystemObjectAttribute : Attribute
    {
        public SAPbobsCOM.BoObjectTypes Value { get; private set; }

        public LinkedSystemObjectAttribute(int value)
        {
            this.Value = (SAPbobsCOM.BoObjectTypes)value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ObjectTypeAttribute : Attribute
    {
        public SAPbobsCOM.BoUDOObjType Value { get; private set; }

        public ObjectTypeAttribute(int value)
        {
            this.Value = (SAPbobsCOM.BoUDOObjType)value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CanFindAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public CanFindAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CanCancelAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public CanCancelAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CanDeleteAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public CanDeleteAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CanCloseAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public CanCloseAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ManageSeriesAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public ManageSeriesAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CanYearTransferAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public CanYearTransferAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CanApproveAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public CanApproveAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CanLogAttribute : Attribute
    {
        public SAPbobsCOM.BoYesNoEnum Value { get; private set; }

        public CanLogAttribute(bool value)
        {
            this.Value = value ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ChildTableAttribute : Attribute
    {
    }
}