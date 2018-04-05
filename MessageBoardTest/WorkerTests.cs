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
            //Testclient 1
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
            //Worker 1
            worker_.Tell(new FinishCommunication(10));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            testclient_.ReceivedMessages.Dequeue();

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
            UserMessage message = new UserMessage("group-16", "First M");

            worker_.Tell(new Publish(message, 10));

            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            Message operationAckMessage = testclient_.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());
            OperationAck operationAck = (OperationAck)operationAckMessage;


            Assert.AreEqual(10, operationAck.CommunicationId);

            worker_.Tell(new RetrieveMessages("group-16", 10));

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




            //Add like dislike with wrong id
            Like like_wrongid = new Like(testclient_.ToString(), comm_id, 5);
            Dislike dislike_wrongid = new Dislike(testclient_.ToString(), comm_id, 5);

            //Publish wrong like
            worker_.Tell(like_wrongid);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Console.WriteLine(operationAckMessage);
            Assert.AreEqual(typeof(OperationFailed), operationAckMessage.GetType());


            //Publish wrong dislike
            worker_.Tell(dislike_wrongid);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Assert.AreEqual(typeof(OperationFailed), operationAckMessage.GetType());



            //Add like/dislike 2nd time with same client and same message;
            Like like_wrongclient = new Like(testclient_.ToString(), comm_id, message_id);
            Dislike dislike_wrongclient = new Dislike(testclient_.ToString(), comm_id, message_id);

            //Publish 2nd like form same client on same message
            worker_.Tell(like_wrongclient);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Console.WriteLine(operationAckMessage);
            Assert.AreEqual(typeof(OperationFailed), operationAckMessage.GetType());


            //Publish 2nd dislike form same client on same message
            worker_.Tell(dislike_wrongclient);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            operationAckMessage = testclient_.ReceivedMessages.Dequeue(); //OperationAckMessage tells if Message was published
            Assert.AreEqual(typeof(OperationFailed), operationAckMessage.GetType());



        }


        [TestMethod]
        public void TestMessageId()
        {
            //Message with same Id will be published
            //----------------------------------------------------------
            long comm_id = 10;

            //Create Message
            String author = "group-16";
            String message = "same";
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


            UserMessage same_message = foundMessages.Messages.FirstOrDefault();
            //Publish Message
            worker_.Tell(new Publish(same_message, comm_id));
            //Get Message
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            //operationFailedMessage tells if Message was published
            Message operationFailedMessage = testclient_.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationFailed), operationFailedMessage.GetType());

        }

        [TestMethod]
        public void TestMessageStore()
        {
            //same message will be published two times (second time already has an Message ID)
            //----------------------------------------------------------

            long comm_id = 10;

            //Create Message
            String author = "group-16";
            String message = "same mess";
            UserMessage test_message = new UserMessage(author, message);

            //Publish Message
            worker_.Tell(new Publish(test_message, comm_id));
            //Get Message
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
            //OperationAckMessage tells if Message was published
            Message operationAckMessage = testclient_.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationAck), operationAckMessage.GetType());
            OperationAck operationAck = (OperationAck)operationAckMessage;

            //Publish Message
            worker_.Tell(new Publish(test_message, comm_id));
            //Get Message
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            //operationFailedMessage tells if Message was published
            Message operationFailedMessage = testclient_.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationFailed), operationFailedMessage.GetType());

            //same Message but with new MessageID
            //--------------------------------------------------------------------------
            UserMessage test_message2 = new UserMessage(author, message);

            //Publish Message
            worker_.Tell(new Publish(test_message2, comm_id));
            //Get Message
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

            //operationFailedMessage tells if Message was published
            operationFailedMessage = testclient_.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationFailed), operationFailedMessage.GetType());


            //test toString Method
            //--------------------------------------------------------------------------
            Assert.AreEqual("group-16:same mess liked by : disliked by :", test_message.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownClientException), "Unknown communication ID")]
        public void TestUnknownClientException()
        {
            long wrong_comm_id = 12;
            TestClient unknownclient = new TestClient();

            String author = "Group16";
            String message = "My message";
            UserMessage test_message = new UserMessage(author, message);

            worker_.Tell(new Publish(test_message, wrong_comm_id));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

        }

        [TestMethod]
        [ExpectedException(typeof(UnknownClientException), "Unknown communication ID")]
        public void TestUnknownClientExceptionLike()
        {
            long wrong_comm_id = 12;
            long random_message_id = 3;

            Like test_like = new Like(testclient_.ToString(), wrong_comm_id, random_message_id);
            worker_.Tell(test_like);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);

        }

        [TestMethod]
        [ExpectedException(typeof(UnknownClientException), "Unknown communication ID")]
        public void TestUnknownClientExceptionDislike()
        {
            long wrong_comm_id = 12;
            long random_message_id = 3;

            Dislike test_dislike = new Dislike(testclient_.ToString(), wrong_comm_id, random_message_id);
            worker_.Tell(test_dislike);
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownClientException), "Unknown communication ID")]
        public void TestUnknownClientExceptionRetrieve()
        {
            long wrong_comm_id = 12;
            String author = "Group16";

            worker_.Tell(new RetrieveMessages(author, wrong_comm_id));
            while (testclient_.ReceivedMessages.Count == 0)
                system_.RunFor(1);
        }

        [TestMethod]
        public void TestFinishCommunication()
        {
            //testing only the acks
            SimulatedActorSystem system = new SimulatedActorSystem();
            Dispatcher dispatcher = new Dispatcher(system, 2);
            system.Spawn(dispatcher);
            TestClient client = new TestClient();
            system.Spawn(client);

            dispatcher.Tell(new InitCommunication(client, 10));

            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
            Message initAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(InitAck), initAckMessage.GetType());
            InitAck initAck = (InitAck)initAckMessage;
            Assert.AreEqual(10, initAck.CommunicationId);

            SimulatedActor worker = initAck.Worker;

            dispatcher.Tell(new Stop());

            system.RunUntil(system.currentTime + 10);

            worker.Tell(new FinishCommunication(10));
            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);

            Message operationFailedMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationFailed), operationFailedMessage.GetType());
           
            // TODO run system until workers and dispatcher are stopped
            int endtime = system.currentTime + 20;
            system.RunUntil(endtime);
            Assert.AreEqual(system.currentTime, endtime + 1);
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownClientException), "Unknown communication ID")]
        public void TestUnknownClientExceptionFinishCommunication()
        {
            //testing only the acks
            SimulatedActorSystem system = new SimulatedActorSystem();
            Dispatcher dispatcher = new Dispatcher(system, 2);
            system.Spawn(dispatcher);
            TestClient client = new TestClient();
            system.Spawn(client);

            dispatcher.Tell(new InitCommunication(client, 10));

            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
            Message initAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(InitAck), initAckMessage.GetType());
            InitAck initAck = (InitAck)initAckMessage;
            Assert.AreEqual(10, initAck.CommunicationId);

            SimulatedActor worker = initAck.Worker;

            worker.Tell(new FinishCommunication(11));
            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownClientException), "Unknown communication ID")]
        public void TestUnknownClientExceptionFinishCommunicationStopping()
        {
            //testing only the acks
            SimulatedActorSystem system = new SimulatedActorSystem();
            Dispatcher dispatcher = new Dispatcher(system, 2);
            system.Spawn(dispatcher);
            TestClient client = new TestClient();
            system.Spawn(client);

            dispatcher.Tell(new InitCommunication(client, 10));

            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
            Message initAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(InitAck), initAckMessage.GetType());
            InitAck initAck = (InitAck)initAckMessage;
            Assert.AreEqual(10, initAck.CommunicationId);

            SimulatedActor worker = initAck.Worker;

            dispatcher.Tell(new Stop());

            int endtime = system.currentTime + 10;

            system.RunUntil(endtime);

            Assert.AreEqual(endtime + 1, system.currentTime);

            worker.Tell(new FinishCommunication(11));
            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
        }

        /// <summary>
        /// Simple first test initiating a communication and closing it afterwards.
        /// </summary>
        [TestMethod]
        public void TestDispatcherStoppingFail()
        {
            //testing only the acks
            SimulatedActorSystem system = new SimulatedActorSystem();
            Dispatcher dispatcher = new Dispatcher(system, 2);
            system.Spawn(dispatcher);
            TestClient client = new TestClient();
            system.Spawn(client);

            dispatcher.Tell(new Stop());

            dispatcher.Tell(new InitCommunication(client, 10));

            while (client.ReceivedMessages.Count == 0)
                system.RunFor(1);
            Message initAckMessage = client.ReceivedMessages.Dequeue();
            Assert.AreEqual(typeof(OperationFailed), initAckMessage.GetType());
            OperationFailed initAck = (OperationFailed)initAckMessage;
            Assert.AreEqual(10, initAck.CommunicationId);

            // TODO run system until workers and dispatcher are stopped
            int endtime = system.currentTime + 20;
            system.RunUntil(endtime);
            Assert.AreEqual(system.currentTime, endtime + 1);
        }

        [TestMethod]
        public void TestMessageDrop()
        {
            SimulatedActor[] workers = new SimulatedActor[10];
            TestClient[] clients = new TestClient[10];
            for (int i = 0; i < 10; i++)
            {
                clients[i]  = new TestClient();
                system_.Spawn(clients[i]);

                dispatcher_.Tell(new InitCommunication(clients[i], i + 12));

                while (clients[i].ReceivedMessages.Count == 0)
                    system_.RunFor(1);
                Message initAckMessage = clients[i].ReceivedMessages.Dequeue();
                Assert.AreEqual(typeof(InitAck), initAckMessage.GetType());
                InitAck initAck = (InitAck)initAckMessage;
                Assert.AreEqual(i+12, initAck.CommunicationId);

                workers[i] = initAck.Worker;
            }

            int j = 0;
            foreach(SimulatedActor worker in workers)
            {
                worker.Tell(new Like("group-16", 12 + j, 1));
                j++;
            }

            for(int i = 0; i < 10; i++)
            {
                while (clients[i].ReceivedMessages.Count == 0)
                    system_.RunFor(1);
                Message failedMessage = clients[i].ReceivedMessages.Dequeue();
                if(failedMessage.GetType() != typeof(OperationAck))
                    Assert.AreEqual(typeof(OperationFailed), failedMessage.GetType());
            }
        }

    }
}