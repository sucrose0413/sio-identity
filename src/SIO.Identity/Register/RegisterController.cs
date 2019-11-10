﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenEventSourcing.Events;
using SIO.Domain.User.Events;
using SIO.Identity.Register.Requests;
using SIO.Migrations;

namespace SIO.Identity.Register
{
    public class RegisterController: Controller
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventBusPublisher _eventBusPublisher;

        public RegisterController(IConfiguration configuration, UserManager<SIOUser> userManager, IEventBusPublisher eventBusPublisher)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            _configuration = configuration;
            _userManager = userManager;
            _eventBusPublisher = eventBusPublisher;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            return View();
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return View(request);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email is already in use");
                }
                else
                {
                    ModelState.AddModelError("", "Your account is not verified, but a new activation link has been sent to your email.");
                    await _userManager.GenerateEmailConfirmationTokenAsync(user);
                }

                return View(request);
            }

            user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
            };

            var userResult = await _userManager.CreateAsync(user);

            if (!userResult.Succeeded)
            {
                ModelState.AddModelError("", "There was an error completing registration,  please try again");

                return View(request);
            }               

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _eventBusPublisher.PublishAsync(new UserRegistered(new Guid(user.Id), Guid.NewGuid(), user.Version++, user.Id, user.Email, user.FirstName, user.LastName, token));
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Registered));
        }

        [HttpGet("registered")]
        public IActionResult Registered()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            return View();
        }
    }
}