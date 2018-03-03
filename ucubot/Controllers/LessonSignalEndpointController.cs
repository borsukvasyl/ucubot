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
            string connectionString = _configuration.GetConnectionString("BotDatabase");
            
            DataTable dataTable = new DataTable();
            
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                MySqlCommand command = new MySqlCommand("SELECT * FROM lesson_signal", connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                connection.Open();
                adapter.Fill(dataTable);
                adapter.Dispose();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }
            connection.Close();

            List<LessonSignalDto> lessonSignals = new List<LessonSignalDto>();
            
            foreach (DataRow row in dataTable.Rows)
            {
                lessonSignals.Add(new LessonSignalDto
                {
                    Id = (int) row["id"],
                    Timestamp = (DateTime) row["timestamp"],
                    Type = (LessonSignalType) row["signal_type"],
                    UserId = (string) row["user_id"]
                });
            }
            
            return lessonSignals;
        }
        
        [HttpGet("{id}")]
        public LessonSignalDto ShowSignal(long id)
        {
            string connectionString = _configuration.GetConnectionString("BotDatabase");
            
            DataTable dataTable = new DataTable();

            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                MySqlCommand command = new MySqlCommand("SELECT * FROM lesson_signal WHERE id=@id", connection);
                command.Parameters.AddWithValue("@id", id);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                connection.Open();
                adapter.Fill(dataTable);
                adapter.Dispose();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }
            connection.Close();

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
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var userId = message.user_id;
            var signalType = message.text.ConvertSlackMessageToSignalType();
            Console.Out.WriteLine(userId + ", " + signalType);

            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("INSERT INTO lesson_signal (user_id, signal_type) " +
                                                        "VALUES (@userId, @signalType);", connection);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@signalType", signalType);

                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }

            return Accepted();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSignal(long id)
        {
            MySqlConnection connection = new MySqlConnection(_configuration.GetConnectionString("BotDatabase"));
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("DELETE FROM lesson_signal WHERE ID = @id;", connection);
                command.Parameters.AddWithValue("@id", id);

                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }

            return Accepted();
        }
    }
}
