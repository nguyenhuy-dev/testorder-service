using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestOrderService.API.Controllers;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException; // Ensure this points to your custom Exception

namespace TestOrderService.API.Test.Controllers
{
    [TestFixture]
    public class BaseApiControllerTest
    {

        [SetUp]
        public void Setup()
        {
            _controller = new TestStubController();
        }
        private TestStubController _controller;

        // -------------------------------------------------------
        // 1. DEFINE A STUB CONTROLLER
        // We need this because BaseApiController is abstract, 
        // and we need to expose the 'protected' members to test them.
        // -------------------------------------------------------
        protected class TestStubController : BaseApiController
        {
            public Guid PublicUserId
            {
                get => CurrentUserId;
            }
            public string PublicUserName
            {
                get => CurrentUserName;
            }
            public string PublicUserRole
            {
                get => CurrentUserRole;
            }
        }

        // -------------------------------------------------------
        // 2. TESTS FOR CurrentUserId (The complex logic)
        // -------------------------------------------------------

        [Test]
        public void CurrentUserId_ShouldPrioritizeHeader_WhenHeaderAndClaimBothExist()
        {
            // Arrange
            var headerId = Guid.NewGuid();
            var claimId = Guid.NewGuid();

            // Setup Context with BOTH Header and Claim
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, claimId.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            httpContext.Request.Headers["X-User-Id"] = headerId.ToString();

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = _controller.PublicUserId;

            // Assert
            Assert.That(result, Is.EqualTo(headerId), "Should return Header ID even if Claim exists");
        }

        [Test]
        public void CurrentUserId_ShouldUseClaim_WhenHeaderIsMissing()
        {
            // Arrange
            var claimId = Guid.NewGuid();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, claimId.ToString()) };
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

            // Ensure Header is empty
            httpContext.Request.Headers.Clear();

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = _controller.PublicUserId;

            // Assert
            Assert.That(result, Is.EqualTo(claimId), "Should fallback to Claim when Header is missing");
        }

        [Test]
        public void CurrentUserId_ShouldThrow_WhenHeaderExistsButIsInvalid()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-User-Id"] = "im-not-a-guid"; // INVALID GUID

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act & Assert
            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
            {
                _ = _controller.PublicUserId;
            });

            Assert.That(ex!.Message, Does.Contain("missing or invalid"));
        }

        [Test]
        public void CurrentUserId_ShouldThrow_WhenHeaderMissingAndClaimIsInvalid()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "im-not-a-guid") }; // INVALID GUID
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                _ = _controller.PublicUserId;
            });
        }

        [Test]
        public void CurrentUserId_ShouldThrow_WhenBothHeaderAndClaimAreMissing()
        {
            // Arrange
            // Empty context
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                _ = _controller.PublicUserId;
            });
        }

        // -------------------------------------------------------
        // 3. TESTS FOR CurrentUserName (Null Coalescing Logic)
        // -------------------------------------------------------

        [Test]
        public void CurrentUserName_ShouldReturnName_WhenClaimExists()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "Dr. Strange") };
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = _controller.PublicUserName;

            // Assert
            Assert.That(result, Is.EqualTo("Dr. Strange"));
        }

        [Test]
        public void CurrentUserName_ShouldReturnUnknown_WhenClaimIsMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext(); // No claims
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = _controller.PublicUserName;

            // Assert
            Assert.That(result, Is.EqualTo("Unknown"));
        }

        // -------------------------------------------------------
        // 4. TESTS FOR CurrentUserRole (Null Coalescing Logic)
        // -------------------------------------------------------

        [Test]
        public void CurrentUserRole_ShouldReturnRole_WhenClaimExists()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Admin") };
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = _controller.PublicUserRole;

            // Assert
            Assert.That(result, Is.EqualTo("Admin"));
        }

        [Test]
        public void CurrentUserRole_ShouldReturnEmptyString_WhenClaimIsMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext(); // No claims
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = _controller.PublicUserRole;

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}
