using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vidly.Data;
using Vidly.Models;
using Vidly.ViewModels;

namespace Vidly.Controllers;

public class CustomersController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.Customers.Include(c => c.MembershipType).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search));

        ViewData["Search"] = search;
        return View(await query.OrderBy(c => c.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await context.Customers.Include(c => c.MembershipType).AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id);
        return customer == null ? NotFound() : View(customer);
    }

    [Authorize]
    public async Task<IActionResult> New() =>
        View("CustomerForm", await BuildForm(new Customer()));

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        return customer == null ? NotFound() : View("CustomerForm", await BuildForm(customer));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Customer")] Customer customer)
    {
        if (!ModelState.IsValid)
            return View("CustomerForm", await BuildForm(customer));

        if (customer.Id == 0)
        {
            context.Customers.Add(customer);
        }
        else
        {
            var customerInDb = await context.Customers.FindAsync(customer.Id);
            if (customerInDb == null)
                return NotFound();

            customerInDb.Name = customer.Name;
            customerInDb.Birthdate = customer.Birthdate;
            customerInDb.MembershipTypeId = customer.MembershipTypeId;
            customerInDb.IsSubscribedToNewsletter = customer.IsSubscribedToNewsletter;
        }

        await context.SaveChangesAsync();
        TempData["Message"] = $"Saved {customer.Name}.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
        TempData["Message"] = $"Deleted {customer.Name}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CustomerFormViewModel> BuildForm(Customer customer) => new()
    {
        Customer = customer,
        MembershipTypes = await context.MembershipTypes.AsNoTracking().OrderBy(m => m.Id).ToListAsync(),
    };
}
