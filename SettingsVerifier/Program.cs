﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CadEditor;

namespace SettingsEditor
{
    class Program
    {
        private static int totalVerified, totalFailed;

        static void Main(string[] args)
        {
            var counter = 0;
            Console.WriteLine("Checking settings files...");
            string rootDirName = Path.GetFullPath(".");
            var dirNames = new List<string>();
            dirNames.AddRange(Directory.GetDirectories(rootDirName + "\\settings_nes", "*", SearchOption.AllDirectories));
            dirNames.AddRange(Directory.GetDirectories(rootDirName + "\\settings_smd", "*", SearchOption.AllDirectories));
            dirNames.AddRange(Directory.GetDirectories(rootDirName + "\\settings_gb" , "*", SearchOption.AllDirectories));
            dirNames.AddRange(Directory.GetDirectories(rootDirName + "\\settings_gba", "*", SearchOption.AllDirectories));

            foreach (var dirName in dirNames)
            {
                string[] fileNames = Directory.GetFiles(dirName, "Settings_*.cs");
                foreach (var f in fileNames)
                {
                    counter++;
                    if (counter > 900) //change to verify only some part of configs
                    {
                        checkAndPrint(f);
                    }
                }
            }
            Console.ResetColor();
            Console.WriteLine("Total verified files: {0}", totalVerified);
            Console.WriteLine("Total failed files  : {0}", totalFailed);
            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        static void checkAndPrint(string filename)
        {
            bool result = checkFile(filename);
            if (result)
            {
                totalVerified++;
            }
            else
            {
                totalFailed++;
            }
            Console.ForegroundColor = result ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(result ? "File verified: {0}" : "File not verified: {0}", filename);
        }

        static bool checkFile(string filename)
        {
            try
            {
                ConfigScript.LoadFromFile(filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
