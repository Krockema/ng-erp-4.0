using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.DemandDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.SchedulingDomain;
using Zpp.Utils;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_ProductionOrder
     */
    public class ProductionOrder : Provider
    {
        public ProductionOrder(IProvider provider, IDbMasterDataCache dbMasterDataCache) : base(
            provider, dbMasterDataCache)
        {
        }

        public static ProductionOrder CreateProductionOrder(Demand demand,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Quantity lotSize)
        {
            if (!demand.GetArticle().ToBuild)
            {
                throw new MrpRunException(
                    "You are trying to create a productionOrder for a purchaseArticle.");
            }

            T_ProductionOrder tProductionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            tProductionOrder.DueTime = demand.GetDueTime(dbTransactionData).GetValue();
            tProductionOrder.Article = demand.GetArticle();
            tProductionOrder.ArticleId = demand.GetArticle().Id;
            tProductionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            tProductionOrder.Quantity = lotSize.GetValue();

            ProductionOrder productionOrder =
                new ProductionOrder(tProductionOrder, dbMasterDataCache);

            productionOrder.CreateNeededDemands(demand.GetArticle(), dbTransactionData,
                productionOrder, productionOrder.GetQuantity());

            return productionOrder;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="article"></param>
        /// <param name="dbTransactionData"></param>
        /// <param name="dbMasterDataCache"></param>
        /// <param name="parentProductionOrder"></param>
        /// <param name="quantity">of production article to produce
        /// --> is used for childs as: articleBom.Quantity * quantity</param>
        /// <returns></returns>
        private static Demands CreateProductionOrderBoms(M_Article article,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Provider parentProductionOrder, Quantity quantity)
        {
            M_Article readArticle = dbTransactionData.M_ArticleGetById(article.GetId());
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> newDemands = new List<Demand>();
                // for backward scheduling
                List<ProductionOrderBom> productionOrderBoms = new List<ProductionOrderBom>();

                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    Id articleChildId = new Id(articleBom.ArticleChildId);
                    M_Article childArticle = dbMasterDataCache.M_ArticleGetById(articleChildId);
                    Demand newDemand = ProductionOrderBom.CreateProductionOrderBom(articleBom,
                        parentProductionOrder, dbMasterDataCache, quantity);
                    // for backwards scheduling
                    ProductionOrderBom newProductionOrderBom = (ProductionOrderBom) newDemand;
                    if (newProductionOrderBom.HasOperation())
                    {
                        productionOrderBoms.Add(newProductionOrderBom);
                    }

                    newDemands.Add(newDemand);
                }

                // backwards scheduling
                OperationBackwardsSchedule lastOperationBackwardsSchedule =
                    new OperationBackwardsSchedule(parentProductionOrder.GetDueTime(dbTransactionData), null,
                        null);

                IEnumerable<ProductionOrderBom> sortedProductionOrderBoms =
                    productionOrderBoms.OrderByDescending(x =>
                        ((T_ProductionOrderBom) x.GetIDemand()).ProductionOrderOperation
                        .HierarchyNumber);

                foreach (var productionOrderBom in sortedProductionOrderBoms)
                {
                    lastOperationBackwardsSchedule = productionOrderBom.ScheduleBackwards(lastOperationBackwardsSchedule);
                }


                return new ProductionOrderBoms(newDemands);
            }

            return null;
        }

        public override IProvider ToIProvider()
        {
            return (T_ProductionOrder) _provider;
        }

        public override Id GetArticleId()
        {
            Id articleId = new Id(((T_ProductionOrder) _provider).ArticleId);
            return articleId;
        }

        public override void CreateNeededDemands(M_Article article,
            IDbTransactionData dbTransactionData, Provider parentProvider, Quantity quantity)
        {
            _dependingDemands = CreateProductionOrderBoms(article, dbTransactionData,
                _dbMasterDataCache, parentProvider, quantity);
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"P(PrO);{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            T_ProductionOrder productionOrder = (T_ProductionOrder) _provider;
            return new DueTime(productionOrder.DueTime);
        }
    }
}