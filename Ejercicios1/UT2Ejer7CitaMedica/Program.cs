using System;

// Todos los datos de esta ficha son ficticios.
string nombrePaciente = "Alex Ejemplo";
string tarjetaSanitaria = "001234567890";
int edad = 20;
double temperaturaCorporal = 38.2;
bool tieneFiebre = true;
int? numeroConsulta = null;
char inicialProfesional = 'L';

Console.WriteLine("--- CITA MÉDICA ---");
Console.WriteLine("Paciente: " + nombrePaciente);
Console.WriteLine("Tarjeta sanitaria: " + tarjetaSanitaria);
Console.WriteLine("Edad: " + edad);
Console.WriteLine("Temperatura corporal: " + temperaturaCorporal + " °C");
Console.WriteLine("Tiene fiebre: " + tieneFiebre);
Console.WriteLine("Consulta: " + numeroConsulta + " (null: pendiente de asignar)");
Console.WriteLine("Inicial del profesional: " + inicialProfesional);
