# Ejercicio 1.29

29. Permisos de una aplicación
En este ejercicio combinarás if, else if y switch para implementar un pequeño sistema de permisos.
Dispones de:
bool autenticado = true;
string rol = "editor";
string accion = "editar";
bool propietario = false;
Los roles posibles son:
admin
editor
usuario
Y las acciones:
leer
editar
eliminar
Aplica estas reglas:
- Un usuario no autenticado no puede realizar ninguna acción.
- Un admin puede realizar cualquier acción.
- Un editor puede leer y editar.
- Un editor puede eliminar únicamente si es propietario.
- Un usuario puede leer.
- Un usuario puede editar únicamente si es propietario.
- Un usuario nunca puede eliminar.
El programa debe terminar mostrando únicamente:
Acción permitida
o:
Acción denegada
Puedes utilizar un if para realizar primero una comprobación general y un switch para organizar posteriormente los diferentes roles.
Nombre del proyecto: UT2Ejer29Permisos
