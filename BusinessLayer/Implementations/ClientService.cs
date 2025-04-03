using System;
using BusinessLayer.Interfaces;
using FeatureLayer.Entities;
using DataLayer.Implementations;

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
    }
}
