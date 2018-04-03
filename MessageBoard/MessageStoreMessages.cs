using ActorSystem;

namespace MessageBoard
{
    /// <summary>
    /// Base class for all messages sent to the message store.
    /// </summary>
    public abstract class MessageStoreMessage : Message
    {
        /// <summary>
        /// The actor to which the message store sends its replies.
        /// </summary>
        public SimulatedActor StoreClient { get; set; }
        /// <summary>
        /// the id of the communication during which this persistence operation 
        /// is performed
        /// </summary>
        public long CommunicationId { get; set; }

        public int Duration()
        {
            return 1; // store is supposed to be fast 
        }
    }
    /// <summary>
    /// Message which signals that a new user message should be added to 
    /// the store.
    /// </summary>
    public class UpdateMessageStore : MessageStoreMessage
    {
        /// <summary>
        /// the actual user message to be added
        /// </summary>
        public UserMessage Message { get; private set; }
        public UpdateMessageStore(UserMessage message, long commId)
        {
            this.Message = message;
            this.CommunicationId = commId;
        }
    }
    /// <summary>
    /// Message used to signal that messages should be retrieved from 
    /// the store.
    /// </summary>
    public class RetrieveFromStore : MessageStoreMessage
    {
        /// <summary>
        /// the author of the message which should be looked up
        /// </summary>
        public string Author { get; private set; }
        public RetrieveFromStore(string author, long commId)
        {
            this.Author = author;
            this.CommunicationId = commId;
        }
    }
    /// <summary>
    /// Message used to signal that a like should be added to a message. 
    /// </summary>
    public class AddLike : MessageStoreMessage
    {
        /// <summary>
        /// user message id of the user message which should be liked
        /// </summary>
        public long MessageId { get; set; }
        /// <summary>
        /// name of the person which likes the message
        /// </summary>
        public string ClientName { get; set; }
        public AddLike(string clientName, long messageId, long commId)
        {
            this.ClientName = clientName;
            this.MessageId = messageId;
            this.CommunicationId = commId;
        }
    }
    /// <summary>
    /// Message used to signal that a like should be added to a message. 
    /// </summary>
    public class AddDislike : MessageStoreMessage
    {
        /// <summary>
        /// user message id of the user message which should be liked
        /// </summary>
        public long MessageId { get; set; }
        /// <summary>
        /// name of the person which likes the message
        /// </summary>
        public string ClientName { get; set; }
        public AddDislike(string clientName, long messageId, long commId)
        {
            this.ClientName = clientName;
            this.MessageId = messageId;
            this.CommunicationId = commId;
        }
    }
}