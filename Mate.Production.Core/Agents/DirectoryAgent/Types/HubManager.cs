﻿using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;

namespace Mate.Production.Core.Agents.DirectoryAgent.Types
{
    public class HubManager
    {
        private Dictionary<IActorRef, HashSet<string>> _hubRelations;

        public HubManager()
        {
            _hubRelations = new Dictionary<IActorRef, HashSet<string>>();
        }

        public IActorRef GetHubActorRefBy(string capability)
        {
            var entry = _hubRelations.SingleOrDefault(x => x.Value.Contains(capability));
            return entry.Key;
        }
        /// <summary>
        /// Loop through all capabilities the hub provides.
        /// </summary>
        /// <param name="hubRef"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public bool AddOrCreateRelation(IActorRef hubRef, M_ResourceCapability capability)
        {
            AddOrCreateRelation(hubRef, capability.Name);
            bool trueForAll = true;
            if (capability.ChildResourceCapabilities == null) return trueForAll;
            foreach (var item in capability.ChildResourceCapabilities)
            {
                trueForAll = trueForAll && AddOrCreateRelation(hubRef, item);
            }
            return trueForAll;
        }


        public bool AddOrCreateRelation(IActorRef hubRef, string capability)
        {
            if(_hubRelations.TryGetValue(hubRef, out HashSet<string> capabilities))
            {
                return capabilities.Add(capability);
            }
            //else
            _hubRelations.Add(hubRef, new HashSet<string>{ capability });
            return true;
        }

    }
}