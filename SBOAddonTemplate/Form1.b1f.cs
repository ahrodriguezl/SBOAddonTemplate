using System;
using System.Collections.Generic;
using System.Xml;
using SAPbouiCOM.Framework;
using Avantis.ORM;
using Avantis.Settings;

namespace Avantis
{
    [FormAttribute("SBOAddonTemplate.Form1", "Form1.b1f")]
    class Form1 : UserFormBase
    {
        public const string FORM_TYPE = "AVS_FT_FORM1";

        private SAPbouiCOM.Button bInstall;
        private SAPbouiCOM.Button bUninstall;

        public Form1()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.bInstall = ((SAPbouiCOM.Button)(this.GetItem("bInstall").Specific));
            this.bInstall.ClickBefore += new SAPbouiCOM._IButtonEvents_ClickBeforeEventHandler(this.Button0_ClickBefore);
            this.bUninstall = ((SAPbouiCOM.Button)(this.GetItem("bUninstall").Specific));
            this.bUninstall.ClickBefore += new SAPbouiCOM._IButtonEvents_ClickBeforeEventHandler(this.Button1_ClickBefore);
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
        }

        private void OnCustomInitialize()
        {
        }

        private void Button0_ClickBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
            int result = Application.SBO_Application.MessageBox("¿Deseas actualizar el add-on en la BD?", 1, "Si", "No");

            if (result != 1)
            {
                BubbleEvent = false;
                return;
            }

            CreateTables();
            EndProcess();

            BubbleEvent = true;
        }

        private void Button1_ClickBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
            int result = Application.SBO_Application.MessageBox("¿Deseas desinstalar el add-on en la BD?", 1, "Si", "No");

            if (result != 1)
            {
                BubbleEvent = false;
                return;
            }

            DropTables();
            EndProcess();

            BubbleEvent = true;
        }

        private void CreateTables()
        {
            var _db = new SBODatabase();
            var _tables = SBOTables.GetTables();

            for (int i = 0; i < _tables.Count; i++)
            {
                _db.CreateTable(_tables[i]);
            }

            SetCurrentVersion();
        }

        private void DropTables()
        {
            var _db = new SBODatabase();
            var _tables = SBOTables.GetTables();

            for (int i = 0; i < _tables.Count; i++)
            {
                _db.DropTable(_tables[i]);
            }
        }

        private static void SetCurrentVersion()
        {
            var _db = new SBODatabase();
            _db.CreateTable<SBOSetup>();

            var _setup = new SBOSetup();

            if (!_setup.GetByKey(SBOSetup.ADDON_CODE))
            {
                _setup.Code = SBOSetup.ADDON_CODE;
                _setup.Name = SBOSetup.ADDON_NAME;
            }

            _setup.Version = SBOSetup.ADDON_VERSION;
            _setup.Save();
        }

        private void EndProcess()
        {
            Application.SBO_Application.StatusBar.SetText("Actualización finalizada.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
        }
    }
}