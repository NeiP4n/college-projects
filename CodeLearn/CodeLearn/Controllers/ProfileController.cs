using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CodeLearn.Data;
using CodeLearn.Helpers;
using CodeLearn.Models;
using CodeLearn.Services;

namespace CodeLearn.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext         _db;
        private readonly UserManager<AppUser> _users;
        private readonly IAvatarService       _avatar;

        public ProfileController(AppDbContext db, UserManager<AppUser> users, IAvatarService avatar)
        {
            _db     = db;
            _users  = users;
            _avatar = avatar;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(_users.GetUserId(User)!);
            var user = await _db.Users
                .Include(u => u.LoginRecords)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            return View(BuildVm(user));
        }

        // POST /Profile/Save — изменить ник и/или аватар
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string nickName, IFormFile? avatar)
        {
            var userId = int.Parse(_users.GetUserId(User)!);
            var user = await _db.Users
                .Include(u => u.LoginRecords)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var cleanNick = InputSanitizer.SanitizeNickname(nickName ?? "");
            if (!InputSanitizer.IsValidNickname(cleanNick))
            {
                ModelState.AddModelError("NickName", "Недопустимый никнейм (3–20 символов)");
                return View("Index", BuildVm(user));
            }

            user.NickName = cleanNick;

            if (avatar is { Length: > 0 })
            {
                var (ok, error, url) = await _avatar.SaveAsync(avatar);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, error!);
                    return View("Index", BuildVm(user));
                }
                user.AvatarUrl = url!;
            }

            await _db.SaveChangesAsync();
            TempData["ProfileSaved"] = "Профиль обновлён";
            return RedirectToAction("Index");
        }

        private static ProfileViewModel BuildVm(AppUser user)
        {
            var days = user.LoginRecords
                .Select(r => r.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int streak = 0;
            var today = DateTime.UtcNow.Date;
            for (int i = 0; i < days.Count; i++)
            {
                if (days[i] == today.AddDays(-i)) streak++;
                else break;
            }

            return new ProfileViewModel
            {
                NickName      = user.NickName,
                AvatarUrl     = AvatarHelper.GetUrl(user.NickName, user.AvatarUrl),
                CurrentModule = user.CurrentModule,
                TotalDays     = days.Count,
                Streak        = streak
            };
        }
    }
}
