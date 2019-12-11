namespace Vidly.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteDateInOfTheValueOfCustomerTable : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE Customers SET Birthdate = NULL WHERE Name='Dolly Patel'");
        }
        
        public override void Down()
        {
        }
    }
}
