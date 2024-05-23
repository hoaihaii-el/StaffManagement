﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StaffManagmentNET.Models;
using StaffManagmentNET.Repositories;
using StaffManagmentNET.Responses;
using StaffManagmentNET.StaticServices;
using StaffManagmentNET.ViewModels;
using System.Text.RegularExpressions;

namespace StaffManagmentNET.Services
{
    public class AccountService : IAccountRepo
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWTManager _JWTManager;
        private readonly DataContext _context;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            JWTManager JWTManager, DataContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _JWTManager = JWTManager;
        }

        public async Task<Staff> Register(RegisterVM register)
        {
            var user = new AppUser
            {
                Id = await AutoID(),
                Email = register.PersonalEmail,
                UserName = Guid.NewGuid().ToString(),
                PhoneNumber = register.Phone,
            };

            if (!await _roleManager.RoleExistsAsync(AppRole.Admin))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(AppRole.Admin)
                );
            }

            if (!await _roleManager.RoleExistsAsync(AppRole.Staff))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(AppRole.Staff)
                );
            }

            if (!await _roleManager.RoleExistsAsync(AppRole.CEO))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(AppRole.CEO)
                );
            }

            if (!await _roleManager.RoleExistsAsync(AppRole.Accountant))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(AppRole.Accountant)
                );
            }

            if (!await _roleManager.RoleExistsAsync(AppRole.HRManager))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(AppRole.HRManager)
                );
            }

            if (!await _roleManager.RoleExistsAsync(AppRole.HRStaff))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(AppRole.HRStaff)
                );
            }

            if (!await _roleManager.RoleExistsAsync(AppRole.DivisionManager))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(AppRole.DivisionManager)
                );
            }

            var staff = new Staff
            {
                StaffID = user.Id,
                StaffName = register.FullName,
                Title = register.Title,
                Level = register.Level,
                Phone = register.Phone,
                Male = register.Male,
                Address = register.Address,
                DateBirth = register.DateBirth,
                PersonalEmail = register.PersonalEmail,
                ManagerID = register.ManagerID,
                DivisionID = register.DivisionID,
                AvatarURL = "https://static.vecteezy.com/system/resources/previews/009/292/244/original/default-avatar-icon-of-social-media-user-vector.jpg"
            };

            staff.Division = await _context.Divisions.FindAsync(staff.DivisionID);

            _context.Staffs.Add(staff);

            await _userManager.CreateAsync(user, register.Password);

            foreach (var role in register.Roles.Split("_", StringSplitOptions.TrimEntries))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            await _context.SaveChangesAsync();

            return staff;
        }

        public async Task<SignInResponse> SignIn(SigninVM vm)
        {
            var user = await _userManager.FindByIdAsync(vm.UserID);

            if (user == null)
            {
                throw new ArgumentNullException("User not found!");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, vm.Password);

            if (!passwordValid)
            {
                throw new KeyNotFoundException("Incorrect password!");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            return new SignInResponse
            {
                AccessToken = _JWTManager.GenerateToken(user.Id, (List<string>)userRoles),
                Roles = (List<string>)userRoles,
                Staff = await _context.Staffs.FindAsync(user.Id),
            };
        }

        public async Task ChangePassword(string id, string oldPw, string newPw)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var pwHasher = new PasswordHasher<AppUser>();
            if (pwHasher.VerifyHashedPassword(user, user.PasswordHash!, oldPw) == PasswordVerificationResult.Failed)
            {
                throw new Exception("Password verified failed");
            }

            user.PasswordHash = pwHasher.HashPassword(user, newPw);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Changed password failed!");
            }
        }

        private async Task<string> AutoID()
        {
            var ID = DateTime.Now.Year.ToString().Substring(2, 2) + "001";

            var maxID = await _context.Staffs
                .OrderByDescending(s => s.StaffID)
                .Select(v => v.StaffID)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(maxID))
            {
                return ID;
            }

            var numeric = Regex.Match(maxID, @"\d+").Value;

            if (string.IsNullOrEmpty(numeric))
            {
                return ID;
            }

            ID = DateTime.Now.Year.ToString().Substring(2, 2);

            numeric = (int.Parse(numeric) + 1).ToString();

            while (ID.Length + numeric.Length < 6)
            {
                ID += '0';
            }

            return ID + numeric;
        }
    }
}
