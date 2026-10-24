using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vidly.Models;

namespace Vidly.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MembershipType> MembershipTypes => Set<MembershipType>();
    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MembershipType>().HasData(
            new MembershipType { Id = MembershipType.PayAsYouGo, Name = "Pay as You Go", SignUpFee = 0, DurationInMonths = 0, DiscountRate = 0 },
            new MembershipType { Id = 2, Name = "Monthly", SignUpFee = 30, DurationInMonths = 1, DiscountRate = 10 },
            new MembershipType { Id = 3, Name = "Quarterly", SignUpFee = 90, DurationInMonths = 3, DiscountRate = 15 },
            new MembershipType { Id = 4, Name = "Annual", SignUpFee = 300, DurationInMonths = 12, DiscountRate = 20 });

        builder.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 2, Name = "Thriller" },
            new Genre { Id = 3, Name = "Family" },
            new Genre { Id = 4, Name = "Romance" },
            new Genre { Id = 5, Name = "Comedy" });

        var added = new DateTime(2017, 12, 12);
        builder.Entity<Movie>().HasData(
            new Movie { Id = 1, Name = "A Love So Beautiful", GenreId = 4, ReleaseDate = new DateTime(2017, 11, 3), DateAdded = added, NumberInStock = 3 },
            new Movie { Id = 2, Name = "The Hangover", GenreId = 5, ReleaseDate = new DateTime(2009, 6, 5), DateAdded = added, NumberInStock = 5 },
            new Movie { Id = 3, Name = "The Hangover Part II", GenreId = 5, ReleaseDate = new DateTime(2011, 5, 26), DateAdded = added, NumberInStock = 4 },
            new Movie { Id = 4, Name = "The Hangover Part III", GenreId = 5, ReleaseDate = new DateTime(2013, 5, 23), DateAdded = added, NumberInStock = 2 });
    }
}
