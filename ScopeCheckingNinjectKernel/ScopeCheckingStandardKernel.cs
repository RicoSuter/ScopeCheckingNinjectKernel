using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ninject;
using Ninject.Activation;
using Ninject.Planning.Bindings;

namespace ScopeCheckingNinjectKernel
{
    /// <summary>A <see cref="StandardKernel"/> which additionally checks that injected objects are correctly scoped.</summary>
    public class ScopeCheckingStandardKernel : StandardKernel
    {
        /// <summary>Gets or sets a value indicating whether a transient scoped object is allowed in a singleton scoped object.</summary>
        public bool AllowTransientScopeInSingletonScope { get; set; }

        /// <summary>Gets or sets a value indicating whether a transient scoped object is allowed in a thread scoped object.</summary>
        public bool AllowTransientScopeInThreadScope { get; set; }

        /// <summary>Gets or sets a value indicating whether a transient scoped object is allowed in a custom scoped object.</summary>
        public bool AllowTransientScopeInCustomScope { get; set; }

        /// <summary>Gets or sets a value indicating whether to disable checks on transient scoped parent objects.</summary
        public bool IgnoreTransientScopedParents { get; set; }

        /// <summary>Resolves instances for the specified request. 
        /// The instances are not actually resolved until a consumer iterates over the enumerator.</summary>
        /// <param name="request">The request to resolve.</param>
        /// <returns>An enumerator of instances that match the request.</returns>
        /// <exception cref="InvalidOperationException">The scope of the injected object is not compatible with the scope of the parent object.</exception>
        public override IEnumerable<object> Resolve(IRequest request)
        {
            var isInjectedIntoParent = request.ActiveBindings.Any();
            if (isInjectedIntoParent)
            {
                var parentBinding = request.ActiveBindings.Last();
                var parentScope = parentBinding.GetScope(CreateContext(request, parentBinding));

                var bindings = GetBindings(request.Service).Where(SatifiesRequest(request));
                if (bindings.Any(binding => IsScopeAllowed(request, parentScope, binding) == false))
                {
                    throw new InvalidOperationException("The scope of the injected object (" + request.Service.FullName + ") " +
                                                        "is not compatible with the scope of the parent object (" + parentBinding.Service.FullName + ").");
                }
            }

            return base.Resolve(request);
        }

        private bool IsScopeAllowed(IRequest request, object parentScope, IBinding binding)
        {
            var scope = binding.GetScope(CreateContext(request, binding));

            var haveSameScope = scope == parentScope;
            if (haveSameScope)
                return true;

            var isChildSingletonScoped = scope == this;
            var isChildTransientScoped = scope == null;

            var isParentSingletonScoped = parentScope == this;
            if (isParentSingletonScoped)
                return isChildSingletonScoped || (AllowTransientScopeInSingletonScope && isChildTransientScoped);

            var isParentThreadScoped = parentScope is Thread;
            if (isParentThreadScoped)
                return isChildSingletonScoped || (AllowTransientScopeInThreadScope && isChildTransientScoped);

            var isParentTransientScoped = parentScope == null;
            if (isParentTransientScoped)
                return isChildSingletonScoped || IgnoreTransientScopedParents;

            return (AllowTransientScopeInCustomScope && isChildTransientScoped);
        }
    }
}