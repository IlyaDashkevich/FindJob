namespace JobService.Test.UnitTests
{
    public class UserUnitTest
    {
        private UserController _controller;
        private Mock<IUserService> _userServiceMock;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserController(_userServiceMock.Object);
        }

        [Test]
        public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Username", "Required");

            var loginRequestDto = new LoginRequestDto
            {
                Username = "test",
                Password = "password"
            };

            // Act
            var result = await _controller.Login(loginRequestDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Login_ShouldReturnBadRequest_WhenUserNotFound()
        {
            // Arrange
            var loginRequestDto = new LoginRequestDto
            {
                Username = "test",
                Password = "wrongpassword"
            };

            _userServiceMock.Setup(x => x.Login(loginRequestDto))
                .ReturnsAsync(new LoginResponseDto { User = null });

            // Act
            var result = await _controller.Login(loginRequestDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.Value.Should().Be("Username or password is incorrect.");
        }

        [Test]
        public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var loginRequestDto = new LoginRequestDto
            {
                Username = "test",
                Password = "password"
            };

            var user = new LocalUser { UserName = "test" };
            var loginResponseDto = new LoginResponseDto { User = user, Token = "token" };

            _userServiceMock.Setup(x => x.Login(loginRequestDto))
                .ReturnsAsync(loginResponseDto);

            // Act
            var result = await _controller.Login(loginRequestDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(loginResponseDto);
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("UserName", "Required");

            var registrationRequestDto = new RegistrationRequestDto
            {
                UserName = "test",
                Password = "password"
            };

            // Act
            var result = await _controller.Register(registrationRequestDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenUsernameIsNotUnique()
        {
            // Arrange
            var registrationRequestDto = new RegistrationRequestDto
            {
                UserName = "test",
                Password = "password"
            };

            _userServiceMock.Setup(x => x.IsUserUnique(registrationRequestDto.UserName))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Register(registrationRequestDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.Value.Should().Be("Username is already taken.");
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registrationRequestDto = new RegistrationRequestDto
            {
                UserName = "test",
                Password = "password"
            };

            var user = new LocalUser { UserName = "test" };

            _userServiceMock.Setup(x => x.IsUserUnique(registrationRequestDto.UserName))
                .ReturnsAsync(true);
            _userServiceMock.Setup(x => x.Register(registrationRequestDto))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Register(registrationRequestDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(user);
        }
    }
}