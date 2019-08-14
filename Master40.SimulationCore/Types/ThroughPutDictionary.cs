﻿using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.Types
{
    public class ThroughPutDictionary 
    {
        private Dictionary<string, EstimatedThroughPut> dic = new Dictionary<string, EstimatedThroughPut>();

        /// <summary>
        /// If no throughput for article exists return fix time, TODO forward scheduling
        /// </summary>
        /// <param name="name">Name of the Article</param>
        /// <returns></returns>
        public EstimatedThroughPut Get(string name)
        {
            if (dic.TryGetValue(name, out EstimatedThroughPut eta))
            {
                return eta;
            }
            return new EstimatedThroughPut(0);
        }

        public bool UpdateOrCreate(string name, long time)
        {
            if(dic.TryGetValue(name, out EstimatedThroughPut eta))
            {
                eta.Set(time);
                return false;
            }
            // else
            dic.Add(name, new EstimatedThroughPut(time));
            return true;
        }
    }
}
