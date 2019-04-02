using System;
using System.Collections.Generic;
using SAPbouiCOM.Framework;
using Avantis.Login;
using Avantis.Settings;

namespace Avantis
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SBOClient _client = SBOClient.GetInstance();

            try
            {
                Application _application = null;
                if (args.Length < 1)
                {
                    _application = new Application();
                }
                else
                {
                    _application = new Application(args[0]);
                }

                SBOMenu.Create();
                _client.Company = (SAPbobsCOM.Company)Application.SBO_Application.Company.GetDICompany();

                _application.RegisterMenuEventHandler(SBOMenu.GetInstance().SBO_Application_MenuEvent);
                
                Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);
                Application.SBO_Application.StatusBar.SetText(SBOSetup.ADDON_NAME + ": Iniciado correctamente.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                _application.Run();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    //Exit Add-On
                    System.Windows.Forms.Application.Exit();
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    break;
                default:
                    break;
            }
        }
    }
}
