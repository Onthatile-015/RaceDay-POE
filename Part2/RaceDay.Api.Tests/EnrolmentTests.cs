using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EnrolmentTests
    {
        private RaceDayDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        private CategoriesController GetControllerAsUser(RaceDayDbContext context, int userId, string role)
        {
            var controller = new CategoriesController(context);
            var claims = new List<System.Security.Claims.Claim>
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
                new(System.Security.Claims.ClaimTypes.Role, role)
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(identity)
                }
            };
            return controller;
        }

        [Fact]
        public async Task Enrol_ValidCategory_CreatesEnrolment()
        {
            var context = GetContext();
            context.Categories.Add(new Category { CategoryId = 1, EventId = 1, Name = "10km", MaxParticipants = 100, EntryFee = 50 });
            await context.SaveChangesAsync();

            var controller = GetControllerAsUser(context, 5, "Participant");

            var result = await controller.Enrol(1);

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, ((ObjectResult)result).StatusCode);
            Assert.Single(context.Enrolments);
        }

        [Fact]
        public async Task Enrol_AlreadyEnrolled_ReturnsConflict()
        {
            var context = GetContext();
            context.Categories.Add(new Category { CategoryId = 1, EventId = 1, Name = "10km", MaxParticipants = 100, EntryFee = 50 });
            context.Enrolments.Add(new Enrolment { ParticipantId = 5, CategoryId = 1, Status = "Active" });
            await context.SaveChangesAsync();

            var controller = GetControllerAsUser(context, 5, "Participant");

            var result = await controller.Enrol(1);

            Assert.IsType<ConflictObjectResult>(result);
        }


    }
}
