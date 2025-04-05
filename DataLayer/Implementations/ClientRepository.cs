using System;
using MySql.Data.MySqlClient;
using FeatureLayer.Entities;
using DataLayer.Connection;

namespace DataLayer.Implementations
{
    public class ClientRepository
    {
        private readonly DbConnection _dbConnection;

        // Constructor que inyecta la conexión a la base de datos
        public ClientRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Método para registrar un cliente usando el procedimiento almacenado
        public void RegisterClient(User user)
        {
            // Nombre del procedimiento almacenado
            string storedProcedure = "RegisterUserAndClient";

            using (var connection = _dbConnection.GetConnection())
            {
                // Comando para ejecutar el procedimiento almacenado
                MySqlCommand command = new MySqlCommand(storedProcedure, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                // Añadir los parámetros del procedimiento almacenado
                command.Parameters.AddWithValue("p_username", user.Username);
                command.Parameters.AddWithValue("p_password", user.Password); // Recuerda cifrar la contraseña antes de pasarla
                command.Parameters.AddWithValue("p_client_name", user.ClientName);
                command.Parameters.AddWithValue("p_email", user.Email);
                command.Parameters.AddWithValue("p_phone", user.Phone);

                // Abrir la conexión y ejecutar el procedimiento
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // Procesar el resultado si lo necesitas (si deseas mostrar un mensaje)
                    while (reader.Read())
                    {
                        string message = reader["Message"].ToString();
                        Console.WriteLine(message); // Mostrar el mensaje, puedes manejarlo como desees
                    }
                }
            }
        }

        // Método para realizar el login del usuario usando el procedimiento almacenado LoginUser
        public string LoginUser(string username, string password)
        {
            // Nombre del procedimiento almacenado
            string storedProcedure = "LoginUser";
            string userRole = string.Empty;

            using (var connection = _dbConnection.GetConnection())
            {
                // Comando para ejecutar el procedimiento almacenado
                MySqlCommand command = new MySqlCommand(storedProcedure, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                // Añadir los parámetros del procedimiento almacenado
                command.Parameters.AddWithValue("p_username", username);
                command.Parameters.AddWithValue("p_password", password); // Recuerda que la contraseña ya está cifrada en el procedimiento almacenado

                // Abrir la conexión y ejecutar el procedimiento
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // Procesar el resultado de la consulta
                    while (reader.Read())
                    {
                        userRole = reader["Role"].ToString(); // Obtener el rol del usuario
                    }
                }
            }

            // Devolver el resultado
            return userRole;
        }
    }
}
    

