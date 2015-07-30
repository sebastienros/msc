using System.Data;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using NHibernate;

namespace Orchard.Data {
    public interface ITransactionManager : IDependency {
        ISession Demand();
        void RequireNew();
        void RequireNew(IsolationLevel level);
        void Cancel();
    }

    public class TransactionFilter : FilterProvider, IExceptionFilter {
        private readonly ITransactionManager _transactionManager;

        public TransactionFilter(ITransactionManager transactionManager) {
            _transactionManager = transactionManager;
        }

        public void OnException(ExceptionContext filterContext) {
            _transactionManager.Cancel();
        }
    }
}
