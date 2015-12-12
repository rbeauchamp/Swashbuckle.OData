using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SwashbuckleODataSample.Repositories;

namespace SwashbuckleODataSample.ApiControllers
{
    public class ClientsController : ApiController
    {
        private readonly SwashbuckleODataContext _db = new SwashbuckleODataContext();

        /// <summary>
        /// List clients
        /// </summary>
        public IQueryable<Client> GetClients()
        {
            return _db.Clients;
        }

        /// <summary>
        /// Get the client by id
        /// </summary>
        /// <param name="id">Client id</param>
        [ResponseType(typeof (Client))]
        public async Task<IHttpActionResult> GetClient(int id)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            return Ok(client);
        }

        /// <summary>
        /// Replace all data for the client with the given id
        /// </summary>
        /// <param name="id">Client id</param>
        /// <param name="client">Client details</param>
        [ResponseType(typeof (void))]
        public async Task<IHttpActionResult> PutClient(int id, Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != client.Id)
            {
                return BadRequest();
            }

            _db.Entry(client).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="client">Client details</param>
        [ResponseType(typeof (Client))]
        public async Task<IHttpActionResult> PostClient(Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new
            {
                id = client.Id
            }, client);
        }

        /// <summary>
        /// Delete the client with the given id
        /// </summary>
        /// <param name="id">Client id</param>
        [ResponseType(typeof (Client))]
        public async Task<IHttpActionResult> DeleteClient(int id)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _db.Clients.Remove(client);
            await _db.SaveChangesAsync();

            return Ok(client);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ClientExists(int id)
        {
            return _db.Clients.Count(e => e.Id == id) > 0;
        }
    }
}