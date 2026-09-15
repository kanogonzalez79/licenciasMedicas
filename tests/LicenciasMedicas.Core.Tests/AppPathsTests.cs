using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Core.Tests;

public sealed class AppPathsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AppPaths _paths;

    public AppPathsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _paths = new AppPaths(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    [Fact]
    public void ResolverRutaArchivoPermanente_NombreEnMayusculas_SeNormalizaATituloCase()
    {
        var (_, absoluta) = _paths.ResolverRutaArchivoPermanente("LM-001", "JUAN CARLOS PEREZ SOTO", ".pdf");

        Assert.Equal("Juan Carlos Perez Soto - LM-001.pdf", Path.GetFileName(absoluta));
    }

    [Fact]
    public void ResolverRutaArchivoPermanente_NombreConMezclaDeMayusculasYMinusculas_SeNormalizaATituloCase()
    {
        var (_, absoluta) = _paths.ResolverRutaArchivoPermanente("LM-002", "pEdRo GONZALEZ", ".pdf");

        Assert.Equal("Pedro Gonzalez - LM-002.pdf", Path.GetFileName(absoluta));
    }

    [Fact]
    public void ResolverRutaArchivoPermanente_NombreConTildesYEnie_SePreservan()
    {
        var (_, absoluta) = _paths.ResolverRutaArchivoPermanente("LM-003", "JOSÉ NÚÑEZ", ".pdf");

        Assert.Equal("José Núñez - LM-003.pdf", Path.GetFileName(absoluta));
    }

    [Fact]
    public void ResolverRutaArchivoPermanente_NombreConCaracteresInvalidosParaArchivo_SeReemplazan()
    {
        var (_, absoluta) = _paths.ResolverRutaArchivoPermanente("LM-004", "JUAN/PEREZ:SOTO", ".pdf");

        var nombreArchivo = Path.GetFileName(absoluta);
        Assert.DoesNotContain('/', nombreArchivo);
        Assert.DoesNotContain(':', nombreArchivo);
        Assert.All(Path.GetInvalidFileNameChars(), c => Assert.DoesNotContain(c, nombreArchivo));
    }

    [Fact]
    public void ResolverRutaArchivoPermanente_DosFoliosDistintosMismaPersona_ProducenNombresDistintosSinColisionar()
    {
        var (_, absolutaUno) = _paths.ResolverRutaArchivoPermanente("LM-005", "Juan Perez Soto", ".pdf");
        var (_, absolutaDos) = _paths.ResolverRutaArchivoPermanente("LM-006", "Juan Perez Soto", ".pdf");

        Assert.NotEqual(absolutaUno, absolutaDos);
        Assert.Equal("Juan Perez Soto - LM-005.pdf", Path.GetFileName(absolutaUno));
        Assert.Equal("Juan Perez Soto - LM-006.pdf", Path.GetFileName(absolutaDos));
    }

    [Fact]
    public void ResolverRutaArchivoPermanente_OrganizaPorAnioYMesComoAntes()
    {
        var ahora = DateTime.UtcNow;

        var (relativa, _) = _paths.ResolverRutaArchivoPermanente("LM-007", "Juan Perez Soto", ".pdf");

        var carpeta = Path.GetDirectoryName(relativa);
        Assert.Equal(Path.Combine(ahora.Year.ToString(), ahora.Month.ToString("D2")), carpeta);
    }
}
