using Anonimize.Migrator.IO;
using System;
using NLog;
using System.Runtime.InteropServices;
using Anonimize.Migrator.Services;
using System.Globalization;
using Anonimize.Services;

namespace Anonimize.Migrator
{
    class Program
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static JConfig jConfig;
        static XConfig xConfig;
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
            Console.WriteLine();
            logger.Info("Updating converters");

            RunUpdateService(new ConverterUpdateService(jConfig, xEntities));

            // Update database
            Console.WriteLine();
            logger.Info("Updating database");

            var iCryptoService = AnonimizeProvider.GetInstance().GetCryptoService();

            logger.Warn($"CryptoService: {iCryptoService.GetType().Name}");

            if(iCryptoService is BaseSymmetricCryptoService cryptoService)
            {
                if (string.IsNullOrWhiteSpace(xConfig.Iv))
                {
                    logger.Warn("Using default Anonimize:Iv");
                }
                else
                {
                    logger.Warn($"Anonimize:Iv: '{xConfig.Iv}'");
                }

                if (string.IsNullOrWhiteSpace(xConfig.Key))
                {
                    logger.Warn("Using default Anonimize:Key");
                }
                else
                {
                    logger.Warn($"Anonimize:Key: '{xConfig.Key}'");
                }
            }

            logger.Warn("Connection: {0}", xConfig.ConnectionString);
            
            RunUpdateService(new DatabaseUpdateService(jConfig, xConfig));

            DisposeAndExit();
        }

        static bool LoadResources()
        {
            try
            {
                jConfig = new JConfig();
                jConfig.ReadJsonDocument();

                xConfig = new XConfig();
                xConfig.ReadXmlDocument();

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

        static void RunUpdateService(AUpdateService service)
        {
            if (!CanProceed())
                DisposeAndExit();

            var success = service.Update();
            if (success)
            {
                logger.Info("Updated with success");
            }
            else
            {
                logger.Fatal("One or more errors occurred while updating");
                logger.Warn("Check log file for debug info");
                DisposeAndExit();
            }
        }

        static void DisposeAndExit(int? exitCode = null, bool exit = true)
        {
            NullSafeDispose(jConfig);
            NullSafeDispose(xConfig);
            NullSafeDispose(xEntities);

            if (exitCode.HasValue)
                logger.Warn($"Application terminated ({0}).", exitCode.Value);
            else
                logger.Debug($"Application terminated.");

            LogManager.Flush();

            if (exit)
            {
                Console.Write("Press any key to exit . . . ");
                Console.ReadKey();
                Console.WriteLine();
                Environment.Exit(exitCode ?? 0);
            }
        }

        static void NullSafeDispose(IDisposable disposable)
        {
            if (disposable != null)
                disposable.Dispose();
        }

        static bool CanProceed()
        {
            Console.Write("Proceed? (Y/n) ");

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
