using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Huskui.Avalonia.Controls;
using RA3_Launcher.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Managers
{
    public static class DialogsManager
    {
        private static TopLevel? _topLevel;
        private static AppWindow? _appWindow;

        // Статический метод для инициализации
        public static void Initialize(TopLevel topLevel, AppWindow appWindow)
        {
            _topLevel = topLevel;
            _appWindow = appWindow;
        }

        #region Методы диалогов

        public static async Task<bool?> ShowMsgDialogAsync(
            string message,
            string title,
            bool showPrimaryButton,
            string? primaryButtonText,
            string? secondaryButtonText)
        {
            if (_appWindow == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title cannot be null or empty", nameof(title));
            }

            MessageDialog dialog = new()
            {
                Title = title,
                Content = message,
                PrimaryText = primaryButtonText ?? "Ok",
                SecondaryText = secondaryButtonText ?? "Cancel",
                IsPrimaryButtonVisible = showPrimaryButton
            };

            _appWindow.PopDialog(dialog);
            return await dialog.CompletionSource.Task;
        }

        #endregion Методы диалогов

        #region Методы выбора папок

        public static async Task<IStorageFolder?> ShowOpenSingleFolderDialogAsync(string title)
        {
            if (_topLevel == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title cannot be null or empty", nameof(title));
            }

            var options = CreateFolderPickerOptions(title, false);
            var folder = await _topLevel.StorageProvider.OpenFolderPickerAsync(options);

            return folder.Count > 0 ? folder[0] : null;
        }

        public static async Task<List<IStorageFolder>> ShowOpenMultipleFolderDialogAsync(string title)
        {
            if (_topLevel == null)
            {
                return [];
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title cannot be null or empty", nameof(title));
            }

            var options = CreateFolderPickerOptions(title, false);
            var folders = await _topLevel.StorageProvider.OpenFolderPickerAsync(options);

            return (List<IStorageFolder>)(folders.Count > 0 ? folders : []);
        }

        #endregion Методы выбора папок

        #region Методы выбора файлов

        public static async Task<IStorageFile?> ShowOpenSingleFileDialogAsync(string title, IEnumerable<string>? allowedExtensions = null)
        {
            if (_topLevel == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title cannot be null or empty", nameof(title));
            }

            var options = CreateFilePickerOptions(title, allowedExtensions, false);
            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(options);

            return files.Count > 0 ? files[0] : null;
        }

        public static async Task<List<IStorageFile>> ShowOpenMultipleFilesDialogAsync(string title, IEnumerable<string>? allowedExtensions = null)
        {
            if (_topLevel == null)
            {
                return [];
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title cannot be null or empty", nameof(title));
            }

            var options = CreateFilePickerOptions(title, allowedExtensions, true);
            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(options);

            return (List<IStorageFile>)(files.Count > 0 ? files : []);
        }

        #endregion Методы выбора файлов

        #region Приватные помощники

        private static FilePickerOpenOptions CreateFilePickerOptions(
            string title,
            IEnumerable<string>? allowedExtensions,
            bool allowMultiple)
        {
            FilePickerOpenOptions options = new()
            {
                Title = title,
                AllowMultiple = allowMultiple
            };

            if (allowedExtensions?.Any() == true)
            {
                options.FileTypeFilter =
                [
                    new FilePickerFileType(title)
                    {
                        Patterns = [.. allowedExtensions]
                    }
                ];
            }

            return options;
        }

        private static FolderPickerOpenOptions CreateFolderPickerOptions(
            string title,
            bool allowMultiple)
        {
            FolderPickerOpenOptions options = new()
            {
                Title = title,
                AllowMultiple = allowMultiple
            };

            return options;
        }

        #endregion Приватные помощники
    }
}