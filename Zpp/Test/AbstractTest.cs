using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Zpp.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Zpp.Test.Configurations;
using Zpp.Test.Configurations.Scenarios;

namespace Zpp.Test
{
    /**
     * A test can be initialized via base() constructor on three ways:
     * - no dbInit: use base(false)
     * - dbInit: default db (truck scenario) use base(true) else use base(TestConfigurationFileNames.X)
     * - dbInit + CO/COP: use base(false) and call InitTestScenario(TestConfigurationFileNames.X)
     */
    public abstract class AbstractTest : IDisposable
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        protected readonly ProductionDomainContext ProductionDomainContext;

        protected static TestConfiguration TestConfiguration;

        public AbstractTest() : this(true)
        {
        }

        /**
         * This constructor must be always called (else ProductionDomainContext is null)
         * --> seems a bit strange, but is needed to enable parameterized tests:
         * the default db should not be initialized in this case, but the testConfig is not available as constructor parameter
         */
        public AbstractTest(bool initDefaultTestConfig)
        {
            ProductionDomainContext = Dbms.getDbContext();

            if (initDefaultTestConfig)
            {
                InitDefaultDb();
            }
        }

        // @before
        public AbstractTest(string testConfiguration) : this(false)
        {
            InitDb(testConfiguration);
        }

        // @after
        public void Dispose()
        {
            ProductionDomainContext.Database.CloseConnection();
        }

        private void InitDefaultDb()
        {
            InitDb(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2);

            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true,
                TestConfiguration.CustomerOrderPartQuantity);
        }

        /**
         * Initialize the db:
         * - deletes current
         * - creates db according to given configuration
         */
        protected void InitDb(string testConfiguration)
        {
            TestConfiguration = ReadTestConfiguration(testConfiguration);

            if (Constants.IsLocalDb)
            {
                bool isDeleted = ProductionDomainContext.Database.EnsureDeleted();
                if (!isDeleted)
                {
                    LOGGER.Error("Database could not be deleted.");
                }
            }
            else
            {
                bool wasDropped = Dbms.DropDatabase(Constants.GetDbName());
                if (wasDropped == false)
                {
                    LOGGER.Warn($"Database {Constants.GetDbName()} could not be dropped.");
                }
            }

            Type dbSetInitializer = Type.GetType(TestConfiguration.DbSetInitializer);
            dbSetInitializer.GetMethod("DbInitialize")
                .Invoke(null, new[] {ProductionDomainContext});
            
            LotSize.LotSize.SetDefaultLotSize(new Quantity(TestConfiguration.LotSize));
            LotSize.LotSize.SetLotSizeType(TestConfiguration.LotSizeType);
            }

        /**
         * init db and customerOrders
         */
        protected void InitTestScenario(string testConfiguration)
        {
            InitDb(testConfiguration);

            Type testScenarioType = Type.GetType(TestConfiguration.TestScenario);
            TestScenario testScenario = (TestScenario) Activator.CreateInstance(testScenarioType);
            testScenario.CreateCustomerOrders(
                new Quantity(TestConfiguration.CustomerOrderPartQuantity), ProductionDomainContext);
        }

        private static TestConfiguration ReadTestConfiguration(string testConfigurationFileNames)
        {
            return JsonConvert.DeserializeObject<TestConfiguration>(
                File.ReadAllText(testConfigurationFileNames));
        }
    }
}