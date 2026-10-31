public classs Admin : UsuarioBase
{
    public Admin(string nombreUsurio, string contraseñaHash, string nombre ,  string apellido,
        string? email, string? telefono, DateTime fecha_creacion, bool activo = true, int id = 0)
        : base(nombreUsuario, contraseñaHash, nombre, apellido, email, telefono, fecha_creacion, activo, id)
    {
    }

    public override string rol => "admin";
    public override bool PuedeDarDeBajaSocios => true;
    public override bool PuedeModificarPrecios => true;
    public override bool PuedeGestionarUsuarios => true;
    public override bool PuedeExportar => true;
}