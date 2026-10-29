using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoExam
{
    internal static class Permissions
    {
        public static string roles;

        public static bool canEdit = false;
        public static bool IsAdmin => canEdit;
        public static bool IsClient => !canEdit;
        public static bool IsManager => canEdit;

        public static void AccessDvg(DataGridView dvg, bool canEdit) 
        {
            if (IsAdmin)
            {
                dvg.ReadOnly = canEdit;

            }
            else if (IsClient)
            {
                dvg.ReadOnly = canEdit;
                
            }
            else if (IsManager)
            {
                dvg.ReadOnly = canEdit;
            }
        }
    }
}
