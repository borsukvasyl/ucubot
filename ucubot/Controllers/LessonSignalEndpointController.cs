using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ucubot.Model;

namespace ucubot.Controllers
{
    [Route("api/[controller]")]
    public class LessonSignalEndpointController : Controller
    {
        private readonly IConfiguration _configuration;

        public LessonSignalEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<LessonSignalDto> ShowSignals()
        {
            string query = "SELECT * FROM lesson_signal";
            string connectionString = _configuration.GetConnectionString("BotDatabase");
            
            MySqlConnection connection = new MySqlConnection(connectionString);        
            MySqlCommand cmd = new MySqlCommand(query, connection);
            
            DataTable dataTable = new DataTable();
            
            connection.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(dataTable);
            connection.Close();
            adapter.Dispose();
            
            IEnumerable<LessonSignalDto> lessonSignals = new List<LessonSignalDto>();
            
            foreach (DataRow row in dataTable.Rows)
            {
                lessonSignals.Append(
                    new LessonSignalDto 
                    {
                        Id = (int) row["id"],
                        Timestamp = (DateTime) row["timestamp"],
                        Type = (LessonSignalType)Convert.ToInt32(row["signal_type"]),
                        UserId = (string) row["user_id"]
                    }
                );
            }
            
            return lessonSignals;
        }
        
        [HttpGet("{id}")]
        public LessonSignalDto ShowSignal(long id)
        {
            string query = String.Format("SELECT * FROM lesson_signal WHERE id={0}", id);
            string connectionString = _configuration.GetConnectionString("BotDatabase");
            
            MySqlConnection connection = new MySqlConnection(connectionString);        
            MySqlCommand cmd = new MySqlCommand(query, connection);
            
            DataTable dataTable = new DataTable();
            
            connection.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(dataTable);
            connection.Close();
            adapter.Dispose();

            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = dataTable.Rows[0];
            return new LessonSignalDto
            {
                Id = (int) row["id"],
                Timestamp = (DateTime) row["timestamp"],
                Type = (LessonSignalType) Convert.ToInt32(row["signal_type"]),
                UserId = (string) row["user_id"]
            };
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateSignal(SlackMessage message)
        {
            var userId = message.user_id;
            var signalType = message.text.ConvertSlackMessageToSignalType();

            MySqlConnection connection = new MySqlConnection(_configuration.GetConnectionString("BotDatabase"));

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                String.Format("INSERT INTO lesson_signal (user_id, signal_type) VALUES ({0}, {2});",
                    userId, signalType);
            await command.ExecuteNonQueryAsync();
            
            return Accepted();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSignal(long id)
        {
            MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("BotDatabase"));
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText =
                "DELETE FROM lesson_signal WHERE ID = @id;";
            command.Parameters.Add(new MySqlParameter("id", id));
            await command.ExecuteNonQueryAsync();
            
            return Accepted();
        }
    }
}
