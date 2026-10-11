using System;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

Console.Write("¿Cuántos jugadores participan? ");
int jugadores = int.Parse(Console.ReadLine() ?? "0");
Console.Write("¿Cuántas partidas juega cada jugador? ");
int partidas = int.Parse(Console.ReadLine() ?? "0");
if (jugadores <= 0 || partidas <= 0)
{
    Console.WriteLine("Error: debe haber al menos un jugador y una partida.");
    return;
}

string mejorJugador = "";
int mejorPuntuacion = -1;

for (int jugador = 1; jugador <= jugadores; jugador++)
{
    Console.WriteLine($"Jugador {jugador}");
    Console.Write("Nombre: ");
    string nombre = Console.ReadLine() ?? "";
    int total = 0;
    int partidasCon100 = 0;

    for (int partida = 1; partida <= partidas; partida++)
    {
        int puntos;
        while (true)
        {
            Console.Write($"Partida {partida} - Puntos: ");
            string? entrada = Console.ReadLine();
            if (entrada == null)
                return;
            if (int.TryParse(entrada, out puntos) && puntos >= 0)
                break;
            Console.WriteLine("Error: los puntos deben ser un número entero no negativo.");
        }

        total += puntos;
        if (puntos >= 100)
            partidasCon100++;
    }

    Console.WriteLine($"--- RESULTADO DE {nombre.ToUpperInvariant()} ---");
    Console.WriteLine($"Puntos totales: {total}");
    Console.WriteLine($"Media: {(double)total / partidas:F2}");
    Console.WriteLine($"Partidas con 100 puntos o más: {partidasCon100}");

    // En caso de empate se conserva el primer jugador con esa puntuación.
    if (total > mejorPuntuacion)
    {
        mejorPuntuacion = total;
        mejorJugador = nombre;
    }
}

Console.WriteLine("--- CAMPEONATO FINALIZADO ---");
Console.WriteLine($"Mejor jugador: {mejorJugador}");
Console.WriteLine($"Puntuación total: {mejorPuntuacion}");
