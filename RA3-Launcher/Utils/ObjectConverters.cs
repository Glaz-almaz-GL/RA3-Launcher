using Avalonia.Data.Converters;
using Items.Mod;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class ObjectConverters
    {
        public static readonly IValueConverter IsNotNullOrEmpty =
        new FuncValueConverter<string?, bool>(s => !string.IsNullOrWhiteSpace(s));

        public static readonly IValueConverter HasChangelog =
            new FuncValueConverter<List<ModVersionMetadata>?, bool>(versions =>
                versions?.FirstOrDefault()?.Changelog is string changelog &&
                !string.IsNullOrWhiteSpace(changelog));
    }
}
