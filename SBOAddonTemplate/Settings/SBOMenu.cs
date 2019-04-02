using System;
using System.Collections.Generic;
using System.Text;
using SAPbouiCOM.Framework;

namespace Avantis.Settings
{
    class SBOMenu
    {
        #region CONSTANTS UID MENU

        public const string UID_AVS_MAIN_MENU = "M_AVS_BI";
        public const string UID_SM_FORM1 = "SM_FORM1";

        #endregion

        private static SBOMenu _instance = null;

        public static SBOMenu GetInstance()
        {
            if (_instance == null)
                _instance = new SBOMenu();

            return _instance;
        }

        public static void OpenUserForm<T>(string UniqueID) where T : new()
        {
            try
            {
                Application.SBO_Application.Forms.Item(UniqueID).Visible = true;
            }
            catch
            {
                var tmp = new T();
                var form = (UserFormBase)Convert.ChangeType(tmp, typeof(T));
                form.Show();
            }
        }

        public static void InsertMenuItem(string FatherUID, string UniqueID, string Caption, SAPbouiCOM.BoMenuType Type, string Image = null, int Position = -1)
        {
            string sImage = string.Empty;

            SAPbouiCOM.MenuItem oMenuItem = null;
            SAPbouiCOM.Menus oMenus = null;

            oMenuItem = Application.SBO_Application.Menus.Item(FatherUID);
            oMenus = oMenuItem.SubMenus;

            try
            {
                var newItem = oMenus.Add(UniqueID, Caption, Type, Position == -1 ? oMenus.Count + 1 : Position);

                if (!string.IsNullOrEmpty(Image))
                {
                    sImage = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Icons", Image);
                    newItem.Image = sImage;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("FatherUID: {0} | UniqueID: {1} | Type: {2} | Error: {3}", FatherUID, UniqueID, Type.ToString(), ex.Message));
            }
        }

        public static void RemoveMenuItem(string MenuUID)
        {
            try
            {
                var oMenuItem = Application.SBO_Application.Menus.Item(MenuUID);
                Application.SBO_Application.Menus.Remove(oMenuItem);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("UniqueID: {0} | Error: {1}", MenuUID, ex.Message));
            }
        }

        public static void Create()
        {
            #region [M_AVS_BI] AVANTIS

            InsertMenuItem("43520", UID_AVS_MAIN_MENU, "Avantis", SAPbouiCOM.BoMenuType.mt_POPUP);
            InsertMenuItem(UID_AVS_MAIN_MENU, UID_SM_FORM1, "Formulario 1", SAPbouiCOM.BoMenuType.mt_STRING);

            #endregion
        }

        public static void Destroy()
        {
            RemoveMenuItem(UID_AVS_MAIN_MENU);
        }

        public void SBO_Application_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (pVal.BeforeAction)
                {
                    switch (pVal.MenuUID)
                    {
                        case SBOMenu.UID_SM_FORM1:
                            OpenUserForm<Form1>(Form1.FORM_TYPE);
                            break;
                    }
                }
            }
            catch (Exception Ex)
            {
                Application.SBO_Application.StatusBar.SetText(Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
    }
}