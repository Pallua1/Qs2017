using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageBoard;
using ActorSystem;
using System.Collections.Generic;
using System.Linq;

namespace SimulatedActorUnitTests
{
    [TestClass]
    public class WorkerTests
    {
        static private TestClient testclient_;
        static private SimulatedActorSystem system_;
        static private Dispatcher dispatcher_;
        static private SimulatedActor worker_;

        [ClassInitialize]
        static public void setup(TestContext context)
        {
            system_ = new SimulatedActorSystem();
            dispatcher_ = new Dispatcher(system_, 2);
            system_.Spawn(dispatcher_);
        }

        [ClassCleanup]
        static public void cleanup()
        {
            dispatcher_.Tell(new Stop());

            int endtime = system_.currentTime + 13;
            system_.RunUntil(endtime);
            Assert.AreEqual(system_.currentTime, endtime + 1);
        }

        [TestInitialize]
        public void test_setup()
        {
            testclient_ = new TestClient();
            system_.Spawn(testclient_);

            dispatcher_.Tell(new InitCommunication(testclient_, 10));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            Message initAckMessage = testclient_.ReceivedMessages.Dequeue();
            InitAck initAck = (InitAck)initAckMessage;

            worker_ = initAck.Worker;
        }

        [TestCleanup]
        public void test_cleanup()
        {
            worker_.Tell(new FinishCommunication(10));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            Message finAckMessage = testclient_.ReceivedMessages.Dequeue();
        }



        /// <summary>
        /// Simple first test initiating a communication and closing it afterwards.
        /// </summary>
        [TestMethod]
        public void TestCommunication()
        {
            //testing only the acks
            SimulatedActorSystem system = new SimulatedActorSystem();
            Dispatcher dispatcher = new Dispatcher(system, 2);
            system.Spawn(dispatcher);
            TestClient client = new TestClient();
            system.Spawn(client);
            // send request and run system until a response is received
            // communication id is chosen by clients 
            dispatcher.Tell(new InitCommunication(client, 10));
            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
            Message initAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(InitAck), initAckMessage.GetType());
            InitAck initAck = (InitAck)initAckMessage;
            Assert.AreEqual(10, initAck.CommunicationId);

            SimulatedActor worker = initAck.Worker;

            initAck.Worker.Tell(new FinishCommunication(10));
            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);

            Message finAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(FinishAck), finAckMessage.GetType());
            FinishAck finAck = (FinishAck)finAckMessage;

            Assert.AreEqual(10, finAck.CommunicationId);
            dispatcher.Tell(new Stop());

            // TODO run system until workers and dispatcher are stopped
            int endtime = system.currentTime + 20;
            system.RunUntil(endtime);
            Assert.AreEqual(system.currentTime, endtime + 1);
        }

        // Test the Publish and Retrieve Messages
        [TestMethod]
        public void TestPublishRetrieve()
        {
            UserMessage message = new UserMessage("Felix", "Eyyyyy");

            worker_.Tell(new Publish(message, 10));

            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            Message operationAckMessage = testclient_.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());
            OperationAck operationAck = (OperationAck)operationAckMessage;


            Assert.AreEqual(10, operationAck.CommunicationId);

            worker_.Tell(new RetrieveMessages("Felix", 10));

            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            Message foundMessagesMessage = testclient_.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(FoundMessages), foundMessagesMessage.GetType());
            FoundMessages foundMessages = (FoundMessages)foundMessagesMessage;

            Assert.AreEqual(10, foundMessages.CommunicationId);
            Assert.AreEqual(message, foundMessages.Messages.FirstOrDefault());
        }
    }
}