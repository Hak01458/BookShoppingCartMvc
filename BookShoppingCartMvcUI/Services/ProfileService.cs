using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using BookShoppingCartMvcUI.Areas.Identity.Pages.Account.Manage;

namespace BookShoppingCartMvcUI.Services
{
    public interface IProfileService
    {
        Task<IndexModel.InputModel> GetProfileAsync(IdentityUser user);
        Task<bool> UpdateProfileAsync(IdentityUser user, IndexModel.InputModel input);
    }

    // Real subject: performs actual profile operations using UserManager
    public class ProfileService : IProfileService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IndexModel.InputModel> GetProfileAsync(IdentityUser user)
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");
            var addressClaim = claims.FirstOrDefault(c => c.Type == "Address");
            var dobClaim = claims.FirstOrDefault(c => c.Type == "DateOfBirth");

            DateTime? dob = null;
            if (dobClaim != null && DateTime.TryParseExact(dobClaim.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                dob = parsed;
            }

            return new IndexModel.InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = fullNameClaim?.Value,
                Address = addressClaim?.Value,
                DateOfBirth = dob
            };
        }

        public async Task<bool> UpdateProfileAsync(IdentityUser user, IndexModel.InputModel input)
        {
            // update phone
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                    return false;
            }

            var claims = await _userManager.GetClaimsAsync(user);

            async Task<bool> upsertClaim(string type, string value)
            {
                var existing = claims.FirstOrDefault(c => c.Type == type);
                if (existing != null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        var removed = await _userManager.RemoveClaimAsync(user, existing);
                        return removed.Succeeded;
                    }
                    else if (existing.Value != value)
                    {
                        var replaced = await _userManager.ReplaceClaimAsync(user, existing, new Claim(type, value));
                        return replaced.Succeeded;
                    }
                    return true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var added = await _userManager.AddClaimAsync(user, new Claim(type, value));
                        return added.Succeeded;
                    }
                    return true;
                }
            }

            if (!await upsertClaim("FullName", input.FullName ?? string.Empty)) return false;
            if (!await upsertClaim("Address", input.Address ?? string.Empty)) return false;
            var dobString = input.DateOfBirth?.ToString("yyyy-MM-dd");
            if (!await upsertClaim("DateOfBirth", dobString ?? string.Empty)) return false;

            return true;
        }
    }

    // Proxy: caching + simple logging before delegating to ProfileService
    public class ProfileProxy : IProfileService
    {
        private readonly ProfileService _real;
        private readonly IMemoryCache _cache;

        public ProfileProxy(ProfileService real, IMemoryCache cache)
        {
            _real = real;
            _cache = cache;
        }

        public async Task<IndexModel.InputModel> GetProfileAsync(IdentityUser user)
        {
            var key = $"profile_{user.Id}";
            if (_cache.TryGetValue<IndexModel.InputModel>(key, out var cached))
            {
                // return a shallow copy to avoid accidental modification of cached instance
                return new IndexModel.InputModel
                {
                    PhoneNumber = cached.PhoneNumber,
                    FullName = cached.FullName,
                    Address = cached.Address,
                    DateOfBirth = cached.DateOfBirth
                };
            }
            var result = await _real.GetProfileAsync(user);
            // cache for 5 minutes
            _cache.Set(key, result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async Task<bool> UpdateProfileAsync(IdentityUser user, IndexModel.InputModel input)
        {
            // simple logging
            Console.WriteLine($"[ProfileProxy] Updating profile for user {user.Id}");
            var ok = await _real.UpdateProfileAsync(user, input);
            if (ok)
            {
                // invalidate cache
                var key = $"profile_{user.Id}";
                _cache.Remove(key);
            }
            return ok;
        }
    }
}
