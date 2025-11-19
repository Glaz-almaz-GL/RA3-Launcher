using Huskui.Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA3_Launcher.Items
{
    public partial class MessageDialog : Dialog
    {
        protected override bool ValidateResult(object? result) => true;
    }
}
