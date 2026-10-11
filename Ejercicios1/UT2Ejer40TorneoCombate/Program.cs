using System;

int vidaJugador = 100;
int vidaEnemigo = 100;
int pociones = 3;
int turno = 1;
bool huir = false;

while (vidaJugador > 0 && vidaEnemigo > 0)
{
    Console.WriteLine($"--- TURNO {turno} ---");
    Console.WriteLine($"Tu vida: {vidaJugador}");
    Console.WriteLine($"Vida enemigo: {vidaEnemigo}");
    Console.WriteLine($"Pociones: {pociones}");
    Console.WriteLine("1. Atacar");
    Console.WriteLine("2. Ataque fuerte");
    Console.WriteLine("3. Usar poción");
    Console.WriteLine("4. Huir");
    Console.Write("Selecciona una acción: ");
    string? opcion = Console.ReadLine();

    if (opcion == null)
    {
        break;
    }

    switch (opcion)
    {
        case "1":
            vidaEnemigo -= 15;
            break;
        case "2":
            if (turno % 2 != 0)
            {
                Console.WriteLine("Ataque fuerte no disponible.");
                continue;
            }

            vidaEnemigo -= 30;
            break;
        case "3":
            if (pociones > 0)
            {
                vidaJugador += 20;
                pociones--;

                // Reto adicional: una poción no permite superar 100 de vida.
                if (vidaJugador > 100)
                {
                    vidaJugador = 100;
                }
            }
            else
            {
                Console.WriteLine("No quedan pociones.");
            }

            break;
        case "4":
            Console.WriteLine("Has abandonado el combate.");
            huir = true;
            break;
        default:
            Console.WriteLine("Acción no válida.");
            continue;
    }

    // El break del switch no sale del while: comprobamos si se ha huido.
    if (huir)
    {
        break;
    }

    if (vidaEnemigo > 0)
    {
        Console.WriteLine("El enemigo te ataca y pierdes 10 puntos de vida.");
        vidaJugador -= 10;
    }

    turno++;
}

if (vidaEnemigo <= 0)
{
    Console.WriteLine("¡Has ganado el combate!");
}
else if (vidaJugador <= 0)
{
    Console.WriteLine("Has sido derrotado.");
}

Console.WriteLine("--- FIN DEL COMBATE ---");
Console.WriteLine($"Turnos jugados: {turno - 1}");
Console.WriteLine($"Vida restante del jugador: {vidaJugador}");
Console.WriteLine($"Vida restante del enemigo: {vidaEnemigo}");
Console.WriteLine($"Pociones restantes: {pociones}");
