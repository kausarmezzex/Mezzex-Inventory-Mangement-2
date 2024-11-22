using Microsoft.AspNetCore.Mvc;
using Mezzex_Inventory_Mangement.Models;
using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.Data;
using Microsoft.AspNetCore.Identity;
using Mezzex_Inventory_Mangement.ViewModels;

namespace Mezzex_Inventory_Mangement.Controllers
{
    public class PermissionManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Inject UserManager
        }

        // List all roles with permissions
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> AssignPermissions(int roleId)  
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null) return NotFound();

            var allPermissions = await _context.PermissionsName.ToListAsync();
            var viewModel = new RolePermissionViewModel
            {
                RoleId = roleId,
                RoleName = role.Name,
                Permissions = allPermissions.Select(p => new PermissionAssignment
                {
                    PermissionId = p.Id,
                    PermissionName = p.Name,
                    Page = p.Page,
                    IsAssigned = role.RolePermissions.Any(rp => rp.PermissionId == p.Id)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignPermissions(RolePermissionViewModel model)
        {
            if (model.Permissions == null)
            {
                return BadRequest("Permissions list cannot be null.");
            }

            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == model.RoleId);

            if (role == null) return NotFound();

            var existingPermissions = role.RolePermissions.ToList();

            // Remove unassigned permissions
            foreach (var permission in existingPermissions)
            {
                if (!model.Permissions.Any(p => p.IsAssigned && p.PermissionId == permission.PermissionId))
                {
                    _context.RolePermissions.Remove(permission);
                }
            }

            // Add newly assigned permissions
            foreach (var permission in model.Permissions.Where(p => p.IsAssigned))
            {
                if (!existingPermissions.Any(ep => ep.PermissionId == permission.PermissionId))
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = model.RoleId,
                        PermissionId = permission.PermissionId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> OverrideUserPermissions(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var allPermissions = await _context.PermissionsName.ToListAsync();
            var viewModel = new UserPermissionViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                Permissions = allPermissions.Select(p => new PermissionAssignment
                {
                    PermissionId = p.Id,
                    PermissionName = p.Name,
                    Page = p.Page,
                    IsAssigned = user.UserPermissions.Any(up => up.PermissionId == p.Id && up.IsOverride)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> OverrideUserPermissions(UserPermissionViewModel model)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null) return NotFound();

            var existingOverrides = user.UserPermissions.Where(up => up.IsOverride).ToList();

            // Remove unassigned overrides
            foreach (var overridePermission in existingOverrides)
            {
                if (!model.Permissions.Any(p => p.IsAssigned && p.PermissionId == overridePermission.PermissionId))
                {
                    _context.UserPermissions.Remove(overridePermission);
                }
            }

            // Add new overrides
            foreach (var permission in model.Permissions.Where(p => p.IsAssigned))
            {
                if (!existingOverrides.Any(eo => eo.PermissionId == permission.PermissionId))
                {
                    _context.UserPermissions.Add(new UserPermission
                    {
                        UserId = model.UserId,
                        PermissionId = permission.PermissionId,
                        IsOverride = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageRoleUsers(int roleId)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null) return NotFound();

            // Get all users
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserPermissions)
                .ToListAsync();

            // Prepare ViewModel
            var viewModel = new ManageRoleUsersViewModel
            {
                RoleId = roleId,
                RoleName = role.Name,
                Permissions = role.RolePermissions.Select(rp => new PermissionAssignment
                {
                    PermissionId = rp.PermissionId,
                    PermissionName = rp.Permission.Name,
                    Page = rp.Permission.Page
                }).ToList(),
                Users = users.Select(u => new UserAssignment
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IsAssigned = u.UserRoles.Any(ur => ur.RoleId == roleId),
                    Permissions = role.RolePermissions.Select(rp => new PermissionAssignment
                    {
                        PermissionId = rp.PermissionId,
                        PermissionName = rp.Permission.Name,
                        Page = rp.Permission.Page,
                        IsAssigned = u.UserPermissions.Any(up => up.PermissionId == rp.PermissionId && up.IsOverride)
                            ? u.UserPermissions.First(up => up.PermissionId == rp.PermissionId).IsAssigned
                            : true // Default to true if inherited
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManageRoleUsers(ManageRoleUsersViewModel model)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == model.RoleId);

            if (role == null) return NotFound();

            // Update user-role assignments
            foreach (var user in model.Users)
            {
                var userEntity = await _context.Users
                    .Include(u => u.UserRoles)
                    .Include(u => u.UserPermissions)
                    .FirstOrDefaultAsync(u => u.Id == user.UserId);

                if (userEntity == null) continue;

                // Add or remove the user from the role
                if (user.IsAssigned)
                {
                    if (!userEntity.UserRoles.Any(ur => ur.RoleId == model.RoleId))
                    {
                        _context.UserRoles.Add(new IdentityUserRole<int>
                        {
                            UserId = user.UserId,
                            RoleId = model.RoleId
                        });
                    }
                }
                else
                {
                    var userRole = userEntity.UserRoles.FirstOrDefault(ur => ur.RoleId == model.RoleId);
                    if (userRole != null)
                    {
                        _context.UserRoles.Remove(userRole);
                    }
                }

                // Update permissions for the user
                foreach (var permission in user.UserPermissions)
                {
                    var userPermission = userEntity.UserPermissions.FirstOrDefault(up => up.PermissionId == permission.PermissionId);

                    if (userPermission == null && !permission.IsAssigned)
                    {
                        // If the permission is removed as an override, skip
                        continue;
                    }

                    if (userPermission == null)
                    {
                        // Add permission as an override
                        userEntity.UserPermissions.Add(new UserPermission
                        {
                            UserId = user.UserId,
                            PermissionId = permission.PermissionId,
                            IsAssigned = permission.IsAssigned,
                            IsOverride = true
                        });
                    }
                    else
                    {
                        // Update existing permission
                        userPermission.IsAssigned = permission.IsAssigned;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
