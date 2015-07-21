using System.Collections.Generic;
using System.Linq;
using NGM.OpenAuthentication.Models;
using Orchard;
using Orchard.Caching;
using Orchard.Data;
using System;

namespace NGM.OpenAuthentication.Services {
    public interface IProviderConfigurationService : IDependency {
        IEnumerable<ProviderConfiguration> GetAll();
        ProviderConfiguration Get(string providerName);
        void Delete(int id);
        void Create(ProviderConfiguration parameters);
        bool VerifyUnicity(string providerName);
    }

    public class ProviderConfigurationService : IProviderConfigurationService {
        private readonly IRepository<ProviderConfigurationRecord> _repository;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public ProviderConfigurationService(
            IRepository<ProviderConfigurationRecord> repository,
            ICacheManager cacheManager,
            ISignals signals) {
            _repository = repository;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public IEnumerable<ProviderConfiguration> GetAll() {
            return ProviderConfigurations.Values;
        }

        public ProviderConfiguration Get(string providerName) {
            ProviderConfiguration provider;
            if (ProviderConfigurations.TryGetValue(providerName, out provider)) {
                return provider;
            }

            return null;
        }

        public void Delete(int id) {
            _repository.Delete(_repository.Get(o => o.Id == id));
            _signals.Trigger("ProviderConfigurations");
        }
        
        public bool VerifyUnicity(string providerName) {
            return Get(providerName) == null; 
        }

        public void Create(ProviderConfiguration parameters) {
            _repository.Create(new ProviderConfigurationRecord {
                    DisplayName = parameters.DisplayName,
                    ProviderName = parameters.ProviderName,
                    ProviderIdentifier = parameters.ProviderIdentifier,
                    ProviderIdKey = parameters.ProviderIdKey,
                    ProviderSecret = parameters.ProviderSecret,
                    IsEnabled = true
                });

            _signals.Trigger("ProviderConfigurations");
        }

        private Dictionary<string, ProviderConfiguration> ProviderConfigurations {
            get {
                return _cacheManager.Get("ProviderConfigurations", ctx => {
                    ctx.Monitor(_signals.When("ProviderConfigurations"));

                    var allConfigurations = _repository.Table.ToList().Select(Convert);
                    return allConfigurations.ToDictionary(x => x.ProviderName, StringComparer.InvariantCultureIgnoreCase);
                });
            }
        }

        private ProviderConfiguration Convert(ProviderConfigurationRecord record) {
            if (record == null) {
                return null;
            }

            return new ProviderConfiguration {
                Id = record.Id,
                DisplayName = record.DisplayName,
                ProviderName = record.ProviderName,
                ProviderIdentifier = record.ProviderIdentifier,
                ProviderIdKey = record.ProviderIdKey,
                ProviderSecret = record.ProviderSecret,
                IsEnabled = record.IsEnabled
            };
        }
    }

    public class ProviderConfiguration {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string ProviderName { get; set; }
        public string ProviderIdentifier { get; set; }
        public string ProviderIdKey { get; set; }
        public string ProviderSecret { get; set; }
        public bool IsEnabled { get; set; }
    }
}