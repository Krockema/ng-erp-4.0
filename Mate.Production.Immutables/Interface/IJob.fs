﻿module IJobs
open FStartConditions
open Akka.Actor
open System
open Mate.DataCore.DataModel

type public IJob = 
    abstract member Key : Guid with get
    abstract member Name : string with get
    abstract member ForwardStart : int64 with get
    abstract member ForwardEnd : int64 with get
    abstract member BackwardStart : int64 with get
    abstract member BackwardEnd : int64 with get
    abstract member Start : int64 with get
    abstract member End : int64 with get
    abstract member StartConditions : FStartCondition with get
    abstract member Priority : int64 -> double
    abstract member HubAgent : IActorRef
    abstract member DueTime : int64 with get
    abstract member Duration : int64 with get
    abstract member SetupKey : int32 with get
    abstract member RequiredCapability : M_ResourceCapability with get
    abstract member UpdateEstimations : int64 -> IJob
    abstract member Bucket : string
    abstract member UpdateBucket : string -> IJob 
    abstract member ResetSetup : unit -> unit