using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Abstract base class for SimulatedActorSystem class. It is abstract rather than 
    /// an interface to alleviate writing CodeContracts.
    /// 
    /// It is purpose is to manage a list of simulated actors. It is responsible for 
    /// starting and stopping of actors. All active actors in the system
    /// are notified by this class when time units have passed, i.e. 
    /// the <c>ISimulatedActor.Tick()</c> method should only be called
    /// by this class.
    /// </summary>
    public abstract class ISimulatedActorSystem
    {
        /// <summary>
        /// This list contains all actors, which have been started but not stopped.
        /// </summary>
        protected List<SimulatedActor> actors { get; set; }
        /// <summary>
        /// The number of ticks passed since this object was created.
        /// </summary>
        public int currentTime { get; set; }
        /// <summary>
        /// integral number used for creating actor IDs,
        /// which is incremented everytime an actor is started
        /// </summary>
        protected long currentActorId { get; set; }
        /// <summary>
        /// actor ID assigned to new (not yet started) actors
        /// </summary>
        public const long NEW_ACTOR = -1;

        /// <summary>
        /// Constructs a new ISimulatedActorSystem object
        /// </summary>
        public ISimulatedActorSystem()
        {
            actors = new List<SimulatedActor>();
            currentTime = 0;
            currentActorId = 0;
        }
        /// <summary>
        /// Starts a new actor, i.e. assigns it an ID, registers
        /// it in the list of active actors and shall call
        /// <c>ISimulatedActor.atStartUp()</c>
        /// </summary>
        /// <param name="actor">actor to be started</param>
        /// <returns>actor which was started</returns>
        public abstract SimulatedActor Spawn(SimulatedActor actor);

        /// <summary>
        /// Runs the system for the number of ticks (time units)
        /// passed as parameters. The system is run by calling 
        /// <c>ISimulatedActor.Tick()</c> on all active actors.
        /// </summary>
        /// <param name="numberOfTicks">defines how long the system 
        /// should be run</param>
        public abstract void RunFor(int numberOfTicks);
        // including tick at endTime
        /// <summary>
        /// Runs the system until the current time (time units passed since
        /// object creation) equals the number passed as parameter. 
        /// The last Tick shall be executed when the current time is equal 
        /// to the time passed as parameter. So the current time will actually
        /// be equal to endTime + 1 after the call to this method.
        /// </summary>
        /// <param name="endTime">the target time until which the system should 
        /// be run</param>
        public abstract void RunUntil(int endTime);
        /// <summary>
        /// Stops the actor passed as parameter, by removing it 
        /// from the list of active actors.
        /// </summary>
        /// <param name="actor">the actor to be stopped</param>
        public abstract void Stop(SimulatedActor actor);
        /// <summary>
        /// Helper method used to iterate all actors and 
        /// calling <c>ISimulatedActor.Tick()</c> on it.
        /// </summary>
        public abstract void Tick();
    }
}
