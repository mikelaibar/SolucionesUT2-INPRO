using System;

int edad = 14;
double altura = 1.55;
bool tieneEntrada = true;
bool vaAcompanado = false;

bool edadSuficiente = edad >= 12;
bool alturaSuficiente = altura >= 1.50;
bool cumpleEdadYAltura = edadSuficiente && alturaSuficiente;
bool cumpleTodosLosRequisitos = tieneEntrada && cumpleEdadYAltura;
bool puedeAcceder = cumpleTodosLosRequisitos || vaAcompanado;

Console.WriteLine($"Tiene 12 años o más: {edadSuficiente}");
Console.WriteLine($"Mide al menos 1,50 metros: {alturaSuficiente}");
Console.WriteLine($"Cumple edad y altura: {cumpleEdadYAltura}");
Console.WriteLine($"Tiene entrada y cumple edad y altura: {cumpleTodosLosRequisitos}");
Console.WriteLine($"Cumple todos los requisitos o va acompañado: {puedeAcceder}");
