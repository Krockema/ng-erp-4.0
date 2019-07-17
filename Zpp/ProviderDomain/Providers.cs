using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.Utils;
using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
using Zpp.WrappersForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * wraps the collection with all providers
     */
    public class Providers : IProviders
    {
        private readonly List<Provider> _providers = new List<Provider>();

        public Providers(List<Provider> providers)
        {
            if (providers == null)
            {
                throw new MrpRunException("Given list should not be null.");
            }

            _providers = providers;
        }

        public Providers(Provider[] providers)
        {
            if (providers == null)
            {
                throw new MrpRunException("Given list should not be null.");
            }

            List<Provider> providerList = new List<Provider>();
            foreach (var provider in providers)
            {
                providerList.Add(provider);
            }

            _providers = providerList;
        }

        public Providers(Provider provider1, Provider provider2)
        {
            _providers.Add(provider1);
            _providers.Add(provider2);
        }

        public Providers()
        {
        }

        public void Add(Provider provider)
        {
            _providers.Add(provider);
        }

        public void AddAll(IProviders providers)
        {
            _providers.AddRange(providers.GetAll());
        }

        public List<Provider> GetAll()
        {
            return _providers;
        }

        public List<T> GetAllAs<T>()
        {
            List<T> productionOrderBoms = new List<T>();
            foreach (var demand in _providers)
            {
                productionOrderBoms.Add((T) demand.ToIProvider());
            }

            return productionOrderBoms;
        }

        public bool ProvideMoreThanOrEqualTo(Demand demand)
        {
            return GetProvidedQuantity(demand).IsGreaterThanOrEqualTo(demand.GetQuantity());
        }

        public Quantity GetProvidedQuantity(Demand demand)
        {
            Quantity providedQuantity = new Quantity();

            foreach (var provider in _providers)
            {
                if (demand.GetArticleId().Equals(provider.GetArticleId()))
                {
                    providedQuantity.IncrementBy(provider.GetQuantity());
                }
            }

            return providedQuantity;
        }

        public int Size()
        {
            return _providers.Count;
        }

        public bool Any()
        {
            return _providers.Any();
        }

        public void Clear()
        {
            _providers.Clear();
        }

        public List<T_Provider> GetAllAsT_Provider()
        {
            return _providers.Select(x => x.ToT_Provider()).ToList();
        }

        public bool AnyDependingDemands()
        {
            foreach (var provider in _providers)
            {
                if (provider.AnyDependingDemands())
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSatisfied(Demand demand)
        {
            bool isSatisfied = ProvideMoreThanOrEqualTo(demand);
            return isSatisfied;
        }

        public Quantity GetMissingQuantity(Demand demand)
        {
            Quantity missingQuantity = demand.GetQuantity().Minus(GetProvidedQuantity(demand));
            if (missingQuantity.IsNegative())
            {
                return Quantity.Null();
            }

            return missingQuantity;
        }

        public IProviderToDemandsMap GetAllDependingDemandsAsMap()
        {
            IProviderToDemandsMap providerToDemandsMap = new ProviderToDemandsMap();
            foreach (var provider in _providers)
            {
                providerToDemandsMap.AddDemandsForProvider(provider,
                    provider.GetAllDependingDemands());
            }

            return providerToDemandsMap;
        }

        public IDemands CalculateUnsatisfiedDemands(IDemands demands)
        {
            List<Demand> unSatisfiedDemands = new List<Demand>();
            Dictionary<Provider, Quantity> reservableQuantityToProvider =
                new Dictionary<Provider, Quantity>();
            foreach (var provider in _providers)
            {
                reservableQuantityToProvider.Add(provider, provider.GetQuantity());
            }

            foreach (var demand in demands.GetAll())
            {
                Quantity neededQuantity = demand.GetQuantity();
                foreach (var provider in _providers)
                {
                    Quantity reservableQuantity = reservableQuantityToProvider[provider];
                    if (provider.GetArticleId().Equals(demand.GetArticleId()) &&
                        reservableQuantity.IsGreaterThan(Quantity.Null()))
                    {
                        reservableQuantityToProvider[provider] = reservableQuantity
                            .Minus(neededQuantity);
                        neededQuantity = neededQuantity.Minus(reservableQuantity);

                        // neededQuantity < 0
                        if (neededQuantity.IsSmallerThan(Quantity.Null()))
                        {
                            break;
                        }
                        // neededQuantity > 0: continue to provide it
                    }
                }

                if (neededQuantity.IsGreaterThan(Quantity.Null()))
                {
                    unSatisfiedDemands.Add(demand);
                }
            }
            
            return new Demands(unSatisfiedDemands);
        }
    }
}