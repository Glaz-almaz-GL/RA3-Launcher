using Huskui.Avalonia.Controls;

namespace Items
{
    public partial class MessageDialog : Dialog
    {
        protected override bool ValidateResult(object? result)
        {
            return true;
        }
    }
}
