﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Blish_HUD.Debug;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Blish_HUD
{
    public class DebugService : GameService
    {
        #region Debug Overlay

        public void DrawDebugOverlay(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var debugLeft = Graphics.WindowWidth - 750;

            spriteBatch.DrawString(Content.DefaultFont14, $"FPS: {Math.Round(Debug.FrameCounter.CurrentAverage, 0)}",
                new Vector2(debugLeft, 25), Color.Red);

            var i = 0;
            foreach (var timedFuncPair in this._funcTimes.Where(ft => ft.Value.GetAverage() > 1)
                .OrderByDescending(ft => ft.Value.GetAverage()))
            {
                spriteBatch.DrawString(Content.DefaultFont14,
                    $"{timedFuncPair.Key} {Math.Round(timedFuncPair.Value.GetAverage())} ms",
                    new Vector2(debugLeft, 50 + i * 25), Color.Orange);
                i++;
            }

            spriteBatch.DrawString(Content.DefaultFont14, $"3D Entities Displayed: {Graphics.World.Entities.Count}",
                new Vector2(debugLeft, 50 + i * 25), Color.Yellow);
            i++;
            spriteBatch.DrawString(Content.DefaultFont14, "Render Late: " + (gameTime.IsRunningSlowly ? "Yes" : "No"),
                new Vector2(debugLeft, 50 + i * 25), Color.Yellow);
            i++;
            spriteBatch.DrawString(Content.DefaultFont14, "ArcDPS Bridge: " + (ArcDps.RenderPresent ? "Yes" : "No"),
                new Vector2(debugLeft, 50 + i * 25), Color.Yellow);
            i++;
            spriteBatch.DrawString(Content.DefaultFont14, "IsHudActive: " + (ArcDps.HudIsActive ? "Yes" : "No"),
                new Vector2(debugLeft, 50 + i * 25), Color.Yellow);
#if DEBUG
            i++;
            spriteBatch.DrawString(Content.DefaultFont14, "Counter: " + Interlocked.Read(ref ArcDpsService.Counter),
                new Vector2(debugLeft, 50 + i * 25), Color.Yellow);
#endif
        }

        #endregion

        #region Logging

        private static Logger Logger;

        private static LoggingConfiguration _logConfiguration;

        // ${message}
        private const string STANDARD_LAYOUT =
            @"${time:invariant=true}|${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=toString}${newline}}";

        internal static void InitDebug()
        {
            // Make sure crash dir is available for logs as early as possible
            var logPath = DirectoryUtil.RegisterDirectory("logs");

            // Init the Logger
            _logConfiguration = new LoggingConfiguration();

            var headerLayout = $"Blish HUD v{Program.OverlayVersion}";

            var logFile = new FileTarget("logfile")
            {
                Header = headerLayout,
                FileNameKind = FilePathKind.Absolute,
                ArchiveFileKind = FilePathKind.Absolute,
                FileName = Path.Combine(logPath, "blishhud.${cached:${date:format=yyyyMMdd-HHmmss}}.log"),
                ArchiveFileName = Path.Combine(logPath, "blishhud.{#}.log"),
                ArchiveDateFormat = "yyyyMMdd-HHmmss",
                ArchiveNumbering = ArchiveNumberingMode.Date,
                MaxArchiveFiles = 9,
                EnableFileDelete = true,
                CreateDirs = true,
                Encoding = Encoding.UTF8,
                KeepFileOpen = true,
                Layout = STANDARD_LAYOUT
            };

            var asyncLogFile = new AsyncTargetWrapper("asynclogfile", logFile)
            {
                QueueLimit = 200,
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
                ForceLockingQueue = false
            };

            _logConfiguration.AddTarget(asyncLogFile);

            _logConfiguration.AddRule(LogLevel.Info, LogLevel.Fatal, asyncLogFile);

            AddDebugTarget(_logConfiguration);

            LogManager.Configuration = _logConfiguration;

            Logger = Logger.GetLogger<DebugService>();
        }

        public static void TargetDebug(string time, string level, string logger, string message)
        {
            System.Diagnostics.Debug.WriteLine($"{time}|{level.ToUpper()}|{logger}|{message}");
        }

        [Conditional("DEBUG")]
        private static void AddDebugTarget(LoggingConfiguration logConfig)
        {
            LogManager.ThrowExceptions = true;

            var logDebug = new MethodCallTarget("logdebug")
            {
                ClassName = typeof(DebugService).AssemblyQualifiedName,
                MethodName = nameof(TargetDebug),
                Parameters =
                {
                    new MethodCallParameter("${time:invariant=true}"),
                    new MethodCallParameter("${level}"),
                    new MethodCallParameter("${logger}"),
                    new MethodCallParameter("${message}")
                }
            };

            _logConfiguration.AddTarget(logDebug);
            _logConfiguration.AddRule(LogLevel.Debug, LogLevel.Fatal, logDebug);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            InputService.mouseHook?.UnhookMouse();
            InputService.keyboardHook?.UnhookKeyboard();

            var e = (Exception) args.ExceptionObject;

            Logger.Fatal(e, "Blish HUD encountered a fatal crash!");
        }

        #endregion

        #region FPS

        private const int FRAME_DURATION_SAMPLES = 100;

        public FrameCounter FrameCounter { get; private set; }

        #endregion

        #region Measuring

        private const int DEFAULT_DEBUGCOUNTER_SAMPLES = 60;

        private ConcurrentDictionary<string, DebugCounter> _funcTimes;

        /// <summary>
        /// </summary>
        /// <param name="func"></param>
        [Conditional("DEBUG")]
        public void StartTimeFunc(string func)
        {
            StartTimeFunc(func, DEFAULT_DEBUGCOUNTER_SAMPLES);
        }

        [Conditional("DEBUG")]
        public void StartTimeFunc(string func, int length)
        {
            if (!this._funcTimes.ContainsKey(func))
            {
                this._funcTimes.TryAdd(func, new DebugCounter(length));
            }
            else
            {
                this._funcTimes[func].StartInterval();
            }
        }

        [Conditional("DEBUG")]
        public void StopTimeFunc(string func)
        {
            this._funcTimes[func].EndInterval();
        }

        [Conditional("DEBUG")]
        public void StopTimeFuncAndOutput(string func)
        {
            this._funcTimes[func].EndInterval();
            Logger.Debug("{funcName} ran for {$funcTime}.", func,
                this._funcTimes[func]?.GetTotal().Seconds().Humanize());
        }

        #endregion

        #region Service Implementation

        protected override void Initialize()
        {
            this.FrameCounter = new FrameCounter(FRAME_DURATION_SAMPLES);

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
        }

        protected override void Load()
        {
            this._funcTimes = new ConcurrentDictionary<string, DebugCounter>();
        }

        protected override void Update(GameTime gameTime)
        {
            this.FrameCounter.Update(gameTime.GetElapsedSeconds());
        }

        protected override void Unload()
        {
            /* NOOP */
        }

        #endregion
    }
}