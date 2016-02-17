using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;


namespace UrlLoadTester
{
    
    class Program
    {
        const int NUMBER_CALLS_PER_THREAD = 500;
        const int NUMBER_THREADs = 50;

        class ThreadParams
        {
            public ManualResetEvent startEvent;
            public ManualResetEvent finishedEvent;
        }
        static void Main()
        {
            var taskList = new Task[NUMBER_THREADs];

            Console.WriteLine("Main on thread = {0}", Thread.CurrentThread.ManagedThreadId);
            System.Diagnostics.Debug.WriteLine("Main on thread = {0}", Thread.CurrentThread.ManagedThreadId);

            ManualResetEvent[] doneThreads = new ManualResetEvent[NUMBER_THREADs];
            ManualResetEvent syncStartThreads = new ManualResetEvent(false);

            for (int i = 0; i < NUMBER_THREADs; i++)
            {
                doneThreads[i] = new ManualResetEvent(false);
                ThreadParams threadParams = new ThreadParams() { startEvent = syncStartThreads, finishedEvent = doneThreads[i] };
                var thr = new Thread(new ParameterizedThreadStart(ThreadRun));
                thr.Start(threadParams);
            }

            syncStartThreads.Set();

            WaitHandle.WaitAll(doneThreads);
            Console.WriteLine("Finished processing {0} threads", NUMBER_THREADs);
            System.Diagnostics.Debug.WriteLine("Finished processing {0} threads", NUMBER_THREADs);
            Console.ReadLine();


           
        }

        static void ThreadRun(object threadParamsObj)
        {
            Console.WriteLine("ThreadRun {0} started", Thread.CurrentThread.ManagedThreadId);
            System.Diagnostics.Debug.WriteLine("ThreadRun {0} started", Thread.CurrentThread.ManagedThreadId);
            var threadParams = threadParamsObj as ThreadParams;
            //block until synced start
            threadParams.startEvent.WaitOne();
            var t = MultipleGetUrlAsync(NUMBER_CALLS_PER_THREAD);
            t.Wait();
            threadParams.finishedEvent.Set();
        }

       

        static async Task MultipleGetUrlAsync(int count)
        {
            for(int i=0;i<count;i++)
            {
                await GetUrlAsync(i);
            }
            return;
        }

        static async Task GetUrlAsync(int counter)
        {
            // ... Target page.
            string page = "http://traxm.tv/edf1234";

            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;


            // ... Use HttpClient.
            using (HttpClient client = new HttpClient(httpClientHandler))
            {
                var start = DateTime.Now;
                using (HttpResponseMessage response = await client.GetAsync(page))
                {
                    //get location header if it exists
                    var locationList = new List<string>() as IEnumerable<string>;
                    var location = string.Empty;
                    if (response.Headers.TryGetValues("Location", out locationList))
                    {
                        location = locationList.FirstOrDefault();
                    }
                    using (HttpContent content = response.Content)
                    {
                        // ... Read the string.
                        string result = await content.ReadAsStringAsync();
                        var len = string.IsNullOrEmpty(result) ? 0 : result.Length;
                        var end = DateTime.Now;
                        var durationmilliSec = (end - start).TotalMilliseconds;
                        Console.WriteLine("(threadid={0}, counteer={1}) Status={2} ({3}), Content Length={4}, durationMs={5}", Thread.CurrentThread.ManagedThreadId, counter, (int)response.StatusCode, response.StatusCode, len, durationmilliSec);
                        System.Diagnostics.Debug.WriteLine("(threadid={0}, counteer={1}) Status={2} ({3}), Content Length={4}, durationMs={5}", Thread.CurrentThread.ManagedThreadId, counter, (int)response.StatusCode, response.StatusCode, len, durationmilliSec);
                        //Console.WriteLine("\tLocation={0}", location);
                    }
                }
            }
        }
    }

}
