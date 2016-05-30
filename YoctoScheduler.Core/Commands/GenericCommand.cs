using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Commands
{
    public class GenericCommand : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GenericCommand));

        public int ServerID { get; set; }
        protected Command Command { get; set; }
        protected string Payload { get; set; }

        public GenericCommand(int ServerID, Command Command, string Payload)
        {
            this.ServerID = ServerID;
            this.Command = Command;
            this.Payload = Payload;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, ServerID={2:N0}, Command={3:S}, Payload=\"{4:S}\"]",
                this.GetType().FullName,
                base.ToString(), ServerID, Command.ToString(), Payload);
        }

        public static GenericCommand New(SqlConnection conn, SqlTransaction trans, int ServerID, Command Command, string Payload)
        {
            GenericCommand gsc = null;
            switch (Command)
            {
                case Command.RestartServer:
                    gsc = new RestartServer(ServerID, Payload);
                    break;
                case Command.KillTask:
                    gsc = new KillExecutionTask(ServerID, Payload);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Command {0:S} is not supported at this time", Command.ToString()));
            }

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Commands.New"), conn, trans))
            {
                gsc.PopolateParameters(cmd);
                gsc.ID = (int)cmd.ExecuteScalar();
            }
            #endregion
            log.DebugFormat("Created SGenericCommandecret {0:S}", gsc.ToString());

            return gsc;
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }

        protected internal virtual void PopolateParameters(SqlCommand cmd)
        {
            SqlParameter param = new SqlParameter("@ServerID", System.Data.SqlDbType.Int);
            param.Value = ServerID;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@Command", System.Data.SqlDbType.Int);
            param.Value = Command;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@Payload", System.Data.SqlDbType.NVarChar, -1);
            param.Value = Payload;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@ID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        public static List<GenericCommand> DequeueByServerID(SqlConnection conn, SqlTransaction trans, int ServerID)
        {
            List<GenericCommand> lCommands = new List<GenericCommand>();

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Commands.DequeueByServerID"), conn, trans))
            {
                SqlParameter param = new SqlParameter("@ServerID", System.Data.SqlDbType.Int);
                param.Value = ServerID;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lCommands.Add(ParseFromDataReader(reader));
                    }
                }
            }

            return lCommands;
        }

        protected static GenericCommand ParseFromDataReader(SqlDataReader r)
        {
            int id = r.GetInt32(0);
            int serverID = r.GetInt32(1);
            Command command = (Command)r.GetInt32(2);
            string payload = null;
            if (!r.IsDBNull(3))
                payload = r.GetString(3);

            switch (command)
            {
                case Command.RestartServer:
                    return new RestartServer(serverID, payload) { ID = id };
                case Command.KillTask:
                    return new KillExecutionTask(serverID, payload) { ID = id };
            }

            throw new NotSupportedException(string.Format("Command {0:S} is not supported at this time", command));
        }
    }
}
