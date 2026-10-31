using System;
using System.Globalization;
using Microsoft.Data.Sqlite;


//  DAO: UsuarioDAO
//  Es la ÚNICA clase que consulta la tabla Usuario.
public class UsuarioDAO
{
    // Busca un usuario con ese nombre Y esa contraseña (ya hasheada).
    // Une Usuario con empleados (JOIN) para traer sus datos personales
    // y para no dejar entrar a empleados dados de baja.
    // Devuelve un Admin o un UsuarioComun, o null si no existe.
    public UsuarioBase? ObtenerPorCredenciales(string nombreUsuario, string contrasenaHash)
    {
        try
        {
            using (SqliteConnection conexion = ConexionDB.ObtenerConexion())
            {
                // @nombre y @clave son parámetros: evitan la inyección SQL
                string sql = "SELECT u.id_usuario, u.nombre_usuario, u.contrasenahash, u.rol, " +
                             "       e.nombre, e.apellido, e.email, e.telefono, e.fecha_ingreso " +
                             "FROM Usuario u " +
                             "INNER JOIN empleados e ON e.id_empleado = u.id_empleado " +
                             "WHERE u.nombre_usuario = @nombre " +
                             "  AND LOWER(u.contrasenahash) = @clave " +
                             "  AND e.estado = 'Activo'";

                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nombreUsuario);
                    comando.Parameters.AddWithValue("@clave", contrasenaHash.ToLower());

                    using (SqliteDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            return CrearSegunRol(lector);
                        }
                    }
                }
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Error de base de datos: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Las validaciones de UsuarioBase rechazaron un dato guardado en la base
            Console.WriteLine("Los datos del usuario en la base no son válidos: " + ex.Message);
        }

        return null;
    }

    // Según el rol guardado en la BD, creamos el objeto del tipo correcto
    private UsuarioBase CrearSegunRol(SqliteDataReader lector)
    {
        int id = lector.GetInt32(0);
        string nombreUsuario = lector.GetString(1);
        string hash = lector.GetString(2);
        string rol = lector.GetString(3);
        string nombre = lector.GetString(4);
        string apellido = lector.GetString(5);
        string? email = lector.IsDBNull(6) ? null : lector.GetString(6);
        string? telefono = lector.IsDBNull(7) ? null : lector.GetString(7);
        DateTime fecha = LeerFecha(lector.GetString(8));

        if (rol == "admin")
        {
            return new Admin(nombreUsuario, hash, nombre, apellido, email, telefono, fecha, true, id);
        }
        return new UsuarioComun(nombreUsuario, hash, nombre, apellido, email, telefono, fecha, true, id);
    }

    // Las fechas están guardadas como texto
    private DateTime LeerFecha(string texto)
    {
        DateTime fecha;
        if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            return fecha;
        }
        return DateTime.Now;
    }
}