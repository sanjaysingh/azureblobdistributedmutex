using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureBlobDistributedMutex;
using Microsoft.WindowsAzure.Storage;
namespace DistributedSynchronizedApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var storageAccount = ConfigurationManager.AppSettings["StorageAccount"];
            var container = ConfigurationManager.AppSettings["Container"];
            var blobName = ConfigurationManager.AppSettings["BlobName"];
            
            var settings = new BlobSettings(CloudStorageAccount.Parse(storageAccount), container, blobName);
            var mutex = new BlobDistributedMutex(settings, MainTask);
            mutex.RunTaskWhenMutexAcquired(cts.Token).Wait();

            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        private static async Task MainTask(CancellationToken token)
        {
            var interval = TimeSpan.FromSeconds(10);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("Doing main task");
                    await Task.Delay(interval, token);
                }
            }
            catch (OperationCanceledException)
            {
                Trace.TraceInformation("Aborting work, as the lease has been lost");
            }
        }
    }
}
