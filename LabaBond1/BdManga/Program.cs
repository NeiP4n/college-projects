using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BdManga
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            List<User> users = new List<User>(); 
            Application.Run(new Login()); 
        }
    }
}
