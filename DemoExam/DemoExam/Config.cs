using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoExam
{
    internal static class Config
    {
        // строка подключения
        public static string connPath = Properties.Settings.Default.ShopDbConnectionString;
    }
}
