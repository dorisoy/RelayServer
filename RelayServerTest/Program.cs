﻿using NetworkLibrary;
using NetworkLibrary.Components;
using NetworkLibrary.Components.Statistics;
using NetworkLibrary.Utils;
using ProtoBuf;
using Protobuff;
using Protobuff.P2P;
using RelayServer;
using RelayServer.HttpSimple;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace RrlayServerTest
{
    //KFtyEu7h6E
    public class Config
    {
        public int PortRelay { get; set; }
        public int PortHttp { get; set; }
        public bool UpdateProxy { get; set; }
        public string ProxyUri { get; set; }
    }
   

    internal class Program
    {
        static void Main(string[] args)
        {
            Run();

        }
        static Config config;
        static ManualResetEvent m = new ManualResetEvent(false);
        private static string uri;
        public static string Ip;
        public static string Port;
        

        private static void Run()
        {
            if (Environment.UserInteractive)
            {
                MiniLogger.AllLog += (string str) => Console.WriteLine(str);

            }
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

            Configure();
            uri = config.ProxyUri;
            var port = config.PortRelay;
            Port = port.ToString();

            var scert = new X509Certificate2("server.pfx", "greenpass");
            var server = new SecureProtoRelayServer(port, scert);

            var hserv = new SimpleHttpServer(server, config.PortHttp);

            if (config.UpdateProxy)
            {
                ProxyPeriodicUpdate.PeriodicallyUpdateHttpProxy();
            }
            m.WaitOne();
        }

        private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string ex = ((Exception)e.ExceptionObject).Message + ((Exception)e.ExceptionObject).StackTrace;
            string workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            File.WriteAllText(workingDir + "/CrashDump.txt", ex);

           // Process.Start(workingDir+ "/RelayServer.exe");
        }

        private static void Configure()
        {
            string workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string txt = File.ReadAllText(workingDir + "/Config.json");
            config = JsonSerializer.Deserialize<Config>(txt,new JsonSerializerOptions() { AllowTrailingCommas = true});

           
    }

    }
}