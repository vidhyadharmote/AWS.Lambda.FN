using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;

using Amazon.Lambda.SQSEvents;

//using System.Net.Sockets.NetworkStream;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.IO;
using System.Text;
using System.Threading;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Amazon.Runtime;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWS.Lambda.FN
{
    public class Functions
    {
        private IAmazonSQS SQSClient { get; set; }
        private IServiceProvider Services { get; set; }
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            // GetHtml(); configuration
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            var awsOptions = configuration.GetAWSOptions();
            awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
            serviceCollection.AddDefaultAWSOptions(awsOptions);
            serviceCollection.AddAWSService<IAmazonSQS>();
            Services = serviceCollection.BuildServiceProvider();
            SQSClient = Services.GetService<IAmazonSQS>();
        }
        public async Task TestStudent(SQSEvent evnt, ILambdaContext context)
        {
            List<Student> lstStudent = new List<Student>();
            foreach (var message in evnt.Records)
            {
                
                var req = JsonConvert.DeserializeObject<List<Student>>(message.Body);
                
                    foreach (var student in req)
                    {
                        if(string.IsNullOrEmpty(student.RoleNumber))
                        {
                            student.RoleNumber = Convert.ToString(Guid.NewGuid());
                            var messageBody = JsonConvert.SerializeObject(student);
                            await SendMessage("test-student-second-queue", messageBody, context);
                            context.Logger.LogLine("Sending to second queue : ");
                            context.Logger.LogLine("Role Numbers : " + student.RoleNumber.ToString());
                        }
                        else
                        {
                            var messageBody = JsonConvert.SerializeObject(req);
                            await SendMessage("test-student-second-queue", messageBody, context);
                        }
                    }                
              
            }
            await Task.CompletedTask;
        }
        public async Task TestStudentTwo(SQSEvent evnt, ILambdaContext context)
        {
            List<Student> lstStudent = new List<Student>();
            context.Logger.LogLine($"TestNew Batch Count {evnt.Records.Count}");
            //int i = 0;
            foreach (var message in evnt.Records)
            {
                context.Logger.LogLine($"TestNew Batch Count {message.Body}");
                var request = JsonConvert.DeserializeObject<Student>(message.Body);

                context.Logger.LogLine($"Details: {request.Name},{request.RoleNumber},{request.Address},{request.MobileNo}");
                // context.Logger.LogLine($"Message2: {a}");
                //context.Logger.LogLine($"Message a : {a}");
                //context.Logger.LogLine($"Message b : {a}");
                //context.Logger.LogLine($"Addition a+b:{a + b}");
            }
            await Task.CompletedTask;
        }

        private async Task SendMessage(string queueName, string messageBody, ILambdaContext context)
        {
            try
            {
                var response = await SQSClient.GetQueueUrlAsync(queueName);
                await SQSClient.SendMessageAsync(response.QueueUrl, messageBody);
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Send Message Error" + e.Message + " and " + e.InnerException);
            }
            await Task.CompletedTask;
        }

        public async Task GetHtml(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                //var html = await ExecuteAutoRetryRequest(message.body(), context, new Guid(), null);
                //context.Logger.LogLine($"Html :" + html);            
                //string urlAddress = "http://google.com";
                context.Logger.LogLine($"Url1 :" + message.Body);

                //string htmlContent = new System.Net.WebClient().DownloadString(message.Body);



                string result;
                using (HttpClient client = new HttpClient())
                {

                    using (HttpResponseMessage response = client.GetAsync(message.Body).Result)
                    {


                        using (HttpContent content = response.Content)
                        {
                            result = content.ReadAsStringAsync().Result;
                            context.Logger.LogLine($"Html1 :" + result);
                        }
                    }
                }

                //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(message.Body);
                //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //    if (response.StatusCode == HttpStatusCode.OK)
                //    {
                //        Stream receiveStream = response.GetResponseStream();
                //        StreamReader readStream = null;

                //        if (response.CharacterSet == null)
                //        {
                //            readStream = new StreamReader(receiveStream);
                //        }
                //        else
                //        {
                //            readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                //        }

                //        string data = readStream.ReadToEnd();
                //        context.Logger.LogLine($"Html :" + data);

                //        response.Close();
                //        readStream.Close();
                //        //context.Logger.LogLine($"Html2 :" + result);
                //    }
                //    await Task.CompletedTask;
                //}



                //    HttpWebRequest myHttpWebRequest1 =
                //(HttpWebRequest)WebRequest.Create(message.Body);

                //    myHttpWebRequest1.Timeout = Timeout.Infinite;
                //    myHttpWebRequest1.KeepAlive = true;
                //    //myHttpWebRequest1.Timeout = 10;
                //    // Assign the response object of HttpWebRequest to a HttpWebResponse variable.
                //    HttpWebResponse myHttpWebResponse1 =
                //      (HttpWebResponse)myHttpWebRequest1.GetResponse();

                //    Console.WriteLine("\nThe HTTP request Headers for the first request are: \n{0}", myHttpWebRequest1.Headers);
                //    Console.WriteLine("Press Enter Key to Continue..........");
                //    Console.Read();

                //    Stream streamResponse = myHttpWebResponse1.GetResponseStream();
                //    StreamReader streamRead = new StreamReader(streamResponse);
                //    Char[] readBuff = new Char[256];
                //    int count = streamRead.Read(readBuff, 0, 256);
                //    Console.WriteLine("The contents of the Html page are.......\n");
                //    while (count > 0)
                //    {
                //        String outputData = new String(readBuff, 0, count);
                //        context.Logger.LogLine($"Html :" + outputData);
                //        Console.Write(outputData);
                //        count = streamRead.Read(readBuff, 0, 256);
                //    }
                //    Console.WriteLine();
                //    // Close the Stream object.
                //    streamResponse.Close();
                //    streamRead.Close();
                //    // Release the resources held by response object.
                //    myHttpWebResponse1.Close();

            }
        }


            /// <summary>
            /// A Lambda function to respond to HTTP Get methods from API Gateway
            /// </summary>
            /// <param name="request"></param>
            /// <returns>The list of blogs</returns>


            //private async Task<string> ExecuteAutoRetryRequest(string url, ILambdaContext context, Guid scanRequestId, Guid? scanResultId, string StorageBucket = "", HorsemanRequestType reqType = HorsemanRequestType.HTML)
            //{
            //    string html = String.Empty;
            //    url = url.Replace("&amp;", "&");
            //    int numberOfRetry = NumberOfRetry;
            //    HorsemanRequest horsemanRequest = new HorsemanRequest()
            //    {
            //        ResponseLevel = HorsemanResponseLevel.Meta,
            //        Url = url,
            //        Steps = HeadlessHtmlSettings.HeadlessInstructions,
            //        Proxy = HeadlessHtmlSettings.Proxy,
            //        RequestType = reqType
            //    };

            //    if (StorageBucket != "")
            //    {
            //        horsemanRequest.StorageLocation = StorageBucket;
            //    }

            //    while (numberOfRetry > 0)
            //    {
            //        try
            //        {
            //            html = await ExecuteHorsemanRequest(horsemanRequest, context, scanRequestId, scanResultId);
            //            if (!string.IsNullOrEmpty(html) && !html.Contains("Access Denied") && !html.Contains("Internal Error"))
            //            {
            //                break;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            context.Logger.LogLine($"Exception during AutoRetry: {ex.Message}");
            //        }

            //        // Wait two seconds and try again
            //        Thread.Sleep(2000);
            //        numberOfRetry--;
            //    }

            //    return html;
            //}
        }

        //    private DomainHeadlessProcessingSettings HeadlessHtmlSettings
        //{
        //    get
        //    {
        //        if (_headlessHtmlSettings == null)
        //        {
        //            var domain = BPA.Core.Services.GeneralUtilities.ExtractDomainNameFromURL(ThirdPartyDomain, domainOnly: true);
        //            var settingsRequest = new DomainHeadlessProcessingSettings
        //            {
        //                Domain = domain,
        //                HeadlessProcessor = HeadlessProcessors.Horseman,
        //                HeadlessInstructionType = HeadlessInstructionTypes.Html
        //            };
        //            _headlessHtmlSettings = settingsRequest;
        //            var settingsResponse = BpaApiTpmiClient.Client.PostAsync("api/Utility/getdomainheadlessprocessingsettings", settingsRequest, new JsonMediaTypeFormatter()).Result;
        //            if (settingsResponse.IsSuccessStatusCode)
        //            {
        //                var settingsInfo = settingsResponse.Content.ReadAsAsync<ApiResponse<DomainHeadlessProcessingSettings>>().Result;
        //                _headlessHtmlSettings.Proxy = settingsInfo.Data.Proxy;
        //                _headlessHtmlSettings.HeadlessInstructions = settingsInfo.Data.HeadlessInstructions;
        //            }
        //        }
        //        return _headlessHtmlSettings;

        //    }
        //}  
}
