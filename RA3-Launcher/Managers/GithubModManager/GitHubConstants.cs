using System;

namespace Managers.GithubModManager
{
    public static class GitHubConstants
    {
        // Константы для JSON параметров
        public const string DownloadUrlParam = "download_url";
        public const string SizeParam = "size";
        public const string ShaParam = "sha";
        public const string TypeParam = "type";
        public const string NameParam = "name";

        // Константы для Info.txt и VersionInfo.txt
        public const string DescriptionKey = "description";
        public const string CreationDateKey = "creation-date";
        public const string AuthorKey = "author";
        public const string CategoryKey = "category";
        public const string CategoriesKey = "categories";
        public const string GameVersionKey = "gameversion";
        public const string ChangelogKey = "changelog";
        public const string UpdateDateKey = "update-date";
        public const string UpdationDateKey = "updation-date";

        // Новые константы для дополнительных полей
        public const string WebsiteKey = "website";
        public const string RepositoryUrlKey = "repositoryUrl";
        public const string IsFeaturedKey = "isFeatured";
        public const string TotalDownloadsKey = "totalDownloads";
        public const string RequiredGameVersionKey = "gameversion"; // Для VersionInfo.txt

        // Константы для путей
        public const string ModsFolderName = "Mods";
        public const string VersionsFolderName = "Versions";
        public const string LanguagesFolderName = "Languages";
        public const string InfoFileName = "ModInfo.txt";
        public const string VersionInfoFileName = "VersionInfo.txt";
        public const string GitHubApiBaseUrl = "https://api.github.com/repos";

        // Константы для Git LFS
        public const string LfsVersionLine = "version https://git-lfs.github.com/spec/v1";
        public const string LfsOidPrefix = "oid sha256:";
        public const string LfsSizePrefix = "size ";

        // Формат даты RA3
        public const string DateTimeRA3Format = "yyyy-MM-dd:HH-mm";

        // Имя пользователя и репозиторий GitHub
        public const string RepositoryOwner = "Glaz-almaz-GL";
        public const string RepositoryName = "RA3-Translations";
    }

    public static class GitHubSettings
    {
        public static readonly string? GithubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    }
}
