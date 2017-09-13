using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Master40.Agents;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.Simulation.Simulation;
using Master40.DB.Models;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Master40.XUnitTest.DBContext
{
    public class ContextTest
    {
        ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDB")
            .Options);

        InMemmoryContext _inMemmoryContext = new InMemmoryContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDB")
            .Options);

        MasterDBContext _masterDBContext = new MasterDBContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);

        ProductionDomainContext _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);

        public ContextTest()
        {
            //_ctx.Database.EnsureDeleted();
            //MasterDBInitializerLarge.DbInitialize(_ctx);
            //MasterDBInitializerSmall.DbInitialize(_ctx);
            _productionDomainContext.Database.EnsureDeleted();
            MasterDBInitializerLarge.DbInitialize(_productionDomainContext);
        }

        /// <summary>
        /// to Cleanup ctx for every Test // uncomment this. 
        /// </summary>
        /*
        public void Dispose()
        {
            _ctx.Dispose();
        }
        */



        [Fact]
        public void OrderContextTest()
        {
            _ctx.Orders.Add(new Order {Name = "Order1"});
            _ctx.SaveChanges();

            Assert.Equal(2, _ctx.Orders.Count());
        }

        [Fact]
        public async Task MrpTestAsync()
        {
            var scheduling = new Scheduling(_ctx);
            var capacityScheduling = new CapacityScheduling(_ctx);
            var msgHub = new Moc.MessageHub();
            var rebuildNets = new RebuildNets(_ctx);
            var mrpContext = new ProcessMrp(_ctx, scheduling, capacityScheduling, msgHub, rebuildNets);

            var mrpTest = new MrpTest();
            await mrpTest.CreateAndProcessOrderDemandAll(mrpContext);
            Assert.Equal(true, (_ctx.ProductionOrderWorkSchedules.Any()));

        }

        //public DemandToProvider getRequester
        [Fact]
        public async Task AgentSimulationTestAsync()
        {
            var sim = new AgentSimulation(_productionDomainContext, new Moc.MessageHub());
            await sim.RunSim(1);

            Assert.Equal(true, true);
        }


        [Fact]
        public async Task MrpTestForwardAsync()
        {
            _productionDomainContext.Database.EnsureDeleted();
            _productionDomainContext.Database.EnsureCreated();
            MasterDBInitializerLarge.DbInitialize(_productionDomainContext);

            //var scheduling = new Scheduling(_productionDomainContext);
            //var capacityScheduling = new CapacityScheduling(_productionDomainContext);
            var msgHub = new Moc.MessageHub();
            //var rebuildNets = new RebuildNets(_productionDomainContext);
            //var mrpContext = new ProcessMrp(_productionDomainContext, scheduling, capacityScheduling, msgHub, rebuildNets);
            var simulation = new Simulator(_productionDomainContext, msgHub);
            await simulation.InitializeMrp(MrpTask.All);
            //var mrpTest = new MrpTest();
            // await mrpTest.CreateAndProcessOrderForward(mrpContext);
            await simulation.Simulate();

            Assert.Equal(true, _productionDomainContext.ProductionOrderWorkSchedules.Any());
        }

        // Load Database from SimulationJason
        [Fact]
        public async Task LoadContextAsync()
        {
            int last = 0;


            // In-memory database only exists while the connection is open
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());

            // create OptionsBuilder with InMemmory Context
            var builder = new DbContextOptionsBuilder<MasterDBContext>();
            builder.UseSqlite(connection);
            
            for (int i = 0; i < 2; i++)
            {
                using (var c = new ProductionDomainContext(builder.Options))
                {
                    c.Database.OpenConnection();
                    c.Database.EnsureCreated();
                    MasterDBInitializerLarge.DbInitialize(c);

                    c.ArticleTypes.Add(new ArticleType { Name = "Test" + i });
                    c.SaveChanges();
                    last = c.ArticleTypes.Last().Id;

                    Debug.WriteLine("Last Article Type Id: " + last);
                }
                connection.Close();
            }
            Assert.Equal(4, last);

            /*


            var simState = _ctx.SaveSimulationState();


            var stringSimState =
                JsonConvert.SerializeObject(simState, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }
                );

            var deserialized = JsonConvert.DeserializeObject<SimulationDbState>(stringSimState);

            _masterDBContext.Database.EnsureDeleted();
            _masterDBContext.Database.EnsureCreated();
            //_inMemmoryContext.LoadContextFromSimulation(deserialized);

            */

        }

        private static DbContextOptions<MasterDBContext> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<MasterDBContext>();
            builder.UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }

        // HardDatabase To InMemory
        [Fact]
        public async Task CopyContext()
        {
            _productionDomainContext.Database.EnsureCreated();
            MasterDBInitializerLarge.DbInitialize(_productionDomainContext);
            _ctx.Database.EnsureCreated();
            _productionDomainContext.CopyAllTables(_ctx);
            
            Assert.Equal(true, (_ctx.Articles.Any()));
        }

        // Json to InMemory
        [Fact]
        public async Task CopyJsonToInMemmory()
        {

            var json = _ctx.SaveSimulationState();

            _ctx.Database.EnsureDeleted();
            _ctx.Database.EnsureCreated();

            //_ctx.LoadInMemoryDB(json);

            Assert.Equal(true, (_ctx.Articles.Any()));

        }

    }
}

