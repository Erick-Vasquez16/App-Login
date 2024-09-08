
using Microsoft.AspNetCore.Mvc;
using Login.Data;
using Login.Models;
using Microsoft.EntityFrameworkCore;
using Login.ViewModel;
//para guardar la configuracion del usuario
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace Login.Controllers
{
    public class AccesoController : Controller
    {
        //Injeccion de dependencias
        private readonly AppDBContext _appDbContext;
        public AccesoController(AppDBContext appDBContext)
        {
            _appDbContext = appDBContext;
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registro(UsuarioVM modelo)
        {
            //para ver si las contraseñas son iguales 
            if (modelo.Password != modelo.ConfirmarPassword) {
                ViewData["Mensaje "] = "Las contraseñas no coinciden";
            }

            //agregar un nuevo usuario
            Usuario usuario = new Usuario() {
                NombreCompleto = modelo.NombreCompleto,
                Correo = modelo.Correo,
                Password = modelo.Password,
            };

            await _appDbContext.Usuarios.AddAsync(usuario);
            await _appDbContext.SaveChangesAsync();

            if (usuario.IdUsuario != 0) return RedirectToAction("Login", "Acceso");
            ViewData["Mensaje "] = "No se pudo crear el usuario";

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            //si el usuario esta activo en la sesion
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo) {

            Usuario? usuario_encontrado = await _appDbContext.Usuarios.Where(u =>
                                                                             u.Correo == modelo.Correo &&
                                                                             u.Password == modelo.Password
                                                                             ).FirstOrDefaultAsync();

            if (usuario_encontrado == null)
            {
                ViewData["Mensaje "] = "Usuario no encontrado";
                return View();

            }

            //guardar la configuracion del ususario
            List<Claim> claims = new List<Claim>() {
                new Claim(ClaimTypes.Name,usuario_encontrado.NombreCompleto)
            };

            //refresca la sesion automaticamente
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            { 
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );


            return RedirectToAction("Index", "Home");
        }

    }
}
