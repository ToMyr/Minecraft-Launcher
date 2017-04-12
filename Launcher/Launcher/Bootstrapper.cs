﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Launcher.Contracts;
using Launcher.ViewModels;
using Launcher.Views;
using MahApps.Metro.Controls;

namespace Launcher
{
    /// <summary>
    /// The application bootstrapper. Here's the DI container configuration.
    /// </summary>
    public class Bootstrapper : BootstrapperBase
    {
        private IContainer container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            //Register the shell
            builder.RegisterInstance(new ShellView()).As<MetroWindow>().AsSelf().SingleInstance();
            builder.RegisterType<ShellViewModel>().As<IShell>().SingleInstance();

            //Register tabs
            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(IsTab)
                .AsSelf()
                .As<ITab>()
                .SingleInstance();

            //Register services
            builder
                .RegisterAssemblyTypes(currentAssembly)
                .Where(IsService)
                .AsImplementedInterfaces()
                .SingleInstance();

            container = builder.Build();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }

        protected override void BuildUp(object instance)
        {
            container.InjectProperties(instance);
        }

        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrEmpty(key) ? container.Resolve(service) : container.ResolveKeyed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            Type enumerable = typeof(IEnumerable<>).MakeGenericType(service);
            return (IEnumerable<object>)container.Resolve(enumerable);
        }

        private static bool IsTab(Type type)
        {
            string name = type.Name;
            return name.EndsWith("ViewModel") && type.IsAssignableTo<ITab>();
        }

        private static bool IsService(Type type)
        {
            string[] serviceSuffixes = { "Service", "Manager", "Helper" };
            return serviceSuffixes.Any(suffix => type.Name.EndsWith(suffix));
        }
    }
}