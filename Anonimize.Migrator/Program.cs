using System.IO;
using Anonimize.Migrator.JSON;
using Anonimize.Migrator.XML;
using System;
using NLog;
using System.Runtime.InteropServices;
using Anonimize.Migrator.Services;
using System.Globalization;

namespace Anonimize.Migrator
{
    class Program
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static JConfig jConfig;
        static XAppConfig xAppConfig;
        static XEntities xEntities;

        static void Main()
        {
            logger.Info("|-----------------------------------------|");
            logger.Info("|-------------- Anonimizer ---------------|");
            logger.Info("|-----------------------------------------|");

            SetConsoleCtrlHandler(new ConsoleCtrlHandler(OnConsoleEvent), true);

            CultureInfo.CurrentCulture = new CultureInfo("pt-PT");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-PT");

            logger.Info("Start: {0}", DateTime.Now.ToString());

            if (!LoadResources())
                DisposeAndExit();

            // Update converters
            logger.Info("Updating converters");

            if (!CanProceed())
                DisposeAndExit();

            var converterService = new ConverterUpdateService(jConfig, xEntities);
            if (converterService.Update())
            {
                logger.Info("Updated with success");
            }
            else
            {
                logger.Fatal("One or more errors occurred while updating");
                logger.Warn("Check log file for debug info");
                DisposeAndExit();
            }
            
            // Update database
            logger.Info("Updating database");
            logger.Warn("Connection: {0}", xAppConfig.ConnectionString);

            if (!CanProceed())
                DisposeAndExit();

            var databaseService = new DatabaseUpdateService(jConfig, xAppConfig);
            if (databaseService.Update())
            {
                logger.Info("Updated with success");
            }
            else
            {
                logger.Fatal("One or more errors occurred while updating");
                logger.Warn("Check log file for debug info");
                DisposeAndExit();
            }

            DisposeAndExit();
        }

        static bool LoadResources()
        {
            try
            {
                jConfig = new JConfig();
                jConfig.ReadJsonDocument();

                xAppConfig = new XAppConfig();
                xAppConfig.ReadXmlDocument();

                xEntities = new XEntities();
                xEntities.ReadXmlDocument();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.Message);
                logger.Debug(ex);
                return false;
            }

            return true;
        }

        static void DisposeAndExit(int? exitCode = null, bool pressKey = true)
        {
            NullSafeDispose(jConfig);
            NullSafeDispose(xAppConfig);
            NullSafeDispose(xEntities);

            if (exitCode.HasValue)
                logger.Warn($"Application terminated ({0}).", exitCode.Value);
            else
                logger.Debug($"Application terminated.");

            LogManager.Flush();

            if (pressKey)
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }

            Environment.Exit(exitCode ?? 0);
        }

        static void NullSafeDispose(IDisposable disposable)
        {
            if (disposable != null)
                disposable.Dispose();
        }

        static bool CanProceed()
        {
            Console.WriteLine("Proceed? Y/n");

            bool? proceed = null;

            do
            {
                var keyInfo = Console.ReadKey();
                var key = keyInfo.Key;

                switch (key)
                {
                    case ConsoleKey.Escape:
                    case ConsoleKey.N:
                        proceed = false;
                        break;
                    case ConsoleKey.Enter:
                    case ConsoleKey.Y:
                        proceed = true;
                        break;
                    default:
                        break;
                }

            } while (!proceed.HasValue);

            Console.WriteLine();

            return proceed.Value;
        }

        #region ConsoleCtrlHandler
        delegate bool ConsoleCtrlHandler(int eventType);

        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler ctrlHandler, bool add);

        static bool OnConsoleEvent(int eventType)
        {
            DisposeAndExit(eventType, false);
            return false;
        }
        #endregion
    }
}
