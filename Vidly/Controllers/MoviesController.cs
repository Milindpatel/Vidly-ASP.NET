using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vidly.Data;
using Vidly.Models;
using Vidly.ViewModels;

namespace Vidly.Controllers;

public class MoviesController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.Movies.Include(m => m.Genre).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search));

        ViewData["Search"] = search;
        return View(await query.OrderBy(m => m.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var movie = await context.Movies.Include(m => m.Genre).AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);
        return movie == null ? NotFound() : View(movie);
    }

    [Authorize]
    public async Task<IActionResult> New() =>
        View("MovieForm", await WithGenres(new MovieFormViewModel()));

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var movie = await context.Movies.FindAsync(id);
        return movie == null ? NotFound() : View("MovieForm", await WithGenres(new MovieFormViewModel(movie)));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(MovieFormViewModel form)
    {
        if (!ModelState.IsValid)
            return View("MovieForm", await WithGenres(form));

        Movie? movie;
        if (form.Id == 0)
        {
            movie = new Movie { DateAdded = DateTime.UtcNow };
            context.Movies.Add(movie);
        }
        else
        {
            movie = await context.Movies.FindAsync(form.Id);
            if (movie == null)
                return NotFound();
        }

        movie.Name = form.Name;
        movie.GenreId = form.GenreId!.Value;
        movie.ReleaseDate = form.ReleaseDate;
        movie.NumberInStock = form.NumberInStock!.Value;

        await context.SaveChangesAsync();
        TempData["Message"] = $"Saved {movie.Name}.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();

        context.Movies.Remove(movie);
        await context.SaveChangesAsync();
        TempData["Message"] = $"Deleted {movie.Name}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<MovieFormViewModel> WithGenres(MovieFormViewModel form)
    {
        form.Genres = await context.Genres.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
        return form;
    }
}
