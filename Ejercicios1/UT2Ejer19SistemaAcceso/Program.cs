using System;

int edad = 22;
int intentosFallidos = 2;
bool usuarioActivo = true;
bool cuentaBloqueada = false;
bool esAdministrador = false;
bool dobleFactorCorrecto = true;

bool mayorDeEdad = edad >= 18;
bool intentosDisponibles = intentosFallidos < 3;
bool cuentaDisponible = usuarioActivo && !cuentaBloqueada;
bool superaSeguridadBasica = cuentaDisponible && dobleFactorCorrecto;
bool accesoNormal = mayorDeEdad && superaSeguridadBasica && intentosDisponibles;
bool accesoAlSistema = accesoNormal || esAdministrador;

Console.WriteLine("--- INFORME DE ACCESO ---");
Console.WriteLine($"Es mayor de edad: {mayorDeEdad}");
Console.WriteLine($"Tiene menos de 3 intentos fallidos: {intentosDisponibles}");
Console.WriteLine($"La cuenta está disponible: {cuentaDisponible}");
Console.WriteLine($"Supera la seguridad básica: {superaSeguridadBasica}");
Console.WriteLine($"Tiene acceso normal: {accesoNormal}");
Console.WriteLine($"Tiene acceso al sistema: {accesoAlSistema}");

// Cambia los valores iniciales y ejecuta de nuevo para comparar resultados.
// cuentaBloqueada = true impide el acceso normal.
// intentosFallidos = 3 también impide el acceso normal.
// esAdministrador = true permite acceder al sistema incluso sin acceso normal.
