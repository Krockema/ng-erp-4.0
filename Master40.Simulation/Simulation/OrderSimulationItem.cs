﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Master40.DB.DB.Models;

namespace Master40.Simulation.Simulation
{
    public class OrderSimulationItem : ISimulationItem
    {
        public OrderSimulationItem(int start, int end)
        {
            SimulationState = SimulationState.Waiting;
            Start = start;
            End = end;
        }
        public int Start { get; set; }
        public int End { get; set; }
        public SimulationState SimulationState { get; set; }
        public Task<bool> DoAtStart()
        {
            return null;
        }

        public Task<bool> DoAtEnd<T>(List<TimeTable<T>.MachineStatus> listMachineStatus) where T : ISimulationItem
        {
            throw new NotImplementedException();
        }
    }
}
