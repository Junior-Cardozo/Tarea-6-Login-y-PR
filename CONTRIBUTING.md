# Guía de contribución

## 1. Forma de trabajo

Todos los integrantes trabajarán desde sus propios forks del repositorio principal.

No se agregarán integrantes como colaboradores del repositorio base.

## 2. Crear un Fork

1. Entrar al repositorio principal.
2. Presionar el botón Fork.
3. Crear una copia en la cuenta personal de GitHub.
4. Clonar el fork en el equipo local.

## 3. Nombrar las ramas

Cada integrante debe crear ramas independientes según la etapa de trabajo.

Formato:

`tipo/nombre-integrante-descripcion`

Ejemplos:

- `feature/name-tablas`
- `feature/name-autenticacion`
- `feature/name-controladores`
- `test/name-post-duplicado`
- `feature/name-put-softdelete`

No se deben realizar cambios directamente sobre main.

## 4. Nombrar los commits

Los commits deben describir claramente los cambios realizados.

Ejemplos:

- `feat: agregar modelos y tablas de incidentes`
- `feat: implementar autenticacion con Discord`
- `feat: agregar controladores protegidos`
- `feat: implementar operaciones PUT`
- `feat: implementar soft delete`

## 5. Probar los cambios

Antes de subir una contribución se debe:

1. Compilar el proyecto.
2. Comprobar que no existan errores.
3. Probar las funcionalidades implementadas.
4. Verificar que no se incluyan credenciales reales ni archivos innecesarios.

## 6. Enviar una Pull Request

1. Subir la rama al fork personal.
2. Entrar al repositorio principal.
3. Crear una Pull Request desde el fork.
4. Seleccionar como destino la rama main del repositorio principal.
5. Explicar los cambios realizados.
6. Solicitar revisión al líder u otro integrante.

## 7. Revisión e integración

- Todas las Pull Requests deben ser revisadas antes de integrarse.
- El autor de una Pull Request no puede aprobar su propia contribución.
- El líder debe revisar, aprobar e integrar los aportes.
- Las Pull Requests rechazadas deben conservarse como evidencia.

## 8. Reglas generales

- No trabajar directamente sobre el repositorio principal.
- No subir contraseñas, tokens ni credenciales reales.
- Mantener el proyecto actualizado.
- Respetar la distribución de responsabilidades.
- Cada integrante es responsable de documentar sus aportes.
