using SAPbobsCOM;
using Avantis.Login;
using Avantis.ORM;
using System;

namespace Avantis
{
    [TableName("AVS_CATEGORY")]
    [Description("Dato maestro categorias")]
    [ObjectType((int)SAPbobsCOM.BoUDOObjType.boud_MasterData)]
    [CanFind(true)]
    class Category : MasterDataType
    {
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

        public Category()
        {
            oCompany = SBOClient.GetInstance().Company;
            oCompanyService = oCompany.GetCompanyService();
            oGeneralService = oCompanyService.GetGeneralService("AVS_CATEGORY");
        }

        public bool GetByKey(string Code)
        {
            try
            {
                oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                oGeneralParams.SetProperty("Code", Code);
                oGeneralData = oGeneralService.GetByParams(oGeneralParams);

                if (oGeneralData == null)
                    return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        public void Save()
        {
            try
            {
                if (this.DocumentEntry == -1)
                {
                    oGeneralParams = oGeneralService.Add(oGeneralData);
                    this.DocumentEntry = System.Convert.ToInt32(oGeneralParams.GetProperty("DocEntry"));
                }
                else
                {
                    oGeneralService.Update(oGeneralData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void Delete()
        {
            try
            {
                oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                oGeneralParams.SetProperty("Code", Code);
                oGeneralService.Delete(oGeneralParams);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
    }

    [TableName("AVS_TRADEMARK")]
    [Description("Dato maestro marcas")]
    [ObjectType((int)SAPbobsCOM.BoUDOObjType.boud_MasterData)]
    [CanFind(true)]
    class Trademark : MasterDataType
    {
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

        [AliasID("Image")]
        [Description("Logotipo")]
        [FieldType((int)SAPbobsCOM.BoFieldTypes.db_Alpha)]
        [FieldSubType((int)SAPbobsCOM.BoFldSubTypes.st_Image)]
        [FindField(false)]
        public string Image
        {
            get { return Convert.ToString(oGeneralData.GetProperty("U_Image")); }
            private set { oGeneralData.SetProperty("U_Image", value); }
        }

        public Trademark()
        {
            oCompany = SBOClient.GetInstance().Company;
            oCompanyService = oCompany.GetCompanyService();
            oGeneralService = oCompanyService.GetGeneralService("AVS_Trademark");
        }

        public bool GetByKey(string Code)
        {
            try
            {
                oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                oGeneralParams.SetProperty("Code", Code);
                oGeneralData = oGeneralService.GetByParams(oGeneralParams);

                if (oGeneralData == null)
                    return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        public void Save()
        {
            try
            {
                if (this.DocumentEntry == -1)
                {
                    oGeneralParams = oGeneralService.Add(oGeneralData);
                    this.DocumentEntry = System.Convert.ToInt32(oGeneralParams.GetProperty("DocEntry"));
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

        public void Delete()
        {
            try
            {
                oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                oGeneralParams.SetProperty("Code", Code);
                oGeneralService.Delete(oGeneralParams);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    [TableName("AVS_Model")]
    [Description("Dato maestro Modelos")]
    [ObjectType((int)SAPbobsCOM.BoUDOObjType.boud_MasterData)]
    [CanFind(true)]
    class Model : MasterDataType
    {
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

        [AliasID("Category")]
        [Description("Categoría")]
        [FieldType((int)SAPbobsCOM.BoFieldTypes.db_Alpha)]
        [EditSize(50)]
        [FindField(true)]
        public string Category
        {
            get { return Convert.ToString(oGeneralData.GetProperty("U_Category")); }
            set { oGeneralData.SetProperty("U_Category", value); }
        }

        [AliasID("Trademark")]
        [Description("Marca")]
        [FieldType((int)SAPbobsCOM.BoFieldTypes.db_Alpha)]
        [EditSize(50)]
        [FindField(true)]
        public string Trademark
        {
            get { return Convert.ToString(oGeneralData.GetProperty("U_Trademark")); }
            set { oGeneralData.SetProperty("U_Trademark", value); }
        }

        [AliasID("Image")]
        [Description("Imágen del Modelo")]
        [FieldType((int)SAPbobsCOM.BoFieldTypes.db_Alpha)]
        [FieldSubType((int)SAPbobsCOM.BoFldSubTypes.st_Image)]
        [FindField(false)]
        public string Image
        {
            get { return Convert.ToString(oGeneralData.GetProperty("U_Image")); }
            set { oGeneralData.SetProperty("U_Image", value); }
        }

        public Model()
        {
            oCompany = SBOClient.GetInstance().Company;
            oCompanyService = oCompany.GetCompanyService();
            oGeneralService = oCompanyService.GetGeneralService("AVS_Model");
        }

        public bool GetByKey(string Code)
        {
            try
            {
                oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                oGeneralParams.SetProperty("Code", Code);
                oGeneralData = oGeneralService.GetByParams(oGeneralParams);

                if (oGeneralData == null)
                    return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        public void Save()
        {
            try
            {
                if (this.DocumentEntry == -1)
                {
                    oGeneralParams = oGeneralService.Add(oGeneralData);
                    this.DocumentEntry = System.Convert.ToInt32(oGeneralParams.GetProperty("DocEntry"));
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

        public void Delete()
        {
            try
            {
                oGeneralParams = ((SAPbobsCOM.GeneralDataParams)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)));
                oGeneralParams.SetProperty("Code", Code);
                oGeneralService.Delete(oGeneralParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
