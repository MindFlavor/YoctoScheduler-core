using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using YoctoScheduler.Core.Database;
using YoctoScheduler.Logging.Extensions;

namespace YoctoScheduler.WebAPI
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public abstract class ControllerBase<T, K> : System.Web.Http.ApiController
        where T : DatabaseItem<K>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ControllerBase<T, K>));

        public virtual IHttpActionResult Get()
        {
            log.TraceFormat("Get::{0:S} requested", typeof(T).Name);

            if (Attribute.GetCustomAttributes(this.GetType(), typeof(Attributes.GetAllSupportedAttribute), false) == null)
                return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented));

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var r = DatabaseItem<K>.GetAll<T>(conn, trans);
                        trans.Commit();
                        log.TraceFormat("{0:S} Get returning {1:N0} items.", typeof(T).FullName, r.Count);
                        return Ok(r);
                    }
                }
            }
            catch (Core.Exceptions.TSQLNotFoundException)
            {
                return BadRequest(string.Format("Get not supported by {0:S}", typeof(T).FullName));
            }
        }

        public virtual IHttpActionResult Get(K id)
        {
            log.TraceFormat("Get::{0:S}({1:S}) requested", typeof(T).Name, id.ToString());

            if (Attribute.GetCustomAttributes(this.GetType(), typeof(Attributes.GetByIDSupportedAttribute), false) == null)
                return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented));

            try
            {
                T t = null;
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        t = DatabaseItem<K>.GetByID<T>(conn, trans, id);
                        trans.Commit();
                    }
                }

                if (t != null)
                    return Ok(t);
                else
                    return NotFound();

            }
            catch (Core.Exceptions.TSQLNotFoundException)
            {
                return BadRequest(string.Format("GetByID not supported by {0:S}", typeof(T).FullName));
            }
        }

        public virtual IHttpActionResult Put(T value)
        {
            if (Attribute.GetCustomAttributes(this.GetType(), typeof(Attributes.PutSupportedAttribute), false) == null)
                return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented));

            return _PostInternal(value);
        }

        public virtual IHttpActionResult Post(T value)
        {
            if (Attribute.GetCustomAttributes(this.GetType(), typeof(Attributes.PostSupportedAttribute), false) == null)
                return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented));

            return _PostInternal(value);
        }

        public virtual IHttpActionResult Delete(K id)
        {
            if (Attribute.GetCustomAttributes(this.GetType(), typeof(Attributes.DeleteSupportedAttribute), false) == null)
                return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented));

            if (id == null)
                return BadRequest();

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var t = DatabaseItem<K>.GetByID<T>(conn, trans, id);
                        if (t == default(DatabaseItem<K>))
                            return NotFound();

                        DatabaseItem<K>.Delete<T>(conn, trans, t);
                        trans.Commit();
                        return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NoContent));
                    }
                }
            }
            catch (Core.Exceptions.TSQLNotFoundException)
            {
                return BadRequest(string.Format("DELETE not supported by {0:S}", typeof(T).FullName));
            }
            catch (Core.Exceptions.TaskNotFoundException tfe)
            {
                log.ErrorFormat("TaskNotFoundException processing {0:S} DELETE: {1:S}", typeof(T).Name, tfe.ToString());
                return BadRequest();
            }
            catch (System.Exception exce)
            {
                log.ErrorFormat("Unhandled exception processing {0:S} DELETE: {1:S}", typeof(T).Name, exce.ToString());
                return InternalServerError(exce);
            }
        }

        #region Internal methods
        // TODO: Support object update
        protected virtual IHttpActionResult _PostInternal(T value)
        {

            if (value == null)
                return BadRequest();

            log.VerboseFormat("_PostInternal::{0:S}({1:S}) requested", typeof(T).Name, value.ToString());

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        DatabaseItem<K> item = default(DatabaseItem<K>);

                        if (value.HasValidID())
                        {
                            item = DatabaseItem<K>.GetByID<T>(conn, trans, value.ID);
                        }

                        if (item == null)
                        {
                            #region New item
                            var ret = value.Clone<T>(conn, trans);
                            ret.Validate();
                            trans.Commit();
                            // TODO : return a valid URI
                            return Created("", ret);
                            #endregion
                        }
                        else
                        {
                            #region Update
                            value.ID = item.ID;
                            DatabaseItem<K>.Update<T>(conn, trans, value);
                            trans.Commit();

                            return Ok(value);
                            #endregion
                        }
                    }
                }
            }
            catch (Core.Exceptions.TaskNotFoundException tfe)
            {
                log.ErrorFormat("TaskNotFoundException processing {0:S} POST: {1:S}", typeof(T).Name, tfe.ToString());
                return BadRequest();
            }
            catch (System.Exception exce)
            {
                log.ErrorFormat("Unhandled exception processing {0:S} POST: {1:S}", typeof(T).Name, exce.ToString());

                return InternalServerError(exce);
            }
        }
        #endregion


        public virtual IHttpActionResult GetSecondary<SECONDARY>(SECONDARY id, int size)
        {
            log.TraceFormat("GetSecondary<{0:S}::{1:S}({2:S}) requested", typeof(T).Name, typeof(SECONDARY).Name, id.ToString());

            try
            {
                T t = default(T);
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        t = DatabaseItem<K>.GetBySecondary<T, SECONDARY>(conn, trans, id, size);
                        trans.Commit();
                    }
                }

                if (t != null)
                    return Ok(t);
                else
                    return NotFound();
            }
            catch (Core.Exceptions.TSQLNotFoundException ex)
            {
                return BadRequest(string.Format("Get secondary not supported by {0:S} ({1:S})", this.GetType().FullName, ex.ToString()));
            }
        }
    }
}