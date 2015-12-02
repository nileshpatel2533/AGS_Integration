using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Tables;
using stranddService.DataObjects;

namespace stranddService.Models
{
    public class stranddContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to alter your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
        //
        // To enable Entity Framework migrations in the cloud, please ensure that the 
        // service name, set by the 'MS_MobileServiceName' AppSettings in the local 
        // Web.config, is the same as the service name when hosted in Azure.

        //Altered at WEB Config & Publish Settings Level
        private const string connectionStringName = "Name=MS_TableConnectionString";

        public stranddContext() : base(connectionStringName)
        {
        } 

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<AccountRole> AccountRoles { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleAccountLink> VehicleAccountLinks { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<HistoryEvent> HistoryLog { get; set; }
        public DbSet<ExceptionEntry> ExceptionLog { get; set; }
        public DbSet<CommunicationEntry> CommunicationLog { get; set; }
        public DbSet<CostingSlab> CostingSlabs { get; set; }
        public DbSet<IncidentCosting> IncidentCostings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            string schema = ServiceSettingsDictionary.GetSchemaName();
            if (!string.IsNullOrEmpty(schema))
            {
                modelBuilder.HasDefaultSchema(schema);
            }


            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }

    }

}
