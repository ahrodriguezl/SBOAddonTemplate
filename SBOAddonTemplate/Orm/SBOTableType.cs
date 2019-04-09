namespace Avantis.ORM
{
    abstract class ObjectType
    {
        [Ignore]
        abstract public int DocumentEntry { get; set; }
    }

    [TableType((int)SAPbobsCOM.BoUTBTableType.bott_MasterData)]
    abstract class MasterDataType : ObjectType
    {
        [Ignore]
        abstract public string Code { get; set; }

        [Ignore]
        abstract public string Name { get; set; }
    }

    [TableType((int)SAPbobsCOM.BoUTBTableType.bott_MasterDataLines)]
    abstract class MasterDataLinesType : ObjectType
    {
    }

    [TableType((int)SAPbobsCOM.BoUTBTableType.bott_Document)]
    abstract class DocumentDataType : ObjectType
    {
        [Ignore]
        abstract public string DocumentNumber { get; set; }
    }

    [TableType((int)SAPbobsCOM.BoUTBTableType.bott_DocumentLines)]
    abstract class DocumentDataLinesType : ObjectType
    {
    }

    [TableType((int)SAPbobsCOM.BoUTBTableType.bott_NoObject)]
    abstract class NoObjectType
    {
        [Ignore]
        abstract public string Code { get; set; }

        [Ignore]
        abstract public string Name { get; set; }
    }

    [TableType((int)SAPbobsCOM.BoUTBTableType.bott_NoObjectAutoIncrement)]
    abstract class NoObjectAutoIncrementType
    {
        [Ignore]
        abstract public int Code { get; set; }

        [Ignore]
        abstract public string Name { get; set; }
    }
}