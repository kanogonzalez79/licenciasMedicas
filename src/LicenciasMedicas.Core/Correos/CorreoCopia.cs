namespace LicenciasMedicas.Core.Correos;

/// <summary>Dirección de correo que se agrega en copia (CC) a todo aviso enviado, sin importar la unidad.</summary>
public sealed class CorreoCopia
{
    public int CorreoCopiaId { get; set; }
    public string CorreoElectronico { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class CorreoCopiaInvalidoException : Exception
{
    public CorreoCopiaInvalidoException()
        : base("El correo electrónico no tiene un formato válido.")
    {
    }
}

public sealed class CorreoCopiaDuplicadoException : Exception
{
    public CorreoCopiaDuplicadoException()
        : base("Ese correo electrónico ya está registrado en la lista de correos en copia.")
    {
    }
}
