namespace JobService.Api.Controllers;
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobController : Controller
    {
        private readonly IJobService _jobService;
        private readonly ICacheService _cacheService;

        public JobController(IJobService jobService, ICacheService cacheService)
        {
            _jobService = jobService;
            _cacheService = cacheService;
        }

        [HttpGet]

        public async Task<IActionResult>GetJobs ()
        {
            var jobs = await _jobService.GetJobs();
            return Ok(jobs);
        }

        [HttpGet("employer/{employerId}")]
        public async Task<IActionResult> GetJobsByEmployerId(int employerId)
        {
            var cacheKey = $"jobs_employer_{employerId}";
            var cachedJobs = await _cacheService.GetData<List<JobDto>>(cacheKey);
            
            if (cachedJobs == null)
            {
                var jobs = await _jobService.GetJobsByEmployerId(employerId);
                await _cacheService.SetData(cacheKey, jobs, DateTime.UtcNow.AddMinutes(30));
                
                return Ok(jobs);
            }
            return Ok(cachedJobs);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var cacheKey = $"jobs_{id}";
            var cachedJob = await _cacheService.GetData<JobDto>(cacheKey);
            
            if (cachedJob == null)
            {
                var job = await _jobService.GetJob(id);
                if (job == null) return NotFound();
                
                await _cacheService.SetData(cacheKey, job, DateTime.UtcNow.AddMinutes(30));
                
                return Ok(job);
            }
            return Ok(cachedJob);  
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Create (JobDto jobDto)
        { 
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(employerId, out var employerIdInt)) 
                jobDto.EmployerId = employerIdInt; 
            else  
                throw new Exception("Unable to convert Employer ID to int."); 

            var job = await _jobService.CreateJob(jobDto);
            return Ok(job);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Update(int id, JobDto jobDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingJob = await _jobService.GetJob(id);
            if (existingJob == null) return NotFound();

            await _jobService.UpdateJob(id, jobDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult> Delete (int id)
        {
            var job = await _jobService.GetJob(id);
            if (job == null) return NotFound();
            
            await _jobService.DeleteJob(id);
            return NoContent();
        }
    }
