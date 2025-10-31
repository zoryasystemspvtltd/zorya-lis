using LIS.BusinessLogic.Helper;
using LIS.DtoModel;
using LIS.DtoModel.Models;
using LIS.Logger;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lis.Api
{
    public static class GlobalScheduler
    {
        private static ILogger _logger;
        private static System.Timers.Timer _timer;
        private static readonly int SchedulerIntervalMinute = Convert.ToInt32(ConfigurationManager.AppSettings["SchedulerIntervalMinute"]);
        private static AccuHealthDataSynchronizer _accuHealth;
        private static bool isRunning = false;

        public static void StartScheduler(ILogger logger,
            AccuHealthDataSynchronizer accuHealth)
        {
            _logger = logger;
            _accuHealth = accuHealth;
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 1000 * 60 * SchedulerIntervalMinute; // One Hour
            _timer.Enabled = true;
            isRunning = false;
            _logger.LogDebug("Synchronization Scheduler Enabled.");
        }

        private static async void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.LogInfo("Synchronization Scheduler Elapsed Started.");
            if (!isRunning)
            {
                isRunning = true;

                await _accuHealth.SynchronizeOrder();
                

                isRunning = false;
            }
            _logger.LogInfo("Synchronization Scheduler Elapsed End.");
            
        }

       
    }
}