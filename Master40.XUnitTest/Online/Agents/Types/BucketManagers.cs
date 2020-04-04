﻿using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.XUnitTest.Online.Preparations;
using Xunit;

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class BucketManagers : TestKit
    {
            // Create Resources
            private static M_Resource resource = new M_Resource {Name = "Saw", UsedInResourceSetups = new List<M_ResourceSetup>() , RequiresResourceSetups = new List<M_ResourceSetup>()};
            private static M_Resource toolBig = new M_Resource() { Name = "SawBig", UsedInResourceSetups = new List<M_ResourceSetup>(), RequiresResourceSetups = new List<M_ResourceSetup>() };
            private static M_Resource toolSmall = new M_Resource() { Name = "SawSmall", UsedInResourceSetups = new List<M_ResourceSetup>(), RequiresResourceSetups = new List<M_ResourceSetup>() };
            // Create Capabilities
            private static M_ResourceCapability cap = new M_ResourceCapability() {Name = "Sawing", ResourceSetups = new List<M_ResourceSetup>(), ChildResourceCapabilities = new List<M_ResourceCapability>()};
            private static M_ResourceCapability capSmall = new M_ResourceCapability() { Name = "SawingSmall" , ResourceSetups = new List<M_ResourceSetup>(), };
            private static M_ResourceCapability capBig = new M_ResourceCapability() { Name = "SawingBig", ResourceSetups = new List<M_ResourceSetup>() };
            // Create Setups
            private static M_ResourceSetup basicSawSetup = new M_ResourceSetup { ChildResource = resource, ChildResourceId =  resource.Id
                                                    , ResourceCapability = cap, ResourceCapabilityId = cap.Id};
            private static M_ResourceSetup smallSetup = new M_ResourceSetup { ParentResource = resource, ParentResourceId = resource.Id,
                                                   ChildResource = toolSmall, ChildResourceId = toolSmall.Id,
                                                   ResourceCapability = capSmall, ResourceCapabilityId = capSmall.Id };
            private static M_ResourceSetup bigSetup = new M_ResourceSetup { ParentResource = resource, ParentResourceId = resource.Id,
                                                   ChildResource = toolBig, ChildResourceId = toolBig.Id,
                                                   ResourceCapability = capBig, ResourceCapabilityId = capBig.Id };

        public BucketManagers()
        {
            resource.UsedInResourceSetups.Add(basicSawSetup);
            resource.RequiresResourceSetups.Add(smallSetup);
            resource.RequiresResourceSetups.Add(bigSetup);

            cap.ChildResourceCapabilities.Add(capSmall);
            cap.ChildResourceCapabilities.Add(capBig);
            cap.ResourceSetups.Add(basicSawSetup);

            capSmall.ResourceSetups.Add(smallSetup);
            capBig.ResourceSetups.Add(bigSetup);
        }


        [Fact]
        void FindAndAddBucket()
        {
            var bucketManager = CreateTestSetForBuckets();

            //Add operation to Bucket(0)
            var operation1 = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 25, averageTransitionDuration: 10, dueTime: 50, capability: capBig);
            var bucket = bucketManager.AddToBucket(operation1);
            Assert.Equal(3, bucket.Operations.Count);

            //Adds operation to Bucket(1) because first one is over capacity
            var operation2 = TypeFactory.CreateDummyJobItem(jobName: "Job2", jobDuration: 70, averageTransitionDuration: 10, dueTime: 50, capability: capBig);
            bucket = bucketManager.AddToBucket(operation2);
            Assert.Equal(2, bucket.Operations.Count);

            //Create new bucket because ForwardTime is earlier / see currentTime
            var operation3 = TypeFactory.CreateDummyJobItem(jobName: "Job3", currentTime: -10, jobDuration: 70, averageTransitionDuration: 10, dueTime: 50, capability: capBig);
            bucket = bucketManager.AddToBucket(operation3);
            Assert.Null(bucket);

        }


        [Fact]
        void ModifyBucket()
        {


        }

        [Fact]
        void CreateBucket()
        {
            var bucketManager = new BucketManager(240);
            var operationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 35, averageTransitionDuration: 10, capability: capBig);

            var bucket = bucketManager.CreateBucket(operationJob, hubAgent: ActorRefs.Nobody, currentTime: 0);

            Assert.Equal(expected: 1, actual: bucket.Operations.Count);

        }

        [Fact]
        void BackwardAndForwardTestForBucket()
        {
            var bucketManager = new BucketManager(240);
            var tool = new M_Resource() { Name = "SawBig" };
            var operationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 5, averageTransitionDuration: 10, capability: capBig);

            var bucket = bucketManager.CreateBucket(operationJob, hubAgent: ActorRefs.Nobody, currentTime: 0);

            //Forward
            Assert.Equal(expected: 0, actual: bucket.ForwardStart);
            Assert.Equal(expected: 15, actual: bucket.ForwardEnd);

            //Backward
            Assert.Equal(expected: 35, actual: bucket.BackwardStart);
            Assert.Equal(expected: 50, actual: bucket.BackwardEnd);
        }

        [Fact]
        void BackwardAndForwardTestForModifiedBucket()
        {
            var bucketManager = new BucketManager(240);
            var tool = new M_Resource() { Name = "SawBig" };
            var operationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 5, averageTransitionDuration: 10, dueTime: 50, capability: capBig);

            var bucket = bucketManager.CreateBucket(operationJob, hubAgent: ActorRefs.Nobody, currentTime: 0);

            //Forward
            Assert.Equal(expected: 0, actual: bucket.ForwardStart);
            Assert.Equal(expected: 15, actual: bucket.ForwardEnd);

            //Backward
            Assert.Equal(expected: 35, actual: bucket.BackwardStart);
            Assert.Equal(expected: 50, actual: bucket.BackwardEnd);

            var incomingOperationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 5, averageTransitionDuration: 10, dueTime: 70, capability: capBig);
            bucket.AddOperation(incomingOperationJob);

            //Forward
            Assert.Equal(expected: 0, actual: bucket.ForwardStart);
            Assert.Equal(expected: 15, actual: bucket.ForwardEnd);

            //Backward
            Assert.Equal(expected: 35, actual: bucket.BackwardStart);
            Assert.Equal(expected: 50, actual: bucket.BackwardEnd);
            
        }

        /// <summary>
        /// Provides a TestSet of Buckets for UnitTests
        /// IF CHANGES ARE MADE HERE CHECK ALL REFERENCED UNIT TESTS
        /// </summary>
        BucketManager CreateTestSetForBuckets()
        {
            var bucketManager = new BucketManager(240);

            var toolCap1 = new M_ResourceCapability() { Name = "Cutting Small" , ResourceSetups = new List<M_ResourceSetup>() { new M_ResourceSetup() {Name = "Small Saw"}}};
            var toolCap2 = new M_ResourceCapability() { Name = "Cutting Big", ResourceSetups = new List<M_ResourceSetup>() { new M_ResourceSetup() { Name = "Big Saw" } } };
            bucketManager.AddOrUpdateBucketSize(toolCap1, 0);
            

            //Bucket1 Saw Big
            var operation1 = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 25, averageTransitionDuration: 10, dueTime: 150, capability: capBig);
            var operation2 = TypeFactory.CreateDummyJobItem(jobName: "Job2", jobDuration: 25, averageTransitionDuration: 10, dueTime: 150, capability: capBig);
            var bucket = bucketManager.CreateBucket(operation1, hubAgent: ActorRefs.Nobody, currentTime: 0);
            bucket = bucket.AddOperation(operation2);
            bucketManager.Replace(bucket);
            bucketManager.AddOrUpdateBucketSize(toolCap1, operation1.Operation.Duration);
            bucketManager.AddOrUpdateBucketSize(toolCap1, operation2.Operation.Duration);

            //Bucket2 Saw Big
            var operation3 = TypeFactory.CreateDummyJobItem(jobName: "Job3", jobDuration: 15, averageTransitionDuration: 10, dueTime: 150, capability: capBig);
            bucketManager.CreateBucket(operation3, hubAgent: ActorRefs.Nobody, currentTime: 0);
            bucketManager.AddOrUpdateBucketSize(toolCap1, operation3.Operation.Duration);

            //Bucket3 Saw Small
            var operation4 = TypeFactory.CreateDummyJobItem(jobName: "Job4", jobDuration: 15, averageTransitionDuration: 10, dueTime: 150, capability: capSmall);
            bucketManager.CreateBucket(operation4, hubAgent: ActorRefs.Nobody, currentTime: 0);
            bucketManager.AddOrUpdateBucketSize(toolCap2, operation4.Operation.Duration);

            return bucketManager; 
        }
    }
}

