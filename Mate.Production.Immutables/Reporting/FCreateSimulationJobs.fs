﻿module FCreateSimulationJobs
open System
    type public FCreateSimulationJob = {
        Key : string
        DueTime : int64
        ArticleName : string
        OperationName : string
        OperationHierarchyNumber : int32
        OperationDuration : int64
        RequiredCapabilityName : string
        JobType : string
        CustomerOrderId : string
        IsHeadDemand : bool
        ArticleType : string
        fArticleKey : Guid
        ProductionAgent : string
        fArticleName: string
        JobName : string
        CapabilityProvider : string
        Start : int64
        End : int64
    }
