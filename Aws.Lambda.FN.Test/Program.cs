using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Amazon.Lambda;
using System.Net;
using AWS.Lambda.FN;
using Newtonsoft.Json;

namespace Aws.Lambda.FN.Test
{
    class Program
    {

        //s private static IServiceProvider _services;

        private static IAmazonS3 _s3Client;
        private static IServiceProvider _services;
        private static IAmazonSQS SQSClient { get; set; }

        private static IHttpClientFactory _httpClientFactory;

        public static void Main(string[] args)
        {

            var serviceCollection = new ServiceCollection();
            var path = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var awsOptions = configuration.GetAWSOptions();
            serviceCollection.AddDefaultAWSOptions(awsOptions);
            serviceCollection.AddAWSService<IAmazonSQS>();
            _services = serviceCollection.BuildServiceProvider();
            SQSClient = _services.GetService<IAmazonSQS>();

            //Console.Write("Initial Stage...");

            //String url = "https://www.cars.com/";
            //Console.WriteLine("Sending Request with url: " +url);
            //var response = SQSClient.GetQueueUrlAsync("awsgethtmlqueue");
            //var qResp = SQSClient.SendMessageAsync(response.Result.QueueUrl, url).Result;

            //Console.Read();



            //SetService();
            //BeginTestAtRegionBucket();
            TestStudent();
        }
        static void BeginTestAtRegionBucket()
        {
            var httpClient = _httpClientFactory.CreateClient("guimp");

            var requestId = Guid.NewGuid().ToString();
            var testUrls = new List<string>();

            testUrls.Add("new-inventory/index.htm");
            foreach (var testUrl in testUrls)
            {
                Console.WriteLine("Requesting url " + testUrl);
                string result = httpClient.GetStringAsync("/").Result;

                Console.WriteLine("Got HTML for " + testUrl);
                Console.WriteLine("Putting file in results bucket...");

                // Put the HTML doc in your region's S3 bucket
                // Note: Bucket name is <your AWS account number>-tpmi-1p-<AWS region id for deployment, i.e. us-east-2>
                var putRequest = new PutObjectRequest
                {
                    BucketName = "mydemopro",
                    Key = Guid.NewGuid().ToString() + ".html",
                    ContentBody = result
                };
                putRequest.Metadata.Add("x-amz-meta-crawlid", requestId);
                putRequest.Metadata.Add("x-amz-meta-url", testUrl);
                var putResponse = _s3Client.PutObjectAsync(putRequest).Result;

                if (putResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Successfully put HTML file in results bucket!");
                }
                else
                {
                    Console.WriteLine("Failed to put HTML file in results bucket");
                }

                Console.WriteLine();
            }
        }
        public static void TestStudent()
        {
            List<Student> lstStudent = new List<Student>();
            Student student = new Student();

            student.Name = "Vidhayadhar";
            student.RollNumber = "";
            student.MobileNo = "992208587";
            student.Address = "Pune";
            lstStudent.Add(student);
             student = new Student();
            student.Name = "Vidhya";
            student.RollNumber = "";
            student.MobileNo = "992208587";
            student.Address = "Mumbai";
            lstStudent.Add(student);
            student = new Student();
            student.Name = "Satish";
            student.RollNumber = "";
            student.MobileNo = "9924322111";
            student.Address = "Nashik";
            lstStudent.Add(student);

            //foreach (Student currentSt in lstStudent)
            //{            
            //        Console.WriteLine("Dtls :"+currentSt.Name + " " + currentSt.RoleNumber + " " + currentSt.MobileNo + " " + currentSt.Address);

            //}
            //Console.Read();
            if (student.Name != null)
            {
                foreach (var item in lstStudent)
                {
                    Console.WriteLine("["+item.Name+", "+item.RollNumber + ", "+item.Address+", "+item.MobileNo+"]");
                }


                var messageData = JsonConvert.SerializeObject(lstStudent);              
                var response = SQSClient.GetQueueUrlAsync("test-student-queue");
                Console.WriteLine("Sending Student Data : ... ");
                var qResp = SQSClient.SendMessageAsync(response.Result.QueueUrl, messageData).Result;
               
            }
            else
            {
                Console.WriteLine("Empty list");
            }
            Console.ReadLine();
        }


        static void SetService()
        {
            #region AWS Account and Services setup
            // See https://aws.amazon.com/blogs/developer/configuring-aws-sdk-with-net-core/
            // for basics on how to use your AWSSDK credential store for local setup
            var serviceCollection = new ServiceCollection();
            var path = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var awsOptions = configuration.GetAWSOptions();

            serviceCollection.AddDefaultAWSOptions(awsOptions);
            serviceCollection.AddAWSService<IAmazonS3>();
            serviceCollection.AddHttpClient("guimp", client =>
            {
                client.BaseAddress = new Uri("https://www.acadianamazda.com/");
            });
            //serviceCollection.AddHttpClient<BpaApiTpmiClient>();
            serviceCollection.AddAWSService<IAmazonLambda>();

            _services = serviceCollection.BuildServiceProvider();
            _s3Client = _services.GetService<IAmazonS3>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            //_bpaApiTpmiClient = _services.GetService<BpaApiTpmiClient>();
            //_lambdaClient = _services.GetService<IAmazonLambda>();
            #endregion
        }

    }
}