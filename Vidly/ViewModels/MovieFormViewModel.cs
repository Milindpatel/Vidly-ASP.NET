using System.ComponentModel.DataAnnotations;
using Vidly.Models;

namespace Vidly.ViewModels;

public class MovieFormViewModel
{
    public IEnumerable<Genre> Genres { get; set; } = [];

    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = "";

    [Display(Name = "Genre")]
    [Required(ErrorMessage = "Please select a genre.")]
    public byte? GenreId { get; set; }

    [Display(Name = "Release Date")]
    [DataType(DataType.Date)]
    [Required]
    public DateTime? ReleaseDate { get; set; }

    [Display(Name = "Number in Stock")]
    [Range(1, 20)]
    [Required]
    public byte? NumberInStock { get; set; }

    public string Title => Id == 0 ? "New Movie" : "Edit Movie";

    public MovieFormViewModel() { }

    public MovieFormViewModel(Movie movie)
    {
        Id = movie.Id;
        Name = movie.Name;
        ReleaseDate = movie.ReleaseDate;
        NumberInStock = movie.NumberInStock;
        GenreId = movie.GenreId;
    }
}
