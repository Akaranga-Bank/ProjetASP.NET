using AkarangaBank.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,auditeur")]
public class AuditController : Controller
{
    private readonly AkarangaBankContext _db;
    public AuditController(AkarangaBankContext db) => _db = db;

    public async Task<IActionResult> Index(string? actionFilter, string? table)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(actionFilter))
            q = q.Where(a => a.Action.Contains(actionFilter));
        if (!string.IsNullOrWhiteSpace(table))
            q = q.Where(a => a.TableConcerne == table);

        ViewBag.ActionFilter = actionFilter;
        ViewBag.Table = table;

        return View(await q.OrderByDescending(a => a.DateEvenement).Take(200).ToListAsync());
    }
}