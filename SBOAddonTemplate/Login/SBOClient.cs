using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avantis.Login
{
    class SBOClient
    {
        private static SBOClient _instance = null;
        private SAPbobsCOM.Company _company = null;
        private SAPbouiCOM.Application _application = null;

        public static SBOClient GetInstance()
        {
            if (_instance == null)
                _instance = new SBOClient();

            return _instance;
        }

        public SAPbobsCOM.Company Company
        {
            get { return _company; }
            set { _company = value; }
        }

        public SAPbouiCOM.Application SBO_Application
        {
            get { return _application; }
            set
            {
                _company = (SAPbobsCOM.Company)value.Company.GetDICompany();
                _application = value;
            }
        }
    }
}