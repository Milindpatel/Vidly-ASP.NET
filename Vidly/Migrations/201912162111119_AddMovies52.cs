namespace Vidly.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMovies52 : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Movies (Name, GenreId, ReleaseDate, DateAdded, NumberInStock) VALUES ('Hang Over 3',5, 2017-12-12, 2017-12-12,3)");
        }
        
        public override void Down()
        {
        }
    }
}
