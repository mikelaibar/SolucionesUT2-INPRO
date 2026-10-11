using System;

bool autenticado = true;
string rol = "editor";
string accion = "editar";
bool propietario = false;
bool permitida = false;

if (autenticado)
{
    switch (rol)
    {
        case "admin":
            permitida = true;
            break;
        case "editor":
            if (accion == "leer" || accion == "editar")
            {
                permitida = true;
            }
            else if (accion == "eliminar" && propietario)
            {
                permitida = true;
            }
            break;
        case "usuario":
            if (accion == "leer")
            {
                permitida = true;
            }
            else if (accion == "editar" && propietario)
            {
                permitida = true;
            }
            break;
        default:
            permitida = false;
            break;
    }
}

if (permitida)
{
    Console.WriteLine("Acción permitida");
}
else
{
    Console.WriteLine("Acción denegada");
}
