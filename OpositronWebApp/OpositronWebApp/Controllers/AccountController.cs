using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    //Registro -- SignUp
    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SignUp(SignUpViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Guardar usuario en base de datos (lógica aquí)

            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    //Inicio de sesión -- LogIn
    [HttpGet]
    public IActionResult LogIn()
    {
        return View();
    }

    [HttpPost]
    public IActionResult LogIn(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Validar usuario (lógica aquí)
            // Si es válido, redirigir o establecer sesión
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }
}