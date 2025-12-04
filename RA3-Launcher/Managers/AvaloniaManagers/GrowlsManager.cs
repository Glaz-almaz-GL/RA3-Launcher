using Huskui.Avalonia.Controls;
using Huskui.Avalonia.Models;
using System;

namespace Managers.AvaloniaManagers
{
    public static class GrowlsManager
    {
        private static AppWindow? _appWindow;

        // Статический метод для инициализации
        public static void Initialize(AppWindow appWindow)
        {
            _appWindow = appWindow;
        }

        #region Методы показа сообщения
        public static GrowlItem? ShowInfoMsg(string msg, string? title = null)
        {
            return ShowGrowlMsg(GrowlLevel.Information, title, msg);
        }

        public static GrowlItem? ShowProgressInfoMsg(string msg, string? title = null, double progress = 0)
        {
            return ShowGrowlMsg(GrowlLevel.Information, title, msg, true, progress);
        }

        public static GrowlItem? ShowSuccessMsg(string msg, string? title = null)
        {
            return ShowGrowlMsg(GrowlLevel.Success, title, msg);
        }

        public static GrowlItem? ShowWarningMsg(string warnMsg, string? title = null)
        {
            return ShowGrowlMsg(GrowlLevel.Warning, title, warnMsg);
        }

        public static GrowlItem? ShowErrorMsg(Exception ex, string? title = "", bool showInnerEx = true)
        {
            string errorMessage = ex.Message;
            if (showInnerEx)
            {
                errorMessage += ex.InnerException?.Message ?? string.Empty;
            }

            return ShowGrowlMsg(GrowlLevel.Danger, title, errorMessage);
        }

        public static GrowlItem? ShowErrorMsg(string errMsg, string? title = null)
        {
            return ShowGrowlMsg(GrowlLevel.Danger, title, errMsg);
        }
        #endregion

        public static GrowlItem? ShowGrowlMsg(
            GrowlLevel growlLevel,
            string? title,
            string content,
            bool isProgressVisible = false,
            double progress = 0)
        {
            if (_appWindow == null)
            {
                return null;
            }

            GrowlItem growlItem = new()
            {
                Level = growlLevel,
                Title = GetDefaultTitle(growlLevel, title),
                Content = content,
                Progress = isProgressVisible ? progress : 0,
                IsProgressBarVisible = isProgressVisible
            };

            _appWindow.PopGrowl(growlItem);
            return growlItem;
        }

        #region Приватные помощники
        private static string GetDefaultTitle(GrowlLevel level, string? customTitle = null)
        {
            return !string.IsNullOrWhiteSpace(customTitle)
                ? customTitle
                : level switch
                {
                    GrowlLevel.Information => "Информация",
                    GrowlLevel.Success => "Успех",
                    GrowlLevel.Warning => "Предупреждение",
                    GrowlLevel.Danger => "Ошибка",
                    _ => "Сообщение"
                };
        }
        #endregion
    }
}
