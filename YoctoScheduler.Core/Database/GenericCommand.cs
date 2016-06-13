using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    [DatabaseKey(DatabaseName = "ID", Size = 4)]
    public class GenericCommand : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GenericCommand));

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 4)]
        public int ServerID { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 4)]
        public ServerCommand Command { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = -1)]
        public string Payload { get; set; }

        public GenericCommand() { }

        public GenericCommand(int ServerID, ServerCommand Command, string Payload)
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

        public static List<GenericCommand> DequeueByServerID(SqlConnection conn, SqlTransaction trans, int ServerID)
        {
            List<GenericCommand> lCommands = new List<GenericCommand>();

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("GenericCommand.DequeueByServerID"), conn, trans))
            {
                SqlParameter param = new SqlParameter("@ServerID", System.Data.SqlDbType.Int);
                param.Value = ServerID;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GenericCommand gc = new GenericCommand();
                        gc.ParseFromDataReader(reader);
                        lCommands.Add(gc);
                    }
                }
            }

            return lCommands;
        }

        public override void ParseFromDataReader(SqlDataReader r)
        {
            ID = r.GetInt32(0);
            ServerID = r.GetInt32(1);
            Command = (ServerCommand)r.GetInt32(2);
            Payload = null;
            if (!r.IsDBNull(3))
                Payload = r.GetString(3);

            //switch (command)
            //{
            //    case ServerCommand.RestartServer:
            //        return new Commands.RestartServer(serverID, payload) { ID = id };
            //    case ServerCommand.KillTask:
            //        return new Commands.KillExecutionTask(serverID, payload) { ID = id };
            //}

            //throw new NotSupportedException(string.Format("Command {0:S} is not supported at this time", command));
        }
    }
}
