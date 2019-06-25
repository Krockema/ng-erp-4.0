﻿using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_ProductionOrder : BaseEntity, IProvider
    {
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProductionOrderBoms {get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProductionOrderBomChilds { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int DueTime { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderOperations { get; set; }

        public int ProviderId { get; set; }
        public T_Provider Provider { get; set; }

        public T_ProductionOrder()
        {
            // it must be always a T_Provider created for every IProvider
            Provider = new T_Provider();
        }
        
        public M_Article GetArticle()
        {
            return Article;
        }

        public int GetDueTime()
        {
            return DueTime;
        }

        public T_ProductionOrder(IDemand demand)
        {
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            DueTime = demand.GetDueTime();
            Article = demand.GetArticle();
            ArticleId = demand.GetArticle().Id;
            Name = $"ProductionOrder{demand.Id}";
            // connects this provider with table T_Provider
            Provider = new T_Provider();
            Quantity = demand.GetQuantity();
            

            // TODO: check following navigation properties are created
            /*List<T_ProductionOrderBom> productionOrderBoms = new List<T_ProductionOrderBom>();
            List<T_ProductionOrderOperation> productionOrderWorkSchedule = new List<T_ProductionOrderOperation>();
            List<T_ProductionOrderBom> prodProductionOrderBomChilds = new List<T_ProductionOrderBom>();*/

        }

        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }
    }
}
