using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SwashbuckleODataSample.Models;
using SwashbuckleODataSample.Repositories;

namespace SwashbuckleODataSample.ApiControllers
{
    public class ProjectsController : ApiController
    {
        private readonly SwashbuckleODataContext _db = new SwashbuckleODataContext();

        [Route("Projects/v1")]
        public IQueryable<Project> GetProjects()
        {
            return _db.Projects;
        }

        [Route("Projects/v1/{id}")]
        [ResponseType(typeof (Project))]
        public async Task<IHttpActionResult> GetProject(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [Route("Projects/v1/{id}")]
        [ResponseType(typeof (void))]
        public async Task<IHttpActionResult> PutProject(int id, Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != project.ProjectId)
            {
                return BadRequest();
            }

            _db.Entry(project).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("Projects/v1")]
        [ResponseType(typeof (Project))]
        public async Task<IHttpActionResult> PostProject(Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new
            {
                id = project.ProjectId
            }, project);
        }

        [Route("Projects/v1/{id}")]
        [ResponseType(typeof (Project))]
        public async Task<IHttpActionResult> DeleteProject(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();

            return Ok(project);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProjectExists(int id)
        {
            return _db.Projects.Count(e => e.ProjectId == id) > 0;
        }
    }
}