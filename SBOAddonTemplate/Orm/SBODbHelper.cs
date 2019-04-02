using Avantis.Login;
using System;

namespace Avantis.ORM
{
    public static class SBODbHelper
    {
        public static string GetTableIndexesSQL(string TableName)
        {
            var _client = SBOClient.GetInstance();
            string sMask = string.Empty;

            switch (_client.Company.DbServerType)
            {

                case SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012:
                    sMask = @"SELECT * FROM OUKD WHERE [TableName] = '{0}'";
                    break;

                case SAPbobsCOM.BoDataServerTypes.dst_HANADB:
                    sMask = @"SELECT * FROM ""OUKD"" WHERE ""TableName"" = '{0}'";
                    break;

                default:
                    throw new Exception(_client.Company.DbServerType + ": database not supported yet");
            }

            return string.Format(sMask, TableName);
        }

        public static string GetTableFieldsSQL(string TableName)
        {
            var _client = SBOClient.GetInstance();
            string sMask = string.Empty;

            switch (_client.Company.DbServerType)
            {

                case SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012:
                    sMask = @"SELECT * FROM CUFD WHERE [TableID] = '{0}'";
                    break;

                case SAPbobsCOM.BoDataServerTypes.dst_HANADB:
                    sMask = @"SELECT * FROM ""CUFD"" WHERE ""TableID"" = '{0}'";
                    break;

                default:
                    throw new Exception(_client.Company.DbServerType + ": database not supported yet");
            }

            return string.Format(sMask, TableName);
        }

        public static string GetIndexIdSQL(string TableName, string KeyName)
        {
            var _client = SBOClient.GetInstance();
            string sMask = string.Empty;

            switch (_client.Company.DbServerType)
            {

                case SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012:
                    sMask = @"SELECT [KeyId] FROM OUKD WHERE [TableName] = '{0}'  AND [KeyName] = '{1}'";
                    break;

                case SAPbobsCOM.BoDataServerTypes.dst_HANADB:
                    sMask = @"SELECT ""KeyId"" FROM ""OUKD"" WHERE ""TableName"" = '{0}'  AND ""KeyName"" = '{1}'";
                    break;

                default:
                    throw new Exception(_client.Company.DbServerType + ": database not supported yet");
            }

            return string.Format(sMask, TableName, KeyName);
        }

        public static string GetFieldIdSQL(string TableName, string AliasID)
        {
            var _client = SBOClient.GetInstance();
            string sMask = string.Empty;

            switch (_client.Company.DbServerType)
            {

                case SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012:
                    sMask = @"SELECT [TableID], [FieldID] FROM CUFD WHERE [TableID] = '{0}'  AND [AliasID] = '{1}'";
                    break;

                case SAPbobsCOM.BoDataServerTypes.dst_HANADB:
                    sMask = @"SELECT ""TableID"", ""FieldID"" FROM ""CUFD"" WHERE ""TableID"" = '{0}'  AND ""AliasID"" = '{1}'";
                    break;

                default:
                    throw new Exception(_client.Company.DbServerType + ": database not supported yet");
            }

            return string.Format(sMask, TableName, AliasID);
        }
    }
}
