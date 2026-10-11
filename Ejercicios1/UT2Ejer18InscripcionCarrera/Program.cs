using System;

int edad = 17;
bool tieneAutorizacion = true;
bool inscripcionPagada = true;
bool documentacionEntregada = true;
double marcaAnterior = 52.30;

bool mayorDeEdad = edad >= 18;
bool puedeParticiparPorEdad = mayorDeEdad || tieneAutorizacion;
bool tramitesCompletados = inscripcionPagada && documentacionEntregada;
bool puedeParticipar = puedeParticiparPorEdad && tramitesCompletados;
bool marcaInferiorA55 = marcaAnterior < 55;
bool grupoAvanzado = puedeParticipar && marcaInferiorA55;

Console.WriteLine($"Es mayor de edad: {mayorDeEdad}");
Console.WriteLine($"Puede participar por edad: {puedeParticiparPorEdad}");
Console.WriteLine($"Tiene todos los trámites completados: {tramitesCompletados}");
Console.WriteLine($"Puede participar: {puedeParticipar}");
Console.WriteLine($"Su marca es inferior a 55 minutos: {marcaInferiorA55}");
Console.WriteLine($"Puede entrar en el grupo avanzado: {grupoAvanzado}");
