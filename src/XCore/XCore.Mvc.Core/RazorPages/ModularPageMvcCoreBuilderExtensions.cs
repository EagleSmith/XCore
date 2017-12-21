﻿using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace XCore.Mvc.Core.RazorPages
{
    public static class ModularPageMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddModularRazorPages(this IMvcCoreBuilder builder)
        {
            builder.AddRazorPages(options =>
            {
                options.RootDirectory = "/";
                options.Conventions.Add(new DefaultModularPageRouteModelConvention());
            });

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, ModularPageRazorViewEngineOptionsSetup>());

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPageApplicationModelProvider, ModularPageApplicationModelProvider>());

            builder.Services.Replace(
                ServiceDescriptor.Singleton<IActionDescriptorChangeProvider, ModularPageActionDescriptorChangeProvider>());

            return builder;
        }
    }
}
