﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace DemoStartUp
{
    using static CommonHelper;

    internal static class Program
    {
        private static void Main()
        {
            Title("Simple Remote Query [Server]");

            PrintSetup("Starting WCF service...");
            using var dataServiceHost = WcfHelper.CreateServiceHost<Server.TraditionalDataService>()
                .AddNetTcpEndpoint<Common.ServiceContract.ITraditionalDataService>("net.tcp://localhost:8080/traditionaldataservice")
                .OpenService();

            using var remoteLinqDataServiceHost = WcfHelper.CreateServiceHost<Server.RemoteLinqDataService>()
                .AddNetTcpEndpoint<Common.ServiceContract.IRemoteLinqDataService>("net.tcp://localhost:8080/remotelinqdataservice")
                .OpenService();

            PrintSetup("Staring client demo...");
            PrintSetup("-------------------------------------------------");
            Client.Program.RunQueries();

            PrintSetup();
            PrintSetup("-------------------------------------------------");
            PrintSetup("Done.");
            WaitForEnterKey();
        }
    }
}
