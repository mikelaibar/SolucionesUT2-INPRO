# Ejercicio 1.39

39. Caja registradora
En este ejercicio crearás una pequeña caja registradora que permita introducir productos uno a uno. Combinarás entrada por teclado, bucles, condicionales, acumuladores, break y continue.
El programa comenzará con las siguientes variables:
double total = 0;
int productos = 0;
El programa deberá solicitar repetidamente al usuario el precio de un producto:
Introduce el precio del producto (0 para terminar):
Después de leer el precio, pueden ocurrir tres situaciones:
- El precio es negativo.
Se trata de un dato incorrecto. Muestra:
Precio no válido.
Utiliza continue para ignorar ese precio y comenzar directamente una nueva iteración. Ese producto no debe contabilizarse ni sumarse al total.
- El precio es exactamente 0.
Significa que el usuario ha terminado de introducir productos. Utiliza break para salir inmediatamente del bucle.
- El precio es mayor que 0.
Se trata de un producto válido. Debes:
- Sumar su precio a total.
- Incrementar productos.
- Mostrar el número del producto registrado.
- Mostrar el total acumulado hasta ese momento.
Por ejemplo, después de introducir un producto válido:
Producto 3 registrado.
Total acumulado: 74.50 €
Además, si un producto tiene un precio superior a 100 €, muestra también:
Producto de precio elevado.
Cuando el usuario introduzca 0, termina la introducción de productos y muestra el resumen de la compra:
--- RESUMEN DE COMPRA ---
Productos registrados: ...
Total de la compra: ... €
Piensa en el funcionamiento: continue sirve para descartar un precio incorrecto y pasar al siguiente producto, mientras que break termina definitivamente la introducción de productos.
Antes de dar el programa por terminado, prueba como mínimo esta secuencia de precios:
25
-5
120
40
0
Comprueba que:
- El precio -5 se ignora completamente.
- El producto de 120 € muestra el aviso de precio elevado.
- El 0 termina el bucle y no se registra como producto.
- Se han registrado exactamente 3 productos.
- El total de la compra es 185 €.
Importante: decide qué tipo de bucle utilizarás. El programa no sabe de antemano cuántos productos va a introducir el usuario, por lo que un for no es necesariamente la opción más adecuada.
Nombre del proyecto: UT2Ejer39CajaRegistradora
