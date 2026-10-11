using System;

string passwordCorrecta = "CSharp123";
int intentos = 0;
bool accesoConcedido = false;

while (intentos < 5)
{
    Console.Write("Introduce la contraseña: ");
    string? password = Console.ReadLine();

    if (password == null)
    {
        break;
    }

    intentos++;

    if (password == passwordCorrecta)
    {
        Console.WriteLine("Acceso concedido");
        accesoConcedido = true;
        break;
    }

    Console.WriteLine("Contraseña incorrecta");
    Console.WriteLine($"Intentos restantes: {5 - intentos}");
}

if (!accesoConcedido && intentos == 5)
{
    Console.WriteLine("Cuenta bloqueada");
}
