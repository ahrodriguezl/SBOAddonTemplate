using Avantis.Login;
using Avantis.ORM;
using SAPbobsCOM;
using System;

namespace Avantis.Settings
{
    [TableName("AVS_ADDON")]
    [Description("Avantis - Addons Instalados")]
    [ObjectType((int)SAPbobsCOM.BoUDOObjType.boud_MasterData)]
    class SBOSetup : MasterDataType
    {
        public const string ADDON_CODE = "ADDON_CODE";
        public const string ADDON_NAME = "ADDON_NAME";
        public const string ADDON_VERSION = "ADDON_VERSION";

        private Company oCompany = null;
        private GeneralService oGeneralService = null;
        private GeneralData oGeneralData = null;
        private GeneralDataParams oGeneralParams = null;
        private CompanyService oCompanyService = null;

        [Ignore]
        public override int DocumentEntry
        {
            get { return Convert.ToInt32(oGeneralData.GetProperty("DocEntry")); }
            set { oGeneralData.SetProperty("DocEntry", value); }
        }

        [Ignore]
        public override string Code
        {
            get { return Convert.ToString(oGeneralData.GetProperty("Code")); }
            set { oGeneralData.SetProperty("Code", value); }
        }

        [Ignore]
        public override string Name
        {
            get { return Convert.ToString(oGeneralData.GetProperty("Name")); }
            set { oGeneralData.SetProperty("Name", value); }
        }

        [AliasID("Version")]
        [Description("Versión")]
        [FieldType((int)SAPbobsCOM.BoFieldTypes.db_Alpha)]
        [EditSize(50)]
        [Mandatory]
        public string Version
        {
            get { return Convert.ToString(oGeneralData.GetProperty("U_Version")); }
            set { oGeneralData.SetProperty("U_Version", value); }
        }

        public SBOSetup()
        {
            oCompany = SBOClient.GetInstance().Company;

            oCompanyService = oCompany.GetCompanyService();
            oGeneralService = oCompanyService.GetGeneralService("AVS_ADDON");
            oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);
        }

        public bool GetByKey(string Code)
        {
            try
            {
                oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                oGeneralParams.SetProperty("Code", Code);
                oGeneralData = oGeneralService.GetByParams(oGeneralParams);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void Save()
        {
            try
            {
                if (this.DocumentEntry == 0)
                {
                    oGeneralParams = oGeneralService.Add(oGeneralData);
                    this.GetByKey(this.Code);
                }
                else
                {
                    oGeneralService.Update(oGeneralData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
