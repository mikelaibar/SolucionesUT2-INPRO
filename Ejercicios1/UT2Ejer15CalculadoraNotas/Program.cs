using System;

double examen1 = 6.5;
double examen2 = 8.0;
double examen3 = 5.5;

// Primero sumamos las tres notas y después dividimos entre tres.
double notaMedia = (examen1 + examen2 + examen3) / 3;
bool aprobado = notaMedia >= 5;

Console.WriteLine($"Nota 1: {examen1}");
Console.WriteLine($"Nota 2: {examen2}");
Console.WriteLine($"Nota 3: {examen3}");
Console.WriteLine($"Nota media: {notaMedia}");
Console.WriteLine($"¿Está aprobado?: {aprobado}");
