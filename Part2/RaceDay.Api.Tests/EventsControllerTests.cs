using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventsControllerTests
    {
        private RaceDayDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        private EventsController GetControllerAsUser(RaceDayDbContext context, int userId, string role)
        {
            var controller = new EventsController(context);
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
        public async Task Create_AsOrganiser_ReturnsCreated()
        {
            var context = GetContext();
            context.Users.Add(new User { UserId = 1, FullName = "Org", Email = "o@x.com", PasswordHash = "h", Role = "Organiser" });
            await context.SaveChangesAsync();

            var controller = GetControllerAsUser(context, 1, "Organiser");
            var dto = new EventDto { Name = "Test Run", EventDate = DateTime.Now.AddDays(30), DistanceKm = 10 };

            var result = await controller.Create(dto);

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task Update_ByNonOwner_ReturnsForbidden()
        {
            var context = GetContext();
            context.Events.Add(new Event { EventId = 1, OrganiserId = 1, Name = "Run", EventDate = DateTime.Now, DistanceKm = 10, Status = "Upcoming" });
            await context.SaveChangesAsync();

            var controller = GetControllerAsUser(context, 2, "Organiser"); // different organiser
            var dto = new EventDto { Name = "Changed", EventDate = DateTime.Now, DistanceKm = 5 };

            var result = await controller.Update(1, dto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetById_NonExistentEvent_ReturnsNotFound()
        {
            var context = GetContext();
            var controller = new EventsController(context);

            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }


    }
}
