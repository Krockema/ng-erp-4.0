﻿using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel.Interface;

namespace Mate.DataCore.ReportingModel
{
    public class Setup : ResultBaseEntity, ISimulationResourceData
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public long Time { get; set; }
        public long ExpectedDuration { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string CapabilityProvider { get; set; }
        public string CapabilityName { get; set; }
        public int SetupId { get; set; }
        public string Mapping { get => CapabilityName; }
    }
}
