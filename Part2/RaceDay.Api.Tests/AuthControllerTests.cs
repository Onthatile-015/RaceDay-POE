using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            var context = GetContext();
            var controller = new AuthController(context);

            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "Password123",
                Role = "Participant"
            };

            var result = await controller.Register(dto);

            Assert.IsType<ObjectResult>(result);
            var objResult = (ObjectResult)result;
            Assert.Equal(201, objResult.StatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var context = GetContext();
            context.Users.Add(new Models.User
            {
                FullName = "Existing",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "Participant"
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context);
            var dto = new RegisterDto
            {
                FullName = "New User",
                Email = "test@example.com",
                Password = "Password123",
                Role = "Participant"
            };

            var result = await controller.Register(dto);

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            var context = GetContext();
            var controller = new AuthController(context);
            var dto = new RegisterDto
            {
                FullName = "Test",
                Email = "test2@example.com",
                Password = "Password123",
                Role = "Admin" // invalid
            };

            var result = await controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var context = GetContext();
            context.Users.Add(new Models.User
            {
                FullName = "Test",
                Email = "test3@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                Role = "Participant"
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context);
            var dto = new LoginDto { Email = "test3@example.com", Password = "WrongPassword" };

            var result = await controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);

        }


        }
}
