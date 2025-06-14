﻿using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using DBIT_Inventory.Services;

namespace DBIT_Inventory.App_Start
{
    public class DependencyConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterType<DBIT_Inventory>().AsSelf().InstancePerRequest();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerRequest();
            builder.RegisterType<DBIT_Inventory>().AsSelf().InstancePerRequest();
            builder.RegisterType<AssetService>().As<IAssetService>().InstancePerRequest();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterType<FileService>().As<IFileService>().InstancePerRequest();
            builder.RegisterType<DropdownService>().As<IDropdownService>().InstancePerRequest();



            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));


        }

    }
}