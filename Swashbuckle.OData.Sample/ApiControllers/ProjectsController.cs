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
        private readonly SwashbuckleODataContext db = new SwashbuckleODataContext();

        [Route("Projects/v1")]
        public IQueryable<Project> GetProjects()
        {
            return db.Projects;
        }

        [Route("Projects/v1/{id}")]
        [ResponseType(typeof (Project))]
        public async Task<IHttpActionResult> GetProject(int id)
        {
            var project = await db.Projects.FindAsync(id);
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

            db.Entry(project).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
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

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new
            {
                id = project.ProjectId
            }, project);
        }

        [Route("Projects/v1/{id}")]
        [ResponseType(typeof (Project))]
        public async Task<IHttpActionResult> DeleteProject(int id)
        {
            var project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            return Ok(project);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProjectExists(int id)
        {
            return db.Projects.Count(e => e.ProjectId == id) > 0;
        }
    }
}