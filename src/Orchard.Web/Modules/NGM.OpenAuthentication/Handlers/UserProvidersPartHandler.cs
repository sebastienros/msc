using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NGM.OpenAuthentication.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace NGM.OpenAuthentication.Handlers {
    [UsedImplicitly]
    public class UserProvidersPartHandler : ContentHandler {
        private readonly IRepository<UserProviderRecord> _userProviderRepository;

        public UserProvidersPartHandler(IRepository<UserProviderRecord> userProviderRepository) {
            _userProviderRepository = userProviderRepository;

            Filters.Add(new ActivatingFilter<UserProvidersPart>("User"));

            OnLoaded<UserProvidersPart>((context, part) =>
                part.ProviderEntriesField.Loader(x => {
                    return _userProviderRepository
                        .Table
                        .Where(u => u.UserId == part.Id)
                        .Select(u => new UserProviderEntry {
                            Id = u.Id,
                            ProviderUserId = u.ProviderUserId,
                            ProviderName = u.ProviderName
                        }).ToList();
                })
            );
        }
    }
}