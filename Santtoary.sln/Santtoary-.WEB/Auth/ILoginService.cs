namespace Santtoary_.WEB.Auth 
{ 

    // Interfaz para el servicio de autenticación, que define los métodos para iniciar y cerrar sesión.
    public interface ILoginService
    {
        Task LoginAsync(string token);
        Task LogoutAsync();
    }
}