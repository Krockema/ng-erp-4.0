﻿using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.SimulationCore.Types
{
    public class OrderKpi: Dictionary<OrderKpi.OrderState, Quantity>
    {
        public enum OrderState
        {
            New,
            Finished,
            Open,
            Total,
            InDue,
            InDueTotal,
            OverDue,
            OverDueTotal,
            Tardiness,
            Lateness,
            CycleTime
        }

        public OrderKpi()
        {
            var values = Enum.GetValues(typeof(OrderState));
            foreach(OrderState val in values )
            {
                this.Add(val, new Quantity(0));
            }
        }
    }
}