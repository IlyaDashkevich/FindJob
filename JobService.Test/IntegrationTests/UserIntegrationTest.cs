using JobService.Core.Enums;

namespace JobService.Test.IntegrationTests
{
    public class UserControllerTests
    {
        private UserController _controller;
        private DataContext _context;
        private IUserService _userService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite("DataSource=jobservice.db") 
                .Options;

            _context = new DataContext(options);
            _context.Database.EnsureCreated(); 

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Jwt:Key", "ThisIsAVeRySecretKeyThatNobodyCantUseExceptMe987654321" },
                    { "Jwt:Issuer", "JobService" },
                    { "Jwt:Audience", "YourAudience" }
                }!)
                .Build();
            
            _userService = new UserService(_context, configuration);
            _controller = new UserController(_userService);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private string GenerateJwtToken(LocalUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_here"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "your_issuer",
                audience: "your_audience",
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var user = new LocalUser
            {
                Name = "Test User", 
                Contacts = "testuser@example.com", 
                UserName = "testuser",
                Password = "password", 
                Role = RoleEnum.Applicant 
            };
            
            await _context.LocalUsers.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginRequestDto = new LoginRequestDto
            {
                Username = "testuser",
                Password = "password"
            };

            // Act
            var result = await _controller.Login(loginRequestDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().NotBeNull();
        }

        [Test]
        public async Task Register_UniqueUsername_ReturnsOkResult()
        {
            // Arrange
            var registrationRequestDto = new RegistrationRequestDto
            {
                UserName = "newuser",
                Password = "password",
                Name = "New User",
                Contacts = "newuser@example.com",
                Role = RoleEnum.Applicant
            };

            // Act
            var result = await _controller.Register(registrationRequestDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().NotBeNull();
        }

        [Test]
        public async Task Register_DuplicateUsername_ReturnsBadRequest()
        {
            // Arrange
            var registrationRequestDto = new RegistrationRequestDto
            {
                UserName = "duplicateuser",
                Password = "password",
                Name = "Duplicate User",
                Contacts = "duplicateuser@example.com",
                Role = RoleEnum.Employer
            };

            await _controller.Register(registrationRequestDto); 

            // Act
            var result = await _controller.Register(registrationRequestDto); 

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Username is already taken.");
        }
    }
}