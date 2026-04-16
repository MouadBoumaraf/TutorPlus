using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using System.Windows;
using DevExpress.Xpf.Core;

namespace Gestion_De_Cours
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()    
        {
            string sourceFolder = @"C:\ProgramData\EcoleSetting\DLL";  // Update with your actual path

            // Get the directory of the running executable
            string targetFolder = Directory.GetCurrentDirectory();

            // Define the target folder as the "2.0" folder in the same directory as the executable
           

            try
            {
               
                // Get all DLL files in the source folder
                string[] files = Directory.GetFiles(sourceFolder, "*.dll");

                foreach (string file in files)
                {
                    // Get the file name without the directory
                    string fileName = Path.GetFileName(file);

                    // Define the destination path
                    string destFile = Path.Combine(targetFolder, fileName);

                    // Copy the file to the target directory
                    File.Copy(file, destFile, true); // Set 'true' to overwrite existing files
                }

                Console.WriteLine("DLLs copied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error copying DLLs: " + ex.Message);
            }
        }
    }
}
    