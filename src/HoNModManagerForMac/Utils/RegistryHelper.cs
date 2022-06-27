﻿using System.Reflection;
using Microsoft.Win32;

namespace HonModManagerForMac.Utils
{
    public static class RegistryHelper
    {
        public static string GetRegistryEntry(string name)
        {
            try
            {
                var key = Registry.CurrentUser;
                key = key.OpenSubKey("Software");
                if (key == null) return "";
                key = key.OpenSubKey("Notausgang");
                if (key == null) return "";
                key = key.OpenSubKey("HoN_ModMan");
                if (key == null) return "";
                var myOutput = key.GetValue(name, "");
                key.Close();
                return (string)myOutput;
            }
            catch
            {
                return "";
            }
        }

        public static void SetRegistryEntry(string name, string value)
        {
            var key = Registry.CurrentUser;
            key = key.CreateSubKey("Software");
            if (key == null) return;
            key = key.CreateSubKey("Notausgang");
            if (key == null) return;
            key = key.CreateSubKey("HoN_ModMan");
            if (key == null) return;
            key.SetValue(name, value);
            key.Close();
        }

        public static void RegisterFileExtension()
        {
            try
            {
                var Key = Registry.ClassesRoot.CreateSubKey(".honmod");
                var OldReg = Key.GetValue("") as string;
                if (OldReg != "HoN_ModMan")
                    //SetRegistryEntry("oldreg", OldReg);
                    Key.SetValue("", "HoN_ModMan", RegistryValueKind.String);
                Registry.ClassesRoot.CreateSubKey("HoN_ModMan").SetValue("", "HoN Modification",
                    RegistryValueKind.String);
                Registry.ClassesRoot.CreateSubKey("HoN_ModMan\\shell\\open\\command").SetValue("",
                    Assembly.GetAssembly(typeof(MauiProgram)).CodeBase + //???????
                    " \"%l\"",
                    RegistryValueKind.String);
            }
            catch
            {
            }

            SetRegistryEntry("fileextension", "yes");
        }

        public static void UnregisterFileExtension()
        {
            try
            {
                var Key = Registry.ClassesRoot.CreateSubKey(".honmod");
                var OldReg = Key.GetValue("").ToString();
                var StoredReg = GetRegistryEntry("oldreg");
                if (OldReg == "HoN_ModMan")
                {
                    if (StoredReg != "")
                        Key.SetValue("", StoredReg, RegistryValueKind.String);
                    else
                        Registry.ClassesRoot.DeleteSubKeyTree(".honmod");
                }

                Registry.ClassesRoot.DeleteSubKeyTree("HoN_ModMan");
            }
            catch
            {
            }

            SetRegistryEntry("fileextension", "no");
        }
    }
}