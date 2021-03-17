using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Newtonsoft.Json;
using StockAnalyzer.Core.Domain;

namespace StockAnalyzer.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private  async void Search_Click(object sender, RoutedEventArgs e)
        {
            #region Before loading stock data
            var watch = new Stopwatch();
            watch.Start();
            StockProgress.Visibility = Visibility.Visible;
            StockProgress.IsIndeterminate = true;
            #endregion

            // When we call this method we want to make sure that we await this as well because you've noticed that GetStock doesn't return anything by itself.
            // The task returned from an asynchronous method is a reference to the operation, its result potential errors
            // You should always use the await keywords when you call sth. that's async. We normally refer to this as using async all the way up.
            // if we were change to URL that'd throw an exception but we can't se on UI cuz Task propagete the exception inside the application then occur from code block without try catch block.
            // Exceptions occuring in an async void method cannot be caught , that swallow to the exception that doesn't work well. At the moment we use async task.
            try
            {
                await GetStocks();
            }
            catch (Exception ex)
            {

                Notes.Text += ex.Message;
            }

            
            #region After stock data is loaded
            StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
            StockProgress.Visibility = Visibility.Hidden;
            #endregion
        }

        /// <summary>
        /// Compose own async method
        /// </summary>
        /// Async void is pretty much evil, you should always avoid if you can.
        /// You could be allowed.The reason of course event handlers like as above if u want to asynchnorous code inside your event handlers, the delegates for an event is most certainly returning void.
        /// Using async void is 'only' appropriate for event handlers.
        /// Async 'Task' means that the method will in fact automatically return a reference to the ongoing operation which you could then await somewhere else.
        /// Methods marked as async task will automatically have Task returned without explicitly having return anything because complier will do all of that for as we don't need return smt.
        public async Task GetStocks()
        {
            using (var client = new HttpClient())
            {
                // Calling result or wait will block and potentially deadblock your application if u were use without await and requesting by response.Result.It will block the thread until this result is available. This is really problematic cuz this means that the code will run synchronously.Instead of response.Result we'll use 'await' keyword that only proceed when this is done.
                //The await keyword is way for us to indicate that we want to get the result out of this asynchronous operation only once the data is available without blocking the current thread.
                var response = await client.GetAsync($"http://localhost:61363/api/stocks/{Ticker.Text}");
                try
                {
                    // The await keyword introduces a 'continuation', allowing us to get back to the original context(thread)
                    // After the 'await' keyword we're inside something called a continuation and when the using the away keyword the continuation allows us to work on the orginal context.This neans that we don't have to worry about working across different threads that makes a lot easier 
                    // The await keyword
                    // - Gives you a potential result
                    // - Validates the success of the operation
                    // - Continuation is back on calling thread

                    // Summary
                    //DO NOT
                    //- Don't call Result or Wait() that are perfroms sync. So deadblock occur and applications won't work until retrieve the data 
                    //- In the continuation after you've done await, it is tataly fine to use the Result property.
                    // DO
                    // -Always use async and await together 
                    // -Always return a Task from async. method.
                    // -Always await an async. method to validate the operation.
                    // -Use async and await all the way up the chain. 
                    
                    response.EnsureSuccessStatusCode();
                    //The await keyword will pause execution of the method until a result is available without blocking the calling (UI) thread.
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);

                    Stocks.ItemsSource = data;
                }
                catch (Exception ex)
                {
                    Notes.Text += ex.Message;

                }

            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }
        

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
