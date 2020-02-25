﻿using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResourceCapability
    {
        internal M_ResourceCapability CUTTING;
        internal M_ResourceCapability DRILLING;
        internal M_ResourceCapability ASSEMBLING;

        internal MasterTableResourceCapability()
        {
            CUTTING = new M_ResourceCapability { Name = "Cutting" };
            DRILLING = new M_ResourceCapability { Name = "Drilling" };
            ASSEMBLING = new M_ResourceCapability { Name = "Assembling" };
        }

        internal M_ResourceCapability[] Init(MasterDBContext context)
        {
            var resourceCapabilities = new M_ResourceCapability[]
            {
                CUTTING,
                DRILLING,
                ASSEMBLING,
            };

            context.ResourceCapabilities.AddRange(resourceCapabilities);
            context.SaveChanges();
            return resourceCapabilities;
        }
    }
}