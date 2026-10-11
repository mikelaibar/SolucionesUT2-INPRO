using System;

string nombreJugador = "Lucía";
int partidasJugadas = 10;
int partidasGanadas = 6;
int puntosObtenidos = 1255;
int penalizaciones = 2;
bool inscripcionPagada = true;
bool cuentaActiva = true;
bool sancionado = false;

int partidasNoGanadas = partidasJugadas - partidasGanadas;
// Convertimos a double para conservar los decimales de la división.
double mediaPuntos = (double)puntosObtenidos / partidasJugadas;
bool alMenos5Victorias = partidasGanadas >= 5;
bool mediaSuficiente = mediaPuntos >= 100;
bool menosDe3Penalizaciones = penalizaciones < 3;
bool puedeParticipar = inscripcionPagada && cuentaActiva && !sancionado;
bool cumpleRequisitosDeportivos = alMenos5Victorias && mediaSuficiente && menosDe3Penalizaciones;
bool puedeClasificarse = puedeParticipar && cumpleRequisitosDeportivos;

Console.WriteLine("--- INFORME DEL JUGADOR ---");
Console.WriteLine($"Jugador: {nombreJugador}");
Console.WriteLine($"Partidas: {partidasJugadas}");
Console.WriteLine($"Victorias: {partidasGanadas}");
Console.WriteLine($"Puntos obtenidos: {puntosObtenidos}");
Console.WriteLine($"Penalizaciones: {penalizaciones}");
Console.WriteLine($"Inscripción pagada: {inscripcionPagada}");
Console.WriteLine($"Cuenta activa: {cuentaActiva}");
Console.WriteLine($"Sancionado: {sancionado}");
Console.WriteLine($"Partidas no ganadas: {partidasNoGanadas}");
Console.WriteLine($"Media de puntos: {mediaPuntos}");
Console.WriteLine($"Tiene al menos 5 victorias: {alMenos5Victorias}");
Console.WriteLine($"Tiene una media de al menos 100 puntos: {mediaSuficiente}");
Console.WriteLine($"Tiene menos de 3 penalizaciones: {menosDe3Penalizaciones}");
Console.WriteLine($"Puede participar: {puedeParticipar}");
Console.WriteLine($"Cumple requisitos deportivos: {cumpleRequisitosDeportivos}");
Console.WriteLine($"Puede clasificarse: {puedeClasificarse}");

// Con estos datos puedeClasificarse es true.
// Cambia puntosObtenidos a 900 y ejecuta de nuevo: la media será 90 y será false.
