using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActorSystem;
namespace MessageBoard
{
    /// <summary>
    /// Actor responsible for storage and retrieval of user messages.
    /// </summary>
    public class MessageStore : SimulatedActor
    {
        /// <summary>
        /// All messages stored, the key of the dictionary corresponds to 
        /// the message ID of the user message stored as value.
        /// </summary>
        protected Dictionary<long, UserMessage> messages {get;set;}
        /// <summary>
        /// integral number which is used to create new message IDs
        /// </summary>
        protected long currentId { get; set; }

        /// <summary>
        /// Constructs a new MessageStore object, the channel is set to a 
        /// deterministic channel with no delay to simulate a good connection to 
        /// the store.
        /// </summary>
        public MessageStore()
        {
            this.messages = new Dictionary<long, UserMessage>();
            this.currentId = 0;
            // good connection between WorkerHelper and MessageStore -> no delay
            this.channel = new DeterministicChannel(0);
        }

        /// <summary>
        /// The message processing logic for the store.
        /// <para>If the message passed as parameter is of type <c>RetrieveFromStore</c>,
        /// all messages of a given author are looked up and sent back to the client of the 
        /// store.
        /// </para>
        /// <para>If the message passed as parameter is of type <c>AddLike</c>, a 
        /// like is added to the given message if the message exists and has not 
        /// already been liked by the given person.
        /// If the message passed as parameter is of type <c>UpdateMessageStore</c>,
        /// a message is stored if the message is new and if the same message has not already
        /// been stored by the same author.
        /// In case of success a OperationAck message is sent to the client, otherwise
        /// an OperationFailed message is sent. 
        /// </para>
        /// </summary>
        /// <param name="m">message sent to the store</param>
        public override void Receive(Message m)
        {
            if (m is RetrieveFromStore)
            {
                RetrieveFromStore retrieve = (RetrieveFromStore)m;
                List<UserMessage> foundMessage = FindByAuthor(retrieve.Author);
                retrieve.StoreClient.Tell(new FoundMessages(foundMessage, retrieve.CommunicationId));
            }
            else if (m is AddLike)
            {
                AddLike addLikeMessage = (AddLike)m;
                if (addLike(addLikeMessage.ClientName, addLikeMessage.MessageId))
                {
                    addLikeMessage.StoreClient.Tell(new OperationAck(addLikeMessage.CommunicationId));
                }
                else
                {
                    addLikeMessage.StoreClient.Tell(new OperationFailed(addLikeMessage.CommunicationId));
                }
            }
            else if (m is AddDislike)
            {
                AddDislike addDislikeMessage = (AddDislike)m;
                if (addDislike(addDislikeMessage.ClientName, addDislikeMessage.MessageId))
                {
                    addDislikeMessage.StoreClient.Tell(new OperationAck(addDislikeMessage.CommunicationId));
                }
                else
                {
                    addDislikeMessage.StoreClient.Tell(new OperationFailed(addDislikeMessage.CommunicationId));
                }
            }
            else if (m is UpdateMessageStore)
            {
                UpdateMessageStore updateMessage = (UpdateMessageStore)m;
                if (update(updateMessage.Message))
                    updateMessage.StoreClient.Tell(new OperationAck(updateMessage.CommunicationId));
                else
                    updateMessage.StoreClient.Tell(new OperationFailed(updateMessage.CommunicationId));
            }
        }
        /// <summary>
        /// Internal helper method containing the update logic.
        /// </summary>
        /// <param name="message">the user message to be saved</param>
        /// <returns>true if successful, false otherwise</returns>
        internal bool update(UserMessage message)
        {
            if (message.MessageId == UserMessage.NEW)
            {
                bool containsSameMessage = false;
                foreach(UserMessage m in messages.Values)
                {
                    if(m.Author.Equals(message.Author) && m.Message.Equals(message.Message))
                        containsSameMessage = true;
                }
                if(!containsSameMessage)
                {
                    message.MessageId = currentId++;
                    messages[message.MessageId] = message;
                    return true;
                }
            }
            return false;
            
        }
        /// <summary>
        /// Internal helper method containing the logic for looking up messages.
        /// </summary>
        /// <param name="author">the name of the author of the returned messages</param>
        /// <returns>all messages posted by the given author</returns>
        internal List<UserMessage> FindByAuthor(string author)
        {
            List<UserMessage> foundMessages = new List<UserMessage>();
            foreach (UserMessage message in messages.Values)
            {
                if (message.Author.Equals(author))
                    foundMessages.Add(message);
            }
            return foundMessages;
        }

        /// <summary>
        /// Internal helper method containing the logic for adding likes.
        /// </summary>
        /// <param name="clientName">the name of the person who likes the message</param>
        /// <param name="messageId">the id of message to be liked</param>
        /// <returns>true if successful, false otherwise</returns>
        internal bool addLike(string clientName, long messageId)
        {
            if (!messages.ContainsKey(messageId))
                return false;
            UserMessage message = messages[messageId];
            if (message.Likes.Contains(clientName))
                return false;
            message.Likes.Add(clientName);
            return true;
        }
        /// <summary>
        /// Internal helper method containing the logic for adding dislikes.
        /// </summary>
        /// <param name="clientName">the name of the person who dislikes the message</param>
        /// <param name="messageId">the id of message to be disliked</param>
        /// <returns>true if successful, false otherwise</returns>
        internal bool addDislike(string clientName, long messageId)
        {
            if (!messages.ContainsKey(messageId))
                return false;
            UserMessage message = messages[messageId];
            if (message.Dislikes.Contains(clientName))
                return false;
            message.Dislikes.Add(clientName);
            return true;
        }
    }
}
