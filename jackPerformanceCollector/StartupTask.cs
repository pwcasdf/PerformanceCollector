using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Background;

using Windows.System.Threading;
using System.Diagnostics;
using Microsoft.Azure.Devices.Client;
using Windows.System;
using Windows.System.Diagnostics;

namespace jackPerformanceCollector
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        ThreadPoolTimer timer = null;
        private const string DeviceConnectionString = "CONNECTION_STRING_HERE";
        private const string DeviceID = "DEVICE_ID_HERE";
        private DeviceClient data = DeviceClient.CreateFromConnectionString(DeviceConnectionString, DeviceID);
        private setSystemPerformance performance = new setSystemPerformance();

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();

            timer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(timer_tick), TimeSpan.FromSeconds(2));
        }

        private void timer_tick(ThreadPoolTimer timer)
        {
            var jsonMessage = string.Format("{{ displayname:null, location:\"Korea\", organization:\"MDS\", guid: \"41c2e437-6c3d-48d0-8e12-81eab2aa5013\", timecreated: \"{0}\", measurename: \"CPU\", unitofmeasure: \"%\", value:{1}}}", DateTime.UtcNow.ToString("o"), getCpuPerformance().ToString());
            this.SendMessage(jsonMessage);

            jsonMessage = string.Format("{{ displayname:null, location:\"Korea\", organization:\"MDS\", guid: \"41c2e437-6c3d-48d0-8e12-81eab2aa5013\", timecreated: \"{0}\", measurename: \"Memory\", unitofmeasure: \"%\", value:{1}}}", DateTime.UtcNow.ToString("o"), getMemoryPerformance().ToString());
            this.SendMessage(jsonMessage);
        }

        private int getCpuPerformance()
        {
            IReadOnlyList<ProcessDiagnosticInfo> list = ProcessDiagnosticInfo.GetForProcesses();

            double sumUsage = 0, sumTotal = 0;

            foreach (var item in list)
            {
                sumUsage += item.CpuUsage.GetReport().UserTime.Milliseconds;
                sumTotal += item.CpuUsage.GetReport().UserTime.Milliseconds + item.CpuUsage.GetReport().KernelTime.Milliseconds;
            }

            sumTotal = (sumUsage * 100 / sumTotal);

            return (int)sumTotal;
        }

        private int getMemoryPerformance()
        {
            IReadOnlyList<ProcessDiagnosticInfo> list = ProcessDiagnosticInfo.GetForProcesses();

            var a = MemoryManager.GetProcessMemoryReport();
            long b = (long)a.TotalWorkingSetUsage;
            long c = (long)a.PrivateWorkingSetUsage;

            return (int)(c * 100 / b);
        }

        public async void SendMessage(string message)
        {
            // Send message to an IoT Hub using IoT Hub SDK
            try
            {
                var content = new Message(Encoding.UTF8.GetBytes(message));
                await data.SendEventAsync(content);

                Debug.WriteLine("Message Sent: {0}", message, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception when sending message:" + e.Message);
            }
        }
    }
}
