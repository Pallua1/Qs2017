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

        // Tests the ID of an yet not started client.
        [TestMethod]
        public void TestActorID()
        {
            TestClient client = new TestClient();
            long expected = -1;

            Assert.AreEqual(expected, client.Id);
        }

        // Test the Publish and Retrieve Messages
        [TestMethod]
        public void TestPublishRetrieve()
        {
            //testing only the acks
            SimulatedActorSystem system = new SimulatedActorSystem();
            Dispatcher dispatcher = new Dispatcher(system, 2);
            system.Spawn(dispatcher);
            TestClient client = new TestClient();
            system.Spawn(client);
            // send request and run system until a response is received
            // communication id is chosen by clients 
            dispatcher.Tell(new InitCommunication(client, 11));
            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
            Message initAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(InitAck), initAckMessage.GetType());
            InitAck initAck = (InitAck)initAckMessage;
            Assert.AreEqual(11, initAck.CommunicationId);

            SimulatedActor worker = initAck.Worker;


            // Actual Testcase - Start 

            UserMessage message = new UserMessage("Felix", "Eyyyyy");

            worker.Tell(new Publish(message, 11));

            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);

            Message operationAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());
            OperationAck operationAck = (OperationAck)operationAckMessage;


            Assert.AreEqual(11, operationAck.CommunicationId);

            worker.Tell(new RetrieveMessages("Felix", 11));

            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);

            Message foundMessagesMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(FoundMessages), foundMessagesMessage.GetType());
            FoundMessages foundMessages = (FoundMessages)foundMessagesMessage;

            Assert.AreEqual(11, foundMessages.CommunicationId);
            Assert.AreEqual(message, foundMessages.Messages.FirstOrDefault());

            // Actual Testcase - End 

            initAck.Worker.Tell(new FinishCommunication(11));
            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);

            Message finAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(FinishAck), finAckMessage.GetType());
            FinishAck finAck = (FinishAck)finAckMessage;

            Assert.AreEqual(11, finAck.CommunicationId);
            dispatcher.Tell(new Stop());

            // TODO run system until workers and dispatcher are stopped
            int endtime = system.currentTime + 20;
            system.RunUntil(endtime);
            Assert.AreEqual(system.currentTime, endtime + 1);
        }

    }
}


//while (client.ReceivedMessages.Count == 0)
//    system.RunFor(1);

//Message operationAckMessage = client.ReceivedMessages.Dequeue();
//Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());
//OperationAck operationAck = (OperationAck)operationAckMessage;

//Assert.AreEqual(15, operationAck.CommunicationId);