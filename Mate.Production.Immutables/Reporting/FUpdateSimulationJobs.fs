﻿module FUpdateSimulationJobs

open IJobs

    type public FUpdateSimulationJob = {   
        Job : IJob
        JobType : string
        Duration : int64
        Start : int64
        CapabilityProvider : string
        Capability : string
        ReadyAt : int64
        Bucket : string
        SetupId : int32
    } 

