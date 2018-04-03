using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Concrete implementation of <c>ISimulatedActorSystem</c>. See 
    /// base class documentation.
    /// </summary>
    public class SimulatedActorSystem : ISimulatedActorSystem
    {
        /// <summary>
        /// Constructs a new SimulatedActorSystem object.
        /// </summary>
        public SimulatedActorSystem()
        {

        }
        public override SimulatedActor Spawn(SimulatedActor actor)
        {
            actors.Add(actor);
            actor.Id = currentActorId++;
            actor.atStartUp();
            actor.TimeSinceSystemStart = currentTime;
            return actor;
        }
        public override void RunFor(int numberOfTicks)
        {
            for (int i = 0; i < numberOfTicks; i++)
            {
                Tick();
            }
        }
        public override void RunUntil(int endTime)
        {
            while (currentTime <= endTime)
            {
                Tick();
            }
        }
        public override void Tick()
        {
            // need to copy list, because actors might be spawned or stopped during
            // tick which modifies the actors-list
            List<SimulatedActor> currentlyAliveActors = new List<SimulatedActor>(actors);
            foreach (SimulatedActor actor in currentlyAliveActors)
                actor.Tick();
            currentTime++;
        }
        public override void Stop(SimulatedActor actor)
        {
            actors.Remove(actor);
        }
    }
}
