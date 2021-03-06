﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Routing;
using System;

namespace XCore.Modules
{
    public class ModularRouteBuilder : IModularRouteBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        // Register one top level TenantRoute per tenant. Each instance contains all the routes
        // for this tenant.
        public ModularRouteBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRouteBuilder Build()
        {
            IApplicationBuilder appBuilder = new ApplicationBuilder(_serviceProvider);

            var routeBuilder = new RouteBuilder(appBuilder)
            {
            };

            return routeBuilder;
        }

        public void Configure(IRouteBuilder builder)
        {

        }
    }
}
