using System;
using BusinessLayer.Interfaces;
using FeatureLayer.Entities;
using DataLayer.Implementations;
using FeatureLayer;
using System.Collections.Generic;

namespace BusinessLayer.Implementations
{
    public class ClientService
    {
        private readonly ClientRepository _clientRepository;

        // Constructor que inyecta el repositorio de la capa de datos
        public ClientService(ClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        // Método para registrar un cliente
        public void RegisterClient(User user)
        {
            // Realizar validaciones de negocio antes de registrar (ejemplo de validaciones simples)
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) ||
                string.IsNullOrEmpty(user.ClientName) || string.IsNullOrEmpty(user.Email) ||
                string.IsNullOrEmpty(user.Phone))
            {
                throw new ArgumentException("Todos los campos son obligatorios.");
            }

            // Llamar al método del repositorio de la capa de datos para registrar el cliente
            _clientRepository.RegisterClient(user);
        }

        // Método para hacer login de un usuario
        public string LoginUser(string username, string password)
        {
            // Validaciones de negocio antes de intentar el login
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("El nombre de usuario y la contraseña son obligatorios.");
            }

            // Llamar al método del repositorio de la capa de datos para realizar el login
            string userRole = _clientRepository.LoginUser(username, password);

            // Validar el resultado del login
            if (userRole == "Invalid credentials")
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }
            else if (userRole == "User is inactive")
            {
                throw new UnauthorizedAccessException("El usuario está inactivo.");
            }

            // Devolver el rol del usuario si el login fue exitoso
            return userRole;
        }

        // Método para buscar un cliente por ID
        public Client SearchClientByID(int clientID)
        {
            // Validación de negocio para asegurarse de que el ID es positivo
            if (clientID <= 0)
            {
                throw new ArgumentException("El ID del cliente debe ser un valor positivo.");
            }

            // Llamar al repositorio para buscar el cliente por ID
            Client client = _clientRepository.SearchClientByID(clientID);

            // Si no se encuentra el cliente, lanzar una excepción
            if (client == null)
            {
                throw new KeyNotFoundException($"No se encontró un cliente con ID {clientID}.");
            }

            // Devolver el cliente encontrado
            return client;
        }
    }
}
