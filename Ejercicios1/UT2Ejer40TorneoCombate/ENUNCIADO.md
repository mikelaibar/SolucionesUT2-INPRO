# Ejercicio 1.40

40. Torneo de combate
En esta última actividad tendrás que diseñar la lógica completa de un pequeño videojuego combinando bucles, condicionales, operadores, switch, break y continue.
El jugador y el enemigo comienzan con los siguientes valores:
int vidaJugador = 100;
int vidaEnemigo = 100;
int pociones = 3;
int turno = 1;
Mientras tanto el jugador como el enemigo continúen con vida, el combate debe seguir ejecutándose. En cada turno se mostrará la situación actual y el siguiente menú:
--- TURNO 1 ---
Tu vida: 100
Vida enemigo: 100
Pociones: 3
1. Atacar
2. Ataque fuerte
3. Usar poción
4. Huir
Selecciona una acción:
La opción deberá solicitarse al jugador mediante Console.ReadLine() y procesarse utilizando un switch.
Las acciones funcionan de la siguiente manera:
- Opción 1 - Atacar: el enemigo pierde 15 puntos de vida.
- Opción 2 - Ataque fuerte: el enemigo pierde 30 puntos de vida, pero este ataque solamente puede utilizarse en los turnos pares. Si el jugador intenta utilizarlo en un turno impar, muestra Ataque fuerte no disponible y utiliza continue para volver al comienzo del bucle.
- Opción 3 - Usar poción: el jugador recupera 20 puntos de vida y se consume una poción. Si no quedan pociones, muestra No quedan pociones y no recupera vida.
- Opción 4 - Huir: muestra Has abandonado el combate y utiliza break para finalizar el combate inmediatamente.
- Cualquier otra opción: muestra Acción no válida. No debe realizarse ningún ataque.
Después de que el jugador realice una acción válida, comprueba la vida del enemigo.
Si el enemigo todavía tiene más de 0 puntos de vida, realiza automáticamente su ataque:
El enemigo te ataca y pierdes 10 puntos de vida.
Por tanto, se restarán 10 puntos a vidaJugador.
Al finalizar cada turno completo, incrementa la variable turno antes de comenzar la siguiente repetición.
Importante: si el ataque del jugador reduce la vida del enemigo a 0 o menos, el enemigo ya está derrotado y no debe realizar su ataque.
El combate puede finalizar de tres formas:
- Si vidaEnemigo llega a 0 o menos, muestra ¡Has ganado el combate!.
- Si vidaJugador llega a 0 o menos, muestra Has sido derrotado.
- Si el jugador selecciona la opción de huir, termina el combate sin mostrar ninguno de los dos mensajes anteriores.
Al finalizar el combate muestra también un pequeño resumen:
--- FIN DEL COMBATE ---
Turnos jugados: ...
Vida restante del jugador: ...
Vida restante del enemigo: ...
Pociones restantes: ...
Antes de programar: piensa qué tipo de bucle utilizarás, cuándo debe incrementarse turno, dónde colocarás break y qué instrucciones dejarán de ejecutarse cuando utilices continue.
Reto adicional: modifica el uso de las pociones para que la vida del jugador nunca pueda superar los 100 puntos. Por ejemplo, si tiene 90 puntos de vida y utiliza una poción, su vida deberá quedarse en 100 y no en 110.
Antes de terminar, prueba al menos estas situaciones:
- Realizar un ataque normal.
- Intentar un ataque fuerte en un turno impar.
- Realizar correctamente un ataque fuerte en un turno par.
- Utilizar una poción.
- Intentar utilizar una poción cuando ya no queda ninguna.
- Derrotar al enemigo.
- Ser derrotado por el enemigo.
- Abandonar el combate utilizando la opción de huir.
Nombre del proyecto: UT2Ejer40TorneoCombate
