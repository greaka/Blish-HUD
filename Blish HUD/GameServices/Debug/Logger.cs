using System;
using NLog;

namespace Blish_HUD
{
    public class Logger
    {
        private readonly NLog.Logger _internalLogger;

        private Logger(Type type)
        {
            this._internalLogger = LogManager.GetLogger(type.FullName);
        }

        public static Logger GetLogger(Type type)
        {
            return new Logger(type);
        }

        public static Logger GetLogger<T>()
        {
            return new Logger(typeof(T));
        }

        #region Trace

        /// <inheritdoc cref="NLog.Logger.Trace(string)" />
        public void Trace(string message)
        {
            this._internalLogger.Trace(message);
        }

        /// <inheritdoc cref="NLog.Logger.Trace(string, object[])" />
        public void Trace(string message, params object[] args)
        {
            this._internalLogger.Trace(message, args);
        }

        /// <inheritdoc cref="NLog.Logger.Trace(Exception, string)" />
        public void Trace(Exception exception, string message)
        {
            this._internalLogger.Trace(exception, message);
        }

        /// <inheritdoc cref="NLog.Logger.Trace(Exception, string, object[])" />
        public void Trace(Exception exception, string message, params object[] args)
        {
            this._internalLogger.Trace(exception, message, args);
        }

        #endregion

        #region Debug

        /// <inheritdoc cref="NLog.Logger.Debug(string)" />
        public void Debug(string message)
        {
            this._internalLogger.Debug(message);
        }

        /// <inheritdoc cref="NLog.Logger.Debug(string, object[])" />
        public void Debug(string message, params object[] args)
        {
            this._internalLogger.Debug(message, args);
        }

        /// <inheritdoc cref="NLog.Logger.Debug(Exception, string)" />
        public void Debug(Exception exception, string message)
        {
            this._internalLogger.Debug(exception, message);
        }

        /// <inheritdoc cref="NLog.Logger.Debug(Exception, string, object[])" />
        public void Debug(Exception exception, string message, params object[] args)
        {
            this._internalLogger.Debug(exception, message, args);
        }

        #endregion

        #region Info

        /// <inheritdoc cref="NLog.Logger.Info(string)" />
        public void Info(string message)
        {
            this._internalLogger.Info(message);
        }

        /// <inheritdoc cref="NLog.Logger.Info(string, object[])" />
        public void Info(string message, params object[] args)
        {
            this._internalLogger.Info(message, args);
        }

        /// <inheritdoc cref="NLog.Logger.Info(Exception, string)" />
        public void Info(Exception exception, string message)
        {
            this._internalLogger.Info(exception, message);
        }

        /// <inheritdoc cref="NLog.Logger.Info(Exception, string, object[])" />
        public void Info(Exception exception, string message, params object[] args)
        {
            this._internalLogger.Info(exception, message, args);
        }

        #endregion

        #region Warn

        /// <inheritdoc cref="NLog.Logger.Warn(string)" />
        public void Warn(string message)
        {
            this._internalLogger.Warn(message);
        }

        /// <inheritdoc cref="NLog.Logger.Warn(string, object[])" />
        public void Warn(string message, params object[] args)
        {
            this._internalLogger.Warn(message, args);
        }

        /// <inheritdoc cref="NLog.Logger.Warn(Exception, string)" />
        public void Warn(Exception exception, string message)
        {
            this._internalLogger.Warn(exception, message);
        }

        /// <inheritdoc cref="NLog.Logger.Warn(Exception, string, object[])" />
        public void Warn(Exception exception, string message, params object[] args)
        {
            this._internalLogger.Warn(exception, message, args);
        }

        #endregion

        #region Error

        /// <inheritdoc cref="NLog.Logger.Error(string)" />
        public void Error(string message)
        {
            this._internalLogger.Error(message);
        }

        /// <inheritdoc cref="NLog.Logger.Error(string, object[])" />
        public void Error(string message, params object[] args)
        {
            this._internalLogger.Error(message, args);
        }

        /// <inheritdoc cref="NLog.Logger.Error(Exception, string)" />
        public void Error(Exception exception, string message)
        {
            this._internalLogger.Error(exception, message);
        }

        /// <inheritdoc cref="NLog.Logger.Error(Exception, string, object[])" />
        public void Error(Exception exception, string message, params object[] args)
        {
            this._internalLogger.Error(exception, message, args);
        }

        #endregion

        #region Fatal

        /// <inheritdoc cref="NLog.Logger.Fatal(string)" />
        public void Fatal(string message)
        {
            this._internalLogger.Fatal(message);
        }

        /// <inheritdoc cref="NLog.Logger.Fatal(string, object[])" />
        public void Fatal(string message, params object[] args)
        {
            this._internalLogger.Fatal(message, args);
        }

        /// <inheritdoc cref="NLog.Logger.Fatal(Exception, string)" />
        public void Fatal(Exception exception, string message)
        {
            this._internalLogger.Fatal(exception, message);
        }

        /// <inheritdoc cref="NLog.Logger.Fatal(Exception, string, object[])" />
        public void Fatal(Exception exception, string message, params object[] args)
        {
            this._internalLogger.Fatal(exception, message, args);
        }

        #endregion
    }
}