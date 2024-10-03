using System.Collections;
using NUnit.Framework;
using System.Threading;
using UnityEngine.Networking;

namespace com.afjk.tinyhttpserver.tests
{
    [TestFixture]
    public class TinyHttpServerTest
    {
        private TinyHttpServer server;

        [SetUp]
        public void SetUp()
        {
            server = new TinyHttpServer
            {
                Port = 8080
            };
        }

        [Test]
        public void TestStartAndStopServer()
        {
            Assert.IsFalse(server.IsRunning);

            server.StartServer();
            Thread.Sleep(100); // Wait for the server to start

            Assert.IsTrue(server.IsRunning);

            server.StopServer();
            Thread.Sleep(100); // Wait for the server to stop

            Assert.IsFalse(server.IsRunning);
        }
    }
}