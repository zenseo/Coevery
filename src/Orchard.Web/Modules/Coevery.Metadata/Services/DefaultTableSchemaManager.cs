﻿using System;
using System.Linq;
using System.Collections.Generic;
using Coevery.Dynamic;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment.ShellBuilders.Models;

namespace Coevery.Metadata.Services
{
    public class DefaultTableSchemaManager : ITableSchemaManager
    {
        private readonly IDataMigrationInterpreter _interpreter;
        private readonly SchemaBuilder _schemaBuilder;
        private readonly ISessionLocator _sessionLocator;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly IDataServicesProviderFactory _dataServiceProviderFactory;

        public DefaultTableSchemaManager(IDataMigrationInterpreter interpreter,
            ISessionLocator sessionLocator,
             ISessionFactoryHolder sessionFactoryHolder,
            IDataServicesProviderFactory dataServiceProviderFactory)
        {
            _interpreter = interpreter;
            _sessionLocator = sessionLocator;
            _schemaBuilder = new SchemaBuilder(_interpreter);
            _sessionFactoryHolder = sessionFactoryHolder;
            _dataServiceProviderFactory = dataServiceProviderFactory;
        }

        public void UpdateSchema(IEnumerable<RecordBlueprint> recordBlueprints)
        {
            var persistenceModel = AbstractDataServicesProvider.CreatePersistenceModel(recordBlueprints.ToList());
            var dataServiceProvider = this._dataServiceProviderFactory.CreateProvider(this._sessionFactoryHolder.GetSessionFactoryParameters());
            var persistenceConfigurer = dataServiceProvider.GetPersistenceConfigurer(true);
            
            var configuration = Fluently.Configure()
                    .Database(persistenceConfigurer)
                    .Mappings(m => m.AutoMappings.Add(persistenceModel))
                    .ExposeConfiguration(c =>
                    {
                        // This is to work around what looks to be an issue in the NHibernate driver:
                        // When inserting a row with IDENTITY column, the "SELET @@IDENTITY" statement
                        // is issued as a separate command. By default, it is also issued in a separate
                        // connection, which is not supported (returns NULL).
                        //c.SetProperty("connection.release_mode", "on_close");
                        //new SchemaExport(c).Create(false /*script*/, true /*export*/);
                       // new SchemaUpdate(c).Execute(false, true);
                    })
                    .BuildConfiguration();
            
            new SchemaUpdate(configuration).Execute(false, true);

        }
    }
}