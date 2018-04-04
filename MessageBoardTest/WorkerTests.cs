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
       // static private TestClient testclient2_;
        static private SimulatedActorSystem system_;
        static private Dispatcher dispatcher_;
        static private SimulatedActor worker_;
      //  static private SimulatedActor worker2_;

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
            //Testclient 1
            testclient_ = new TestClient();
            
            system_.Spawn(testclient_);

            dispatcher_.Tell(new InitCommunication(testclient_, 10));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            Message initAckMessage = testclient_.ReceivedMessages.Dequeue();
            InitAck initAck = (InitAck)initAckMessage;

            worker_ = initAck.Worker;

            //Testclient 2
            //testclient2_ = new TestClient();
            //system_.Spawn(testclient2_);
            //dispatcher_.Tell(new InitCommunication(testclient2_, 10));
            //while (testclient2_.ReceivedMessages.Count == 0)
            //    system_.RunFor(1);
            //Message initAckMessage2 = testclient2_.ReceivedMessages.Dequeue();
            //InitAck initAck2 = (InitAck)initAckMessage;

            //worker2_ = initAck2.Worker;
        }

        [TestCleanup]
        public void test_cleanup()
        {
            //Worker 1
            worker_.Tell(new FinishCommunication(10));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            testclient_.ReceivedMessages.Dequeue();


            ////Worker 2
            //worker2_.Tell(new FinishCommunication(10));
            //while (testclient2_.ReceivedMessages.Count == 0)
            //    system_.RunFor(1);

            //testclient2_.ReceivedMessages.Dequeue();
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

        [TestMethod]
        public void TestLikeDislike()
        {
            long comm_id = 10;

            //Create Message
            String author = "Group16";
            String message = "My message";
            UserMessage test_message = new UserMessage(author, message);

            //Publish Message
            worker_.Tell(new Publish(test_message, comm_id));

            //Get Message
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            //OperationAckMessage tells if Message was published
            Message operationAckMessage = testclient_.ReceivedMessages.Dequeue();
            OperationAck operationAck = (OperationAck)operationAckMessage;
            
            //Holt die Message vom entsprechenden Autor 
            worker_.Tell(new RetrieveMessages(author, comm_id));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            Message foundMessagesMessage = testclient_.ReceivedMessages.Dequeue();
            FoundMessages foundMessages = (FoundMessages)foundMessagesMessage;


            UserMessage userMessage = foundMessages.Messages.FirstOrDefault();

            Assert.AreEqual("My message", userMessage.Message);


            long message_id = userMessage.MessageId;


            Like like_test = new Like(testclient_.ToString(), comm_id, message_id);
            Dislike dislike_test = new Dislike(testclient_.ToString(), comm_id, message_id);


            //AddLike add_like = new AddLike(testclient_.ToString(), message_id , comm_id);
            //AddDislike add_dislike = new AddDislike(testclient_.ToString(), message_id, comm_id);

            //Publish like
            worker_.Tell(like_test);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());

            //Publish dislike
            worker_.Tell(dislike_test);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());


            worker_.Tell(new RetrieveMessages(author, comm_id));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            //Holt sich die Nachrichtt
             foundMessagesMessage = testclient_.ReceivedMessages.Dequeue();
             foundMessages = (FoundMessages)foundMessagesMessage;
            userMessage = foundMessages.Messages.FirstOrDefault();

            int num_likes = userMessage.Likes.Count;
            int num_dislikes = userMessage.Dislikes.Count;

            Assert.AreEqual(1, num_likes);
            Assert.AreEqual(1, num_dislikes);


            /*

            //Add like dislike with wrong id
            Like like_wrongid = new Like(testclient_.ToString(), comm_id, 5);
            Dislike dislike_wrongid = new Dislike(testclient_.ToString(), comm_id, 5);

            worker_.Tell(like_wrongid);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());

            //Publish dislike
            worker_.Tell(dislike_wrongid);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());
       
            */
        }
    }
}