using NLog;
using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace InvoiceServer.Common
{
    public sealed class Logger
    {
        // Logger names
        private const string UserActionLoggerName = "UserAction";
        private const string ErrorLoggerName = "Error";
        private const string SendEmailHistoryLoggerName = "EmailHistory";
        private const string TimeExportLoggerName = "LogTimeExport";
        private const string QuarztJobLoggerName = "Quarzt";

        private readonly NLog.Logger loggerDebug;
        private readonly NLog.Logger loggerUserAction;
        private readonly NLog.Logger loggerError;
        private readonly NLog.Logger loggerEmail;
        private readonly NLog.Logger loggerExport;
        private readonly NLog.Logger loggerQuarzt;

        private CultureInfo originalCultureInfo;

        public Logger(LogTypes logType = LogTypes.Default)
        {
            if ((logType & LogTypes.UserAction) == LogTypes.UserAction)
            {
                this.loggerUserAction = LogManager.GetLogger(UserActionLoggerName);
                this.loggerEmail = LogManager.GetLogger(SendEmailHistoryLoggerName);
                this.loggerExport = LogManager.GetLogger(TimeExportLoggerName);
                this.loggerQuarzt = LogManager.GetLogger(QuarztJobLoggerName);
            }

            if ((logType & LogTypes.Error) == LogTypes.Error)
            {
                this.loggerError = LogManager.GetLogger(ErrorLoggerName);
            }

            if ((logType & LogTypes.Debug) == LogTypes.Debug)
            {
                this.loggerDebug = LogManager.GetCurrentClassLogger();
            }
        }

        #region Common log methods

        public void UserAction(string userId, string ipAddress, string action, string message)
        {
            if (this.loggerUserAction == null)
            {
                return;
            }

            try
            {
                // Format: DateTime INFO [INSERT] 11.22.33.44 UserID LogMessage
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0} {1} {2} ", ipAddress, userId, action);
                sb.Append(message);

                var logEventInfo = new LogEventInfo()
                {
                    Level = NLog.LogLevel.Info,
                    Message = sb.ToString()
                };

                this.loggerUserAction.Log(logEventInfo);
            }
            catch
            {
                // Do nothing
            }
        }

        public void UserAction(string userId, string action, string message)
        {
            UserAction(userId, HttpUtils.GetClientIpAddress(), action, message);
        }

        public void Error(string userId, string ipAddress, Exception exception)
        {
            if (this.loggerError == null)
            {
                return;
            }

            try
            {
                ChangeCultureInfo();

                // Format: DateTime ERROR 11.22.33.44 UserID <Exception message> <NewLine> <Exception stacktrace>
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0} {1} ", ipAddress, userId);

                if (exception != null)
                {
                    sb.AppendLine();
                    sb.Append(exception.ToString());
                }
                var logEventInfo = new LogEventInfo()
                {
                    Level = NLog.LogLevel.Error,
                    Message = sb.ToString()
                };

                this.loggerError.Log(logEventInfo);
            }
            catch
            {
                // Do nothing
            }
            finally
            {
                RestoreCultureInfo();
            }
        }

        public void Error(string userId, Exception exception)
        {
            Error(userId, HttpUtils.GetClientIpAddress(), exception);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            if (this.loggerError == null)
            {
                return;
            }

            try
            {
                ChangeCultureInfo();

                // Format: DateTime ERROR 11.22.33.44 <Exception message> <NewLine> <Exception stacktrace>
                StringBuilder sb = new StringBuilder(HttpUtils.GetClientIpAddress());
                sb.Append(" ");
                if (args == null || args.Length == 0)
                {
                    sb.Append(message);
                }
                else
                {
                    sb.AppendFormat(message, args);
                }

                if (exception != null)
                {
                    sb.AppendLine();
                    sb.Append(exception.ToString());
                }
                var logEventInfo = new LogEventInfo()
                {
                    Level = NLog.LogLevel.Error,
                    Message = sb.ToString()
                };

                this.loggerError.Log(logEventInfo);
            }
            catch
            {
                // Do nothing
            }
            finally
            {
                RestoreCultureInfo();
            }
        }

        public void Trace(string message, params object[] args)
        {
            WriteLogDebug(LogLevel.Trace, message, args);
        }

        public void HistorySendEmail(string message, bool isSendSuccess)
        {
            if (this.loggerEmail == null)
            {
                return;
            }

            try
            {
                ChangeCultureInfo();

                var logEventInfo = new LogEventInfo()
                {
                    Level = NLog.LogLevel.Error,
                    Message = string.Format("{0}{1}Sended: {2}{3}--------------------", message, Environment.NewLine, isSendSuccess, Environment.NewLine)
                };

                this.loggerEmail.Log(logEventInfo);
            }
            catch
            {
                // Do nothing
            }
            finally
            {
                RestoreCultureInfo();
            }
        }

        public void TraceTimeExport(string action, string fileName, long timeProcess)
        {
            if (this.loggerExport == null)
            {
                return;
            }

            try
            {
                ChangeCultureInfo();

                var logEventInfo = new LogEventInfo()
                {
                    Level = NLog.LogLevel.Error,
                    Message = string.Format("Action : {0} file name :{1} time process : {2} Milliseconds", action, fileName, timeProcess)
                };

                this.loggerExport.Log(logEventInfo);
            }
            catch
            {
                // Do nothing
            }
            finally
            {
                RestoreCultureInfo();
            }
        }

        public void QuarztJob(bool isbug, string mesg)
        {
            if (this.loggerQuarzt == null)
            {
                return;
            }

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}: {1}  ", isbug ? "Bug" : "action", mesg);

                var logEventInfo = new LogEventInfo()
                {
                    Level = NLog.LogLevel.Info,
                    Message = sb.ToString()
                };

                this.loggerQuarzt.Log(logEventInfo);
            }
            catch
            {
                // Do nothing
            }

        }
        #endregion Common log methods

        #region Private methods

        private static NLog.LogLevel GetNLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace: return NLog.LogLevel.Trace;
                case LogLevel.Debug: return NLog.LogLevel.Debug;
                case LogLevel.Info: return NLog.LogLevel.Info;
                case LogLevel.Warn: return NLog.LogLevel.Warn;
                case LogLevel.Error: return NLog.LogLevel.Error;
                case LogLevel.Fatal: return NLog.LogLevel.Fatal;
                default: return NLog.LogLevel.Off;
            }
        }

        private void WriteLogDebug(LogLevel level, string message, params object[] args)
        {
            if (this.loggerDebug == null)
            {
                return;
            }

            try
            {
                var logEventInfo = new LogEventInfo()
                {
                    Level = GetNLogLevel(level),
                    Message = (args == null || args.Length == 0) ? message : string.Format(message, args),
                };

                this.loggerDebug.Log(logEventInfo);
            }
            catch
            {
                // Do nothing
            }
        }

        private void ChangeCultureInfo()
        {
            this.originalCultureInfo = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        private void RestoreCultureInfo()
        {
            Thread.CurrentThread.CurrentCulture = this.originalCultureInfo;
        }

        #endregion Private methods
    }

    #region Logger enums

    [Flags]
    public enum LogTypes
    {
        UserAction = 0x01,
        Error = 0x02,
        Debug = 0x04,
        All = UserAction | Error | Debug,
        //Default = UserAction | Error
        Default = All
    }

    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    #endregion Logger enums
}
