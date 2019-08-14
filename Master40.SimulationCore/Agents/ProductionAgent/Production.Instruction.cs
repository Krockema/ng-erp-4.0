﻿using Akka.Actor;
using AkkaSim.Definitions;
using System;
using static FArticles;
using static FAgentInformations;
using static FOperationResults;
using static FOperations;

namespace Master40.SimulationCore.Agents.ProductionAgent
{
    public partial class Production
    {
        public class Instruction
        {
            /// <summary>
            ///      Finished,
            ///      ProductionStarted
            ///      SetComunicationAgent
            /// </summary>
            public class ProductionStarted : SimulationMessage
            {
                public static ProductionStarted Create(Guid message, IActorRef target)
                {
                    return new ProductionStarted(message, target);
                }
                private ProductionStarted(object message, IActorRef target) : base(message, target)
                {

                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }

            public class StartProduction : SimulationMessage
            {
                public static StartProduction Create(FArticle message, IActorRef target)
                {
                    return new StartProduction(message, target);
                }
                private StartProduction(object message, IActorRef target) : base(message, target)
                {

                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }
            }

            public class Finished : SimulationMessage
            {
                public static Finished Create(Guid message, IActorRef target)
                {
                    return new Finished(message, target);
                }
                private Finished(object message, IActorRef target) : base(message, target)
                {

                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }
            public class SetHubAgent : SimulationMessage
            {
                public static SetHubAgent Create(FAgentInformation message, IActorRef target)
                {
                    return new SetHubAgent(message, target);
                }
                private SetHubAgent(object message, IActorRef target) : base(message, target)
                {

                }
                public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
            }

            public class FinishWorkItem : SimulationMessage
            {
                public static FinishWorkItem Create(FOperationResult message, IActorRef target)
                {
                    return new FinishWorkItem(message, target);
                }
                private FinishWorkItem(object message, IActorRef target) : base(message, target)
                {

                }
                public FOperationResult GetObjectFromMessage { get => Message as FOperationResult; }
            }
            public class ProvideRequest : SimulationMessage
            {
                public static ProvideRequest Create(Guid message, IActorRef target)
                {
                    return new ProvideRequest(message, target);
                }
                private ProvideRequest(object message, IActorRef target) : base(message, target)
                {

                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }
        }
    }
}