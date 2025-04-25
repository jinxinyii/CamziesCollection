using CapstoneCC.Models;
using CapstoneCC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CapstoneCC.Pages
{
    public class StaffModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StaffModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public List<ApplicationUser> Users { get; set; }

        // Store roles in the model
        public Dictionary<string, List<string>> UserRoles { get; set; }

        public async Task OnGetAsync()
        {
            // Load all users (you can adjust the filter if needed)
            Users = _userManager.Users.ToList();

            // Initialize UserRoles dictionary
            UserRoles = new Dictionary<string, List<string>>();

            // Retrieve the roles for each user
            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles.Add(user.Id, roles.ToList()); // Store roles by userId
            }
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, role);

                return RedirectToPage();
            }

            return Page();
        }
    }
}
