namespace TaskTracker.Repository.Sql.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Example of PM commands:
    /// A. Add a new DB migration: 
    ///    Add-Migration "DBSchemaV1" -ConnectionString "Data Source=(LocalDB)\MSSQLLocalDB;Initial catalog=TaskTrackerDB1;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" -ConnectionProviderName "System.Data.SqlClient"
    ///    
    /// B. Update DB with the migration: 
    ///    Update-Database -ConnectionString "Data Source=(LocalDB)\MSSQLLocalDB;Initial catalog=TaskTrackerDB1;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" -ConnectionProviderName "System.Data.SqlClient"
    ///    
    /// C. Rollback DB schema to a previous version: 
    ///    Update-Database -TargetMigration:"DBSchemaV0_Existing" -ConnectionString "Data Source=(LocalDB)\MSSQLLocalDB;Initial catalog=TaskTrackerDB1;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" -ConnectionProviderName "System.Data.SqlClient"
    ///    
    /// So, in case of a change of the model required the workflow would be:
    ///   1. Change Model classes (e.g. Project)
    ///   2. Implement the "Seed" method below to handle ONLY THE CHANGE. E.g. init a new column of changed entity. 
    ///   3. Call the Add-Migration command on this project. 
    ///      This will create migration CS file describing the change relatively the current DB schema.
    ///   4. Call the Update-Database command on this project. 
    ///      This will apply the change to DB calling the Seed (where e.g. a new column is filled somehow)
    ///      Also the command inserts a new record into the "__MigrationHistory" table.
    /// </remarks>
    internal sealed class Configuration : DbMigrationsConfiguration<TaskTrackerDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(TaskTrackerDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
