using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ParcInformatiqueContext context;

    public UsersController(ParcInformatiqueContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await context.Users
            .AsNoTracking()
            .ToListAsync());
    }

    [HttpGet("{login}")]
    public async Task<IActionResult> GetUserByLogin(string login)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Login == login);

        if (user == null)
            return NotFound();

        return Ok(user);
    }
}
