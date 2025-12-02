using Avalonia.Data.Converters;
using Items.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA3_Launcher.Utils
{
    public static class ObjectConverters
    {
        public static readonly IValueConverter IsNotNullOrEmpty =
        new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s));

        public static readonly IValueConverter HasChangelog =
            new FuncValueConverter<List<ModVersion>?, bool>(versions =>
                versions?.FirstOrDefault()?.Changelog is string changelog &&
                !string.IsNullOrWhiteSpace(changelog));
    }
}
