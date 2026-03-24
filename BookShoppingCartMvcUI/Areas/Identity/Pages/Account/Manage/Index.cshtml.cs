// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShoppingCartMvcUI.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly BookShoppingCartMvcUI.Services.IProfileService _profileService;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            BookShoppingCartMvcUI.Services.IProfileService profileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _profileService = profileService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Họ tên đầy đủ")]
            [MaxLength(100)]
            public string FullName { get; set; }

            [Display(Name = "Địa chỉ")]
            [MaxLength(200)]
            public string Address { get; set; }

            [Display(Name = "Ngày sinh")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            // load claims for additional profile fields
            var claims = await _userManager.GetClaimsAsync(user);
            var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");
            var addressClaim = claims.FirstOrDefault(c => c.Type == "Address");
            var dobClaim = claims.FirstOrDefault(c => c.Type == "DateOfBirth");

            DateTime? dob = null;
            if (dobClaim != null && DateTime.TryParseExact(dobClaim.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                dob = parsed;
            }

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = fullNameClaim?.Value,
                Address = addressClaim?.Value,
                DateOfBirth = dob
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // load using profile proxy/service
            var input = await _profileService.GetProfileAsync(user);
            Username = await _userManager.GetUserNameAsync(user);
            Input = input;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                // reload profile in case of validation error
                var current = await _profileService.GetProfileAsync(user);
                Username = await _userManager.GetUserNameAsync(user);
                Input = current;
                return Page();
            }

            var ok = await _profileService.UpdateProfileAsync(user, Input);
            if (!ok)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
