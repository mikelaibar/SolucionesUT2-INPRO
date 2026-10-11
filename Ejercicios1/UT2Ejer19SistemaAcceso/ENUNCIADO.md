# Ejercicio 1.19

19. Sistema de acceso
Ahora combinarás operadores de comparación, aritméticos y lógicos para representar las reglas de un sistema de seguridad.
El sistema dispone de la siguiente información:
int edad = 22;
int intentosFallidos = 2;
bool usuarioActivo = true;
bool cuentaBloqueada = false;
bool esAdministrador = false;
bool dobleFactorCorrecto = true;
Crea expresiones y almacena sus resultados en variables bool para determinar:
- Si el usuario es mayor de edad.
- Si todavía tiene menos de 3 intentos fallidos.
- Si la cuenta está disponible: está activa y no está bloqueada.
- Si supera la seguridad básica: cuenta disponible y doble factor correcto.
- Si tiene acceso normal: es mayor de edad, supera la seguridad básica y tiene menos de 3 intentos fallidos.
- Si tiene acceso al sistema: tiene acceso normal o es administrador.
Muestra un informe con todas las comprobaciones.
Después modifica manualmente algunos valores iniciales y vuelve a ejecutar el programa. Por ejemplo:
- cuentaBloqueada = true
- intentosFallidos = 3
- esAdministrador = true
Observa cómo cambian los resultados.
Nombre del proyecto: UT2Ejer19SistemaAcceso
