using IdentityServer.Web.Models;
using IdentityServer.Web.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(userManager, roleManager)
        {
        }

        public IActionResult GetClaims()
        {
            return View(User.Claims.ToList());
        }

        public IActionResult Homepage()
        {
            return View();
        }

        public IActionResult GetUsers()
        {
            return View(_userManager.Users.ToList());
        }


        public IActionResult GetRoles()
        {
            return View(_roleManager.Roles.ToList());
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                AppRole role = new AppRole();
                role.Name = viewModel.Name;

                IdentityResult result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return CreateCustomAlert("Yeni rol kaydedilmistir", "Success", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetRoles)}");
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            AppRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
                return CreateCustomAlert("Sectiginiz rol silinmistir", "Success", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetRoles)}");
            }

            else
            {
                return CreateCustomAlert("Boyle bir rol bulunmamaktadir", "Danger", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetRoles)}");
            }

        }

        [HttpGet]
        public async Task<IActionResult> UpdateRole(string id)
        {
            AppRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                UpdateRoleViewModel viewModel = role.Adapt<UpdateRoleViewModel>();
                return View(viewModel);
            }
            else
            {
                return CreateCustomAlert("Boyle bir rol bulunmamaktadir", "Danger", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetRoles)}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateRoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {

                AppRole role = await _roleManager.FindByIdAsync(viewModel.Id);
                if (role != null)
                {
                    role.Name = viewModel.Name;

                    IdentityResult result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        return CreateCustomAlert("Sectiginiz rol guncellenmistir", "Success", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetRoles)}");
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    return CreateCustomAlert ("Boyle bir rol bulunmamaktadir", "Danger", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetRoles)}");
                }

            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateRolesToUser(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return CreateCustomAlert("Boyle bir kullanici bulunmamaktadir", "Danger", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetRoles)}");
            }
            else
            {
                ViewBag.userName = user.UserName;
                TempData["userId"] = user.Id;

                IQueryable<AppRole> roles = _roleManager.Roles;

                List<string> userRoles = _userManager.GetRolesAsync(user).Result.ToList();

                List<RoleAssignViewModel> viewModels = new List<RoleAssignViewModel>();

                // Assigning checkbox

                foreach (var role in roles)
                {
                    RoleAssignViewModel viewModel = new RoleAssignViewModel()
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                    };

                    viewModel.IsExists = true ? userRoles.Contains(role.Name) : viewModel.IsExists = false;

                    viewModels.Add(viewModel);
                }

                return View(viewModels);
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateRolesToUser(List<RoleAssignViewModel> viewModels)
        {
            AppUser user = await _userManager.FindByIdAsync(TempData["userId"].ToString());
            foreach (var viewModel in viewModels)
            {
                if (viewModel.IsExists)
                {
                    await _userManager.AddToRoleAsync(user, viewModel.RoleName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, viewModel.RoleName);
                }
            }

            return CreateCustomAlert("Sectiginiz kullanicinin rolleri guncellenmistir", "Success", $"/{nameof(AdminController).Replace("Controller", "")}/{nameof(AdminController.GetUsers)}");

        }
    }
}
